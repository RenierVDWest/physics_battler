using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Authority
{
    /// <summary>
    /// The engine stepped by the game itself, several physics steps to each game step (the plan "A different
    /// approach", step 5): the game poses 60 times a second, and a blade at 30 m/s moves 50 cm in one of those
    /// steps, further than anything it could meet is thick. With <see cref="Manual"/> on, <see cref="Run"/> steps
    /// the engine <see cref="Substeps"/> times for each game step and tells whoever moves bodies by hand before
    /// each one (<see cref="BeforeSubstep"/>: kinematic parts go a share of their way, the muscles' targets are a
    /// share of the way to the new pose) and after the last (<see cref="AfterStep"/>).
    /// </summary>
    public static class PhysicsSteps
    {
        /// <summary>Physics steps to a game step (4: 240 a second).</summary>
        public static int Substeps = 4;

        /// <summary>The game steps the engine itself (<see cref="PhysicsStepper"/> in Unity, the runners headless); off, the engine steps once a game step on its own, as it always has.</summary>
        public static bool Manual;

        /// <summary>Before each physics step: which it is (1 to <c>n</c>), how many there are, and how long it is (seconds).</summary>
        public static event Action<int, int, float> BeforeSubstep
        {
            add { before.Add(value); beforeNow = null; }
            remove { before.Remove(value); beforeNow = null; }
        }

        /// <summary>After a game step's last physics step.</summary>
        public static event Action AfterStep
        {
            add { after.Add(value); afterNow = null; }
            remove { after.Remove(value); afterNow = null; }
        }

        private static readonly List<Action<int, int, float>> before = new List<Action<int, int, float>>();
        private static readonly List<Action> after = new List<Action>();
        private static Action<int, int, float>[] beforeNow;
        private static Action[] afterNow;

        /// <summary>How many are told of each physics step (a fighter with a physical body is one), for the reports.</summary>
        public static int Listeners => before.Count;

        /// <summary>How many of those told have thrown and been taken off since the game began.</summary>
        public static int Faults { get; private set; }

        /// <summary>
        /// One game step of <paramref name="dt"/> seconds: the engine stepped <see cref="Substeps"/> times. One of those
        /// told that throws is taken off and said so, and the rest and the engine go on: seen in Unity on 14 November,
        /// fighters destroyed for a fresh set were still told, threw at the first of them, and the engine was never
        /// stepped again: nothing physical moved for the rest of that play.
        /// </summary>
        public static void Run(float dt)
        {
            int n = Mathf.Max(1, Substeps);
            for (int k = 1; k <= n; k++)
            {
                beforeNow ??= before.ToArray();
                foreach (var told in beforeNow)
                {
                    try
                    {
                        told(k, n, dt / n);
                    }
                    catch (Exception fault)
                    {
                        BeforeSubstep -= told;
                        Fault(fault, told.Target);
                    }
                }

                Physics.Simulate(dt / n);
            }

            afterNow ??= after.ToArray();
            foreach (var told in afterNow)
            {
                try
                {
                    told();
                }
                catch (Exception fault)
                {
                    AfterStep -= told;
                    Fault(fault, told.Target);
                }
            }

            // (What was moved by its transform after the step, the shadows, is where queries find it before the next.)
            Physics.SyncTransforms();
        }

        private static void Fault(Exception fault, object whose)
        {
            Faults++;
            Debug.LogError($"[PhysicsSteps] {whose ?? "something"} threw when told of a physics step and is no longer told (the engine goes on): {fault}");
        }
    }

    /// <summary>
    /// In Unity: takes the engine's stepping from Unity for as long as it is enabled, and runs it after every
    /// script's own step (<see cref="PhysicsSteps.Run"/>).
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class PhysicsStepper : MonoBehaviour
    {
        private SimulationMode was;

        private void OnEnable()
        {
            was = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            PhysicsSteps.Manual = true;
        }

        private void OnDisable()
        {
            Physics.simulationMode = was;
            PhysicsSteps.Manual = false;
        }

        private void FixedUpdate()
        {
            PhysicsSteps.Run(Time.fixedDeltaTime);
        }
    }
}
