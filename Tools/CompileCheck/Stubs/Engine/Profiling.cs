using System.Collections.Generic;

namespace Unity.Profiling
{
    public readonly struct ProfilerCategory
    {
        private readonly string name;
        public ProfilerCategory(string name) { this.name = name; }
        public string Name => name ?? "";
        public static ProfilerCategory Internal => new ProfilerCategory("Internal");
        public static ProfilerCategory Memory => new ProfilerCategory("Memory");
        public static ProfilerCategory Physics => new ProfilerCategory("Physics");
        public static ProfilerCategory Render => new ProfilerCategory("Render");
        public static ProfilerCategory Scripts => new ProfilerCategory("Scripts");
        public override string ToString() => Name;
    }

    public struct ProfilerRecorder : System.IDisposable
    {
        public ProfilerRecorder(Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle statHandle, int capacity = 1, ProfilerRecorderOptions options = ProfilerRecorderOptions.Default) { }
        public static ProfilerRecorder StartNew(ProfilerCategory category, string statName, int capacity = 1, ProfilerRecorderOptions options = ProfilerRecorderOptions.Default) => default;
        public bool Valid => false;
        public long LastValue => 0;
        public long CurrentValue => 0;
        public void Start() { }
        public void Dispose() { }
    }

    [System.Flags]
    public enum ProfilerRecorderOptions { None = 0, Default = 24, StartImmediately = 8, KeepAliveDuringDomainReload = 16, CollectOnlyOnCurrentThread = 64, WrapAroundWhenCapacityReached = 128, SumAllSamplesInFrame = 256, GpuRecorder = 512 }
}

namespace Unity.Profiling.LowLevel
{
    public enum ProfilerMarkerDataUnit : byte { Undefined = 0, TimeNanoseconds = 1, Bytes = 2, Count = 3, Percent = 4, FrequencyHz = 5 }
}

namespace Unity.Profiling.LowLevel.Unsafe
{
    using Unity.Profiling.LowLevel;

    public readonly struct ProfilerRecorderHandle
    {
        public bool Valid => false;
        public static void GetAvailable(List<ProfilerRecorderHandle> outRecorderHandleList) => outRecorderHandleList?.Clear();
        public static ProfilerRecorderDescription GetDescription(ProfilerRecorderHandle handle) => default;
    }

    public readonly struct ProfilerRecorderDescription
    {
        public ProfilerCategory Category => default;
        public string Name => "";
        public ProfilerMarkerDataUnit UnitType => ProfilerMarkerDataUnit.Undefined;
    }
}
