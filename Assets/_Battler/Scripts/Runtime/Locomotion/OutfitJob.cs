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
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> X;
        [NativeDisableParallelForRestriction] public NativeArray<float> DNear;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Moved;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Inside;
        [NativeDisableParallelForRestriction] public NativeArray<OutfitSolve.Stats> Stats;
        public OutfitSolve.Data Settings;

        public void Execute(int piece)
        {
            var d = Settings;
            d.Grid = (short*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Grid);
            d.Fields = (OutfitSolve.PartField*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Fields);
            d.Frames = (OutfitSolve.Frame*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Frames);
            d.Anchors = (OutfitSolve.Anchor*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Anchors);
            d.Pieces = (OutfitSolve.Piece*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Pieces);
            d.X = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(X);
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
        private NativeArray<Vector3> x;
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
                dNear = new NativeArray<float>(n, Allocator.Persistent);
                moved = new NativeArray<byte>(n, Allocator.Persistent);
                inside = new NativeArray<byte>(n, Allocator.Persistent);
                stats = new NativeArray<OutfitSolve.Stats>(outfit.PieceArray.Length, Allocator.Persistent);
                made = true;
            }

            frames.CopyFrom(outfit.FrameArray);
            var job = new OutfitJob
            {
                Grid = grid, Fields = fields, Frames = frames, Anchors = anchors, Pieces = pieces,
                X = x, DNear = dNear, Moved = moved, Inside = inside, Stats = stats, Settings = outfit.Settings(measure),
            };
            job.Schedule(pieces.Length, 1).Complete();
            x.CopyTo(outfit.PointArray);
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
            x.Dispose(); dNear.Dispose(); moved.Dispose(); inside.Dispose(); stats.Dispose();
            made = false;
        }
    }
}
#endif
