using System;
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
        public static event Action<int, int, float> BeforeSubstep;

        /// <summary>After a game step's last physics step.</summary>
        public static event Action AfterStep;

        /// <summary>One game step of <paramref name="dt"/> seconds: the engine stepped <see cref="Substeps"/> times.</summary>
        public static void Run(float dt)
        {
            int n = Mathf.Max(1, Substeps);
            for (int k = 1; k <= n; k++)
            {
                BeforeSubstep?.Invoke(k, n, dt / n);
                Physics.Simulate(dt / n);
            }

            AfterStep?.Invoke();
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
