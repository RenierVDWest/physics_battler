using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhysicsBattler.Audio
{
    /// <summary>
    /// Plays the game's sounds: <c>Sfx.Play(SfxIds.HitFlesh, where)</c>. A random take each time (not the same
    /// one twice running) at a slightly different pitch, panned a little toward where it happens on screen.
    /// Each take's silence at the start is skipped so a hit lands on the frame it's played, and a sound's takes
    /// are evened out in loudness (worked out once, when the first sound plays). Two loops play the ambience: the arena crowd, fading into the wind of the deep
    /// as you descend. Before any sounds are made (or with the Sound option off) it simply stays quiet.
    /// </summary>
    public sealed class Sfx : MonoBehaviour
    {
        private const int Voices = 14;
        private const int MostOfOneSound = 3;
        private const float SameSoundGap = 0.035f;

        private static Sfx _instance;
        private static bool _missing;

        private SfxLibrary _library;
        private readonly List<AudioSource> _voices = new List<AudioSource>();
        private readonly Dictionary<string, float> _lastPlayed = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _lastTake = new Dictionary<string, int>();
        private readonly Dictionary<AudioClip, (float Start, float Gain)> _shaped = new Dictionary<AudioClip, (float, float)>();
        private AudioSource _arena;
        private AudioSource _deep;
        private AudioSource _music;
        private AudioSource _fadingMusic;
        private int _musicDepth = 1;
        private float _deepness;
        private float _ambienceLevel = 1f;
        private int _nextVoice;

        /// <summary>Plays a sound. <paramref name="at"/> pans it toward where it happens (null: centred).</summary>
        /// <summary>Every sound asked for, whether or not it can play (the headless runners count them).</summary>
        public static event System.Action<string, float> Requested;

        public static void Play(string id, Vector3? at = null, float volume = 1f)
        {
            Requested?.Invoke(id, volume);
            var player = Instance;
            if (player != null)
            {
                player.PlayNow(id, at, volume);
            }
        }

        /// <summary>Plays a sound <paramref name="delay"/> seconds from now (a swing's whoosh, timed to its blow).</summary>
        public static void PlayLater(string id, float delay, Vector3? at = null, float volume = 1f)
        {
            Requested?.Invoke(id, volume);

            var player = Instance;
            if (player == null)
            {
                return;
            }

            if (delay <= 0f)
            {
                player.PlayNow(id, at, volume);
                return;
            }

            player._later.Add((Time.unscaledTime + delay, id, at, volume));
        }

        private readonly List<(float When, string Id, Vector3? At, float Volume)> _later = new List<(float, string, Vector3?, float)>();

        /// <summary>Whether a sound has takes to play (made, and not muted).</summary>
        public static bool Has(string id)
        {
            var player = Instance;
            var entry = player != null ? player._library.Find(id) : null;
            return entry != null && !entry.Muted && entry.Clips != null && entry.Clips.Exists(c => c != null);
        }

        /// <summary>
        /// Starts a looping sound (a burning firepot) that follows <paramref name="follow"/> round the screen, fading in. It
        /// plays until <see cref="StopLoop"/>, or fades out by itself once what it follows is destroyed. Returns its handle (0
        /// if it can't play: no clip, muted, or too many loops already).
        /// </summary>
        public static int StartLoop(string id, Transform follow = null, float volume = 1f)
        {
            var player = Instance;
            return player != null ? player.StartLoopNow(id, follow, volume) : 0;
        }

        /// <summary>Fades a loop out and stops it (a handle of 0, or one already stopped, does nothing).</summary>
        public static void StopLoop(int handle)
        {
            if (_instance == null || handle == 0)
            {
                return;
            }

            foreach (var loop in _instance._loops)
            {
                if (loop.Handle == handle)
                {
                    loop.Stopping = true;
                }
            }
        }

        /// <summary>
        /// The ambience for how deep the floor is: 0 the living arena's crowd, 1 the wind of the deep (in between,
        /// both). <paramref name="level"/> scales it (0 silences it).
        /// </summary>
        public static void SetAmbience(float deepness, float level = 1f)
        {
            var player = Instance;
            if (player != null)
            {
                player._deepness = Mathf.Clamp01(deepness);
                player._ambienceLevel = Mathf.Clamp01(level);
                player.StartAmbience();
            }
        }

        /// <summary>
        /// The music for the floor's depth: each chapter of the descent has its own track, and a new one fades in
        /// over the last (the same chapter keeps playing on).
        /// </summary>
        public static void SetMusicDepth(int depth)
        {
            var player = Instance;
            if (player != null)
            {
                player._musicDepth = Mathf.Max(1, depth);
                player.ChangeMusic(player._library.MusicFor(player._musicDepth));
            }
        }

        /// <summary>
        /// The music for one of the descent's colour stages (0 is the arena's): each stage has its own track, faded in over
        /// the last as the colours change.
        /// </summary>
        public static void SetMusicStage(int stage, int depth)
        {
            var player = Instance;
            if (player != null)
            {
                player._musicDepth = Mathf.Max(1, depth);
                player.ChangeMusic(player._library.MusicForStage(stage, player._musicDepth));
            }
        }

        private void ChangeMusic(AudioClip clip)
        {
            if (clip == null || _music == null || _music.clip == clip)
            {
                return;
            }

            // The old track fades out on its own source while the new one rises from silence (see Update).
            if (_fadingMusic != null && _fadingMusic != _music)
            {
                _fadingMusic.Stop();
            }

            var old = _music;
            _music = old == _fadingMusic || _fadingMusic == null ? NewSource(true) : _fadingMusic;
            _fadingMusic = old;
            _music.clip = clip;
            _music.volume = 0f;
            _music.Play();
        }

        /// <summary>Turns the ambience up or down (0 silent, 1 full) without changing which ambience it is.</summary>
        public static void SetAmbienceLevel(float level)
        {
            var player = Instance;
            if (player != null)
            {
                player._ambienceLevel = Mathf.Clamp01(level);
            }
        }

        private static Sfx Instance
        {
            get
            {
                if (_instance != null || _missing || !Application.isPlaying)
                {
                    return _instance;
                }

                var settings = SoundSettings.Active;
                var library = settings != null && settings.Library != null ? settings.Library : Resources.Load<SfxLibrary>(SfxLibrary.ResourcePath);
                if (library == null)
                {
                    _missing = true;
                    return null;
                }

                var go = new GameObject("[Sfx]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<Sfx>();
                _instance.Init(library);
                return _instance;
            }
        }

        private void Init(SfxLibrary library)
        {
            _library = library;

            // Nothing is heard without a listener, and the scene has none of its own: this is it (the sounds are 2D).
            if (FindObjectsByType<AudioListener>().Length == 0)
            {
                gameObject.AddComponent<AudioListener>();
            }

            for (int i = 0; i < Voices; i++)
            {
                _voices.Add(NewSource(false));
            }

            _arena = NewSource(true);
            _deep = NewSource(true);
            _music = NewSource(true);
            var firstTrack = _library.MusicFor(_musicDepth);
            if (firstTrack != null)
            {
                // The music starts quiet and rises (see Update).
                _music.clip = firstTrack;
                _music.volume = 0f;
                _music.Play();
            }

            ShapeAll();
        }

        /// <summary>A looping sound on its own source: whose it is, what it follows, how loud it was asked to be.</summary>
        private sealed class LoopVoice
        {
            public int Handle;
            public AudioSource Source;
            public SfxEntry Entry;
            public Transform Follow;
            public bool Following;
            public float Volume;
            public bool Stopping;
        }

        private const int MostLoops = 6;
        private readonly List<LoopVoice> _loops = new List<LoopVoice>();
        private int _nextLoop = 1;

        private int StartLoopNow(string id, Transform follow, float volume)
        {
            var entry = _library.Find(id);
            if (entry == null || entry.Muted || entry.Clips == null || entry.Clips.Count == 0)
            {
                return 0;
            }

            var clip = entry.Clips[Random.Range(0, entry.Clips.Count)];
            if (clip == null)
            {
                return 0;
            }

            var loop = _loops.FirstOrDefault(l => !l.Source.isPlaying);
            if (loop == null)
            {
                if (_loops.Count >= MostLoops)
                {
                    return 0;
                }

                loop = new LoopVoice { Source = NewSource(true) };
                _loops.Add(loop);
            }

            loop.Handle = _nextLoop++;
            loop.Entry = entry;
            loop.Follow = follow;
            loop.Following = follow != null;
            loop.Volume = volume;
            loop.Stopping = false;
            var source = loop.Source;
            source.clip = clip;
            source.volume = 0f;
            source.pitch = 1f + Random.Range(-entry.PitchVariation, entry.PitchVariation);
            source.time = Random.Range(0f, Mathf.Max(0f, clip.length - 0.05f));
            source.panStereo = follow != null ? Pan(follow.position) : 0f;
            source.Play();
            return loop.Handle;
        }

        /// <summary>Each loop eases to its volume (or out, once stopped or what it followed is gone) and pans after what it follows.</summary>
        private void UpdateLoops()
        {
            foreach (var loop in _loops)
            {
                var source = loop.Source;
                if (!source.isPlaying)
                {
                    continue;
                }

                if (loop.Following && loop.Follow == null)
                {
                    loop.Stopping = true;
                }

                float target = loop.Stopping || loop.Entry.Muted ? 0f : Mathf.Clamp01(loop.Entry.Volume * loop.Volume * EffectsMix(loop.Entry));
                source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * (loop.Stopping ? 2.5f : 2f));
                if (loop.Follow != null)
                {
                    source.panStereo = Pan(loop.Follow.position);
                }

                if (loop.Stopping && source.volume <= 0f)
                {
                    source.Stop();
                    loop.Follow = null;
                    loop.Handle = 0;
                }
            }
        }

        private AudioSource NewSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private void PlayNow(string id, Vector3? at, float volume)
        {
            var entry = _library.Find(id);
            if (entry == null || entry.Muted || entry.Clips == null || entry.Clips.Count == 0)
            {
                return;
            }

            float mix = EffectsMix(entry);
            if (mix <= 0f)
            {
                return;
            }

            // The same sound many times in one moment (a spin through three enemies) plays once or twice, not as a wall.
            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(id, out float last) && now - last < SameSoundGap)
            {
                return;
            }

            if (Playing(entry) >= MostOfOneSound)
            {
                return;
            }

            _lastPlayed[id] = now;
            int take = PickTake(id, entry.Clips.Count);
            var clip = entry.Clips[take];
            if (clip == null)
            {
                return;
            }

            var (start, gain) = Shape(clip);
            var source = FreeVoice();
            source.clip = clip;
            source.volume = Mathf.Clamp01(entry.Volume * gain * volume * mix);
            source.pitch = 1f + Random.Range(-entry.PitchVariation, entry.PitchVariation);
            source.panStereo = at.HasValue ? Pan(at.Value) : 0f;
            source.time = Mathf.Min(start, Mathf.Max(0f, clip.length - 0.05f));
            source.Play();
        }

        /// <summary>The Sound Settings' share for a sound: the effects volume times its group's (0 if either is off).</summary>
        private static float EffectsMix(SfxEntry entry)
        {
            var settings = SoundSettings.Active;
            if (settings == null)
            {
                return DefaultEffectsVolume * PlayerVolume;
            }

            if (!settings.SoundOn)
            {
                return 0f;
            }

            var group = entry.Group != SfxGroup.None ? entry.Group : SfxDefaults.GroupFor(entry.Id);
            return settings.EffectsVolume * settings.GroupVolume(group) * PlayerVolume;
        }

        /// <summary>The player's own volume (Hexamus's options menu has it; here there is none yet).</summary>
        private const float PlayerVolume = 1f;

        private const float DefaultEffectsVolume = 0.85f;
        private const float DefaultAmbienceVolume = 0.45f;
        private const float DefaultMusicVolume = 0.5f;

        private int Playing(SfxEntry entry)
        {
            int count = 0;
            foreach (var voice in _voices)
            {
                if (voice.isPlaying && voice.clip != null && entry.Clips.Contains(voice.clip))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>A random take, never the one played last time (when there's a choice).</summary>
        private int PickTake(string id, int count)
        {
            if (count <= 1)
            {
                return 0;
            }

            int take = Random.Range(0, count - 1);
            if (_lastTake.TryGetValue(id, out int last) && take >= last)
            {
                take++;
            }

            _lastTake[id] = take;
            return take;
        }

        /// <summary>A voice not playing, or else the one that started longest ago.</summary>
        private AudioSource FreeVoice()
        {
            for (int i = 0; i < _voices.Count; i++)
            {
                var voice = _voices[(_nextVoice + i) % _voices.Count];
                if (!voice.isPlaying)
                {
                    _nextVoice = (_nextVoice + i + 1) % _voices.Count;
                    return voice;
                }
            }

            var oldest = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Count;
            oldest.Stop();
            return oldest;
        }

        private static float Pan(Vector3 at)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return 0f;
            }

            float x = camera.WorldToViewportPoint(at).x;
            return Mathf.Clamp((x - 0.5f) * 0.7f, -0.45f, 0.45f);
        }

        /// <summary>Where a take really starts (past its silence) and how much to turn it down; worked out in <see cref="ShapeAll"/>.</summary>
        private (float Start, float Gain) Shape(AudioClip clip)
        {
            return _shaped.TryGetValue(clip, out var shape) ? shape : (0f, 1f);
        }

        /// <summary>
        /// Once, at the start: for every take of every effect, where its sound really begins (past the silence)
        /// and its peak; then each sound's louder takes are turned down to its quietest, so the takes match and the
        /// sound's Volume is its loudness. A take whose samples can't be read plays as it is.
        /// </summary>
        private void ShapeAll()
        {
            float[] buffer = null;
            foreach (var entry in _library.Sounds)
            {
                if (entry == null || entry.Loop || entry.Clips == null)
                {
                    continue;
                }

                var peaks = new List<(AudioClip Clip, float Start, float Peak)>();
                foreach (var clip in entry.Clips)
                {
                    if (clip == null || _shaped.ContainsKey(clip))
                    {
                        continue;
                    }

                    if (clip.loadState != AudioDataLoadState.Loaded)
                    {
                        clip.LoadAudioData();
                    }

                    int length = clip.samples * Mathf.Max(1, clip.channels);
                    if (clip.loadState != AudioDataLoadState.Loaded || length <= 0)
                    {
                        _shaped[clip] = (0f, 1f);
                        continue;
                    }

                    if (buffer == null || buffer.Length < length)
                    {
                        buffer = new float[length];
                    }

                    var samples = buffer.Length == length ? buffer : new float[length];
                    if (!clip.GetData(samples, 0))
                    {
                        _shaped[clip] = (0f, 1f);
                        continue;
                    }

                    float peak = 0f;
                    for (int i = 0; i < length; i++)
                    {
                        peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
                    }

                    // The sound starts where it first rises above a small share of its own peak (quiet sounds too).
                    float threshold = Mathf.Max(0.003f, peak * 0.06f);
                    int first = 0;
                    while (first < length && Mathf.Abs(samples[first]) < threshold)
                    {
                        first++;
                    }

                    float start = first >= length ? 0f : Mathf.Max(0f, first / (float)Mathf.Max(1, clip.channels) / clip.frequency - 0.004f);
                    peaks.Add((clip, start, peak));
                }

                float quietest = peaks.Where(p => p.Peak > 0.001f).Select(p => p.Peak).DefaultIfEmpty(1f).Min();
                foreach (var (clip, start, peak) in peaks)
                {
                    _shaped[clip] = (start, peak > 0.001f ? Mathf.Clamp01(quietest / peak) : 1f);
                }
            }
        }

        // ------------------------------------------------------------------ ambience

        private void StartAmbience()
        {
            Begin(_arena, SfxIds.AmbienceArena);
            Begin(_deep, SfxIds.AmbienceDeep);
        }

        private void Begin(AudioSource source, string id)
        {
            var entry = _library.Find(id);
            var clip = entry != null && entry.Clips.Count > 0 ? entry.Clips[0] : null;
            if (clip == null || source.clip == clip && source.isPlaying)
            {
                return;
            }

            source.clip = clip;
            source.volume = 0f;
            source.time = Random.Range(0f, Mathf.Max(0f, clip.length - 0.1f));
            source.Play();
        }

        /// <summary>Eases the two loops toward the mix for the floor's depth, so a new floor doesn't jump.</summary>
        private void Update()
        {
            for (int i = _later.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime >= _later[i].When)
                {
                    var (_, id, at, volume) = _later[i];
                    _later.RemoveAt(i);
                    PlayNow(id, at, volume);
                }
            }

            var settings = SoundSettings.Active;
            float level = _ambienceLevel * PlayerVolume * (settings == null ? DefaultAmbienceVolume
                : settings.SoundOn ? settings.AmbienceVolume * settings.GroupVolume(SfxGroup.Ambience) : 0f);
            Ease(_arena, SfxIds.AmbienceArena, (1f - _deepness) * level);
            Ease(_deep, SfxIds.AmbienceDeep, _deepness * level);
            UpdateLoops();

            if (_music != null && _music.clip != null)
            {
                // The player's Music volume (the menu) times the mix's (the Sound object); quick to follow the menu.
                float music = PlayerVolume * (settings == null ? DefaultMusicVolume : settings.MusicVolume);
                _music.volume = Mathf.MoveTowards(_music.volume, music, Time.unscaledDeltaTime * 0.25f);
            }

            if (_fadingMusic != null && _fadingMusic != _music)
            {
                _fadingMusic.volume = Mathf.MoveTowards(_fadingMusic.volume, 0f, Time.unscaledDeltaTime * 0.25f);
                if (_fadingMusic.volume <= 0f && _fadingMusic.isPlaying)
                {
                    _fadingMusic.Stop();
                }
            }
        }

        private void Ease(AudioSource source, string id, float target)
        {
            if (source == null || source.clip == null)
            {
                return;
            }

            var entry = _library.Find(id);
            target *= entry != null && !entry.Muted ? entry.Volume : 0f;
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * 0.35f);
        }
    }
}
