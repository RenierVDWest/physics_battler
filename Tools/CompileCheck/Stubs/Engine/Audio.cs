// Stand-ins for Unity's audio types the sound player uses (they play nothing headless).
namespace UnityEngine
{
    public enum AudioDataLoadState { Unloaded, Loading, Loaded, Failed }

    public enum AudioClipLoadType { DecompressOnLoad, CompressedInMemory, Streaming }

    public enum AudioCompressionFormat { PCM, Vorbis, ADPCM, MP3 }

    public enum AudioType { UNKNOWN, ACC, AIFF, IT, MOD, MPEG, OGGVORBIS, S3M, WAV, XM, XMA, VAG, AUDIOQUEUE }

    public class AudioClip : Object
    {
        public float length { get; set; }
        public int samples { get; set; }
        public int channels { get; set; } = 1;
        public int frequency { get; set; } = 44100;
        public AudioDataLoadState loadState { get; set; } = AudioDataLoadState.Unloaded;
        public bool LoadAudioData() => false;
        public bool GetData(float[] data, int offsetSamples) => false;
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public float volume { get; set; } = 1f;
        public float pitch { get; set; } = 1f;
        public float panStereo { get; set; }
        public float time { get; set; }
        public bool loop { get; set; }
        public bool playOnAwake { get; set; } = true;
        public float spatialBlend { get; set; }
        public bool isPlaying { get; private set; }
        public void Play() { }
        public void Stop() { }
        public void PlayOneShot(AudioClip clip, float volumeScale = 1f) { }
    }

    public static class Random
    {
        private static readonly System.Random rng = new System.Random(1);
        public static float value => (float)rng.NextDouble();
        public static float Range(float min, float max) => min + (float)rng.NextDouble() * (max - min);
        public static int Range(int min, int max) => max <= min ? min : rng.Next(min, max);
    }

    public class AudioListener : Behaviour
    {
    }
}
