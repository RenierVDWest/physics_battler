using UnityEngine;

namespace PhysicsBattler
{
    /// <summary>The project's physics layers (set in Project Settings > Tags and Layers).</summary>
    public static class Layers
    {
        public const string Body = "Body";
        public const string Weapon = "Weapon";
        public const string Prop = "Prop";
        public const string Ground = "Ground";

        /// <summary>The layer's index, or Default (0) if the layer is missing.</summary>
        public static int Index(string layerName)
        {
            int index = LayerMask.NameToLayer(layerName);
            return index >= 0 ? index : 0;
        }
    }
}
