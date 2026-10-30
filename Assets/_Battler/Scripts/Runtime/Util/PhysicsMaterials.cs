using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler
{
    /// <summary>Shared physics materials, made on demand at runtime (no bounce, friction as asked).</summary>
    public static class PhysicsMaterials
    {
        private static readonly Dictionary<string, PhysicsMaterial> Cache = new Dictionary<string, PhysicsMaterial>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();

        public static PhysicsMaterial Get(string name, float friction)
        {
            string key = name + ":" + friction.ToString("0.###");
            if (Cache.TryGetValue(key, out var material) && material != null)
            {
                return material;
            }

            material = new PhysicsMaterial(name)
            {
                dynamicFriction = friction,
                staticFriction = friction,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Minimum,
            };
            Cache[key] = material;
            return material;
        }
    }
}
