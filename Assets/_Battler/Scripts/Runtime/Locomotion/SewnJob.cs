#if !HEADLESS_STUB
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The sewn shirt and trousers' picture on Unity's worker threads, compiled by Burst: one garment to a thread, both at
    /// once, finished before the picture is drawn (the plan "Clothes cut and sewn like real clothes", step 3). The same
    /// sums as the headless tests run (<see cref="OutfitSolve.Mesh1"/>). As <see cref="OutfitJob"/>: the settings are
    /// plain numbers and the pointers are taken inside Execute, because Unity will not schedule a job that holds one.
    /// </summary>
    [BurstCompile]
    public unsafe struct SewnJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<short> Grid;
        [ReadOnly] public NativeArray<OutfitSolve.PartField> Fields;
        [ReadOnly] public NativeArray<OutfitSolve.Frame> Frames;
        [ReadOnly] public NativeArray<OutfitSolve.Skin> Skins;
        [ReadOnly] public NativeArray<OutfitSolve.MeshPiece> Meshes;
        [ReadOnly] public NativeArray<int> Threads, Tris, AdjStart, Adj;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> X, Home, Shove;
        [NativeDisableParallelForRestriction] public NativeArray<float> DNear;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Moved;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Inside;
        [NativeDisableParallelForRestriction] public NativeArray<OutfitSolve.Stats> Stats;
        public float Size, PointMargin, MiddleMargin, OtherMargin;
        public int LastWord, Rounds, Measure, KeepMost, RadialOut, Smooth, FoldsStay;

        public void Execute(int garment)
        {
            var d = new OutfitSolve.Data
            {
                Size = Size, PointMargin = PointMargin, MiddleMargin = MiddleMargin, OtherMargin = OtherMargin,
                LastWord = LastWord, Rounds = Rounds, Measure = Measure, KeepMost = KeepMost, RadialOut = RadialOut, Smooth = Smooth, FoldsStay = FoldsStay,
            };
            d.Grid = (short*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Grid);
            d.Fields = (OutfitSolve.PartField*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Fields);
            d.Frames = (OutfitSolve.Frame*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Frames);
            d.Skins = (OutfitSolve.Skin*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Skins);
            d.Meshes = (OutfitSolve.MeshPiece*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Meshes);
            d.Threads = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Threads);
            d.Tris = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Tris);
            d.AdjStart = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(AdjStart);
            d.Adj = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Adj);
            d.X = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(X);
            d.Home = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(Home);
            d.Shove = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(Shove);
            d.DNear = (float*)NativeArrayUnsafeUtility.GetUnsafePtr(DNear);
            d.Moved = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Moved);
            d.Inside = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Inside);
            d.Stats = (OutfitSolve.Stats*)NativeArrayUnsafeUtility.GetUnsafePtr(Stats);
            OutfitSolve.Mesh1(ref d, garment);
        }
    }

    /// <summary>A body's sewn clothes on the worker threads: their arrays kept between pictures, the frames copied in each picture.</summary>
    public sealed class SewnRun
    {
        private NativeArray<short> grid;
        private NativeArray<OutfitSolve.PartField> fields;
        private NativeArray<OutfitSolve.Frame> frames;
        private NativeArray<OutfitSolve.Skin> skins;
        private NativeArray<OutfitSolve.MeshPiece> meshes;
        private NativeArray<int> threads, tris, adjStart, adj;
        private NativeArray<Vector3> x, home, shove;
        private NativeArray<float> dNear;
        private NativeArray<byte> moved, inside;
        private NativeArray<OutfitSolve.Stats> stats;
        private bool made;

        /// <summary>The picture: the frames in, the garments run on the worker threads and waited for, the points and measures out.</summary>
        public void Run(SewnOutfit outfit, bool measure)
        {
            if (!made)
            {
                grid = new NativeArray<short>(outfit.GridArray, Allocator.Persistent);
                fields = new NativeArray<OutfitSolve.PartField>(outfit.PartFieldArray, Allocator.Persistent);
                frames = new NativeArray<OutfitSolve.Frame>(outfit.FrameArray.Length, Allocator.Persistent);
                skins = new NativeArray<OutfitSolve.Skin>(outfit.SkinArray, Allocator.Persistent);
                meshes = new NativeArray<OutfitSolve.MeshPiece>(outfit.MeshArray, Allocator.Persistent);
                threads = new NativeArray<int>(outfit.ThreadArray, Allocator.Persistent);
                tris = new NativeArray<int>(outfit.TriArray, Allocator.Persistent);
                adjStart = new NativeArray<int>(outfit.AdjStartArray, Allocator.Persistent);
                adj = new NativeArray<int>(outfit.AdjArray, Allocator.Persistent);
                int n = outfit.PointArray.Length;
                x = new NativeArray<Vector3>(n, Allocator.Persistent);
                home = new NativeArray<Vector3>(n, Allocator.Persistent);
                shove = new NativeArray<Vector3>(n, Allocator.Persistent);
                dNear = new NativeArray<float>(n, Allocator.Persistent);
                moved = new NativeArray<byte>(n, Allocator.Persistent);
                inside = new NativeArray<byte>(n, Allocator.Persistent);
                stats = new NativeArray<OutfitSolve.Stats>(outfit.MeshArray.Length, Allocator.Persistent);
                made = true;
            }

            frames.CopyFrom(outfit.FrameArray);
            var set = outfit.Settings(measure);
            var job = new SewnJob
            {
                Grid = grid, Fields = fields, Frames = frames, Skins = skins, Meshes = meshes, Threads = threads, Tris = tris, AdjStart = adjStart, Adj = adj,
                X = x, Home = home, Shove = shove, DNear = dNear, Moved = moved, Inside = inside, Stats = stats,
                Size = set.Size, PointMargin = set.PointMargin, MiddleMargin = set.MiddleMargin, OtherMargin = set.OtherMargin,
                LastWord = set.LastWord, Rounds = set.Rounds, Measure = set.Measure, KeepMost = set.KeepMost, RadialOut = set.RadialOut, Smooth = set.Smooth, FoldsStay = set.FoldsStay,
            };
            job.Schedule(meshes.Length, 1).Complete();
            x.CopyTo(outfit.PointArray);
            home.CopyTo(outfit.HomeArray);
            inside.CopyTo(outfit.InsideArray);
            stats.CopyTo(outfit.StatsArray);
        }

        /// <summary>Its memory given back (the clothes are taken off).</summary>
        public void Drop()
        {
            if (!made)
            {
                return;
            }

            grid.Dispose(); fields.Dispose(); frames.Dispose(); skins.Dispose(); meshes.Dispose(); threads.Dispose(); tris.Dispose(); adjStart.Dispose(); adj.Dispose();
            x.Dispose(); home.Dispose(); shove.Dispose(); dNear.Dispose(); moved.Dispose(); inside.Dispose(); stats.Dispose();
            made = false;
        }
    }
}
#endif
