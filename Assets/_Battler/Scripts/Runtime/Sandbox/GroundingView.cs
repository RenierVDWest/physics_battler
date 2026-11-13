using PhysicsBattler.Locomotion;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The legs' opinion drawn under a fighter (the plan "Grounded legs", step 0; the scene's Combat Feel, "Show Legs
    /// Opinion"): each sole's outline in the colour of what the foot is doing, the support's outline, a dot under the
    /// body's weight, and a cross where a foot is needed to stop the body as it moves. Drawn over the game view from
    /// <c>OnGUI</c>, on the ground; it reads <see cref="Grounding"/> and changes nothing.
    /// </summary>
    public static class GroundingView
    {
        private static readonly Color Planted = new Color(0.25f, 0.9f, 0.35f), Sliding = new Color(1f, 0.2f, 0.2f), Landing = new Color(1f, 0.9f, 0.2f),
            Low = new Color(1f, 0.55f, 0.1f), High = new Color(0.45f, 0.7f, 1f), Outline = new Color(1f, 1f, 1f, 0.85f),
            Balanced = new Color(0.25f, 0.9f, 0.35f), Unbalanced = new Color(1f, 0.2f, 0.2f), Calm = new Color(0.8f, 0.8f, 0.8f, 0.9f), Wanted = new Color(1f, 0.45f, 0f);

        /// <summary>The place a foot is needed counts as outside the support from this far out (metres at a Medium's size): the cross turns orange.</summary>
        public const float NeededFrom = 0.10f;

        /// <summary>The colour a foot's sole is drawn in, by what it is doing.</summary>
        public static Color ColorOf(Grounding.FootState state)
        {
            switch (state)
            {
                case Grounding.FootState.Planted: return Planted;
                case Grounding.FootState.Sliding: return Sliding;
                case Grounding.FootState.Landing: return Landing;
                case Grounding.FootState.Low: return Low;
                default: return High;
            }
        }

        /// <summary>Draws one body's (from <c>OnGUI</c>, with the GUI's own scale as the screen's).</summary>
        public static void Draw(Grounding ground, Camera camera, float size)
        {
            if (ground == null || !ground.Known || camera == null || Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            float y = ground.GroundY + 0.005f;
            // The soles: on the ground under each foot, in the colour of what the foot is doing.
            for (int f = 0; f < 2; f++)
            {
                var foot = ground.Feet[f];
                if (float.IsNaN(foot.Lowest))
                {
                    continue;
                }

                var colour = ColorOf(foot.State);
                Vector3 a = Down(foot.A, y), b = Down(foot.B, y), c = Down(foot.C, y), d = Down(foot.D, y);
                Line(camera, a, b, colour, 3f);
                Line(camera, b, c, colour, 3f);
                Line(camera, c, d, colour, 3f);
                Line(camera, d, a, colour, 3f);
            }

            // The support: the outline round what is down.
            int n = ground.Support.Count;
            for (int i = 0; i < n && n >= 2; i++)
            {
                Vector2 p = ground.Support[i], q = ground.Support[(i + 1) % n];
                Line(camera, new Vector3(p.x, y, p.y), new Vector3(q.x, y, q.y), Outline, 1.5f);
                if (n == 2)
                {
                    break;
                }
            }

            // The weight: a dot on the ground under the centre of gravity, a thin line up to it.
            bool balanced = ground.WeightOut <= 0f;
            var weight = Down(ground.Weight, y);
            Dot(camera, weight, balanced ? Balanced : Unbalanced, 9f);
            Line(camera, weight, ground.Weight, new Color(1f, 1f, 1f, 0.25f), 1f);

            // Where a foot is needed: a cross on the ground, orange when that is outside what the feet hold.
            bool wanted = float.IsNaN(ground.NeededOut) || ground.NeededOut > NeededFrom * size;
            float arm = 0.07f * size;
            var need = Down(ground.Needed, y);
            var cross = wanted ? Wanted : Calm;
            Line(camera, need + new Vector3(-arm, 0f, -arm), need + new Vector3(arm, 0f, arm), cross, wanted ? 3f : 1.5f);
            Line(camera, need + new Vector3(-arm, 0f, arm), need + new Vector3(arm, 0f, -arm), cross, wanted ? 3f : 1.5f);
            Line(camera, weight, need, cross, 1f);
        }

        private static Vector3 Down(Vector3 p, float y) => new Vector3(p.x, y, p.z);

        private static bool Screen2(Camera camera, Vector3 world, out Vector2 at)
        {
            Vector3 s = camera.WorldToScreenPoint(world);
            at = new Vector2(s.x, Screen.height - s.y);
            return s.z > 0.01f;
        }

        private static void Dot(Camera camera, Vector3 world, Color colour, float pixels)
        {
            if (!Screen2(camera, world, out var at))
            {
                return;
            }

            var was = GUI.color;
            GUI.color = colour;
            GUI.DrawTexture(new Rect(at.x - 0.5f * pixels, at.y - 0.5f * pixels, pixels, pixels), Texture2D.whiteTexture);
            GUI.color = was;
        }

        private static void Line(Camera camera, Vector3 from, Vector3 to, Color colour, float pixels)
        {
            if (!Screen2(camera, from, out var a) || !Screen2(camera, to, out var b))
            {
                return;
            }

            float length = (b - a).magnitude;
            if (length < 0.5f)
            {
                return;
            }

            var matrix = GUI.matrix;
            var was = GUI.color;
            GUI.color = colour;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg, a);
            GUI.DrawTexture(new Rect(a.x, a.y - 0.5f * pixels, length, pixels), Texture2D.whiteTexture);
            GUI.matrix = matrix;
            GUI.color = was;
        }
    }
}
