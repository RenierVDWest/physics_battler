using PhysicsBattler.Locomotion;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// A blade's path drawn over the game view (the combos plan's step 1; the scene's Combat Feel, "The Blade's Path Is
    /// Drawn"; the duel lab's "The blade's path"): where the attack's maker planned the sword's point to go, in pale
    /// blue, and where the sword's point then went, in orange. Drawn from <c>OnGUI</c> as lines of a set width on the
    /// screen, over everything, so a path behind a body is still seen; it reads <see cref="BladePath"/> and changes
    /// nothing.
    ///
    /// <para>The plan: thin over the wind-up, thick from where the hips start in to where the blade turns after the
    /// cut, thin and broken on the way back to the guard. A few faint spokes show the blade itself through the cut.
    /// A white bar is the blade as it is planned to land, with a small cross on its striking part; a yellow cross is
    /// the spot the attack was asked for. As it went: the point's path in orange with a dark dot for every step of
    /// the game's clock (dots close together: the blade is slow), and an orange bar for the blade at the moment its
    /// clip said it lands.</para>
    /// </summary>
    public static class BladePathView
    {
        private static readonly Color Plan = new Color(0.45f, 0.78f, 1f, 0.95f), PlanFaint = new Color(0.45f, 0.78f, 1f, 0.4f), Spoke = new Color(0.45f, 0.78f, 1f, 0.16f),
            Lands = new Color(1f, 1f, 1f, 0.95f), Asked = new Color(1f, 0.92f, 0.2f, 0.95f), Went = new Color(1f, 0.6f, 0.15f, 0.95f), WentFaint = new Color(1f, 0.6f, 0.15f, 0.45f),
            Dots = new Color(0.55f, 0.2f, 0f, 1f);

        /// <summary>Draws one path (from <c>OnGUI</c>, with the GUI's own scale as the screen's).</summary>
        public static void Draw(BladePath path, Camera camera)
        {
            if (path == null || path.PlanTip == null || camera == null || Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            int n = path.PlanTip.Length;
            int drive = Mathf.Clamp(Mathf.RoundToInt(path.DriveAt / BladePath.PlanStep), 0, n - 1), hit = Mathf.Clamp(Mathf.RoundToInt(path.HitAt / BladePath.PlanStep), 0, n - 1);
            int turn = Mathf.Clamp(Mathf.RoundToInt((path.HitAt + path.PlanTurnAfter) / BladePath.PlanStep), hit, n - 1);

            // The plan: the wind-up, the cut and its carry-through, the way back.
            for (int k = 0; k + 1 < n; k++)
            {
                bool cut = k >= drive && k < turn;
                if (cut)
                {
                    Line(camera, path.PlanTip[k], path.PlanTip[k + 1], Plan, 3f);
                }
                else if (k < drive || k % 2 == 0)
                {
                    Line(camera, path.PlanTip[k], path.PlanTip[k + 1], PlanFaint, 1.5f);
                }
            }

            for (int k = drive; k <= turn; k += 3)
            {
                Line(camera, path.PlanGrip[k], path.PlanTip[k], Spoke, 2f);
            }

            // Where it is planned to land, and the spot it was asked for.
            Line(camera, path.PlanGrip[hit], path.PlanTip[hit], Lands, 3f);
            Cross(camera, path.PlanStrike[hit], Lands, 7f, 2f);
            Cross(camera, path.Target, Asked, 9f, 2.5f);

            // As it went.
            int m = path.WentTip.Count, landed = -1;
            for (int i = 0; i + 1 < m; i++)
            {
                bool cut = path.WentAt[i] >= path.DriveAt;
                Line(camera, path.WentTip[i], path.WentTip[i + 1], cut ? Went : WentFaint, cut ? 3f : 1.5f);
                if (landed < 0 && path.WentAt[i + 1] >= path.HitAt)
                {
                    landed = i + 1;
                }
            }

            // (A dot for every step of the game's clock: where they bunch up the blade is slow, where it was frozen
            // on a hit they lie on one another.)
            for (int i = 0; i < m; i++)
            {
                if (path.WentAt[i] >= path.DriveAt)
                {
                    Dot(camera, path.WentTip[i], Dots, 5f);
                }
            }

            if (landed >= 0)
            {
                Line(camera, path.WentGrip[landed], path.WentTip[landed], Went, 3f);
                Cross(camera, path.WentStrike[landed], Went, 7f, 2f);
            }
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

        private static bool Screen2(Camera camera, Vector3 world, out Vector2 at)
        {
            Vector3 s = camera.WorldToScreenPoint(world);
            at = new Vector2(s.x, Screen.height - s.y);
            return s.z > 0.01f;
        }

        /// <summary>A small upright cross on the screen at a place of the world.</summary>
        private static void Cross(Camera camera, Vector3 world, Color colour, float arm, float pixels)
        {
            if (!Screen2(camera, world, out var at))
            {
                return;
            }

            var was = GUI.color;
            GUI.color = colour;
            GUI.DrawTexture(new Rect(at.x - arm, at.y - 0.5f * pixels, 2f * arm, pixels), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(at.x - 0.5f * pixels, at.y - arm, pixels, 2f * arm), Texture2D.whiteTexture);
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
