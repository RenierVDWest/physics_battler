using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// A part of a body that a blow lands on is tinted red at once and fades back to its own colour over a second (the
    /// plan "Aimed attacks", step 0): what was really struck, not what was aimed at, so a blow that lands on the wrong
    /// part shows as the wrong part turning red. A blow stopped by a shield or a sword tints nothing. Only what is
    /// drawn changes: the part's own material is not touched (the tint is laid over it and taken off again).
    /// </summary>
    public sealed class HitFlashes : MonoBehaviour
    {
        /// <summary>Parts struck are tinted at all (the scene's Combat Feel, "Struck Parts Flash Red").</summary>
        public static bool On = true;

        /// <summary>How long the tint takes to fade (seconds of the game's clock).</summary>
        public const float Fades = 1f;

        /// <summary>The tint at its strongest.</summary>
        public static readonly Color Struck = new Color(0.95f, 0.06f, 0.04f, 1f);

        private static HitFlashes one;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor"), MainColor = Shader.PropertyToID("_Color");

        private sealed class Flash
        {
            public Renderer Renderer;
            public float At;
        }

        private readonly List<Flash> flashes = new List<Flash>();
        private MaterialPropertyBlock block;

        /// <summary>The one that listens for hits is there (made the first time, in play).</summary>
        public static void Ensure()
        {
            if (one == null && Application.isPlaying)
            {
                one = new GameObject("Hit flashes").AddComponent<HitFlashes>();
            }
        }

        private void OnEnable()
        {
            UnitController.Hit += OnHit;
        }

        private void OnDisable()
        {
            UnitController.Hit -= OnHit;
        }

        private void OnHit(HitReport hit)
        {
            if (!On || hit == null || hit.Struck == null || hit.Blocked)
            {
                return;
            }

            FlashPart(hit.Struck, hit.Part);
        }

        /// <summary>Tints one part of a unit now.</summary>
        public void FlashPart(UnitController unit, PartId id)
        {
            if (unit == null || unit.Rig == null)
            {
                return;
            }

            var part = unit.Rig.Part(id);
            if (part == null)
            {
                return;
            }

            foreach (var renderer in part.GetComponentsInChildren<Renderer>())
            {
                // (Not what the hand holds: a sword is no part of the body. And not the parts that hang from this one:
                // a blow on the chest does not redden the arms.)
                if (renderer == null || Under(renderer.transform, unit.Rig.SlotWeaponR) || Under(renderer.transform, unit.Rig.SlotWeaponL)
                    || renderer.GetComponentInParent<BodyPart>() != part)
                {
                    continue;
                }

                var again = flashes.Find(f => f.Renderer == renderer);
                if (again != null)
                {
                    again.At = Time.time;
                }
                else
                {
                    flashes.Add(new Flash { Renderer = renderer, At = Time.time });
                }
            }
        }

        private static bool Under(Transform what, Transform slot) => slot != null && what.IsChildOf(slot);

        private void Update()
        {
            block ??= new MaterialPropertyBlock();
            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                var flash = flashes[i];
                if (flash.Renderer == null)
                {
                    flashes.RemoveAt(i);
                    continue;
                }

                float t = (Time.time - flash.At) / Fades;
                var materials = flash.Renderer.sharedMaterials;
                if (t >= 1f || !On)
                {
                    // (Over: the tint is taken off, and the part is drawn with its own material again.)
                    for (int m = 0; m < materials.Length; m++)
                    {
                        flash.Renderer.SetPropertyBlock(null, m);
                    }

                    flashes.RemoveAt(i);
                    continue;
                }

                // (Full for the first moment, then back to its own colour, slower at the end.)
                float share = 1f - Mathf.Clamp01(t);
                share *= share;
                for (int m = 0; m < materials.Length; m++)
                {
                    var material = materials[m];
                    if (material == null)
                    {
                        continue;
                    }

                    Color own = material.HasProperty(BaseColor) ? material.GetColor(BaseColor) : material.HasProperty(MainColor) ? material.GetColor(MainColor) : Color.white;
                    Color now = Color.Lerp(own, Struck, share);
                    block.Clear();
                    block.SetColor(BaseColor, now);
                    block.SetColor(MainColor, now);
                    flash.Renderer.SetPropertyBlock(block, m);
                }
            }
        }
    }
}
