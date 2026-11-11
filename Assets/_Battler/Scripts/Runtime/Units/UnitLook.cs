using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Where the head looks (milestone 9, <see cref="HeadLook"/>): at what holds its attention. In
    /// order of weight: its own blow's aim, a blow it has seen coming at it, the weapon of the enemy
    /// it is minding as it winds up, someone new at the edge of its view, a glance now and then at
    /// whoever is nearest (enemy or ally), and otherwise that enemy's face. With nothing known, it
    /// looks about. Since it sees from its head, where it looks decides what it knows.
    /// </summary>
    public partial class UnitController
    {
        public HeadLook Look { get; } = new HeadLook();
        private float lookWeight, nextGlanceAt, glanceUntil;
        private UnitController glanceAt;
        private System.Random lookRandom;

        private void InitializeLook(int seed)
        {
            lookRandom = new System.Random(seed + 71);
            Look.Reset();
            Look.Latency = 0.1f + 0.1f * (float)lookRandom.NextDouble();
            lookWeight = 0f;
            nextGlanceAt = 2f + 3f * (float)lookRandom.NextDouble();
            glanceUntil = 0f;
            glanceAt = null;
        }
    }
}
