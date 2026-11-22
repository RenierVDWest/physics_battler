#if !HEADLESS_STUB
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// One picture of one garment, on a worker thread, compiled by Burst (22 November, his "Is it possible to offload
    /// this to more cores?"). Its numbers are copies of the garment's packed array; they are copied back when the
    /// picture is taken up, a picture later (<see cref="ClothRun"/>). The same sums as the headless tests run
    /// (<see cref="ClothSolve"/>).
    /// </summary>
    [BurstCompile]
    public unsafe struct ClothJob : IJob
    {
        public NativeArray<float> F;
        public NativeArray<int> Ends;
        public NativeArray<byte> Kinds, Hidden;
        public NativeArray<ClothSolve.Shape> Shapes;
        public NativeArray<ClothSolve.Now> Work;
        public int Count, Threads, ShapeCount, Steps;
        public float Dt, Gap, Thickness, Air, Floor, Stretch, Bending, GX, GY, GZ;

        public void Execute()
        {
            var d = new ClothSolve.Data
            {
                Count = Count, Threads = Threads, Shapes = ShapeCount, Steps = Steps,
                Dt = Dt, Gap = Gap, Thickness = Thickness, Air = Air, Floor = Floor, Stretch = Stretch, Bending = Bending,
                GX = GX, GY = GY, GZ = GZ,
            };
            ClothNet.Point(ref d, (float*)NativeArrayUnsafeUtility.GetUnsafePtr(F), (int*)NativeArrayUnsafeUtility.GetUnsafePtr(Ends),
                (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Kinds), (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(Hidden),
                (ClothSolve.Shape*)NativeArrayUnsafeUtility.GetUnsafePtr(Shapes), (ClothSolve.Now*)NativeArrayUnsafeUtility.GetUnsafePtr(Work));
            ClothSolve.Step(ref d);
        }
    }

    /// <summary>A garment's picture in flight on another core: begun after the body has moved, taken up at the next picture.</summary>
    public sealed class ClothRun
    {
        private ClothJob job;
        private JobHandle handle;

        public bool Going { get; private set; }

        public void Begin(ClothNet net, float dt)
        {
            var s = net.Settings(dt);
            int shapes = net.Shapes.Count > 0 ? net.Shapes.Count : 1;
            job = new ClothJob
            {
                F = new NativeArray<float>(net.Packed, Allocator.Persistent),
                Ends = new NativeArray<int>(net.ThreadEnds.Length > 0 ? net.ThreadEnds : new int[2], Allocator.Persistent),
                Kinds = new NativeArray<byte>(net.ThreadKinds.Length > 0 ? net.ThreadKinds : new byte[1], Allocator.Persistent),
                Hidden = new NativeArray<byte>(net.Hidden, Allocator.Persistent),
                Shapes = new NativeArray<ClothSolve.Shape>(shapes, Allocator.Persistent),
                Work = new NativeArray<ClothSolve.Now>(shapes, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                Count = s.Count, Threads = s.Threads, ShapeCount = s.Shapes, Steps = s.Steps,
                Dt = s.Dt, Gap = s.Gap, Thickness = s.Thickness, Air = s.Air, Floor = s.Floor, Stretch = s.Stretch, Bending = s.Bending,
                GX = s.GX, GY = s.GY, GZ = s.GZ,
            };
            for (int k = 0; k < net.Shapes.Count; k++)
            {
                var shapeArray = job.Shapes;
                shapeArray[k] = net.Shapes[k];
            }

            handle = job.Schedule();
            Going = true;
        }

        /// <summary>The picture taken up: waited for (it has had a whole picture to run), copied back, its memory given back.</summary>
        public void End(ClothNet net)
        {
            if (!Going)
            {
                return;
            }

            handle.Complete();
            job.F.CopyTo(net.Packed);
            Free();
            net.Finish();
        }

        /// <summary>A picture in flight dropped (the garment is gone).</summary>
        public void Drop()
        {
            if (!Going)
            {
                return;
            }

            handle.Complete();
            Free();
        }

        private void Free()
        {
            job.F.Dispose();
            job.Ends.Dispose();
            job.Kinds.Dispose();
            job.Hidden.Dispose();
            job.Shapes.Dispose();
            job.Work.Dispose();
            Going = false;
        }
    }
}
#endif
