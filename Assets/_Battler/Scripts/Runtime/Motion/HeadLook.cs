using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>Something the head could look at: an id (the same thing keeps its id), where it is, how much it matters, and how long it has been there.</summary>
    public struct LookPoint
    {
        /// <summary>Seconds since it appeared: a new stimulus is answered only after the reaction latency.</summary>
        public float age;
    }

    /// <summary>
    /// Where the head looks (milestone 9). Each step an attention score picks one point: a new one
    /// must beat the current by a margin, and the current is held a moment so the head doesn't
    /// twitch (a much stronger one, like a blow, cuts in). Then the look turns toward it as a person's
    /// does: eased in and out, a big look in a third to half a second, never faster than a neck
    /// turns, and only as far as a head can (with the chest's help).
    /// </summary>
    public sealed class HeadLook
    {
        /// <summary>How far the head turns from the chest (degrees each way), how much more the chest adds, and how far it looks down and up.</summary>
        public const float Turn = 80f, ChestTurn = 30f, Down = 60f, Up = 50f;

        /// <summary>Seconds before a new stimulus is answered (0.1 to 0.2: people differ).</summary>
        public float Latency = 0.15f;

        /// <summary>The look now: degrees right of the unit's facing, and up.</summary>
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        /// <summary>What it is looking at: the point's id (-1: nothing), where, its score and what kind of thing.</summary>
        public int Current { get; private set; } = -1;
        public string Kind { get; private set; } = "";
        private float chosenAt = float.NegativeInfinity;
        private float yawSpeed, pitchSpeed;

        public void Reset()
        {
            Yaw = Pitch = yawSpeed = pitchSpeed = 0f;
            Current = -1;
            Kind = "";
            chosenAt = float.NegativeInfinity;
        }

        /// <summary>
        /// How the look is shared out: the chest's turn (a fifth of the look, and whatever is past
        /// the neck's own reach, at most <see cref="ChestTurn"/>; nothing while a blow owns the chest).
        /// </summary>
        /// <param name="chestNow">How far the chest is already turned from the facing (a side-on stance's), degrees.</param>
        public static float ChestShare(float yaw, float chestNow, float blowBlend)
        {
            float need = Mathf.DeltaAngle(chestNow, yaw);
            float beyond = Mathf.Sign(need) * Mathf.Max(0f, Mathf.Abs(need) - Turn);
            return Mathf.Clamp(0.2f * yaw + beyond, -ChestTurn, ChestTurn) * Mathf.Clamp01(1f - blowBlend);
        }

        /// <summary>The head doesn't point dead on: the eyes take the last few degrees (a quarter of the look, at most 10 across and 8 up or down).</summary>
        public static float EyesAcross(float yaw) => Mathf.Clamp(0.25f * yaw, -10f, 10f);
        public static float EyesUpDown(float pitch) => Mathf.Clamp(0.2f * pitch, -8f, 8f);
    }
}
