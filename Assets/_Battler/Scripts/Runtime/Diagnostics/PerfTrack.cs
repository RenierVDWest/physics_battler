using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// The playtest log's performance analytics (perf.csv.gz, one row per rendered frame):
    /// - how long the frame took (wall clock) and how many physics steps it ran (a long frame makes
    ///   the next one catch up with several, which is a stutter of its own);
    /// - the game's own work by section (<see cref="Perf"/>: unit steps, poses, the planner's tries,
    ///   arm solves, sweeps, the recorder's sampling, writing and pictures) and the contact
    ///   modification on PhysX's threads;
    /// - Unity's own counters and markers where it has them (the main thread, physics, rendering,
    ///   the editor's loop, garbage collection, memory): every one Unity offers is listed in
    ///   perf_counters.txt, and the useful ones are recorded (prof_*, milliseconds or KB);
    /// - the managed heap and garbage collections.
    /// A frame over <see cref="HitchMs"/> is a hitch: it goes in events.log ("perf") with what took
    /// the time, and the worst are listed in session.md.
    /// </summary>
    public sealed class PerfTrack : IDisposable
    {
        /// <summary>A frame longer than this is a hitch (milliseconds).</summary>
        public const float HitchMs = 40f;

        // Unity's counters and markers worth recording, by name (those Unity doesn't have are skipped).
        private static readonly string[] Wanted =
        {
            "Main Thread", "Render Thread", "CPU Main Thread Frame Time", "CPU Render Thread Frame Time", "CPU Total Frame Time", "GPU Frame Time",
            "PlayerLoop", "EditorLoop", "FixedUpdate.PhysicsFixedUpdate", "FixedUpdate.ScriptRunBehaviourFixedUpdate",
            "Physics.Simulate", "Physics.Processing", "Physics.FetchResults", "Physics.UpdateBodies", "Physics.ProcessReports",
            "Physics.Contacts", "Physics.SyncColliderTransform", "Physics.SyncRigidbodyTransform", "Physics.Interpolation", "Physics.ProcessingCCD",
            "Update.ScriptRunBehaviourUpdate", "PreLateUpdate.ScriptRunBehaviourLateUpdate", "BehaviourUpdate", "FixedBehaviourUpdate", "LateBehaviourUpdate",
            "Camera.Render", "RenderPipelineManager.DoRenderLoop_Internal()", "Inl_ScriptableRenderer.Execute", "Gfx.WaitForPresentOnGfxThread",
            "Gfx.WaitForGfxCommandsFromMainThread", "Gfx.PresentFrame", "WaitForTargetFPS",
            "GC.Collect", "GC.Alloc", "GC Allocated In Frame", "GC Allocation In Frame Count", "GC Used Memory", "GC Reserved Memory",
            "Total Used Memory", "System Used Memory",
            "Active Dynamic Bodies", "Active Kinematic Bodies", "Dynamic Bodies", "Kinematic Bodies", "Static Colliders",
            "Contacts Count", "Physics Contacts", "Active Constraints", "Trigger Overlaps",
            "Batches Count", "SetPass Calls Count", "Draw Calls Count", "Triangles Count",
            "AsyncGPUReadback.Update", "Loading.ReadObject", "Shader.CreateGPUProgram", "Inspector.Repaint", "GUIView.RepaintAll",
        };

        private readonly TableWriter table;
        private readonly List<(string column, ProfilerRecorder recorder, ProfilerMarkerDataUnit unit)> recorders = new List<(string, ProfilerRecorder, ProfilerMarkerDataUnit)>();
        private readonly double[] ms = new double[(int)PerfSection.Count], lastMs = new double[(int)PerfSection.Count];
        private readonly int[] count = new int[(int)PerfSection.Count], lastCount = new int[(int)PerfSection.Count];
        private double lastContactMs;
        private long lastContactPairs, lastContactCalls;
        private int fixedThisFrame, lastFixed;
        private bool haveLast;
        private readonly int[] gcWas = new int[3], gc = new int[3];
        private readonly List<(string, float)> prof = new List<(string, float)>();
        private static readonly string[] MsColumn = new string[(int)PerfSection.Count], CountColumn = new string[(int)PerfSection.Count];

        // For the summary.
        private readonly List<float> frameMs = new List<float>();
        private readonly double[] sectionTotal = new double[(int)PerfSection.Count];
        private readonly List<(float ms, float t, string what)> worst = new List<(float, float, string)>();
        private int hitches, bigHitches, frames, gcCollections;
        private double profiledSeconds;

        public string Counters { get; private set; } = "";

        public PerfTrack(string sessionPath)
        {
            table = new TableWriter(Path.Combine(sessionPath, "perf.csv.gz"));
            var handles = new List<ProfilerRecorderHandle>();
            var byName = new Dictionary<string, ProfilerRecorderHandle>();
            var list = new StringBuilder();
            list.AppendLine("# Unity's profiler counters and markers available in this session (category | name | unit)");
            try
            {
                ProfilerRecorderHandle.GetAvailable(handles);
                var lines = new List<string>();
                foreach (var h in handles)
                {
                    var d = ProfilerRecorderHandle.GetDescription(h);
                    if (string.IsNullOrEmpty(d.Name))
                    {
                        continue;
                    }

                    lines.Add($"{d.Category.Name} | {d.Name} | {d.UnitType}");
                    if (!byName.ContainsKey(d.Name))
                    {
                        byName[d.Name] = h;
                    }
                }

                lines.Sort(StringComparer.Ordinal);
                foreach (var l in lines)
                {
                    list.AppendLine(l);
                }
            }
            catch (Exception exception)
            {
                list.AppendLine("(could not list them: " + exception.Message + ")");
            }

            try
            {
                File.WriteAllText(Path.Combine(sessionPath, "perf_counters.txt"), list.ToString());
            }
            catch (Exception)
            {
                // (Only a listing.)
            }

            var started = new StringBuilder();
            foreach (var name in Wanted)
            {
                if (!byName.TryGetValue(name, out var handle))
                {
                    continue;
                }

                try
                {
                    var unit = ProfilerRecorderHandle.GetDescription(handle).UnitType;
                    var recorder = new ProfilerRecorder(handle, 1, ProfilerRecorderOptions.Default | ProfilerRecorderOptions.SumAllSamplesInFrame);
                    recorder.Start();
                    if (recorder.Valid)
                    {
                        recorders.Add(("prof_" + Column(name), recorder, unit));
                        started.Append(started.Length > 0 ? ", " : "").Append(name);
                    }
                    else
                    {
                        recorder.Dispose();
                    }
                }
                catch (Exception)
                {
                    // (Not this one.)
                }
            }

            Counters = started.ToString();
            for (int i = 0; i < (int)PerfSection.Count; i++)
            {
                MsColumn[i] = "ms_" + (PerfSection)i;
                CountColumn[i] = "n_" + (PerfSection)i;
            }

            for (int g = 0; g < 3; g++)
            {
                gcWas[g] = GC.CollectionCount(g);
            }

            Perf.Take(ms, count, out _, out _, out _);   // (start clean)
            Perf.Enabled = true;
        }

        private static string Column(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return sb.ToString().Trim('_');
        }

        /// <summary>A physics step ran (the recorder's FixedUpdate).</summary>
        public void NoteFixedStep() => fixedThisFrame++;

        /// <summary>
        /// Once a rendered frame (the recorder's LateUpdate). Unity's frame time and its counters are
        /// the last whole frame's, so each row is written a frame late, with that frame's own sections
        /// and physics steps.
        /// </summary>
        public void EndFrame(float t, int step, int units, Action<string> hitch)
        {
            Perf.Take(ms, count, out double contactMs, out long pairs, out long calls);
            int steps = fixedThisFrame;
            fixedThisFrame = 0;
            if (haveLast)
            {
                WriteRow(t, step, units, hitch);
            }

            Array.Copy(ms, lastMs, ms.Length);
            Array.Copy(count, lastCount, count.Length);
            lastContactMs = contactMs;
            lastContactPairs = pairs;
            lastContactCalls = calls;
            lastFixed = steps;
            haveLast = true;
        }

        private void WriteRow(float t, int step, int units, Action<string> hitch)
        {
            float frame = Time.unscaledDeltaTime * 1000f;
            frames++;
            frameMs.Add(frame);
            profiledSeconds += Time.unscaledDeltaTime;
            for (int g = 0; g < 3; g++)
            {
                int now = GC.CollectionCount(g);
                gc[g] = now - gcWas[g];
                gcWas[g] = now;
            }

            gcCollections += gc[0];
            table.Begin();
            table.Add("t", t, 3);
            table.Add("step", step);
            table.Add("frame_ms", frame, 2);
            table.Add("dt_ms", Time.deltaTime * 1000f, 2);
            table.Add("timeScale", Time.timeScale, 2);
            table.Add("fixedSteps", lastFixed);
            table.Add("units", units);
            for (int i = 0; i < (int)PerfSection.Count; i++)
            {
                table.Add(MsColumn[i], (float)lastMs[i], 3);
                table.Add(CountColumn[i], lastCount[i]);
                sectionTotal[i] += lastMs[i];
            }

            table.Add("ms_ContactModify", (float)lastContactMs, 3);
            table.Add("n_ContactPairs", (int)Math.Min(int.MaxValue, lastContactPairs));
            table.Add("n_ContactCallbacks", (int)Math.Min(int.MaxValue, lastContactCalls));
            table.Add("gc0", gc[0]);
            table.Add("gc1", gc[1]);
            table.Add("gc2", gc[2]);
            table.Add("heapMB", GC.GetTotalMemory(false) / (1024f * 1024f), 1);
            prof.Clear();
            foreach (var (column, recorder, unit) in recorders)
            {
                float v = Value(recorder, unit);
                table.Add(column, v, unit == ProfilerMarkerDataUnit.TimeNanoseconds ? 3 : 1);
                if (unit == ProfilerMarkerDataUnit.TimeNanoseconds)
                {
                    prof.Add((column.Substring(5), v));
                }
            }

            table.End();

            if (frame < HitchMs)
            {
                return;
            }

            hitches++;
            bigHitches += frame >= 100f ? 1 : 0;
            var what = new StringBuilder();
            what.Append($"{frame:0} ms frame, {lastFixed} physics steps");
            if (gc[0] > 0)
            {
                what.Append($", garbage collected (gen0 {gc[0]}, gen1 {gc[1]}, gen2 {gc[2]})");
            }

            what.Append("; ours: ");
            var ours = new List<(string name, double ms, int n)>();
            for (int i = 0; i < (int)PerfSection.Count; i++)
            {
                if (lastMs[i] >= 0.5)
                {
                    ours.Add((((PerfSection)i).ToString(), lastMs[i], lastCount[i]));
                }
            }

            if (lastContactMs >= 0.5)
            {
                ours.Add(("ContactModify", lastContactMs, (int)lastContactPairs));
            }

            ours.Sort((a, b) => b.ms.CompareTo(a.ms));
            for (int i = 0; i < ours.Count && i < 6; i++)
            {
                what.Append(i > 0 ? ", " : "").Append($"{ours[i].name} {ours[i].ms:0.0} ms ({ours[i].n})");
            }

            if (ours.Count == 0)
            {
                what.Append("nothing over 0.5 ms");
            }

            prof.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            if (prof.Count > 0)
            {
                what.Append("; Unity: ");
                for (int i = 0; i < prof.Count && i < 8; i++)
                {
                    what.Append(i > 0 ? ", " : "").Append($"{prof[i].Item1} {prof[i].Item2:0.0} ms");
                }
            }

            string text = what.ToString();
            hitch?.Invoke(text);
            worst.Add((frame, t, text));
            worst.Sort((a, b) => b.ms.CompareTo(a.ms));
            if (worst.Count > 12)
            {
                worst.RemoveAt(worst.Count - 1);
            }
        }

        private static float Value(ProfilerRecorder recorder, ProfilerMarkerDataUnit unit)
        {
            long v = recorder.LastValue;
            switch (unit)
            {
                case ProfilerMarkerDataUnit.TimeNanoseconds: return v / 1e6f;   // ms
                case ProfilerMarkerDataUnit.Bytes: return v / 1024f;             // KB
                default: return v;
            }
        }

        public void Flush()
        {
            try { table.Flush(); } catch (Exception) { /* next time */ }
        }

        /// <summary>The session's performance, for session.md.</summary>
        public void AppendSummary(StringBuilder md)
        {
            md.AppendLine("## Performance");
            md.AppendLine();
            if (frames == 0)
            {
                md.AppendLine("No frames measured.");
                md.AppendLine();
                return;
            }

            var sorted = new List<float>(frameMs);
            sorted.Sort();
            float P(float q) => sorted[Mathf.Clamp((int)(q * (sorted.Count - 1)), 0, sorted.Count - 1)];
            var inv = CultureInfo.InvariantCulture;
            md.AppendLine(string.Format(inv, "{0} frames in {1:0.0} s ({2:0} fps on average). Frame time: median {3:0.0} ms, 95% {4:0.0}, 99% {5:0.0}, worst {6:0.0}. "
                                             + "{7} hitches over {8:0} ms ({9} over 100 ms). {10} garbage collections.",
                frames, profiledSeconds, frames / Math.Max(1e-3, profiledSeconds), P(0.5f), P(0.95f), P(0.99f), sorted[sorted.Count - 1],
                hitches, HitchMs, bigHitches, gcCollections));
            md.AppendLine();
            md.AppendLine("Our work, milliseconds per second of play (a section inside another counts in both):");
            md.AppendLine();
            var totals = new List<(string, double)>();
            for (int i = 0; i < (int)PerfSection.Count; i++)
            {
                totals.Add((((PerfSection)i).ToString(), sectionTotal[i] / Math.Max(1e-3, profiledSeconds)));
            }

            totals.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            foreach (var (name, perSecond) in totals)
            {
                md.AppendLine(string.Format(inv, "- {0}: {1:0.0}", name, perSecond));
            }

            md.AppendLine();
            if (worst.Count > 0)
            {
                md.AppendLine("The worst frames:");
                md.AppendLine();
                foreach (var (_, t, what) in worst)
                {
                    md.AppendLine(string.Format(inv, "- {0:0.00} s: {1}", t, what));
                }

                md.AppendLine();
            }

            md.AppendLine("Unity's counters recorded: " + (Counters.Length > 0 ? Counters : "none (perf_counters.txt lists what there was)") + ".");
            md.AppendLine();
        }

        public void Dispose()
        {
            Perf.Enabled = false;
            foreach (var (_, recorder, _) in recorders)
            {
                recorder.Dispose();
            }

            recorders.Clear();
            try { table.Dispose(); } catch (Exception) { /* the editor is closing */ }
        }
    }
}
