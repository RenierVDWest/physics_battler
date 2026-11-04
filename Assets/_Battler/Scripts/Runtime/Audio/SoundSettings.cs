using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhysicsBattler.Audio
{
    /// <summary>
    /// The mix, on the Sound object in L_Game: overall effects and ambience volume, a volume and an on/off
    /// switch for each group of sounds (steps, combat, voices...), and, through its inspector, each sound's own
    /// volume and mute (kept in the sound list, DA_Sfx). Changes are heard straight away in play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoundSettings : MonoBehaviour
    {
        [Serializable]
        public sealed class GroupMix
        {
            public SfxGroup Group;

            [Tooltip("Off: none of this group's sounds play.")]
            public bool On = true;

            [Range(0f, 2f)]
            public float Volume = 1f;
        }

        [SerializeField, Tooltip("The sound list (DA_Sfx): each sound's own volume and mute live there.")]
        private SfxLibrary _library;

        [SerializeField, Tooltip("Off: no sound at all (for testing; the player's Sound option is separate).")]
        private bool _soundOn = true;

        [SerializeField, Range(0f, 1f), Tooltip("Every sound effect's volume.")]
        private float _effectsVolume = 0.85f;

        [SerializeField, Range(0f, 1f), Tooltip("The ambience's volume (the crowd, the wind of the deep).")]
        private float _ambienceVolume = 0.45f;

        [SerializeField, Tooltip("Off: no music (for testing; the player's Music volume is separate).")]
        private bool _musicOn = true;

        [SerializeField, Range(0f, 1f), Tooltip("The music's volume, under the player's own Music volume in the menu.")]
        private float _musicVolume = 0.5f;

        [SerializeField] private List<GroupMix> _groups = new List<GroupMix>();

        /// <summary>The Sound Settings in the scene (null if there are none: then the defaults are used).</summary>
        public static SoundSettings Active { get; private set; }

        public SfxLibrary Library => _library;

        public bool SoundOn => _soundOn;

        public float EffectsVolume => _effectsVolume;

        public float AmbienceVolume => _ambienceVolume;

        /// <summary>The music's volume here (0 when switched off).</summary>
        public float MusicVolume => _musicOn ? _musicVolume : 0f;

        public IReadOnlyList<GroupMix> Groups => _groups;

        /// <summary>How loud a group plays: 0 when it's switched off.</summary>
        public float GroupVolume(SfxGroup group)
        {
            var mix = _groups.FirstOrDefault(g => g.Group == group);
            return mix == null ? 1f : mix.On ? mix.Volume : 0f;
        }

        public void SetLibrary(SfxLibrary library)
        {
            _library = library;
        }

        private void OnEnable()
        {
            Active = this;
            FillGroups();
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        private void OnValidate()
        {
            FillGroups();
        }

        private void Reset()
        {
            FillGroups();
        }

        /// <summary>One row per group, in order.</summary>
        private void FillGroups()
        {
            foreach (SfxGroup group in Enum.GetValues(typeof(SfxGroup)))
            {
                if (group != SfxGroup.None && _groups.All(g => g.Group != group))
                {
                    _groups.Add(new GroupMix { Group = group });
                }
            }

            _groups.RemoveAll(g => g == null || g.Group == SfxGroup.None);
            _groups.Sort((a, b) => a.Group.CompareTo(b.Group));
        }
    }
}
