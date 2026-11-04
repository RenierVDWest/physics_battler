using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PhysicsBattler.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PhysicsBattler.Editor.Audio
{
    /// <summary>
    /// Physics Battler > Audio > Sound Effects (the tool from Hexamus, as it is): makes the game's sounds from the prompts in DA_Sfx with ElevenLabs.
    /// Paste your API key once (it's kept in this computer's Unity preferences, never in the project), press
    /// Generate Missing, and every take that isn't on disk yet is made and filed as Audio/Sfx/{id}_{take}.mp3,
    /// imported and linked. Don't like one? Change its prompt in DA_Sfx if you like, and press Redo.
    /// </summary>
    internal sealed class SfxGeneratorWindow : EditorWindow
    {
        // (The same preference as Hexamus's: Unity keeps it per computer, not per project, so the ElevenLabs key
        // entered there is already here. Never in the project or on GitHub.)
        private const string KeyPref = "Hexamus.ElevenLabs.ApiKey";
        private const string Endpoint = "https://api.elevenlabs.io/v1/sound-generation?output_format=mp3_44100_128";
        private const string Model = "eleven_text_to_sound_v2";
        private const int AtOnce = 2;

        private SfxLibrary _library;
        private string _key;
        private Vector2 _scroll;
        private static readonly HashSet<string> OpenPrompts = new HashSet<string>();
        private readonly Queue<Job> _queue = new Queue<Job>();
        private readonly List<Job> _running = new List<Job>();
        private int _done;
        private int _total;
        private int _creditsUsed;
        private string _status = "";
        private bool _failed;
        private double _resumeAt;

        private sealed class Job
        {
            public SfxEntry Entry;
            public int Take;
            public UnityWebRequest Request;
            public int Tries;
        }

        /// <summary>The body ElevenLabs expects (JsonUtility writes the field names as they are).</summary>
        [Serializable]
        private struct Body
        {
            public string text;
            public float duration_seconds;
            public float prompt_influence;
            public bool loop;
            public string model_id;
        }

        [MenuItem("Physics Battler/Audio/Sound Effects", priority = 40)]
        public static void Open()
        {
            var window = GetWindow<SfxGeneratorWindow>("Sound Effects");
            window.minSize = new Vector2(460f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            _key = EditorPrefs.GetString(KeyPref, "");
            _library = SfxAssets.LoadOrCreate();
            EditorApplication.update += Tick;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            StopAll("Stopped.");
        }

        private bool Busy => _queue.Count > 0 || _running.Count > 0;

        private void OnGUI()
        {
            if (_library == null)
            {
                _library = SfxAssets.LoadOrCreate();
                if (_library == null)
                {
                    EditorGUILayout.HelpBox("The sound list (DA_Sfx) is still importing. One moment…", MessageType.Info);
                    return;
                }
            }

            EditorGUILayout.LabelField("ElevenLabs", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _key = EditorGUILayout.PasswordField(new GUIContent("API key", "Kept in this computer's Unity preferences, never in the project."), _key);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(KeyPref, _key.Trim());
            }

            EditorGUILayout.HelpBox("The key stays on this computer (Unity's preferences), not in the project or on GitHub. " +
                "Sound effects cost 40 credits a second.", MessageType.None);

            var missing = Missing().ToList();
            int credits = SfxLibrary.CreditsFor(missing.GroupBy(m => m.Entry).Select(g => (g.Key, g.Count())));
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{_library.Sounds.Count} sounds, {missing.Count} takes still to make (about {credits:N0} credits).");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(Busy || missing.Count == 0 || string.IsNullOrWhiteSpace(_key)))
                {
                    if (GUILayout.Button("Generate missing", GUILayout.Height(30)))
                    {
                        BeginGenerating(missing);
                    }
                }

                using (new EditorGUI.DisabledScope(!Busy))
                {
                    if (GUILayout.Button("Stop", GUILayout.Height(30), GUILayout.Width(80)))
                    {
                        StopAll("Stopped.");
                    }
                }

                if (GUILayout.Button("Sound list", GUILayout.Height(30), GUILayout.Width(90)))
                {
                    Selection.activeObject = _library;
                    EditorGUIUtility.PingObject(_library);
                }
            }

            if (Busy || _total > 0)
            {
                var rect = EditorGUILayout.GetControlRect(false, 20f);
                EditorGUI.ProgressBar(rect, _total > 0 ? _done / (float)_total : 0f, $"{_done} / {_total}   ({_creditsUsed:N0} credits)");
            }

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.HelpBox(_status, _failed ? MessageType.Error : MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var entry in _library.Sounds)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }

                int made = SfxAssets.TakesMade(entry);
                bool canMake = !Busy && !string.IsNullOrWhiteSpace(_key) && !string.IsNullOrWhiteSpace(entry.Prompt);
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Open a sound to see and change its prompt.
                    bool open = OpenPrompts.Contains(entry.Id);
                    bool nowOpen = EditorGUILayout.Foldout(open, new GUIContent(entry.Id, entry.Prompt), true,
                        made >= entry.Takes ? EditorStyles.foldout : BoldFoldout());
                    if (nowOpen != open)
                    {
                        if (nowOpen)
                        {
                            OpenPrompts.Add(entry.Id);
                        }
                        else
                        {
                            OpenPrompts.Remove(entry.Id);
                        }
                    }

                    EditorGUILayout.LabelField($"{made}/{entry.Takes}  {entry.Duration:0.#}s{(entry.Loop ? " loop" : "")}", GUILayout.Width(90));

                    // Its volume, right here (kept in DA_Sfx, as on the Sound object).
                    EditorGUI.BeginChangeCheck();
                    float volume = EditorGUILayout.Slider(entry.Volume, 0f, 2f, GUILayout.MinWidth(80));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_library, "Sound volume");
                        entry.Volume = volume;
                        EditorUtility.SetDirty(_library);
                    }

                }

                if (OpenPrompts.Contains(entry.Id))
                {
                    DrawPrompt(entry);
                }

                // Each take: ▶ to hear it (at the volume above), ⟳ to make just that one again.
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(16);
                    for (int take = 0; take < entry.Takes; take++)
                    {
                        var clip = SfxAssets.ClipFor(entry, take);
                        using (new EditorGUI.DisabledScope(clip == null))
                        {
                            if (GUILayout.Button(new GUIContent($"▶ {take + 1}", clip == null ? "Not made yet" : $"Hear take {take + 1}"),
                                    EditorStyles.miniButtonLeft, GUILayout.Width(40)))
                            {
                                SfxPreview.Play(clip, entry.Volume);
                            }
                        }

                        using (new EditorGUI.DisabledScope(!canMake))
                        {
                            if (GUILayout.Button(new GUIContent("⟳", $"Make take {take + 1} again (the others stay)"), EditorStyles.miniButtonRight,
                                    GUILayout.Width(24)))
                            {
                                BeginGenerating(new List<(SfxEntry, int)> { (entry, take) });
                            }
                        }

                        GUILayout.Space(6);
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("■", "Stop the sound"), EditorStyles.miniButton, GUILayout.Width(24)))
                    {
                        SfxPreview.Stop();
                    }
                }

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private static GUIStyle _boldFoldout;

        private static GUIStyle BoldFoldout()
        {
            return _boldFoldout ??= new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
        }

        /// <summary>
        /// What a sound is made from, to change before a ⟳: its prompt, how closely ElevenLabs follows it (lower gives
        /// takes that differ more), and its length. Kept in DA_Sfx.
        /// </summary>
        private void DrawPrompt(SfxEntry entry)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                var wrap = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                string prompt = EditorGUILayout.TextArea(entry.Prompt ?? "", wrap, GUILayout.MinHeight(48));
                float influence = EditorGUILayout.Slider(new GUIContent("Follow prompt",
                    "How closely ElevenLabs follows the prompt. Lower (0.2 or so) gives takes that differ more from each other; higher keeps them literal and alike."),
                    entry.PromptInfluence, 0f, 1f);
                float duration = EditorGUILayout.Slider(new GUIContent("Seconds", "How long each take is (40 credits a second)."), entry.Duration, 0.5f, 30f);
                int takes = EditorGUILayout.IntSlider(new GUIContent("Takes", "How many takes to make; one is picked at random each time it plays."), entry.Takes, 1, 6);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_library, "Sound prompt");
                    entry.Prompt = prompt;
                    entry.PromptInfluence = influence;
                    entry.Duration = Mathf.Round(duration * 10f) / 10f;
                    entry.Takes = takes;
                    EditorUtility.SetDirty(_library);
                }

                var defaults = SfxDefaults.All().FirstOrDefault(d => d.Id == entry.Id);
                if (defaults != null && defaults.Prompt != entry.Prompt)
                {
                    if (GUILayout.Button("Back to the original prompt", EditorStyles.miniButton, GUILayout.Width(170)))
                    {
                        Undo.RecordObject(_library, "Sound prompt");
                        entry.Prompt = defaults.Prompt;
                        EditorUtility.SetDirty(_library);
                        GUI.FocusControl(null);
                    }
                }
            }

            EditorGUILayout.Space(2);
        }

        /// <summary>Every take of every sound that isn't on disk yet.</summary>
        private IEnumerable<(SfxEntry Entry, int Take)> Missing()
        {
            foreach (var entry in _library.Sounds)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || string.IsNullOrWhiteSpace(entry.Prompt))
                {
                    continue;
                }

                for (int take = 0; take < entry.Takes; take++)
                {
                    if (!SfxAssets.Exists(SfxAssets.TakePath(entry.Id, take)))
                    {
                        yield return (entry, take);
                    }
                }
            }
        }

        private void BeginGenerating(List<(SfxEntry Entry, int Take)> work)
        {
            if (work.Count == 0)
            {
                return;
            }

            _queue.Clear();
            foreach (var (entry, take) in work)
            {
                _queue.Enqueue(new Job { Entry = entry, Take = take });
            }

            _total = work.Count;
            _done = 0;
            _creditsUsed = 0;
            _failed = false;
            _status = "Making sounds… they appear as they arrive. Keep this window open, and don't press Play or change scripts " +
                "until it's done (that stops it; press Generate missing again to carry on).";
            SfxAssets.EnsureFolder(SfxAssets.ClipsFolder);
        }

        private void Tick()
        {
            if (!Busy || _library == null)
            {
                return;
            }

            // Finished requests: save every take that arrived, then deal with anything that went wrong.
            string failure = null;
            foreach (var job in _running.Where(j => j.Request.isDone).ToList())
            {
                _running.Remove(job);
                failure ??= Finish(job);
                job.Request.Dispose();
            }

            if (failure != null)
            {
                StopAll(failure);
                _failed = true;
                Repaint();
                return;
            }

            while (_running.Count < AtOnce && _queue.Count > 0 && EditorApplication.timeSinceStartup >= _resumeAt)
            {
                var job = _queue.Dequeue();
                job.Request = Send(job.Entry);
                _running.Add(job);
            }

            if (!Busy)
            {
                AssetDatabase.Refresh();
                SfxAssets.Relink(_library);
                AssetDatabase.SaveAssets();
                if (!_failed)
                {
                    _status = $"Done: {_done} takes made, {_creditsUsed:N0} credits. Press Play and listen; Redo any you don't like.";
                }
            }

            Repaint();
        }

        private UnityWebRequest Send(SfxEntry entry)
        {
            var body = JsonUtility.ToJson(new Body
            {
                text = entry.Prompt,
                duration_seconds = Mathf.Clamp(entry.Duration, 0.5f, 30f),
                prompt_influence = Mathf.Clamp01(entry.PromptInfluence),
                loop = entry.Loop,
                model_id = Model,
            });
            var request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 120,
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "audio/mpeg");
            request.SetRequestHeader("xi-api-key", _key.Trim());
            request.SendWebRequest();
            return request;
        }

        /// <summary>Saves a finished take (or queues it to try again); the reason to stop, if it failed for good.</summary>
        private string Finish(Job job)
        {
            var request = job.Request;
            long code = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success && code == 200 && request.downloadHandler.data != null
                && request.downloadHandler.data.Length > 0)
            {
                File.WriteAllBytes(SfxAssets.TakePath(job.Entry.Id, job.Take) + ".mp3", request.downloadHandler.data);
                _done++;
                _creditsUsed += Mathf.CeilToInt(job.Entry.Duration * 40f);
                if (_done % 6 == 0)
                {
                    // Bring what's arrived into the project as it goes.
                    AssetDatabase.Refresh();
                }

                return null;
            }

            string detail = request.downloadHandler != null ? request.downloadHandler.text : "";
            if ((code == 429 || code >= 500 || request.result == UnityWebRequest.Result.ConnectionError) && job.Tries < 4)
            {
                // Busy or a hiccup: wait a moment and try that one again.
                job.Tries++;
                _queue.Enqueue(job);
                _resumeAt = EditorApplication.timeSinceStartup + 3.0 * job.Tries;
                _status = $"ElevenLabs is busy; trying again… ({job.Entry.Id})";
                return null;
            }

            string reason = detail.Contains("quota") ? "Out of credits for this month (or the key's credit limit was reached)."
                : code == 401 ? "The API key was refused: check it, and that it's allowed Sound Effects."
                : code == 422 ? $"ElevenLabs didn't accept the request for {job.Entry.Id} (check its prompt and length)."
                : $"Couldn't make {job.Entry.Id}: {request.error}";
            return reason + (string.IsNullOrEmpty(detail) ? "" : "\n" + Trim(detail));
        }

        private void StopAll(string message)
        {
            _queue.Clear();
            foreach (var job in _running)
            {
                job.Request.Abort();
                job.Request.Dispose();
            }

            bool wasBusy = _running.Count > 0;
            _running.Clear();
            _status = message;
            if (wasBusy && !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                AssetDatabase.Refresh();
                if (_library != null)
                {
                    SfxAssets.Relink(_library);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static string Trim(string text)
        {
            return text.Length > 300 ? text.Substring(0, 300) + "…" : text;
        }
    }
}
