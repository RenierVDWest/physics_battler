using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhysicsBattler.Editor.Audio
{
    /// <summary>
    /// Hears a clip in the editor. In play, through a little source of its own at the given volume; out of play,
    /// through Unity's own clip preview (as in the Inspector, at full volume), reached by reflection.
    /// </summary>
    internal static class SfxPreview
    {
        private static AudioSource _source;

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            Stop();
            if (Application.isPlaying)
            {
                if (_source == null)
                {
                    var go = new GameObject("[Sound preview]") { hideFlags = HideFlags.HideAndDontSave };
                    _source = go.AddComponent<AudioSource>();
                    _source.playOnAwake = false;
                    _source.spatialBlend = 0f;
                }

                _source.clip = clip;
                _source.volume = Mathf.Clamp01(volume);
                _source.Play();
                return;
            }

            try
            {
                var play = Util()?.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
                play?.Invoke(null, new object[] { clip, 0, false });
            }
            catch (Exception)
            {
                // This Unity has moved its preview: select the clip and use the Inspector's play button instead.
                Selection.activeObject = clip;
            }
        }

        public static void Stop()
        {
            if (_source != null)
            {
                _source.Stop();
            }

            try
            {
                Util()?.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
            }
            catch (Exception)
            {
                // Nothing to stop.
            }
        }

        private static Type Util() => typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
    }
}
