#if !HEADLESS_STUB
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The shirt and trousers' picture on Unity's worker threads, compiled by Burst: one piece (a sleeve, a trouser leg,
    /// the shirt's body...) to a thread, all at once, finished before the picture is drawn (22 November). The same sums
    /// as the headless tests run (<see cref="OutfitSolve"/>).
    /// </summary>
    [BurstCompile]
    public unsafe struct OutfitJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<short> Grid;
        [ReadOnly] public NativeArray<OutfitSolve.PartField> Fields;
        [ReadOnly] public NativeArray<OutfitSolve.Frame> Frames;
        [ReadOnly] public NativeArray<OutfitSolve.Anchor> Anchors;
        [ReadOnly] public NativeArray<OutfitSolve.Piece> Pieces;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> X, Home, Prev;
        [ReadOnly] public NativeArray<float> Sway, RestU, RestV;
        [NativeDisableParallelForRestriction] public NativeArray<float> DNear;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Moved;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Inside;
        [NativeDisableParallelForRestriction] public NativeArray<OutfitSolve.Stats> Stats;
        // (The settings as plain numbers: Unity refuses to schedule a job that holds a pointer, and the solver's Data
        // holds its arrays as pointers. Sent whole, every picture threw before it began and nothing was drawn: his
        // first look, 22 November, 11:31.)
        public float Size, PointMargin, MiddleMargin, OtherMargin;
        public int LastWord, Rounds, Measure, Smooth, WobbleOn, Iters, Reset;
        public float Dt, Wobble, Weight, Hold, Damp;

        public void Execute(int piece)
        {
            var d = new OutfitSolve.Data
            {
                Size = Size, PointMargin = PointMargin, MiddleMargin = MiddleMargin, OtherMargin = OtherMargin,
                LastWord = LastWord, Rounds = Rounds, Measure = Measure, Smooth = Smooth,
                WobbleOn = WobbleOn, Iters = Iters, Reset = Reset, Dt = Dt, Wobble = Wobble, Weight = Weight, Hold = Hold, Damp = Damp,
            };
            d.Grid = (short*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Grid);
            d.Fields = (OutfitSolve.PartField*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Fields);
            d.Frames = (OutfitSolve.Frame*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Frames);
            d.Anchors = (OutfitSolve.Anchor*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Anchors);
            d.Pieces = (OutfitSolve.Piece*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Pieces);
            d.X = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(X);
            d.Home = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(Home);
            d.Prev = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(Prev);
            d.Sway = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Sway);
            d.RestU = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(RestU);
            d.RestV = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(RestV);
            d.DNear = (float*)NativeArrayUnsafeUtility.GetUnsafePtr(DNear);
            d.Moved = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Moved);
            d.Inside = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Inside);
            d.Stats = (OutfitSolve.Stats*)NativeArrayUnsafeUtility.GetUnsafePtr(Stats);
            OutfitSolve.Piece1(ref d, piece);
        }
    }

    /// <summary>A body's outfit on the worker threads: its arrays kept between pictures, the frames copied in each picture.</summary>
    public sealed class OutfitRun
    {
        private NativeArray<short> grid;
        private NativeArray<OutfitSolve.PartField> fields;
        private NativeArray<OutfitSolve.Frame> frames;
        private NativeArray<OutfitSolve.Anchor> anchors;
        private NativeArray<OutfitSolve.Piece> pieces;
        private NativeArray<Vector3> x, home, prev;
        private NativeArray<float> sway, restU, restV;
        private NativeArray<float> dNear;
        private NativeArray<byte> moved, inside;
        private NativeArray<OutfitSolve.Stats> stats;
        private bool made;

        /// <summary>The picture: the frames in, the pieces run on the worker threads and waited for, the points and measures out.</summary>
        public void Run(Outfit outfit, bool measure)
        {
            if (!made)
            {
                grid = new NativeArray<short>(outfit.GridArray, Allocator.Persistent);
                fields = new NativeArray<OutfitSolve.PartField>(outfit.PartFieldArray, Allocator.Persistent);
                frames = new NativeArray<OutfitSolve.Frame>(outfit.FrameArray.Length, Allocator.Persistent);
                anchors = new NativeArray<OutfitSolve.Anchor>(outfit.AnchorArray, Allocator.Persistent);
                pieces = new NativeArray<OutfitSolve.Piece>(outfit.PieceArray, Allocator.Persistent);
                int n = outfit.PointArray.Length;
                x = new NativeArray<Vector3>(n, Allocator.Persistent);
                home = new NativeArray<Vector3>(n, Allocator.Persistent);
                prev = new NativeArray<Vector3>(n, Allocator.Persistent);
                sway = new NativeArray<float>(outfit.SwayArray, Allocator.Persistent);
                restU = new NativeArray<float>(outfit.RestUArray, Allocator.Persistent);
                restV = new NativeArray<float>(outfit.RestVArray, Allocator.Persistent);
                dNear = new NativeArray<float>(n, Allocator.Persistent);
                moved = new NativeArray<byte>(n, Allocator.Persistent);
                inside = new NativeArray<byte>(n, Allocator.Persistent);
                stats = new NativeArray<OutfitSolve.Stats>(outfit.PieceArray.Length, Allocator.Persistent);
                made = true;
            }

            frames.CopyFrom(outfit.FrameArray);
            // (Where the points were last picture, and the picture before: the wobble carries on from them.)
            x.CopyFrom(outfit.PointArray);
            prev.CopyFrom(outfit.PrevArray);
            var set = outfit.Settings(measure);
            var job = new OutfitJob
            {
                Grid = grid, Fields = fields, Frames = frames, Anchors = anchors, Pieces = pieces,
                X = x, Home = home, Prev = prev, Sway = sway, RestU = restU, RestV = restV, DNear = dNear, Moved = moved, Inside = inside, Stats = stats,
                Size = set.Size, PointMargin = set.PointMargin, MiddleMargin = set.MiddleMargin, OtherMargin = set.OtherMargin,
                LastWord = set.LastWord, Rounds = set.Rounds, Measure = set.Measure, Smooth = set.Smooth,
                WobbleOn = set.WobbleOn, Iters = set.Iters, Reset = set.Reset, Dt = set.Dt, Wobble = set.Wobble, Weight = set.Weight, Hold = set.Hold, Damp = set.Damp,
            };
            job.Schedule(pieces.Length, 1).Complete();
            x.CopyTo(outfit.PointArray);
            prev.CopyTo(outfit.PrevArray);
            inside.CopyTo(outfit.InsideArray);
            stats.CopyTo(outfit.StatsArray);
        }

        /// <summary>Its memory given back (the outfit is taken off).</summary>
        public void Drop()
        {
            if (!made)
            {
                return;
            }

            grid.Dispose(); fields.Dispose(); frames.Dispose(); anchors.Dispose(); pieces.Dispose();
            x.Dispose(); home.Dispose(); prev.Dispose(); sway.Dispose(); restU.Dispose(); restV.Dispose(); dNear.Dispose(); moved.Dispose(); inside.Dispose(); stats.Dispose();
            made = false;
        }
    }
}
#endif
