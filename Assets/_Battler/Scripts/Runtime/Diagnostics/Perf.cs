using System.Diagnostics;
using System.Threading;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>The game's own work, timed for the playtest log's performance table (perf.csv.gz).</summary>
    public enum PerfSection
    {
        /// <summary>A unit's whole physics step (everything below that happens in it is inside it too).</summary>
        UnitStep,

        /// <summary>Posing a unit for the step (the animation rig).</summary>
        Compose,

        /// <summary>Posing a unit dry: the planner's tries, the clearance checks, the contact hand.</summary>
        ComposeDry,

        /// <summary>One arm solved on the arm rig (in either compose).</summary>
        ArmSolve,

        /// <summary>The planner choosing a blow (every try of every move and part inside).</summary>
        Plan,

        /// <summary>The planner locking one blow (its path, the contact hand).</summary>
        Prepare,

        /// <summary>A blow tried at its contact (one dry pose).</summary>
        TryOut,

        /// <summary>A blow's clearance of its own body (32 dry poses).</summary>
        Clearance,

        /// <summary>A blow's weapon swept against the bodies.</summary>
        Sweep,

        /// <summary>The physical body driven toward the pose.</summary>
        Authority,

        /// <summary>The sandbox's step: the fight loop or the brawl, choosing targets and blows.</summary>
        Sandbox,

        /// <summary>The recorder sampling every unit (frames and poses).</summary>
        RecorderSample,

        /// <summary>The recorder writing its files out (gzip).</summary>
        RecorderFlush,

        /// <summary>The recorder's picture: the extra camera rendering.</summary>
        CaptureRender,

        /// <summary>The recorder's picture: encoded and saved (JPEG).</summary>
        CaptureSave,

        Count,
    }

    /// <summary>
    /// Performance analytics: how long the game's own work takes, section by section, summed per
    /// rendered frame (main thread; a section inside another counts in both). Off unless the
    /// playtest recorder is recording. The contact modification runs on PhysX's threads: its pairs
    /// and time are counted on their own.
    /// </summary>
    public static class Perf
    {
        public static bool Enabled;

        private static readonly long[] ticks = new long[(int)PerfSection.Count];
        private static readonly int[] calls = new int[(int)PerfSection.Count];
        private static long contactTicks, contactPairs, contactCalls;

        /// <summary>Milliseconds per Stopwatch tick.</summary>
        public static readonly double TickMs = 1000.0 / Stopwatch.Frequency;

        public struct Scope : System.IDisposable
        {
            private readonly int section;
            private readonly long start;

            public Scope(PerfSection section)
            {
                this.section = (int)section;
                start = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (start != 0L)
                {
                    ticks[section] += Stopwatch.GetTimestamp() - start;
                    calls[section]++;
                }
            }
        }

        /// <summary>Times a section until disposed (nothing when off).</summary>
        public static Scope Begin(PerfSection section) => Enabled ? new Scope(section) : default;

        /// <summary>The contact modification callback's work (PhysX's threads).</summary>
        public static void NoteContacts(long elapsedTicks, int pairs)
        {
            if (!Enabled)
            {
                return;
            }

            Interlocked.Add(ref contactTicks, elapsedTicks);
            Interlocked.Add(ref contactPairs, pairs);
            Interlocked.Increment(ref contactCalls);
        }

        /// <summary>The sums since the last take (milliseconds and calls per section), then cleared.</summary>
        public static void Take(double[] ms, int[] count, out double contactMs, out long pairs, out long contactCallbacks)
        {
            for (int i = 0; i < (int)PerfSection.Count; i++)
            {
                ms[i] = ticks[i] * TickMs;
                count[i] = calls[i];
                ticks[i] = 0L;
                calls[i] = 0;
            }

            contactMs = Interlocked.Exchange(ref contactTicks, 0L) * TickMs;
            pairs = Interlocked.Exchange(ref contactPairs, 0L);
            contactCallbacks = Interlocked.Exchange(ref contactCalls, 0L);
        }
    }
}
