using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Where a blade is around the body when it turns: the corners a cut starts in and ends in (the plan "Combos that
    /// flow and end in a bang" of 20 November 2026, rule 1: a cut may follow another only if it starts where the other
    /// one ended).
    /// </summary>
    public enum BladeCorner
    {
        /// <summary>The hand before his sword side under the shoulder, the blade standing: the guard, where a cut that has not been wound up yet is.</summary>
        Guard,

        /// <summary>The hand up at his sword side, the blade back over that shoulder: a forehand's wind-up, a rising backhand's end.</summary>
        HighRight,

        /// <summary>The hand down across his body at his other hip, the point behind him on that side: a forehand's end, a backhand's wind-up.</summary>
        LowLeft,

        /// <summary>The arm swept out to his sword side: where a flat backhand ends.</summary>
        OutRight,

        /// <summary>The blade back over his other shoulder, the hand across at it or coming there: where a thrust comes back to, where a spinning backhand begins.</summary>
        LeftShoulder,

        /// <summary>The hand low before him, at the ground: where a chop ends.</summary>
        LowFront,

        /// <summary>The hand over his head, the blade behind it: a chop's wind-up.</summary>
        Overhead,

        /// <summary>The hand low at his sword side, the point behind him: where a lunging chop ends.</summary>
        LowRight,

        /// <summary>The blade out before him: the middle of a cut, a thrust as it lands. No corner: what a frame that is in none of them is called.</summary>
        Before,
    }

    /// <summary>
    /// How one recorded cut joins another (<see cref="BladeFlow.Join"/>): where the first is best left and the second
    /// best entered, when the second then lands, and how much there is to make up between the two blades there.
    /// </summary>
    public struct BladeJoin
    {
        public BladeFlow First, Next;

        /// <summary>The frame the first is left at, and the frame the next is entered at.</summary>
        public int Leave, Enter;

        /// <summary>How long after its landing the first is left, and how long before its landing the next is entered (seconds, at the recorded pace).</summary>
        public float After, Before;

        /// <summary>From the first landing to the next, joined there with nothing between (seconds, at the recorded pace).</summary>
        public float HitToHit => After + Before;

        /// <summary>Between the two points and between the two hands at the join (metres, against the body); and between the two blades' ways (degrees).</summary>
        public float Gap, HandGap, Angle;

        /// <summary>How fast the point goes as the first is left and as the next is entered (m/s), and how much the two velocities differ (m/s).</summary>
        public float SpeedOut, SpeedIn, SpeedOff;

        /// <summary>How far the point's path turns at the join (degrees): well under 90 the blade goes on round in a loop, over 90 it comes back the way it went.</summary>
        public float PathTurn;

        /// <summary>How far the body is turned differently at the two frames, each against where its own blow lands (degrees, + the next has him further to his right): what the hips have to turn on the way, or the next blow's aim has to take up.</summary>
        public float TurnOff;

        /// <summary>
        /// How long the blade and the body would need to get from the one to the other at a blade's and a body's own
        /// pace (seconds): the join's whole mismatch as one number. 0: the same blade in the same place going the
        /// same way, as where a performer went straight on from one cut into the next.
        /// </summary>
        public float Bend;

        /// <summary>How far the point has gone from the first landing to where it is left (metres along its path), and the slowest it goes between the two landings (m/s).</summary>
        public float Carry, Slowest;

        /// <summary>
        /// What the search goes by: the bend, and <see cref="BladeFlow.TimeCounts"/> of the time from landing to
        /// landing on top. The join with the least to make up, and of two that are about as good the sooner.
        /// </summary>
        public float Cost => Bend + BladeFlow.TimeCounts * (After + Before);
    }

    /// <summary>
    /// One recorded strike's blade, frame by frame (the combos plan, step 1): where the hand and the point are against
    /// the body, how fast the point goes, the top of its swing before it lands and where it turns after (its
    /// corners), and how far it is carried through. Read off the strike's score (<see cref="AttackScore"/>) with a
    /// Medium's hold of its sword (<see cref="SwordHold.Medium"/>): the blade the rig has, on the recorded hand.
    ///
    /// <para>It measures; it changes nothing a fighter does. The join of two blows into one swing (the plan's step 2)
    /// is to be built on it: <see cref="Join"/> says where one recording is best left and the next best entered, and
    /// how much there is to make up between them.</para>
    ///
    /// <para><b>The frames.</b> A place is against the body as it is at that frame: the ground under his hips at the
    /// origin, x to his hips' right, y up, z the way his hips face (metres). Not his root: a recording's root faces
    /// where his head looked as the recording began, so two slices of one take have two roots for one moment. A
    /// velocity is the point's as seen from outside with his hips' travel over the ground taken off, in the same
    /// axes: a man who walks does not move his blade by walking, but a man who spins does swing it by spinning.</para>
    /// </summary>
    public sealed class BladeFlow
    {
        public readonly AttackScore Score;
        public readonly string Name;
        public readonly float Rate;
        public readonly int Count;

        /// <summary>The grip and the point each frame, against the body at that frame (metres).</summary>
        public readonly Vector3[] Grip, Tip;

        /// <summary>The hand against his sword shoulder and the blade's way each frame, in his chest's own axes (x along his shoulders to his sword side, y up, z the way his chest faces; metres, and a unit vector): what a corner is named by.</summary>
        public readonly Vector3[] Hand, Way;

        /// <summary>The point's velocity each frame (m/s; his hips' axes at that frame, his travel taken off) and how fast that is.</summary>
        public readonly Vector3[] Velocity;
        public readonly float[] Speed;

        /// <summary>How his hips are turned each frame, counted on from the recording's first frame (degrees, + to his right; the axes the recording began in).</summary>
        public readonly float[] Turn;

        /// <summary>The way the blow lands: the bearing of the blade's striking part from his hips as it lands (degrees, the axes the recording began in).</summary>
        public readonly float Lands;

        /// <summary>The frames: the blade lands; the top of its swing before that (<see cref="TopOfSwing"/>: where the cut starts); it first turns after (<see cref="TurnAfter"/>: where the cut ends).</summary>
        public readonly int Hit, Enter, Leave;

        /// <summary>The fastest the point goes about the landing (m/s).</summary>
        public readonly float Peak;

        /// <summary>The corner the cut starts in and the one it ends in.</summary>
        public readonly BladeCorner From, To;

        /// <summary>How far the point goes from the landing to where the blade turns (metres along its path), and how far round the blade swings in that time (degrees).</summary>
        public readonly float Carry, CarrySwing;

        /// <summary>The same from the top of the swing to the landing: the cut's own run-up.</summary>
        public readonly float RunUp, RunUpSwing;

        public float Duration => (Count - 1) / Rate;
        public float HitAt => Hit / Rate;
        public float EnterAt => Enter / Rate;
        public float LeaveAt => Leave / Rate;

        /// <summary>A turn of the blade counts as one when the point is slower than this share of its fastest.</summary>
        public const float TurnsUnder = 0.35f;

        /// <summary>The top of the swing is looked for this long before the landing and the blade's turn this long after it, and neither nearer the landing than <see cref="NotWithin"/> (seconds).</summary>
        public const float LooksBefore = 0.45f, LooksAfter = 0.8f, NotWithin = 0.1f;

        /// <summary>A turn is the slowest moment within this long either side of it (seconds): a hitch in a blade that is speeding up is not one.</summary>
        public const float TurnOver = 0.1f;

        /// <summary>
        /// A join's bend (<see cref="BladeJoin.Bend"/>) is the longest of: the gap between the points at this pace
        /// (m/s), the gap between the hands at this (m/s), the difference of the point's velocities at this
        /// quickening (m/s²), and what the body is turned differently, over what the next blow's aim takes up
        /// (<see cref="AttackMaker.MostTurn"/>), at this rate (degrees a second). About what a recorded blade does
        /// between two landings: its point goes at 5 to 20 m/s, its hand at up to 4, it gains or loses 100 to 200
        /// m/s in a second, and a spinning man turns 360 degrees a second.
        /// </summary>
        public const float BendPointPace = 8f, BendHandPace = 4f, BendQuickening = 100f, BendTurnRate = 360f;

        /// <summary>A join leaves the first cut no later than this long after its blade has turned (seconds): later is its way back to the guard, and a blow from the guard is no join.</summary>
        public const float LeavesWithin = 0.07f;

        /// <summary>
        /// The time from landing to landing counts this much against a join's bend (<see cref="BladeJoin.Cost"/>): a
        /// tenth of a second of bend is worth a second of waiting. Counted as much as the bend (and then three times
        /// less), the search left every first cut a tenth of a second after it landed and entered every next one at
        /// the top of its swing: 1.7 m between the two points "cost" 0.2 s, where the performer's own blade took 0.3 s
        /// round its loop to get there; and it went from a flat backhand straight to a chop's top, where the
        /// performer loops over his head and leaps for 0.9 s.
        /// </summary>
        public const float TimeCounts = 0.1f;

        /// <summary>The bend the plan allows a join (seconds): "bent onto the new path within about 0.15 s".</summary>
        public const float PlanBend = 0.15f;

        private static readonly Dictionary<AttackScore, BladeFlow> flows = new Dictionary<AttackScore, BladeFlow>();

        /// <summary>The point each frame against where his hips are, in the axes the recording began in (metres).</summary>
        private readonly Vector3[] off;

        /// <summary>A strike's blade flow (made once for each score).</summary>
        public static BladeFlow Of(AttackScore score)
        {
            if (score == null)
            {
                return null;
            }

            if (!flows.TryGetValue(score, out var flow))
            {
                flows[score] = flow = new BladeFlow(score, SwordHold.Medium);
            }

            return flow;
        }

        public static BladeFlow Of(LocoClip clip) => Of(AttackScore.Of(clip));

        private BladeFlow(AttackScore score, in SwordHold hold)
        {
            Score = score;
            Name = score.Name;
            Rate = score.Rate;
            int n = Count = score.Frames.Length;
            Grip = new Vector3[n];
            Tip = new Vector3[n];
            Hand = new Vector3[n];
            Way = new Vector3[n];
            Velocity = new Vector3[n];
            Speed = new float[n];
            Turn = new float[n];
            off = new Vector3[n];
            var m = BodyMeasures.OfClips;
            float leg = hold.Leg;
            Hit = Mathf.Clamp(score.Contact, 0, n - 1);

            // (Where the blow lands: the striking part against his hips as it lands. A thrust's point lands on the
            // arm's line.)
            AttackScore.SwordOf(score.Frames[Hit], m, hold, score.Thrust, out _, out _, out Vector3 gripThen, out Vector3 along);
            Vector3 lands = gripThen + along * hold.Length, struck = gripThen + along * hold.Strike - Flat(score.Frames[Hit].Pelvis);
            Lands = Mathf.Atan2(struck.x, struck.z) * Mathf.Rad2Deg;

            for (int f = 0; f < n; f++)
            {
                var t = score.Frames[f];
                AttackScore.SwordOf(t, m, hold, false, out Vector3 shoulder, out _, out Vector3 grip, out Vector3 way);
                if (score.Thrust)
                {
                    // (A thrust's blade: the recorded hand's, turned by the wrist toward where the point lands as it
                    // is driven in and let go again after, as a made attack's is: MadeAttack.Blade. The recorded
                    // thrusting hands don't say where a blade points; where it lands is on the arm's line.)
                    float at = f / Rate, c = score.Contact / Rate, from = 0.4f * c;
                    float v = at <= c ? Mathf.Clamp01((at - from) / Mathf.Max(0.05f, c - from)) : 1f - Mathf.Clamp01((at - c) / 0.25f);
                    way = AttackMaker.Steered(way, lands - grip, hold.MostThrustSteer * v * v * (3f - 2f * v), out _);
                }

                Vector3 tip = grip + way * hold.Length, hips = Flat(t.Pelvis);
                Turn[f] = t.PelvisTurn.y;
                var toHips = Quaternion.Euler(0f, -Turn[f], 0f);
                Grip[f] = toHips * (grip - hips) * leg;
                Tip[f] = toHips * (tip - hips) * leg;
                off[f] = (tip - hips) * leg;

                // (His chest's axes: along his shoulders, level.)
                Vector3 across = Quaternion.Euler(t.PelvisTurn) * Quaternion.Euler(t.ChestTurn) * Vector3.right;
                var toChest = Quaternion.Euler(0f, -Mathf.Atan2(-across.z, across.x) * Mathf.Rad2Deg, 0f);
                Hand[f] = toChest * (grip - shoulder) * leg;
                Way[f] = toChest * way;
            }

            for (int f = 0; f < n; f++)
            {
                int a = Mathf.Max(0, f - 1), b = Mathf.Min(n - 1, f + 1);
                Vector3 v = b > a ? (off[b] - off[a]) * (Rate / (b - a)) : Vector3.zero;
                Velocity[f] = Quaternion.Euler(0f, -Turn[f], 0f) * v;
                Speed[f] = v.magnitude;
            }

            int near = Mathf.Max(1, Mathf.RoundToInt(0.13f * Rate));
            float peak = 0f;
            for (int f = Mathf.Max(0, Hit - near); f <= Mathf.Min(n - 1, Hit + near); f++)
            {
                peak = Mathf.Max(peak, Speed[f]);
            }

            Peak = peak;
            Enter = TopOfSwing();
            Leave = TurnAfter();
            From = CornerAt(Enter);
            To = CornerAt(Leave);
            Carry = Path(Hit, Leave, out CarrySwing);
            RunUp = Path(Enter, Hit, out RunUpSwing);
        }

        private static Vector3 Flat(Vector3 p) => new Vector3(p.x, 0f, p.z);

        /// <summary>
        /// The top of the swing: the frame before the landing at which the point is furthest from where it lands
        /// (between <see cref="LooksBefore"/> and <see cref="NotWithin"/> seconds before it; against where he
        /// stands, in the axes the recording began in). A recorded wind-up runs into its drive without the blade
        /// slowing (a forehand's point is at 8 m/s there, going round in a loop), so this is not read off its speed
        /// as the turn after the landing is. A recording that begins inside its cut (a combo's slice): its first frame.
        /// </summary>
        private int TopOfSwing()
        {
            int hi = Hit - Mathf.RoundToInt(NotWithin * Rate), lo = Mathf.Max(0, Hit - Mathf.RoundToInt(LooksBefore * Rate));
            if (hi <= lo)
            {
                return 0;
            }

            int best = hi;
            float most = -1f;
            for (int f = hi; f >= lo; f--)
            {
                float far = Vector3.Distance(off[f], off[Hit]);
                if (far > most)
                {
                    most = far;
                    best = f;
                }
            }

            return best;
        }

        /// <summary>The blade turns at a frame: the point is slower there than anywhere within <see cref="TurnOver"/> seconds either side, and under <see cref="TurnsUnder"/> of its fastest.</summary>
        private bool Turns(int f)
        {
            if (Speed[f] >= TurnsUnder * Peak)
            {
                return false;
            }

            int w = Mathf.Max(1, Mathf.RoundToInt(TurnOver * Rate));
            for (int g = Mathf.Max(0, f - w); g <= Mathf.Min(Count - 1, f + w); g++)
            {
                if (Speed[g] < Speed[f])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The first frame after the landing at which the blade turns (<see cref="Turns"/>); none: where it is slowest. A recording that ends inside its carry-through (a combo's slice): its last frame.</summary>
        private int TurnAfter()
        {
            int lo = Hit + Mathf.RoundToInt(NotWithin * Rate), hi = Mathf.Min(Count - 2, Hit + Mathf.RoundToInt(LooksAfter * Rate));
            if (hi <= lo)
            {
                return Count - 1;
            }

            for (int f = lo; f <= hi; f++)
            {
                if (Turns(f))
                {
                    return f;
                }
            }

            int best = lo;
            for (int f = lo; f <= hi; f++)
            {
                best = Speed[f] < Speed[best] ? f : best;
            }

            return hi == Count - 2 && Speed[Count - 1] <= Speed[best] ? Count - 1 : best;
        }

        /// <summary>How far the point goes between two frames (metres along its path, his travel taken off), and how far round the blade swings (degrees, summed frame by frame).</summary>
        public float Path(int from, int to, out float swing)
        {
            float path = 0f;
            swing = 0f;
            from = Mathf.Clamp(from, 0, Count - 1);
            to = Mathf.Clamp(to, 0, Count - 1);
            for (int f = from; f < to; f++)
            {
                path += Vector3.Distance(off[f], off[f + 1]);
                swing += Vector3.Angle(Outside(f, Tip[f] - Grip[f]), Outside(f + 1, Tip[f + 1] - Grip[f + 1]));
            }

            return path;
        }

        /// <summary>A way in his hips' axes at a frame, in the axes the recording began in.</summary>
        private Vector3 Outside(int f, Vector3 way) => Quaternion.Euler(0f, Turn[f], 0f) * way;

        /// <summary>How his hips are turned at a frame against where the blow lands (degrees; 0: square to it).</summary>
        public float Facing(int f) => Turn[Mathf.Clamp(f, 0, Count - 1)] - Lands;

        /// <summary>The slowest the point goes between two frames (m/s), and at which frame.</summary>
        public float Slowest(int from, int to, out int at)
        {
            from = Mathf.Clamp(from, 0, Count - 1);
            to = Mathf.Clamp(to, from, Count - 1);
            at = from;
            for (int f = from; f <= to; f++)
            {
                at = Speed[f] < Speed[at] ? f : at;
            }

            return Speed[at];
        }

        /// <summary>The corner the blade is in at a frame: by where the hand is against his sword shoulder and which way the blade points.</summary>
        public BladeCorner CornerAt(int f)
        {
            f = Mathf.Clamp(f, 0, Count - 1);
            return Corner(Hand[f], Way[f], Grip[f].y);
        }

        /// <summary>
        /// The corner for a hand against the sword shoulder and a blade's way, both in the chest's axes, and the
        /// hand's height over the ground (metres; see <see cref="Hand"/>). The hand's place says most (as the
        /// fencing books name a guard by where the hilt is); the blade's way tells apart the corners a hand passes
        /// through on its way elsewhere.
        /// </summary>
        public static BladeCorner Corner(Vector3 hand, Vector3 way, float height)
        {
            // (The sword shoulder is some 0.19 m out from the middle of his chest: a hand at -0.19 is over his head
            // or before his breastbone, one at -0.38 at his other shoulder.)
            if (height < 0.35f)
            {
                return BladeCorner.LowFront;
            }

            if (hand.y > 0.3f && hand.x < 0.1f)
            {
                return BladeCorner.Overhead;
            }

            if (hand.y > 0.1f && hand.x >= 0.1f)
            {
                return BladeCorner.HighRight;
            }

            bool swept = hand.x > 0.25f && way.x > 0.5f, behind = way.x > 0.3f && hand.z < -0.3f;
            // (In none of the corners: the guard if the blade stands, else somewhere before him.)
            var rest = way.y > 0.5f ? BladeCorner.Guard : BladeCorner.Before;
            if (hand.y > -0.25f)
            {
                if (hand.x < -0.15f)
                {
                    return BladeCorner.LeftShoulder;
                }

                // (The hand before his sword shoulder with the blade back over it: high right as well.)
                return swept ? BladeCorner.OutRight : hand.x > 0f && hand.y > -0.1f && way.z < -0.3f ? BladeCorner.HighRight : behind ? BladeCorner.LowRight : rest;
            }

            return hand.x < -0.15f ? BladeCorner.LowLeft : swept ? BladeCorner.OutRight : behind ? BladeCorner.LowRight : rest;
        }

        /// <summary>
        /// How one cut joins another: where the first is best left (from <see cref="NotWithin"/> after its landing
        /// to <see cref="LeavesWithin"/> after its blade has turned) and the next best entered (from its first frame
        /// to the top of its swing: a blow entered later has lost its run-up), by <see cref="BladeJoin.Cost"/>: the
        /// least to make up. Entered early, the next cut's own wind-up is the bridge (a loop over the head before a
        /// chop); left late, the first cut's carry-through is.
        /// </summary>
        public static BladeJoin Join(BladeFlow first, BladeFlow next) => Join(first, next, 0f);

        /// <summary>
        /// The same, but the soonest second landing of all the joins with no more than <paramref name="mostBend"/>
        /// to make up (seconds; <see cref="PlanBend"/>: what the plan allows); none so good: the one with the least.
        /// </summary>
        public static BladeJoin Join(BladeFlow first, BladeFlow next, float mostBend) => Join(first, next, mostBend, 0f);

        /// <summary>
        /// The same, with the next cut entered no later than <paramref name="leastBefore"/> seconds before it lands (a
        /// combo's last blow, the plan's step 3: a big blow has its whole swing behind it, and its own wind-up is the
        /// loop that brings the blade round: over the shoulder for a forehand, over the head for a chop).
        /// </summary>
        public static BladeJoin Join(BladeFlow first, BladeFlow next, float mostBend, float leastBefore)
        {
            int iLo = Mathf.Min(first.Count - 1, first.Hit + Mathf.RoundToInt(NotWithin * first.Rate));
            int iHi = Mathf.Clamp(first.Leave + Mathf.RoundToInt(LeavesWithin * first.Rate), iLo, first.Count - 1);
            int jHi = Mathf.Clamp(next.Enter, 0, next.Count - 1);
            if (leastBefore > 0f)
            {
                jHi = Mathf.Clamp(Mathf.Min(jHi, next.Hit - Mathf.RoundToInt(leastBefore * next.Rate)), 0, next.Count - 1);
            }

            BladeJoin best = At(first, next, iHi, 0), soonest = best;
            bool any = false;
            for (int i = iLo; i <= iHi; i++)
            {
                for (int j = 0; j <= jHi; j++)
                {
                    var join = At(first, next, i, j);
                    if (join.Cost < best.Cost)
                    {
                        best = join;
                    }

                    if (join.Bend <= mostBend && (!any || join.HitToHit < soonest.HitToHit - 1e-4f || (join.HitToHit < soonest.HitToHit + 1e-4f && join.Bend < soonest.Bend)))
                    {
                        soonest = join;
                        any = true;
                    }
                }
            }

            return any ? soonest : best;
        }

        /// <summary>The join of two cuts with the first left at one frame and the next entered at another.</summary>
        public static BladeJoin At(BladeFlow first, BladeFlow next, int leave, int enter)
        {
            leave = Mathf.Clamp(leave, 0, first.Count - 1);
            enter = Mathf.Clamp(enter, 0, next.Count - 1);
            Vector3 wayOut = first.Tip[leave] - first.Grip[leave], wayIn = next.Tip[enter] - next.Grip[enter];
            Vector3 goesOut = first.Velocity[leave], goesIn = next.Velocity[enter];
            var join = new BladeJoin
            {
                First = first, Next = next, Leave = leave, Enter = enter,
                After = (leave - first.Hit) / first.Rate,
                Before = (next.Hit - enter) / next.Rate,
                Gap = Vector3.Distance(first.Tip[leave], next.Tip[enter]),
                HandGap = Vector3.Distance(first.Grip[leave], next.Grip[enter]),
                Angle = Vector3.Angle(wayOut, wayIn),
                SpeedOut = goesOut.magnitude, SpeedIn = goesIn.magnitude, SpeedOff = (goesOut - goesIn).magnitude,
                PathTurn = goesOut.sqrMagnitude > 1e-4f && goesIn.sqrMagnitude > 1e-4f ? Vector3.Angle(goesOut, goesIn) : 0f,
                TurnOff = Mathf.DeltaAngle(first.Facing(leave), next.Facing(enter)),
                Carry = first.Path(first.Hit, leave, out _),
            };
            join.Slowest = Mathf.Min(first.Slowest(first.Hit, leave, out _), next.Slowest(enter, next.Hit, out _));
            // (A turn of the body the next blow's aim can take up is no mismatch: a made attack turns him that far
            // to bring its blade onto its line. And where a cut "lands" is known no better than that: a flat cut's
            // blade goes round 27 degrees in one frame as it lands.)
            float turn = Mathf.Max(0f, Mathf.Abs(join.TurnOff) - AttackMaker.MostTurn);
            join.Bend = Mathf.Max(Mathf.Max(join.Gap / BendPointPace, join.HandGap / BendHandPace), Mathf.Max(join.SpeedOff / BendQuickening, turn / BendTurnRate));
            return join;
        }

        public static string Say(BladeCorner corner)
        {
            switch (corner)
            {
                case BladeCorner.HighRight: return "high right";
                case BladeCorner.LowLeft: return "low left";
                case BladeCorner.OutRight: return "out right";
                case BladeCorner.LeftShoulder: return "left shoulder";
                case BladeCorner.LowFront: return "low front";
                case BladeCorner.Overhead: return "overhead";
                case BladeCorner.LowRight: return "low right";
                case BladeCorner.Before: return "before him";
                default: return "guard";
            }
        }
    }
}
