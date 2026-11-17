using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Postures made of a made attack (the plan "Fight Rhythm from Recordings", step 6): the short bursts a fighter
    /// throws between blows to draw the other out. A posture is the beginning of a real attack, taken back.
    ///
    /// The attack is made for its target as any attack is (<see cref="AttackMaker"/>), so a posture starts exactly as
    /// that blow would. Then its body is taken apart into its parts again (<see cref="BodyTracks"/>) and the parts
    /// are sent two ways, each a part of the way into the attack and back along the way they came:
    /// - the upper body (the chest on the hips, the head, the arms, the hands) into the wind-up: a feint;
    /// - the lower body (the hips, the legs, and the body's travel over the ground) into the lunge: a fake lunge.
    /// One alone, or both together, which reads as a blow he means. The chest keeps facing the way its own moment of
    /// the attack has it face, whatever the hips under it are doing.
    /// </summary>
    public static class PostureMaker
    {
        /// <summary>How far into the wind-up a feint goes (of the wind-up, to where the hips would start in), how long it is held there and how long it takes to come back (seconds; the way back is never shorter than a quarter of a second).</summary>
        public static float FeintDepth = 0.85f, FeintHold = 0.06f, FeintBack = 0.45f;

        /// <summary>A fake lunge: how far it carries his hips (metres at a Medium's size), how long it takes to get there and how long to come back (seconds). Its legs and hips go as far into the attack as carries the hips that far.</summary>
        public static float LungeFar = 0.3f, LungeOut = 0.28f, LungeBack = 0.4f;

        /// <summary>
        /// A fake lunge asked for while he walks is thrown on the move (Combat Feel "A Fake Lunge Is Thrown On The
        /// Move"): the walk's legs go on under it, and his hips dart toward the other this much of the lunge's
        /// distance and sink as the lunge's do. Off: he stops for it and lunges on the attack's own legs, as it was.
        /// </summary>
        public static bool LungesOnTheMove = true;
        public static float LungeOnTheMove = 0.7f;

        /// <summary>A drop: how far his hips sink (metres at a Medium's size) and he leans (degrees), how long it takes to get down, how long he is held there and how long he takes to come up (seconds).</summary>
        public static float DropSinks = 0.10f, DropLeans = 8f, DropDown = 0.12f, DropHeld = 0.05f, DropUp = 0.3f;

        /// <summary>The blade's bluffs: how far the point dips, goes aside and goes round (degrees), all times <see cref="BluffSize"/>; the shield's press: how far it is thrust out (0 to 1); a high guard's lift and a low guard's drop of the sword arm (degrees).</summary>
        public static float BluffSize = 1f, ShieldPressFar = 1f, GuardHigh = 35f, GuardLow = 25f;
        public const float BladeDips = 38f, BladeFlicks = 32f, BladeCircles = 22f;

        /// <summary>
        /// A dodge (rebuilt from the recordings "Dodging" and "Dodging Right": the hips sink 15 to 21 cm and go 14 cm
        /// aside, the head 40 cm aside and 25 to 40 cm down, in a third of a second, and come back): how far his hips
        /// sink and go aside (metres at a Medium's size), how far he leans to that side and forward (degrees), and
        /// how long it takes down, is held, and comes back up (seconds). His feet stay where they are.
        /// </summary>
        public static float DodgeSinks = 0.17f, DodgeAside = 0.13f, DodgeRolls = 26f, DodgeLeans = 10f, DodgeDown = 0.24f, DodgeHeld = 0.15f, DodgeUp = 0.4f;

        /// <summary>
        /// An uncovering (the plan's step 7, the shield test; <see cref="ClipFighter.Uncover"/>): how long its tell
        /// lasts before the shield starts away (seconds: his blade's point dips and is held down, a gesture of his
        /// sword and not of his shield, so the two are told apart), and how long the part is left open (seconds).
        /// </summary>
        public static float UncoverTell = 0.3f, UncoverFor = 0.4f;

        /// <summary>An uncovering: how fast the shield is carried away and brought back (m/s at a Medium's size), how much room it leaves about the line to the part (metres), and the furthest it is carried (metres).</summary>
        public const float UncoverRate = 1.8f, UncoverBack = 1.5f, UncoverMost = 0.6f;
        public static float UncoverRoom = 0.15f;

        /// <summary>An uncovering starts back this much sooner than its travel says (seconds): the arm trails what it is asked, and the body as seen trails the arm, so the board is on the line again that much later than asked.</summary>
        public const float UncoverLag = 0.12f;

        /// <summary>A feint is not held where its blade would be nearer the middle of his own head, neck or chest than this (metres; a head is 0.1 thick about its middle): it stops short, at the last moment of the wind-up the blade is clear.</summary>
        public static float BladeClear = 0.18f;

        /// <summary>How near the blade is to the middle of his own head, neck or chest at a moment of an attack (metres at a Medium's size).</summary>
        private static float BladeFromSelf(LocoClip src, in SwordHold hold, float time, Vector3[] p)
        {
            var m = BodyMeasures.OfClips;
            src.Pose(time, p, 1f);
            var t = m.Decompose(p);
            AttackScore.SwordOf(t, m, hold, false, out _, out _, out Vector3 grip, out Vector3 way);
            Vector3 tip = grip + way.normalized * hold.Length;
            float nearest = float.MaxValue;
            // (His head from its foot to its top, his neck, and his chest, which is thicker: counted 5 cm nearer.)
            Vector3 head = p[(int)LocoBone.Head], top = p[(int)LocoBone.HeadTop];
            var middles = new[] { (head, 0f), (0.5f * (head + top), 0f), (top, 0f), (p[(int)LocoBone.Neck], 0f), (p[(int)LocoBone.Spine2], 0.05f / Locomotor.ClipLeg) };
            foreach (var (c, thicker) in middles)
            {
                float u = Mathf.Clamp01(Vector3.Dot(c - grip, tip - grip) / Mathf.Max(1e-6f, (tip - grip).sqrMagnitude));
                // (Not counted near the grip: the hand is always by the body.)
                if (u > 0.15f)
                {
                    nearest = Mathf.Min(nearest, (grip + (tip - grip) * u - c).magnitude - thicker);
                }
            }

            return nearest * Locomotor.ClipLeg;
        }

        /// <summary>The slowing into a feint's furthest point takes this long (seconds).</summary>
        public const float Eases = 0.14f;

        /// <summary>What the next attack asked of a fighter is to be instead: how much of a feint (0: none; 1: as <see cref="FeintDepth"/> has it) and how much of a fake lunge. Taken by the next attack made (<see cref="Take"/>).</summary>
        private static float askedUpper, askedLower;
        private static bool asked;

        /// <summary>The next attack a fighter is asked for is a posture: a feint, a fake lunge, or both (each 0 to 1 of its own depth).</summary>
        public static void Ask(float feint, float lunge)
        {
            askedUpper = Mathf.Clamp01(feint);
            askedLower = Mathf.Clamp01(lunge);
            asked = askedUpper > 0f || askedLower > 0f;
        }

        /// <summary>Whether a posture was asked for, and which; it is asked for once.</summary>
        public static bool Take(out float feint, out float lunge)
        {
            feint = askedUpper;
            lunge = askedLower;
            bool was = asked;
            asked = false;
            return was;
        }

        /// <summary>The moment of the attack the upper body is at, <paramref name="t"/> seconds into a feint: at the attack's own pace out to <paramref name="stop"/>, slowing into it, held, and back to the start.</summary>
        private static float Feinted(float t, float stop, float hold, float back, out float turnsAt)
        {
            float ease = Mathf.Min(Eases, 2f * stop);
            // (At the attack's own pace to half the slowing short of the stop, then slowing to nothing over it.)
            float runs = stop - 0.5f * ease;
            turnsAt = runs + ease;
            if (stop <= 0f)
            {
                turnsAt = 0f;
                return 0f;
            }

            if (t <= runs)
            {
                return t;
            }

            if (t < turnsAt)
            {
                float u = (t - runs) / Mathf.Max(1e-4f, ease);
                return stop - 0.5f * ease * (1f - u) * (1f - u);
            }

            return stop * Back(t - turnsAt - hold, back);
        }

        /// <summary>The moment of the attack the lower body is at, <paramref name="t"/> seconds into a fake lunge: out to <paramref name="stop"/> in <paramref name="reach"/> seconds, sharply off and slowing all the way, held, and back.</summary>
        private static float Lunged(float t, float stop, float reach, float hold, float back)
        {
            if (t < reach)
            {
                float u = 1f - t / Mathf.Max(0.05f, reach);
                return stop * (1f - u * u);
            }

            return stop * Back(t - reach - hold, back);
        }

        /// <summary>How much of the way out is left, so long after the hold is over: all of it, then eased down to none.</summary>
        private static float Back(float since, float back)
        {
            float v = Mathf.Clamp01(since / Mathf.Max(0.05f, back));
            return 1f - v * v * (3f - 2f * v);
        }

        private static Vector3 HipsAt(LocoClip src, float time, Vector3[] p)
        {
            src.Pose(time, p, 1f);
            src.Root(time, out Vector2 place, out float facing);
            return new Vector3(place.x, 0f, place.y) + Quaternion.Euler(0f, facing, 0f) * p[(int)LocoBone.Hips];
        }

        /// <summary>
        /// The posture's clip, made of a made attack: <paramref name="feint"/> of a feint and <paramref name="lunge"/>
        /// of a fake lunge (each 0 to 1 of its own depth). <paramref name="turnsAt"/>: when its furthest point is (seconds
        /// into the clip). Null: nothing asked for, or no attack.
        /// </summary>
        public static LocoClip Make(MadeAttack made, float feint, float lunge, out float turnsAt)
        {
            turnsAt = 0f;
            if (made == null || made.Clip == null || (feint <= 0f && lunge <= 0f))
            {
                return null;
            }

            var src = made.Clip;
            Vector3[] up = new Vector3[LocoClip.BoneCount], low = new Vector3[LocoClip.BoneCount], p = new Vector3[LocoClip.BoneCount];
            float drive = Mathf.Clamp(made.DriveAt, 0.05f, src.Duration), lands = Mathf.Clamp(made.ContactAt, drive, src.Duration);
            float stopUp = feint * FeintDepth * drive;
            if (stopUp > 0f)
            {
                // (Not held with the blade against himself: the real cut whips through there, a feint would rest in it.)
                float clearAt = -1f, bestAt = stopUp, best = -1f;
                for (float at = stopUp; at >= 0.5f * stopUp; at -= 1f / src.Rate)
                {
                    float clear = BladeFromSelf(src, made.Hold, at, p);
                    if (clear >= BladeClear)
                    {
                        clearAt = at;
                        break;
                    }

                    if (clear > best)
                    {
                        best = clear;
                        bestAt = at;
                    }
                }

                stopUp = clearAt >= 0f ? clearAt : bestAt;
            }

            float backUp = Mathf.Max(0.25f, FeintBack);
            Feinted(0f, stopUp, FeintHold, backUp, out float turnsUp);
            // A fake lunge goes as far into the attack as carries his hips its distance.
            float stopLow = stopUp, reach = turnsUp, holdLow = FeintHold, backLow = backUp;
            if (lunge > 0f)
            {
                float far = lunge * LungeFar / Locomotor.ClipLeg;
                Vector3 from = HipsAt(src, 0f, p);
                stopLow = 0.8f * lands;
                for (float at = 0f; at <= lands; at += 1f / src.Rate)
                {
                    if ((HipsAt(src, at, p) - from).magnitude >= far)
                    {
                        stopLow = at;
                        break;
                    }
                }

                // (With a feint on it the legs take as long as the sword does, and the two are at their furthest together.)
                reach = stopUp > 0f ? Mathf.Max(turnsUp, LungeOut) : LungeOut;
                holdLow = stopUp > 0f ? FeintHold : 0.03f;
                backLow = Mathf.Max(0.25f, LungeBack);
            }

            float lateUp = stopUp > 0f ? Mathf.Max(0f, reach - turnsUp) : 0f;
            turnsAt = lunge > 0f ? reach : turnsUp;
            float length = Mathf.Max(stopUp > 0f ? turnsUp + lateUp + FeintHold + backUp : 0f, lunge > 0f ? reach + holdLow + backLow : 0f) + 0.05f;

            float rate = src.Rate;
            int frames = Mathf.CeilToInt(length * rate) + 1;
            int turnFrame = Mathf.Clamp(Mathf.RoundToInt(turnsAt * rate), 1, frames - 2);
            var data = new LocoClipData
            {
                name = "made_posture_" + src.Name.Replace("made_", ""),
                role = "strike",
                kind = "",
                set = "guard",
                loop = false,
                rate = rate,
                frames = frames,
                legLength = Locomotor.ClipLeg,
                duration = (frames - 1) / rate,
                markers = new[] { turnFrame, Mathf.Min(frames - 1, turnFrame + 1) },
                root = new float[frames * 3],
                points = new float[frames * LocoClip.BoneCount * 3],
                contacts = new int[frames * 2],
                about = "a posture made of " + src.Name,
            };
            var m = BodyMeasures.OfClips;
            BodyTracks? lastUp = null, lastLow = null;
            for (int f = 0; f < frames; f++)
            {
                float t = f / rate;
                float atUp = Feinted(Mathf.Max(0f, t - lateUp), stopUp, FeintHold, backUp, out _);
                float atLow = lunge > 0f ? Lunged(t, stopLow, reach, holdLow, backLow) : atUp;
                src.Pose(atUp, up, 1f);
                src.Pose(atLow, low, 1f);
                var u = m.Decompose(up, lastUp);
                var l = m.Decompose(low, lastLow);
                lastUp = u;
                lastLow = l;
                // The lower body's hips and legs; the upper body's chest, head and arms on them, the chest facing as
                // its own moment has it face.
                var b = l;
                b.ChestTurn = new Vector3(u.ChestTurn.x, u.PelvisTurn.y + u.ChestTurn.y - l.PelvisTurn.y, u.ChestTurn.z);
                b.HeadTurn = u.HeadTurn;
                b.ClavL = u.ClavL; b.ClavR = u.ClavR; b.UpperL = u.UpperL; b.UpperR = u.UpperR; b.ForeL = u.ForeL; b.ForeR = u.ForeR;
                b.HandL = u.HandL; b.HandR = u.HandR;
                m.Compose(b, p, BodyMeasures.SpineOf(up));
                src.Root(atLow, out Vector2 place, out float facing);
                data.root[f * 3] = place.x;
                data.root[f * 3 + 1] = place.y;
                data.root[f * 3 + 2] = facing;
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = p[i].x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = p[i].z;
                }

                data.contacts[f * 2] = (int)src.Contact(0, atLow);
                data.contacts[f * 2 + 1] = (int)src.Contact(1, atLow);
            }

            return new LocoClip(data);
        }
    }
}
