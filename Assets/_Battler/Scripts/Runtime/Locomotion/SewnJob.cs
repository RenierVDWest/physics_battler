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
        [ReadOnly] public NativeArray<int> Threads, Tris, AdjStart, Adj, Bends;
        [ReadOnly] public NativeArray<float> ThreadRest, BendRest, Fence, Leash;
        [ReadOnly] public NativeArray<int> LeashTo;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> X, Home, Shove, R, RPrev, HomeWas, HomeWas2;
        [NativeDisableParallelForRestriction] public NativeArray<float> DNear;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Moved;
        [NativeDisableParallelForRestriction] public NativeArray<byte> Inside;
        [NativeDisableParallelForRestriction] public NativeArray<OutfitSolve.Stats> Stats;
        public float Size, PointMargin, MiddleMargin, OtherMargin;
        public int LastWord, Rounds, Measure, KeepMost, RadialOut, Smooth, FoldsStay, Dynamic, Steps, Iters, Reset;
        public float Dt, Gravity, Keep, Inertia, Squash, BendK, HoldShape;
        public Vector3 Pluck;
        public int Backstop, Tucked, CollideEvery;
        public float Longest;
        public OutfitSolve.SlideSet Slide;
        public OutfitSolve.MoreSet More;

        public void Execute(int garment)
        {
            var d = new OutfitSolve.Data
            {
                Size = Size, PointMargin = PointMargin, MiddleMargin = MiddleMargin, OtherMargin = OtherMargin,
                LastWord = LastWord, Rounds = Rounds, Measure = Measure, KeepMost = KeepMost, RadialOut = RadialOut, Smooth = Smooth, FoldsStay = FoldsStay,
                Dynamic = Dynamic, Steps = Steps, Iters = Iters, Reset = Reset, Dt = Dt, Gravity = Gravity, Keep = Keep, Inertia = Inertia, Squash = Squash, BendK = BendK, HoldShape = HoldShape, Pluck = Pluck, Backstop = Backstop, Tucked = Tucked, CollideEvery = CollideEvery, Longest = Longest, Slide = Slide, More = More,
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
            d.Bends = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Bends);
            d.ThreadRest = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(ThreadRest);
            d.BendRest = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(BendRest);
            d.Fence = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Fence);
            d.LeashTo = (int*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(LeashTo);
            d.Leash = (float*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(Leash);
            d.R = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(R);
            d.RPrev = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(RPrev);
            d.HomeWas = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(HomeWas);
            d.HomeWas2 = (Vector3*)NativeArrayUnsafeUtility.GetUnsafePtr(HomeWas2);
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
        private NativeArray<int> threads, tris, adjStart, adj, bends;
        private NativeArray<float> threadRest, bendRest, fence, leash;
        private NativeArray<int> leashTo;
        private NativeArray<Vector3> r, rPrev, homeWas, homeWas2;
        private NativeArray<Vector3> x, home, shove;
        private NativeArray<float> dNear;
        private NativeArray<byte> moved, inside;
        private NativeArray<OutfitSolve.Stats> stats;
        private bool made;
        private int skinsVersion;

        private JobHandle handle;
        private bool running;

        /// <summary>The picture: the frames in, the garments run on the worker threads and waited for, the points and measures out.</summary>
        public void Run(SewnOutfit outfit, bool measure)
        {
            Start(outfit, measure);
            Finish(outfit);
        }

        /// <summary>The picture begun: the frames in and the garments given to the worker threads, not waited for (see <see cref="Finish"/>).</summary>
        public void Start(SewnOutfit outfit, bool measure)
        {
            if (running)
            {
                handle.Complete();
                running = false;
            }

            if (!made)
            {
                grid = new NativeArray<short>(outfit.GridArray, Allocator.Persistent);
                fields = new NativeArray<OutfitSolve.PartField>(outfit.PartFieldArray, Allocator.Persistent);
                frames = new NativeArray<OutfitSolve.Frame>(outfit.FrameArray.Length, Allocator.Persistent);
                skins = new NativeArray<OutfitSolve.Skin>(outfit.SkinArray, Allocator.Persistent);
                skinsVersion = outfit.SkinsVersion;
                meshes = new NativeArray<OutfitSolve.MeshPiece>(outfit.MeshArray, Allocator.Persistent);
                threads = new NativeArray<int>(outfit.ThreadArray, Allocator.Persistent);
                tris = new NativeArray<int>(outfit.TriArray, Allocator.Persistent);
                adjStart = new NativeArray<int>(outfit.AdjStartArray, Allocator.Persistent);
                adj = new NativeArray<int>(outfit.AdjArray, Allocator.Persistent);
                bends = new NativeArray<int>(outfit.BendArray, Allocator.Persistent);
                threadRest = new NativeArray<float>(outfit.ThreadRestArray, Allocator.Persistent);
                bendRest = new NativeArray<float>(outfit.BendRestArray, Allocator.Persistent);
                fence = new NativeArray<float>(outfit.FenceArray.Length, Allocator.Persistent);
                leash = new NativeArray<float>(outfit.LeashArray, Allocator.Persistent);
                leashTo = new NativeArray<int>(outfit.LeashToArray, Allocator.Persistent);
                int n = outfit.PointArray.Length;
                x = new NativeArray<Vector3>(n, Allocator.Persistent);
                home = new NativeArray<Vector3>(n, Allocator.Persistent);
                shove = new NativeArray<Vector3>(n, Allocator.Persistent);
                r = new NativeArray<Vector3>(n, Allocator.Persistent);
                rPrev = new NativeArray<Vector3>(n, Allocator.Persistent);
                homeWas = new NativeArray<Vector3>(n, Allocator.Persistent);
                homeWas2 = new NativeArray<Vector3>(n, Allocator.Persistent);
                dNear = new NativeArray<float>(n, Allocator.Persistent);
                moved = new NativeArray<byte>(n, Allocator.Persistent);
                inside = new NativeArray<byte>(n, Allocator.Persistent);
                stats = new NativeArray<OutfitSolve.Stats>(outfit.MeshArray.Length, Allocator.Persistent);
                made = true;
            }

            frames.CopyFrom(outfit.FrameArray);
            if (skinsVersion != outfit.SkinsVersion)
            {
                // (The seams carried smoothly or by side: changed in the lab.)
                skins.CopyFrom(outfit.SkinArray);
                skinsVersion = outfit.SkinsVersion;
            }

            fence.CopyFrom(outfit.FenceArray);
            var set = outfit.Settings(measure);
            var job = new SewnJob
            {
                Grid = grid, Fields = fields, Frames = frames, Skins = skins, Meshes = meshes, Threads = threads, Tris = tris, AdjStart = adjStart, Adj = adj, Bends = bends, ThreadRest = threadRest, BendRest = bendRest, Fence = fence, Leash = leash, LeashTo = leashTo,
                R = r, RPrev = rPrev, HomeWas = homeWas, HomeWas2 = homeWas2,
                X = x, Home = home, Shove = shove, DNear = dNear, Moved = moved, Inside = inside, Stats = stats,
                Size = set.Size, PointMargin = set.PointMargin, MiddleMargin = set.MiddleMargin, OtherMargin = set.OtherMargin,
                LastWord = set.LastWord, Rounds = set.Rounds, Measure = set.Measure, KeepMost = set.KeepMost, RadialOut = set.RadialOut, Smooth = set.Smooth, FoldsStay = set.FoldsStay,
                Dynamic = set.Dynamic, Steps = set.Steps, Iters = set.Iters, Reset = set.Reset, Dt = set.Dt, Gravity = set.Gravity, Keep = set.Keep, Inertia = set.Inertia, Squash = set.Squash, BendK = set.BendK, HoldShape = set.HoldShape, Pluck = set.Pluck, Backstop = set.Backstop, Tucked = set.Tucked, CollideEvery = set.CollideEvery, Longest = set.Longest, Slide = set.Slide, More = set.More,
            };
            handle = job.Schedule(meshes.Length, 1);
            running = true;
            JobHandle.ScheduleBatchedJobs();
        }

        /// <summary>Waits for the picture begun by <see cref="Start"/> and copies the points and measures out.</summary>
        public void Finish(SewnOutfit outfit)
        {
            if (!running)
            {
                return;
            }

            handle.Complete();
            running = false;
            x.CopyTo(outfit.PointArray);
            home.CopyTo(outfit.HomeArray);
            inside.CopyTo(outfit.InsideArray);
            stats.CopyTo(outfit.StatsArray);
        }

        /// <summary>Its memory given back (the clothes are taken off).</summary>
        public void Drop()
        {
            if (running)
            {
                handle.Complete();
                running = false;
            }

            if (!made)
            {
                return;
            }

            grid.Dispose(); fields.Dispose(); frames.Dispose(); skins.Dispose(); meshes.Dispose(); threads.Dispose(); tris.Dispose(); adjStart.Dispose(); adj.Dispose(); bends.Dispose(); threadRest.Dispose(); bendRest.Dispose(); fence.Dispose(); leash.Dispose(); leashTo.Dispose();
            r.Dispose(); rPrev.Dispose(); homeWas.Dispose(); homeWas2.Dispose();
            x.Dispose(); home.Dispose(); shove.Dispose(); dNear.Dispose(); moved.Dispose(); inside.Dispose(); stats.Dispose();
            made = false;
        }
    }
}
#endif
