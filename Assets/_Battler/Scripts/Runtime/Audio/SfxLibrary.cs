using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhysicsBattler.Audio
{
    /// <summary>
    /// Every sound the game plays: what each one is (the prompt it was generated from, how long, how many
    /// takes), how loud, and its clips. Lives at Audio/Resources/DA_Sfx so the game finds it without wiring.
    /// The generator (Physics Battler > Audio > Sound Effects) makes the clips from the prompts with ElevenLabs and
    /// files them as Audio/Sfx/{id}_{take}.mp3; any clip named that way is picked up, so you can drop in
    /// your own too.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Sound Library", fileName = "DA_Sfx")]
    public sealed class SfxLibrary : ScriptableObject
    {
        public const string ResourcePath = "DA_Sfx";

        [SerializeField, Tooltip("The first chapter's music (the arena), looping under everything: Audio/Music.")]
        private AudioClip _music;

        [SerializeField, Tooltip("The descent's later chapters, one for each of the Depth Look's colour stages after the first (the first, " +
                                 "the arena, is Music above): the second stage plays the first chapter, and so on. A new chapter fades in " +
                                 "over the last as the colours change. Without a Depth Look, each chapter plays from its depth on.")]
        private List<MusicChapter> _chapters = new List<MusicChapter>();

        [SerializeField] private List<SfxEntry> _sounds = new List<SfxEntry>();

        public IReadOnlyList<SfxEntry> Sounds => _sounds;

        public AudioClip Music => _music;

        public IReadOnlyList<MusicChapter> Chapters => _chapters;

        /// <summary>The music for a depth: the deepest chapter that has begun by then, else the first.</summary>
        public AudioClip MusicFor(int depth)
        {
            AudioClip clip = _music;
            int from = 1;
            foreach (var chapter in _chapters)
            {
                if (chapter != null && chapter.Clip != null && chapter.FromDepth <= depth && chapter.FromDepth >= from)
                {
                    clip = chapter.Clip;
                    from = chapter.FromDepth;
                }
            }

            return clip;
        }

        /// <summary>
        /// The music for a colour stage of the descent (0 for the first, the arena): Music, then each chapter in order of
        /// depth. A stage past the last chapter keeps the last; with no chapters set, <see cref="MusicFor"/>.
        /// </summary>
        public AudioClip MusicForStage(int stage, int depth)
        {
            if (stage <= 0)
            {
                return _music != null ? _music : MusicFor(depth);
            }

            var ordered = _chapters.Where(c => c != null && c.Clip != null).OrderBy(c => c.FromDepth).ToList();
            if (ordered.Count == 0)
            {
                return MusicFor(depth);
            }

            return ordered[System.Math.Min(stage, ordered.Count) - 1].Clip;
        }

        /// <summary>The setup's move: a chapter the setup put at one depth goes to another (the colour stages' depths).</summary>
        public bool MoveChapter(AudioClip clip, int fromDepth, int toDepth)
        {
            var chapter = _chapters.FirstOrDefault(c => c != null && c.Clip == clip && c.FromDepth == fromDepth);
            if (clip == null || chapter == null || _chapters.Any(c => c != null && c != chapter && c.FromDepth == toDepth))
            {
                return false;
            }

            chapter.FromDepth = toDepth;
            _chapters.Sort((a, b) => (a?.FromDepth ?? 0).CompareTo(b?.FromDepth ?? 0));
            return true;
        }

        /// <summary>The setup's fill: a chapter from a depth, if there's none from that depth yet.</summary>
        public bool AddChapter(int fromDepth, AudioClip clip)
        {
            if (clip == null || _chapters.Any(c => c != null && (c.FromDepth == fromDepth || c.Clip == clip)))
            {
                return false;
            }

            _chapters.Add(new MusicChapter { FromDepth = fromDepth, Clip = clip });
            _chapters.Sort((a, b) => (a?.FromDepth ?? 0).CompareTo(b?.FromDepth ?? 0));
            return true;
        }

        public void SetMusic(AudioClip music)
        {
            _music = music;
        }

        public SfxEntry Find(string id)
        {
            for (int i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i] != null && _sounds[i].Id == id)
                {
                    return _sounds[i];
                }
            }

            return null;
        }

        /// <summary>Adds the default sounds this library doesn't have yet (by id). True if any were added.</summary>
        public bool AddMissing()
        {
            bool added = false;
            foreach (var entry in SfxDefaults.All())
            {
                var have = Find(entry.Id);
                if (have == null)
                {
                    _sounds.Add(entry);
                    added = true;
                }
                else if (have.Group == SfxGroup.None)
                {
                    // A sound from before groups: it gets its group.
                    have.Group = entry.Group;
                    added = true;
                }
            }

            return added;
        }

        /// <summary>Takes a sound out of the list (one that's been replaced). True if it was there.</summary>
        public bool Remove(string id)
        {
            return _sounds.RemoveAll(s => s != null && s.Id == id) > 0;
        }

        /// <summary>Credits a generation of these sounds would cost (ElevenLabs: 40 a second).</summary>
        public static int CreditsFor(IEnumerable<(SfxEntry Entry, int Takes)> work)
        {
            return work.Sum(w => Mathf.CeilToInt(w.Entry.Duration * 40f) * w.Takes);
        }
    }

    /// <summary>The kinds of sound, each with its own volume and switch on the Sound Settings.</summary>
    public enum SfxGroup
    {
        None,
        Steps,
        Combat,
        Weapons,
        Bodies,
        Ambience,
    }

    /// <summary>One sound: its prompt and takes, and how it plays.</summary>
    [Serializable]
    public sealed class SfxEntry
    {
        [Tooltip("Its name in code and in the file names (Audio/Sfx/{id}_01.mp3...).")]
        public string Id;

        [Tooltip("Its group on the Sound Settings (each group can be turned down or off).")]
        public SfxGroup Group;

        [Tooltip("Never play it (for testing, or a sound you don't want).")]
        public bool Muted;

        [TextArea(2, 4), Tooltip("What to ask ElevenLabs for. Concrete is best: the material, the action, how heavy, how close, no music.")]
        public string Prompt;

        [Range(0.5f, 30f), Tooltip("Seconds long.")]
        public float Duration = 1f;

        [Range(1, 6), Tooltip("How many takes to make; one is picked at random each time, so repeats don't sound the same.")]
        public int Takes = 1;

        [Tooltip("A seamless loop (the ambience).")]
        public bool Loop;

        [Range(0f, 1f), Tooltip("How closely it follows the prompt: higher is more literal, lower more varied.")]
        public float PromptInfluence = 0.45f;

        [Range(0f, 2f), Tooltip("How loud it plays (after evening out the takes).")]
        public float Volume = 1f;

        [Range(0f, 0.3f), Tooltip("Random change of pitch each time it plays, so repeats vary.")]
        public float PitchVariation = 0.06f;

        [Tooltip("The takes. Filled in from Audio/Sfx/{id}_NN files.")]
        public List<AudioClip> Clips = new List<AudioClip>();

        public SfxEntry Copy()
        {
            var copy = (SfxEntry)MemberwiseClone();
            copy.Clips = new List<AudioClip>(Clips);
            return copy;
        }
    }

    /// <summary>One chapter of the descent's music: its track, from a depth on.</summary>
    [Serializable]
    public sealed class MusicChapter
    {
        [Min(1), Tooltip("The depth this chapter's music starts at.")]
        public int FromDepth = 5;

        public AudioClip Clip;
    }
}
