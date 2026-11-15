using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a leap is thrown (the plan "Fight Rhythm from Recordings", step 2): how far each part of the body goes in it.
    /// The scene's Combat Feel ("Leap ...").
    /// </summary>
    public struct LeapStyle
    {
        /// <summary>How deep the hips sink before the push and as the legs take the landing, times this (1: a hand's breadth for a short leap, two for a long one).</summary>
        public float Crouch;

        /// <summary>How high the hips rise in the flight, times what the flight's time needs to stay off the ground; the legs are tucked under by as much.</summary>
        public float Height;

        /// <summary>How far the chest leans the way of the leap as it pushes off (degrees); it comes back upright through the flight and leans a little the other way to land.</summary>
        public float Lean;

        /// <summary>How much of the recorded jump's arm swing the arms take (0: they stay in the guard; 1: all of it).</summary>
        public float Arms;

        /// <summary>How much of the recorded jump's bend of the chest and tilt of the hips the trunk takes (the fold forward in the crouch, the stretch in the flight).</summary>
        public float Body;

        /// <summary>How long the crouch takes, the push from its lowest to the feet leaving, the landing from the feet touching to its lowest, and the way back up (seconds).</summary>
        public float PrepTime, PushTime, LandTime, BackTime;

        public static LeapStyle Plain => new LeapStyle { Crouch = 1f, Height = 1f, Lean = 14f, Arms = 0.4f, Body = 0.6f, PrepTime = 0.22f, PushTime = 0.12f, LandTime = 0.14f, BackTime = 0.26f };
    }

    /// <summary>
    /// Makes a leap of a recorded jump (the plan "Fight Rhythm from Recordings", step 2), as an attack is made of a
    /// recorded strike: the jump is taken apart into each body part's own track, and the leap put together of them on
    /// the fighter's own stance, as a clip the walk plays.
    ///
    /// <para><b>What a jump is</b> (read off the recording, a jump on the spot of 1.7 s): the hips sink 30 cm as the
    /// chest folds forward and the arms swing back (the crouch, half a second); the legs straighten and the arms swing
    /// up as the hips shoot up (the push, a fifth of a second), one foot leaving the ground after the other; the body
    /// is in the air with the legs tucked (the flight, 0.45 s, the hips 31 cm above standing); the feet touch one after
    /// the other and the hips sink 14 cm as the legs take the weight (the landing); and it straightens.</para>
    /// <para><b>What a leap takes of it.</b> The recording's phases, each stretched to the leap's own time. Its hips'
    /// sink and rise, scaled to the leap (a short leap is a small crouch and a low flight). Its trunk's fold and
    /// stretch and its arms' swing, added to the guard's own chest and arms (so the shield and sword stay held).
    /// Its legs' tuck in the air.</para>
    /// <para><b>What the leap adds: the way it goes.</b> The body travels the way asked: a little as it crouches (the
    /// hips lead), faster through the push, evenly through the flight, slowing as it lands. The chest leans that way
    /// as it pushes off and comes back through the flight. The foot nearer the way of the leap leaves the ground
    /// first; the other, behind, stays planted while the hips go away from it, so that leg straightens and pushes,
    /// and leaves last. They land in the same order.</para>
    /// </summary>
    public static class LeapMaker
    {
        /// <summary>The duel lab's leaps are made of the recorded jump (the scene's Combat Feel, "Leaps Are Made"); off, the stance's hop, as before.</summary>
        public static bool On = true;

        /// <summary>The style leaps are made in (the scene's Combat Feel).</summary>
        public static LeapStyle Style = LeapStyle.Plain;

        public const float Rate = 60f;

        /// <summary>The foot nearer the way of the leap leaves the ground this long before the push ends, the other this long after; the second lands this long after the first (seconds).</summary>
        public const float LeadLiftsBefore = 0.05f, TrailLiftsAfter = 0.04f, TrailLandsAfter = 0.05f;

        private sealed class Score
        {
            public BodyTracks[] Frames;
            public int Bottom, Takeoff, Landing, Absorb;
            public float Rate, Dip, Apex;
        }

        private static readonly Dictionary<string, Score> scores = new Dictionary<string, Score>();

        /// <summary>The recorded jump taken apart, and its moments: its lowest in the crouch, the feet leaving, the feet touching, its lowest in the landing.</summary>
        private static Score Of(LocoClip jump)
        {
            if (scores.TryGetValue(jump.Name, out var score))
            {
                return score;
            }

            var m = BodyMeasures.OfClips;
            var p = new Vector3[LocoClip.BoneCount];
            score = new Score { Frames = new BodyTracks[jump.Frames], Rate = jump.Rate, Takeoff = -1, Landing = -1 };
            for (int f = 0; f < jump.Frames; f++)
            {
                jump.Pose(f / jump.Rate, p, 1f);
                score.Frames[f] = f > 0 ? m.Decompose(p, score.Frames[f - 1]) : m.Decompose(p);
                bool down = (jump.Contact(0, f / jump.Rate) & LocoContact.Down) != 0 || (jump.Contact(1, f / jump.Rate) & LocoContact.Down) != 0;
                if (score.Takeoff < 0 && !down && f > 0)
                {
                    score.Takeoff = f;
                }
                else if (score.Takeoff >= 0 && score.Landing < 0 && down)
                {
                    score.Landing = f;
                }
            }

            int n = jump.Frames;
            score.Takeoff = score.Takeoff < 0 ? n / 3 : score.Takeoff;
            score.Landing = score.Landing < 0 ? Mathf.Min(n - 2, score.Takeoff + n / 4) : score.Landing;
            float stood = score.Frames[0].Pelvis.y;
            score.Bottom = 0;
            for (int f = 0; f <= score.Takeoff; f++)
            {
                if (score.Frames[f].Pelvis.y < score.Frames[score.Bottom].Pelvis.y)
                {
                    score.Bottom = f;
                }
            }

            score.Absorb = score.Landing;
            for (int f = score.Landing; f < n; f++)
            {
                if (score.Frames[f].Pelvis.y < score.Frames[score.Absorb].Pelvis.y)
                {
                    score.Absorb = f;
                }

                score.Apex = Mathf.Max(score.Apex, 0f);
            }

            for (int f = score.Takeoff; f <= score.Landing; f++)
            {
                score.Apex = Mathf.Max(score.Apex, score.Frames[f].Pelvis.y - stood);
            }

            score.Bottom = Mathf.Clamp(score.Bottom, 1, score.Takeoff - 1);
            score.Absorb = Mathf.Clamp(score.Absorb, score.Landing + 1, n - 2);
            score.Dip = Mathf.Max(0.02f, stood - score.Frames[score.Bottom].Pelvis.y);
            score.Apex = Mathf.Max(0.02f, score.Apex);
            return scores[jump.Name] = score;
        }

        private static float Smooth(float u)
        {
            u = Mathf.Clamp01(u);
            return u * u * (3f - 2f * u);
        }

        private static Vector3 Angles(Vector3 stance, Vector3 from, Vector3 to, float times) => new Vector3(
            stance.x + Mathf.DeltaAngle(from.x, to.x) * times, stance.y + Mathf.DeltaAngle(from.y, to.y) * times, stance.z + Mathf.DeltaAngle(from.z, to.z) * times);

        /// <summary>A way of the stance's turned as the recording's own way turns from its first frame to now, by so much of that turn.</summary>
        private static Vector3 Turned(Vector3 stance, Vector3 from, Vector3 to, float times)
        {
            if (times <= 0f || from.sqrMagnitude < 1e-8f || to.sqrMagnitude < 1e-8f)
            {
                return stance;
            }

            Quaternion.FromToRotation(from, to).ToAngleAxis(out float angle, out Vector3 axis);
            return angle < 0.01f || axis.sqrMagnitude < 1e-8f ? stance : Quaternion.AngleAxis(angle * times, axis) * stance;
        }

        /// <summary>
        /// A leap: of a recorded jump, on a stance (its clip's first pose), the way given (the fighter's own frame: x to
        /// its right, z forward), so far (leg lengths), its feet in the air for so long (seconds), by a body with a leg
        /// so long (metres). Null when either clip is missing.
        /// </summary>
        public static LocoClip Make(LocoClip jump, LocoClip stance, Vector3 way, float far, float flight, float leg, in LeapStyle style)
        {
            if (jump == null || stance == null)
            {
                return null;
            }

            var m = BodyMeasures.OfClips;
            var J = Of(jump);
            var p = new Vector3[LocoClip.BoneCount];
            stance.Pose(0f, p, 1f);
            BodyTracks S = m.Decompose(p), J0 = J.Frames[0];
            way.y = 0f;
            way = way.sqrMagnitude > 1e-6f ? way.normalized : Vector3.forward;
            flight = Mathf.Clamp(flight, 0.12f, 0.6f);
            leg = Mathf.Max(0.1f, leg);

            // The times of its phases, and the recording's frame at each.
            float t1 = Mathf.Max(0.05f, style.PrepTime), t2 = t1 + Mathf.Max(0.04f, style.PushTime), t3 = t2 + flight;
            float t4 = t3 + Mathf.Max(0.04f, style.LandTime), t5 = t4 + Mathf.Max(0.05f, style.BackTime);
            int frames = Mathf.CeilToInt(t5 * Rate) + 1, last = J.Frames.Length - 1;
            float Recorded(float t) => t <= t1 ? Mathf.Lerp(0f, J.Bottom, t / t1)
                : t <= t2 ? Mathf.Lerp(J.Bottom, J.Takeoff, (t - t1) / (t2 - t1))
                : t <= t3 ? Mathf.Lerp(J.Takeoff, J.Landing, (t - t2) / (t3 - t2))
                : t <= t4 ? Mathf.Lerp(J.Landing, J.Absorb, (t - t3) / (t4 - t3))
                : Mathf.Lerp(J.Absorb, last, Mathf.Clamp01((t - t4) / (t5 - t4)));

            // How far the body has got (the share of the leap): a little in the crouch, faster through the push, evenly
            // through the flight, slowing in the landing.
            float Got(float t)
            {
                if (t <= t1) return 0.04f * Smooth(t / t1);
                if (t <= t2) { float u = (t - t1) / (t2 - t1); return 0.04f + 0.16f * u * u; }
                if (t <= t3) return 0.2f + 0.66f * (t - t2) / (t3 - t2);
                if (t <= t4) { float u = (t - t3) / (t4 - t3); return 0.86f + 0.11f * (1f - (1f - u) * (1f - u)); }
                return 0.97f + 0.03f * Smooth((t - t4) / (t5 - t4));
            }

            // How much the chest leans the way of the leap: in through the crouch and the push, back through the flight,
            // a little the other way to land.
            float Leans(float t) => t <= t1 ? 0.5f * Smooth(t / t1) : t <= t2 ? Mathf.Lerp(0.5f, 1f, (t - t1) / (t2 - t1))
                : t <= t3 ? Mathf.Lerp(1f, -0.4f, (t - t2) / (t3 - t2)) : t <= t4 ? Mathf.Lerp(-0.4f, -0.1f, (t - t3) / (t4 - t3))
                : -0.1f * (1f - Smooth((t - t4) / (t5 - t4)));

            // The hips' sink and rise: the recording's, the sink scaled to the leap's length and the rise to what the
            // flight's time needs to stay off the ground (leg lengths).
            float sink = Mathf.Clamp(0.08f + 0.1f * far * leg, 0.08f, 0.22f) / leg * style.Crouch / J.Dip;
            float rise = (0.5f * 9.81f * 0.25f * flight * flight / leg + 0.03f) * style.Height;
            float lift = rise / J.Apex;

            // The foot nearer the way of the leap goes first; the other pushes.
            bool leftLeads = Vector3.Dot(S.AnkleL, way) > Vector3.Dot(S.AnkleR, way);
            float liftLead = t2 - LeadLiftsBefore, liftTrail = t2 + TrailLiftsAfter, landLead = t3, landTrail = t3 + TrailLandsAfter;

            var data = new LocoClipData
            {
                name = "leap", role = "leap", kind = "", set = "guard", loop = false, rate = Rate, frames = frames, legLength = Locomotor.ClipLeg,
                duration = (frames - 1) / Rate, root = new float[frames * 3], points = new float[frames * LocoClip.BoneCount * 3], contacts = new int[frames * 2],
                about = "a leap made of " + jump.Name + " on " + stance.Name,
            };
            for (int f = 0; f < frames; f++)
            {
                float time = Mathf.Min(f / Rate, t5), at = Mathf.Clamp(Recorded(time), 0f, last);
                int a = Mathf.Min(Mathf.FloorToInt(at), last - 1);
                var Jt = BodyTracks.Lerp(J.Frames[a], J.Frames[a + 1], at - a);
                Vector3 root = way * (far * Got(time));
                float leans = style.Lean * Leans(time);
                float went = Jt.Pelvis.y - J0.Pelvis.y;
                var t = S;

                // The hips: over the root, sunk and risen; tilted as the recording's are, and a little the way of the leap.
                t.Pelvis = new Vector3(S.Pelvis.x + root.x, S.Pelvis.y + (went < 0f ? went * sink : went * lift), S.Pelvis.z + root.z);
                t.PelvisTurn = Angles(S.PelvisTurn, J0.PelvisTurn, Jt.PelvisTurn, 0.7f * style.Body);
                t.PelvisTurn.x += 0.4f * leans * way.z;
                t.PelvisTurn.z -= 0.4f * leans * way.x;

                // The chest: the recording's fold and stretch on the guard's own, and the lean; the head stays up.
                t.ChestTurn = Angles(S.ChestTurn, J0.ChestTurn, Jt.ChestTurn, style.Body);
                t.ChestTurn.x += leans * way.z;
                t.ChestTurn.z -= leans * way.x;
                t.HeadTurn = Angles(S.HeadTurn, J0.HeadTurn, Jt.HeadTurn, 0.5f * style.Body);
                t.HeadTurn.x -= 0.6f * leans * way.z;

                // The arms: the guard's, swung as the recording's swing.
                t.UpperL = Turned(S.UpperL, J0.UpperL, Jt.UpperL, style.Arms);
                t.UpperR = Turned(S.UpperR, J0.UpperR, Jt.UpperR, style.Arms);
                t.ForeL = Turned(S.ForeL, J0.ForeL, Jt.ForeL, style.Arms);
                t.ForeR = Turned(S.ForeR, J0.ForeR, Jt.ForeR, style.Arms);

                // The feet: each planted until it leaves, over to where the stance has it at the leap's end in one arc
                // (tucked under as the recording's are), and planted again.
                for (int foot = 0; foot < 2; foot++)
                {
                    bool leads = (foot == 0) == leftLeads;
                    float lifts = leads ? liftLead : liftTrail, lands = leads ? landLead : landTrail;
                    float u = Mathf.Clamp01((time - lifts) / Mathf.Max(0.02f, lands - lifts));
                    Vector3 stood = foot == 0 ? S.AnkleL : S.AnkleR, recorded = foot == 0 ? Jt.AnkleL : Jt.AnkleR, recorded0 = foot == 0 ? J0.AnkleL : J0.AnkleR;
                    float tuck = Mathf.Max(0f, (recorded.y - Jt.Pelvis.y) - (recorded0.y - J0.Pelvis.y)) * lift;
                    Vector3 ankle = stood + way * (far * Smooth(u)) + Vector3.up * (4f * u * (1f - u) * (0.5f * rise + 0.04f) + tuck * Mathf.Sin(Mathf.PI * u));
                    bool air = u > 0f && u < 1f;
                    if (foot == 0)
                    {
                        t.AnkleL = ankle;
                        t.FootL = air ? Vector3.Slerp(S.FootL, Jt.FootL, 0.6f * Mathf.Sin(Mathf.PI * u)) : S.FootL;
                        t.ToeL = air ? Vector3.Slerp(S.ToeL, Jt.ToeL, 0.6f * Mathf.Sin(Mathf.PI * u)) : S.ToeL;
                    }
                    else
                    {
                        t.AnkleR = ankle;
                        t.FootR = air ? Vector3.Slerp(S.FootR, Jt.FootR, 0.6f * Mathf.Sin(Mathf.PI * u)) : S.FootR;
                        t.ToeR = air ? Vector3.Slerp(S.ToeR, Jt.ToeR, 0.6f * Mathf.Sin(Mathf.PI * u)) : S.ToeR;
                    }

                    data.contacts[f * 2 + foot] = air ? 0 : (int)(LocoContact.Down | LocoContact.Heel | LocoContact.Toe);
                }

                m.Compose(t, p);
                data.root[f * 3] = root.x;
                data.root[f * 3 + 1] = root.z;
                data.root[f * 3 + 2] = 0f;
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int o = (f * LocoClip.BoneCount + i) * 3;
                    data.points[o] = p[i].x - root.x;
                    data.points[o + 1] = p[i].y;
                    data.points[o + 2] = p[i].z - root.z;
                }
            }

            return new LocoClip(data);
        }
    }
}
