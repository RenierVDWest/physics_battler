using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A body at one moment as its parts' own turns and travels (the plan "Aimed attacks", "What an attack is made
    /// of"): where the pelvis is and how it is turned, how the chest is turned on the pelvis and the head on the chest,
    /// which way each bone of each arm points on the chest and how the hand is turned, where each ankle is, which way
    /// the knee points and how the foot lies. In a clip's own units (leg lengths), in the frame the fighter faced as
    /// the clip began: x to its right, y up, z forward. Turns are (pitch, yaw, roll) in degrees, as
    /// <see cref="Quaternion.Euler(Vector3)"/> takes them: yaw + to the right, pitch + tipped forward.
    /// Index 0 is the left side, 1 the right.
    /// </summary>
    public struct BodyTracks
    {
        public Vector3 Pelvis, PelvisTurn, ChestTurn, HeadTurn;
        public Vector3 ClavL, ClavR, UpperL, UpperR, ForeL, ForeR;
        public Quaternion HandL, HandR;
        public Vector3 AnkleL, AnkleR, KneeL, KneeR, FootL, FootR, ToeL, ToeR;

        /// <summary>The body part of the way from one moment to another: places and turns straight between, ways and hands the short way round.</summary>
        public static BodyTracks Lerp(in BodyTracks a, in BodyTracks b, float t) => new BodyTracks
        {
            Pelvis = Vector3.LerpUnclamped(a.Pelvis, b.Pelvis, t),
            PelvisTurn = Vector3.LerpUnclamped(a.PelvisTurn, b.PelvisTurn, t),
            ChestTurn = Vector3.LerpUnclamped(a.ChestTurn, b.ChestTurn, t),
            HeadTurn = Vector3.LerpUnclamped(a.HeadTurn, b.HeadTurn, t),
            ClavL = Way(a.ClavL, b.ClavL, t), ClavR = Way(a.ClavR, b.ClavR, t),
            UpperL = Way(a.UpperL, b.UpperL, t), UpperR = Way(a.UpperR, b.UpperR, t),
            ForeL = Way(a.ForeL, b.ForeL, t), ForeR = Way(a.ForeR, b.ForeR, t),
            HandL = Quaternion.SlerpUnclamped(a.HandL, b.HandL, t), HandR = Quaternion.SlerpUnclamped(a.HandR, b.HandR, t),
            AnkleL = Vector3.LerpUnclamped(a.AnkleL, b.AnkleL, t), AnkleR = Vector3.LerpUnclamped(a.AnkleR, b.AnkleR, t),
            KneeL = Way(a.KneeL, b.KneeL, t), KneeR = Way(a.KneeR, b.KneeR, t),
            FootL = Way(a.FootL, b.FootL, t), FootR = Way(a.FootR, b.FootR, t),
            ToeL = Way(a.ToeL, b.ToeL, t), ToeR = Way(a.ToeR, b.ToeR, t),
        };

        /// <summary>A way (a unit vector) part of the way from one to another, and on past it.</summary>
        public static Vector3 Way(Vector3 a, Vector3 b, float t)
        {
            Vector3 v = Vector3.LerpUnclamped(a, b, t);
            return v.sqrMagnitude > 1e-10f ? v.normalized : b;
        }
    }

    /// <summary>
    /// How a spine is bent: where each joint from the hips to the head sits on the one below it (leg lengths; the
    /// same frames as <see cref="BodyMeasures"/>' own). The tracks say how the hips, the chest and the head are
    /// turned; this is what they leave out, the back's and the neck's own curve, which differs from one way of
    /// carrying the body to another (<see cref="Carriage"/>).
    /// </summary>
    public struct SpineShape
    {
        public Vector3 Spine, Spine1, Spine2, Neck, Head;

        public static SpineShape Lerp(in SpineShape a, in SpineShape b, float t) => new SpineShape
        {
            Spine = Vector3.LerpUnclamped(a.Spine, b.Spine, t), Spine1 = Vector3.LerpUnclamped(a.Spine1, b.Spine1, t), Spine2 = Vector3.LerpUnclamped(a.Spine2, b.Spine2, t),
            Neck = Vector3.LerpUnclamped(a.Neck, b.Neck, t), Head = Vector3.LerpUnclamped(a.Head, b.Head, t),
        };
    }

    /// <summary>
    /// The mocap body's own measures, the same in every clip (leg lengths): where its joints sit on the pelvis, the
    /// chest, the head and the hand, and its bones' lengths. With them a body is put together from its
    /// <see cref="BodyTracks"/> (<see cref="Compose"/>); taken from the clips themselves (<see cref="Of"/>).
    /// </summary>
    public sealed class BodyMeasures
    {
        /// <summary>How much of the chest's turn on the pelvis the two spine bones between them have taken.</summary>
        public const float SpineShare1 = 0.35f, SpineShare2 = 0.7f;

        public Vector3 Spine, Spine1, Spine2, Neck, Head, HeadTop;
        public readonly Vector3[] Eye = new Vector3[2], Hip = new Vector3[2], Clav = new Vector3[2], Index = new Vector3[2], Middle = new Vector3[2], Pinky = new Vector3[2];
        public readonly float[] ClavLen = new float[2], UpperLen = new float[2], ForeLen = new float[2], ThighLen = new float[2], ShinLen = new float[2], FootLen = new float[2], ToeLen = new float[2];

        private static BodyMeasures ofClips;

        /// <summary>The measures of the body the combat clips were recorded on (taken once, from every strike and the guard's stand).</summary>
        public static BodyMeasures OfClips
        {
            get
            {
                if (ofClips == null)
                {
                    var from = new List<LocoClip>();
                    foreach (var pair in LocoLibrary.Clips)
                    {
                        if (pair.Value.IsGuard && (pair.Value.Role == "strike" || pair.Key == "sns_idle_b"))
                        {
                            from.Add(pair.Value);
                        }
                    }

                    ofClips = Of(from);
                }

                return ofClips;
            }
        }

        private static readonly LocoBone[] Shoulders = { LocoBone.LeftShoulder, LocoBone.RightShoulder }, Arms = { LocoBone.LeftArm, LocoBone.RightArm },
            ForeArms = { LocoBone.LeftForeArm, LocoBone.RightForeArm }, Hands = { LocoBone.LeftHand, LocoBone.RightHand },
            Indexes = { LocoBone.LeftHandIndex, LocoBone.RightHandIndex }, Middles = { LocoBone.LeftHandMiddle, LocoBone.RightHandMiddle }, Pinkies = { LocoBone.LeftHandPinky, LocoBone.RightHandPinky },
            UpLegs = { LocoBone.LeftUpLeg, LocoBone.RightUpLeg }, Legs = { LocoBone.LeftLeg, LocoBone.RightLeg }, Feet = { LocoBone.LeftFoot, LocoBone.RightFoot },
            ToeBases = { LocoBone.LeftToeBase, LocoBone.RightToeBase }, ToeEnds = { LocoBone.LeftToeEnd, LocoBone.RightToeEnd }, Eyes = { LocoBone.LeftEye, LocoBone.RightEye };

        /// <summary>A part's turn from its right and (about) its up.</summary>
        internal static Quaternion Axes(Vector3 right, Vector3 up)
        {
            Vector3 x = right.normalized, z = Vector3.Cross(x, up).normalized;
            return Quaternion.LookRotation(z, Vector3.Cross(z, x));
        }

        /// <summary>The pelvis's, the chest's and the head's turns, and each hand's, from a body's points.</summary>
        internal static void Frames(Vector3[] p, out Quaternion pelvis, out Quaternion chest, out Quaternion head, out Quaternion handL, out Quaternion handR)
        {
            pelvis = Axes(p[(int)LocoBone.RightUpLeg] - p[(int)LocoBone.LeftUpLeg], p[(int)LocoBone.Spine] - p[(int)LocoBone.Hips]);
            chest = Axes(p[(int)LocoBone.RightShoulder] - p[(int)LocoBone.LeftShoulder], p[(int)LocoBone.Neck] - p[(int)LocoBone.Spine2]);
            head = Axes(p[(int)LocoBone.RightEye] - p[(int)LocoBone.LeftEye], p[(int)LocoBone.HeadTop] - p[(int)LocoBone.Head]);
            handL = HandFrame(p, 0);
            handR = HandFrame(p, 1);
        }

        internal static Quaternion HandFrame(Vector3[] p, int side)
        {
            // (Its right across the knuckles, its forward along the fingers.)
            Vector3 along = p[(int)Middles[side]] - p[(int)Hands[side]], x = (p[(int)Indexes[side]] - p[(int)Pinkies[side]]).normalized;
            Vector3 z = (along - x * Vector3.Dot(along, x)).normalized;
            return Quaternion.LookRotation(z, Vector3.Cross(z, x));
        }

        /// <summary>A clip's frame as points in the frame the clip began in (leg lengths): the root's own travel and turn put back in.</summary>
        public static void World(LocoClip clip, int frame, Vector3[] into)
        {
            float t = frame / clip.Rate;
            clip.Pose(t, into, 1f);
            clip.Root(0f, out Vector2 at0, out float yaw0);
            clip.Root(t, out Vector2 at, out float yaw);
            var turn = Quaternion.Euler(0f, Mathf.DeltaAngle(yaw0, yaw), 0f);
            Vector3 root = Quaternion.Euler(0f, -yaw0, 0f) * new Vector3(at.x - at0.x, 0f, at.y - at0.y);
            for (int i = 0; i < into.Length; i++)
            {
                float y = into[i].y;
                Vector3 w = turn * new Vector3(into[i].x, 0f, into[i].z) + root;
                into[i] = new Vector3(w.x, y, w.z);
            }
        }

        /// <summary>The measures as the mean over every frame of some clips.</summary>
        public static BodyMeasures Of(IEnumerable<LocoClip> clips)
        {
            var m = new BodyMeasures();
            var p = new Vector3[LocoClip.BoneCount];
            int n = 0;
            foreach (var clip in clips)
            {
                for (int f = 0; f < clip.Frames; f++)
                {
                    World(clip, f, p);
                    Frames(p, out var pel, out var che, out var head, out var handL, out var handR);
                    Quaternion toPel = Quaternion.Inverse(pel), toChe = Quaternion.Inverse(che), toHead = Quaternion.Inverse(head);
                    m.Spine += toPel * (p[(int)LocoBone.Spine] - p[(int)LocoBone.Hips]);
                    m.Spine1 += Quaternion.Inverse(Quaternion.Slerp(pel, che, SpineShare1)) * (p[(int)LocoBone.Spine1] - p[(int)LocoBone.Spine]);
                    m.Spine2 += Quaternion.Inverse(Quaternion.Slerp(pel, che, SpineShare2)) * (p[(int)LocoBone.Spine2] - p[(int)LocoBone.Spine1]);
                    m.Neck += toChe * (p[(int)LocoBone.Neck] - p[(int)LocoBone.Spine2]);
                    m.Head += Quaternion.Inverse(Quaternion.Slerp(che, head, 0.5f)) * (p[(int)LocoBone.Head] - p[(int)LocoBone.Neck]);
                    m.HeadTop += toHead * (p[(int)LocoBone.HeadTop] - p[(int)LocoBone.Head]);
                    for (int s = 0; s < 2; s++)
                    {
                        Quaternion toHand = Quaternion.Inverse(s == 0 ? handL : handR);
                        m.Eye[s] += toHead * (p[(int)Eyes[s]] - p[(int)LocoBone.Head]);
                        m.Hip[s] += toPel * (p[(int)UpLegs[s]] - p[(int)LocoBone.Hips]);
                        m.Clav[s] += toChe * (p[(int)Shoulders[s]] - p[(int)LocoBone.Spine2]);
                        m.Index[s] += toHand * (p[(int)Indexes[s]] - p[(int)Hands[s]]);
                        m.Middle[s] += toHand * (p[(int)Middles[s]] - p[(int)Hands[s]]);
                        m.Pinky[s] += toHand * (p[(int)Pinkies[s]] - p[(int)Hands[s]]);
                        m.ClavLen[s] += Vector3.Distance(p[(int)Arms[s]], p[(int)Shoulders[s]]);
                        m.UpperLen[s] += Vector3.Distance(p[(int)ForeArms[s]], p[(int)Arms[s]]);
                        m.ForeLen[s] += Vector3.Distance(p[(int)Hands[s]], p[(int)ForeArms[s]]);
                        m.ThighLen[s] += Vector3.Distance(p[(int)Legs[s]], p[(int)UpLegs[s]]);
                        m.ShinLen[s] += Vector3.Distance(p[(int)Feet[s]], p[(int)Legs[s]]);
                        m.FootLen[s] += Vector3.Distance(p[(int)ToeBases[s]], p[(int)Feet[s]]);
                        m.ToeLen[s] += Vector3.Distance(p[(int)ToeEnds[s]], p[(int)ToeBases[s]]);
                    }

                    n++;
                }
            }

            float k = 1f / Mathf.Max(1, n);
            m.Spine *= k; m.Spine1 *= k; m.Spine2 *= k; m.Neck *= k; m.Head *= k; m.HeadTop *= k;
            for (int s = 0; s < 2; s++)
            {
                m.Eye[s] *= k; m.Hip[s] *= k; m.Clav[s] *= k; m.Index[s] *= k; m.Middle[s] *= k; m.Pinky[s] *= k;
                m.ClavLen[s] *= k; m.UpperLen[s] *= k; m.ForeLen[s] *= k; m.ThighLen[s] *= k; m.ShinLen[s] *= k; m.FootLen[s] *= k; m.ToeLen[s] *= k;
            }

            return m;
        }

        /// <summary>A turn as (pitch, yaw, roll), the yaw the one nearest <paramref name="yawNear"/> (so it carries on round, never jumping).</summary>
        internal static Vector3 EulerOf(Quaternion q, float yawNear)
        {
            Vector3 f = q * Vector3.forward, r = q * Vector3.right, u = q * Vector3.up;
            float yaw = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Asin(Mathf.Clamp(f.y, -1f, 1f)) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(r.y, u.y) * Mathf.Rad2Deg;
            return new Vector3(pitch, yawNear + Mathf.DeltaAngle(yawNear, yaw), roll);
        }

        /// <summary>A body's points taken apart into its tracks (the yaws carried on from <paramref name="last"/>'s when there is one).</summary>
        public BodyTracks Decompose(Vector3[] p, BodyTracks? last = null)
        {
            Frames(p, out var pel, out var che, out var head, out var handL, out var handR);
            Quaternion toPel = Quaternion.Inverse(pel), toChe = Quaternion.Inverse(che);
            var t = new BodyTracks
            {
                Pelvis = p[(int)LocoBone.Hips],
                PelvisTurn = EulerOf(pel, last.HasValue ? last.Value.PelvisTurn.y : 0f),
                ChestTurn = EulerOf(toPel * che, last.HasValue ? last.Value.ChestTurn.y : 0f),
                HeadTurn = EulerOf(toChe * head, last.HasValue ? last.Value.HeadTurn.y : 0f),
                HandL = toChe * handL,
                HandR = toChe * handR,
            };
            for (int s = 0; s < 2; s++)
            {
                Vector3 clav = (toChe * (p[(int)Arms[s]] - p[(int)Shoulders[s]])).normalized;
                Vector3 upper = (toChe * (p[(int)ForeArms[s]] - p[(int)Arms[s]])).normalized;
                Vector3 fore = (toChe * (p[(int)Hands[s]] - p[(int)ForeArms[s]])).normalized;
                Vector3 hip = p[(int)UpLegs[s]], knee = p[(int)Legs[s]], ankle = p[(int)Feet[s]];
                Vector3 axis = (ankle - hip).normalized;
                Vector3 way = (knee - hip) - axis * Vector3.Dot(knee - hip, axis);
                Vector3 kneeWay = way.sqrMagnitude > 1e-10f ? (toPel * way).normalized : Vector3.forward;
                Vector3 foot = (p[(int)ToeBases[s]] - ankle).normalized, toe = (p[(int)ToeEnds[s]] - p[(int)ToeBases[s]]).normalized;
                if (s == 0)
                {
                    t.ClavL = clav; t.UpperL = upper; t.ForeL = fore; t.AnkleL = ankle; t.KneeL = kneeWay; t.FootL = foot; t.ToeL = toe;
                }
                else
                {
                    t.ClavR = clav; t.UpperR = upper; t.ForeR = fore; t.AnkleR = ankle; t.KneeR = kneeWay; t.FootR = foot; t.ToeR = toe;
                }
            }

            // (A hand's turn on the same side as the one before it: turned the short way between frames.)
            if (last.HasValue)
            {
                if (Quaternion.Dot(t.HandL, last.Value.HandL) < 0f) t.HandL = new Quaternion(-t.HandL.x, -t.HandL.y, -t.HandL.z, -t.HandL.w);
                if (Quaternion.Dot(t.HandR, last.Value.HandR) < 0f) t.HandR = new Quaternion(-t.HandR.x, -t.HandR.y, -t.HandR.z, -t.HandR.w);
            }

            return t;
        }

        /// <summary>The body's own spine: how it is bent on average in the clips it was measured on.</summary>
        public SpineShape OwnSpine => new SpineShape { Spine = Spine, Spine1 = Spine1, Spine2 = Spine2, Neck = Neck, Head = Head };

        /// <summary>How the spine is bent in one body's points.</summary>
        public static SpineShape SpineOf(Vector3[] p)
        {
            Frames(p, out var pel, out var che, out var head, out _, out _);
            return new SpineShape
            {
                Spine = Quaternion.Inverse(pel) * (p[(int)LocoBone.Spine] - p[(int)LocoBone.Hips]),
                Spine1 = Quaternion.Inverse(Quaternion.Slerp(pel, che, SpineShare1)) * (p[(int)LocoBone.Spine1] - p[(int)LocoBone.Spine]),
                Spine2 = Quaternion.Inverse(Quaternion.Slerp(pel, che, SpineShare2)) * (p[(int)LocoBone.Spine2] - p[(int)LocoBone.Spine1]),
                Neck = Quaternion.Inverse(che) * (p[(int)LocoBone.Neck] - p[(int)LocoBone.Spine2]),
                Head = Quaternion.Inverse(Quaternion.Slerp(che, head, 0.5f)) * (p[(int)LocoBone.Head] - p[(int)LocoBone.Neck]),
            };
        }

        /// <summary>A body's 33 points put together from its tracks (the same frame and units as the tracks).</summary>
        public void Compose(in BodyTracks t, Vector3[] p) => Compose(t, p, OwnSpine);

        /// <summary>The same with the spine bent as given (<see cref="SpineShape"/>), where the body's own is the clips' average.</summary>
        public void Compose(in BodyTracks t, Vector3[] p, in SpineShape spine)
        {
            Quaternion pel = Quaternion.Euler(t.PelvisTurn), che = pel * Quaternion.Euler(t.ChestTurn), head = che * Quaternion.Euler(t.HeadTurn);
            p[(int)LocoBone.Hips] = t.Pelvis;
            p[(int)LocoBone.Spine] = t.Pelvis + pel * spine.Spine;
            p[(int)LocoBone.Spine1] = p[(int)LocoBone.Spine] + Quaternion.Slerp(pel, che, SpineShare1) * spine.Spine1;
            p[(int)LocoBone.Spine2] = p[(int)LocoBone.Spine1] + Quaternion.Slerp(pel, che, SpineShare2) * spine.Spine2;
            p[(int)LocoBone.Neck] = p[(int)LocoBone.Spine2] + che * spine.Neck;
            p[(int)LocoBone.Head] = p[(int)LocoBone.Neck] + Quaternion.Slerp(che, head, 0.5f) * spine.Head;
            p[(int)LocoBone.HeadTop] = p[(int)LocoBone.Head] + head * HeadTop;
            for (int s = 0; s < 2; s++)
            {
                bool left = s == 0;
                p[(int)Eyes[s]] = p[(int)LocoBone.Head] + head * Eye[s];
                p[(int)Shoulders[s]] = p[(int)LocoBone.Spine2] + che * Clav[s];
                p[(int)Arms[s]] = p[(int)Shoulders[s]] + che * (left ? t.ClavL : t.ClavR).normalized * ClavLen[s];
                p[(int)ForeArms[s]] = p[(int)Arms[s]] + che * (left ? t.UpperL : t.UpperR).normalized * UpperLen[s];
                p[(int)Hands[s]] = p[(int)ForeArms[s]] + che * (left ? t.ForeL : t.ForeR).normalized * ForeLen[s];
                Quaternion hand = che * (left ? t.HandL : t.HandR);
                p[(int)Indexes[s]] = p[(int)Hands[s]] + hand * Index[s];
                p[(int)Middles[s]] = p[(int)Hands[s]] + hand * Middle[s];
                p[(int)Pinkies[s]] = p[(int)Hands[s]] + hand * Pinky[s];

                // The leg: from the hip to where the ankle is, the knee off that line the way it points.
                Vector3 hip = t.Pelvis + pel * Hip[s], ankle = left ? t.AnkleL : t.AnkleR;
                float a = ThighLen[s], b = ShinLen[s];
                Vector3 to = ankle - hip;
                float far = to.magnitude;
                Vector3 axis = far > 1e-6f ? to / far : Vector3.down;
                // (A leg asked longer than it is: the ankle comes in to where the leg reaches.)
                float reach = Mathf.Min(far, 0.999f * (a + b));
                ankle = hip + axis * reach;
                float along = (reach * reach + a * a - b * b) / (2f * Mathf.Max(1e-6f, reach));
                float off = Mathf.Sqrt(Mathf.Max(0f, a * a - along * along));
                Vector3 way = pel * (left ? t.KneeL : t.KneeR);
                way -= axis * Vector3.Dot(way, axis);
                way = way.sqrMagnitude > 1e-10f ? way.normalized : pel * Vector3.forward;
                p[(int)UpLegs[s]] = hip;
                p[(int)Legs[s]] = hip + axis * along + way * off;
                p[(int)Feet[s]] = ankle;
                p[(int)ToeBases[s]] = ankle + (left ? t.FootL : t.FootR).normalized * FootLen[s];
                p[(int)ToeEnds[s]] = p[(int)ToeBases[s]] + (left ? t.ToeL : t.ToeR).normalized * ToeLen[s];
            }
        }
    }

    /// <summary>
    /// How a fighter holds its sword, for the attack maker: the recorded hand's own axes (<see cref="BodyMeasures"/>),
    /// leg lengths. Taken from the fighter's own rig (<see cref="LocoBody.Hold"/>), so the blade the maker aims is the
    /// blade the pose will have.
    /// </summary>
    public struct SwordHold
    {
        /// <summary>How far the fighter's hand is from its shoulder for each leg length the recorded hand is (its arm against the recorded one).</summary>
        public float ArmScale;

        /// <summary>The grip from the wrist, and which way the blade points (a unit vector).</summary>
        public Vector3 Grip, Blade;

        /// <summary>How long the blade is, and how far along it from the grip it strikes.</summary>
        public float Length, Strike;

        /// <summary>The fighter's leg (metres): what a leg length is on this body.</summary>
        public float Leg;

        /// <summary>How far the wrist turns the blade toward its target as it lands (degrees): a cut's, a thrust's.</summary>
        public float MostSteer, MostThrustSteer;

        /// <summary>
        /// A hold from a rig's own measures (metres): where the weapon's slot is in the hand and which way the blade
        /// points from it (the rig hand's own axes; and the line the rig hand lies along at rest), the arm's length from shoulder to wrist, the weapon's length and
        /// the share of it at which it strikes; on a leg so long.
        /// </summary>
        public static SwordHold Of(BodyMeasures m, Vector3 restAlong, Vector3 slotInHand, Vector3 bladeInHand, float arm, float weaponLength, float sweetSpot, float leg, float mostSteer, float mostThrustSteer)
        {
            // (The rig turns its hand so its own line at rest lies along the recorded fingers, the knuckles across:
            // LocoBody's hand. That turn, in the recorded hand's axes, is the same at every frame.)
            Vector3 along = m.Middle[1], side = m.Index[1] - m.Pinky[1];
            restAlong = restAlong.sqrMagnitude > 1e-8f ? restAlong.normalized : Vector3.down;
            var toRig = Quaternion.LookRotation(along.normalized, Vector3.Cross(along, side).normalized) * Quaternion.Inverse(Quaternion.LookRotation(restAlong, Vector3.Cross(restAlong, Vector3.forward).normalized));
            leg = Mathf.Max(0.1f, leg);
            return new SwordHold
            {
                ArmScale = arm > 1e-3f ? arm / ((m.UpperLen[1] + m.ForeLen[1]) * leg) : 1f,
                Grip = toRig * slotInHand / leg,
                Blade = (toRig * bladeInHand).normalized,
                Length = weaponLength / leg,
                Strike = weaponLength * sweetSpot / leg,
                Leg = leg,
                MostSteer = mostSteer,
                MostThrustSteer = mostThrustSteer,
            };
        }

        /// <summary>A Medium's hold of a sword 0.9 m long (what the rig has, read off it on 14 November): for the checks that have no fighter.</summary>
        public static SwordHold Medium => Of(BodyMeasures.OfClips, new Vector3(0.023f, -0.082f, -0.003f), new Vector3(0.023f, -0.082f, -0.003f), new Vector3(0f, -0.34f, 0.94f), 0.533f, 0.9f, 0.7f, 0.845f, 30f, 110f);

    }

    /// <summary>
    /// A strike written as a score (the plan "Aimed attacks"): a recorded strike taken apart into each body part's own
    /// track, frame by frame (<see cref="BodyTracks"/>), with its marks: when the drive begins (the hips start to turn
    /// in and the blow is committed), when the blade lands, when the blow is carried through. An attack is made from
    /// a score (<see cref="AttackMaker"/>): each part's track is taken, changed by the style and the aim, and the body
    /// put together again. The motion of every part, the legs' steps and the pelvis's travel with it, is the
    /// recording's; what is made of it is the maker's.
    /// </summary>
    public sealed class AttackScore
    {
        public readonly string Name, Kind;
        public readonly LocoClip Clip;
        public readonly float Rate;
        public readonly BodyTracks[] Frames;

        /// <summary>The clip's own root track against where it began (leg lengths; x right, z forward) and its facing against the first (degrees), each frame; and each foot's contact.</summary>
        public readonly Vector3[] Root;
        public readonly float[] Facing;
        public readonly LocoContact[] Contacts;

        /// <summary>The marks (frames): the drive begins, the blade lands (the clip's own contact), the blow is carried through, a next blow may follow.</summary>
        public readonly int Drive, Contact, Follow, Link;

        /// <summary>The sword's striking part each frame as the recorded hand holds it (the same frame and units as the tracks), and how fast it goes (leg lengths a second).</summary>
        public readonly Vector3[] Strike;
        public readonly float[] StrikeSpeed;

        public float Duration => (Frames.Length - 1) / Rate;

        /// <summary>
        /// A plain blow, made with the feet on the ground: never both feet off it at once, the body not turned right
        /// round, and long enough to stand alone. The rest are the showy ones: leaps, spins, a combo's fragment.
        /// </summary>
        public readonly bool Plain;

        /// <summary>The furthest the hips turn from where they were in the guard, and the furthest the chest is twisted on the hips (degrees).</summary>
        public float MostHipTurn { get; private set; }
        public float MostTwist { get; private set; }

        /// <summary>It is a thrust: the point goes in, where a cut's edge goes across.</summary>
        public readonly bool Thrust;

        /// <summary>
        /// Its legs are a turn's (<see cref="OfTurn"/>, or a blow put on one): its feet go round him, they do not
        /// carry him toward his enemy. The aim does not lengthen or shorten its travel for the distance
        /// (<see cref="AttackMaker"/>): that multiplies how far each foot goes from where it stood, which for a lunge
        /// is a longer or shorter step and for a turn is a foot left part of the way round. (Seen in Unity on
        /// 17 November with the other straight behind him: the plan was short, the travel was multiplied, and his
        /// feet ended near where they began with his body turned right round: his legs crossed, or slid apart.)
        /// </summary>
        public readonly bool Turns;

        private static readonly Dictionary<string, AttackScore> scores = new Dictionary<string, AttackScore>();

        /// <summary>
        /// Its wind-up takes the sword behind him: before the drive the blade's striking part goes at least
        /// <see cref="BehindAtLeast"/> behind his sword shoulder (leg lengths, against the way he faces as the clip
        /// begins), and his sword hand never first goes out before his chest (further forward of the shoulder than
        /// <see cref="BeforeAtMost"/>, or across to his other side by more than <see cref="AcrossAtMost"/>). A thrust
        /// never is. What a fake may be cut from goes by this (<see cref="PostureMaker.FakesWindBehind"/>; Renier,
        /// 18 November: "Fakes only look right for the attacks where the arm pulls back to behind his back. Attacks
        /// with a wind up where the sword winds up by going in to position in front of his chest do not look right").
        /// </summary>
        public bool WindsBehind
        {
            get
            {
                MeasureWindUp();
                return !Thrust && BladeBehind >= BehindAtLeast && HandBefore <= BeforeAtMost && HandAcross <= AcrossAtMost;
            }
        }

        /// <summary>The wind-up, measured (leg lengths): how far behind the sword shoulder the blade's striking part goes up to a little after the drive begins; how far before the shoulder the sword hand goes before the drive; how far across to his other side.</summary>
        public float BladeBehind { get { MeasureWindUp(); return bladeBehind; } }
        public float HandBefore { get { MeasureWindUp(); return handBefore; } }
        public float HandAcross { get { MeasureWindUp(); return handAcross; } }

        public const float BehindAtLeast = 0.25f, BeforeAtMost = 0.22f, AcrossAtMost = 0.1f;

        private float bladeBehind = float.NaN, handBefore, handAcross;

        private void MeasureWindUp()
        {
            if (!float.IsNaN(bladeBehind))
            {
                return;
            }

            var m = BodyMeasures.OfClips;
            var hold = SwordHold.Medium;
            float behind = float.NegativeInfinity, before = float.NegativeInfinity, across = float.NegativeInfinity;
            int drive = Mathf.Clamp(Drive, 1, Frames.Length - 1), until = Mathf.Min(Frames.Length - 1, drive + Mathf.Max(1, (Contact - drive) / 3));
            for (int f = 0; f <= until; f++)
            {
                SwordOf(Frames[f], m, hold, Thrust, out Vector3 shoulder, out Vector3 wrist, out _, out _);
                behind = Mathf.Max(behind, shoulder.z - Strike[f].z);
                if (f <= drive)
                {
                    before = Mathf.Max(before, wrist.z - shoulder.z);
                    across = Mathf.Max(across, shoulder.x - wrist.x);
                }
            }

            handBefore = before;
            handAcross = across;
            bladeBehind = behind;
        }

        /// <summary>A strike clip's score (made once for each clip).</summary>
        public static AttackScore Of(LocoClip clip)
        {
            if (clip == null || clip.ContactFrame < 0)
            {
                return null;
            }

            if (!scores.TryGetValue(clip.Name, out var score))
            {
                scores[clip.Name] = score = new AttackScore(clip);
            }

            return score;
        }

        /// <summary>
        /// The sword for a body's tracks, as a fighter that holds it so has it (the same frame and units): its grip,
        /// which way its blade points, and the sword shoulder. A thrust's blade carries on the arm's line: the
        /// recorded thrusting hands don't say where a blade points (<see cref="LocoBody.MostThrustSteer"/>).
        /// </summary>
        public static void SwordOf(in BodyTracks t, BodyMeasures m, in SwordHold hold, bool thrust, out Vector3 shoulder, out Vector3 wrist, out Vector3 grip, out Vector3 way)
        {
            Quaternion pel = Quaternion.Euler(t.PelvisTurn), che = pel * Quaternion.Euler(t.ChestTurn);
            Vector3 spine2 = t.Pelvis + pel * m.Spine + Quaternion.Slerp(pel, che, BodyMeasures.SpineShare1) * m.Spine1 + Quaternion.Slerp(pel, che, BodyMeasures.SpineShare2) * m.Spine2;
            shoulder = spine2 + che * (m.Clav[1] + t.ClavR.normalized * m.ClavLen[1]);
            wrist = shoulder + che * (t.UpperR.normalized * m.UpperLen[1] + t.ForeR.normalized * m.ForeLen[1]) * hold.ArmScale;
            Quaternion hand = che * t.HandR;
            grip = wrist + hand * hold.Grip;
            way = thrust && (grip - shoulder).sqrMagnitude > 1e-8f ? (grip - shoulder).normalized : hand * hold.Blade;
        }

        /// <summary>The sword's striking part for a body's tracks (the same frame and units).</summary>
        public static Vector3 StrikeOf(in BodyTracks t, BodyMeasures m, in SwordHold hold, bool thrust = false)
        {
            SwordOf(t, m, hold, thrust, out _, out _, out Vector3 grip, out Vector3 way);
            return grip + way * hold.Strike;
        }

        private AttackScore(LocoClip clip)
        {
            Clip = clip;
            Name = clip.Name;
            Kind = clip.Kind;
            Thrust = Kind != null && Kind.StartsWith("thrust");
            Rate = clip.Rate;
            Contact = clip.ContactFrame;
            Link = clip.LinkFrom;
            var m = BodyMeasures.OfClips;
            int n = clip.Frames;
            Frames = new BodyTracks[n];
            Root = new Vector3[n];
            Facing = new float[n];
            Contacts = new LocoContact[n * 2];
            Strike = new Vector3[n];
            StrikeSpeed = new float[n];
            var p = new Vector3[LocoClip.BoneCount];
            var hold = SwordHold.Medium;
            clip.Root(0f, out Vector2 at0, out float yaw0);
            float facing = 0f;
            for (int f = 0; f < n; f++)
            {
                BodyMeasures.World(clip, f, p);
                Frames[f] = m.Decompose(p, f > 0 ? Frames[f - 1] : (BodyTracks?)null);
                clip.Root(f / Rate, out Vector2 at, out float yaw);
                Root[f] = Quaternion.Euler(0f, -yaw0, 0f) * new Vector3(at.x - at0.x, 0f, at.y - at0.y);
                facing += f > 0 ? Mathf.DeltaAngle(yaw0 + facing, yaw) : 0f;
                Facing[f] = facing;
                Contacts[f * 2] = clip.Contact(0, f / Rate);
                Contacts[f * 2 + 1] = clip.Contact(1, f / Rate);
                Strike[f] = StrikeOf(Frames[f], m, hold, Thrust);
            }

            Marks(out Drive, out Follow, out Plain);
            Pivot = PlantedFoot();
        }

        /// <summary>
        /// A cut with its carry-through ended short (a cut made while turning round, taken from a recording that
        /// spins right round): the recording as it is up to <paramref name="sweep"/> seconds after the blade lands
        /// (the rest of the sweep across his front, out to his side); that pose held until <paramref name="carry"/>
        /// seconds after the landing; and from there what is above the hips eased back, over
        /// <paramref name="back"/> seconds, to how it was held as the recording began. The recorded spinner's body goes on round under his arm after the cut, so
        /// against his chest the arm wraps behind him and the wrist rolls over; on a body that stops turning as the
        /// blade lands, that is an arm folded behind his back. (Made once for each.)
        /// </summary>
        public AttackScore EndedShort(float sweep, float carry, float back)
        {
            int swept = Mathf.Max(1, Mathf.RoundToInt(sweep * Rate)), carried = Mathf.Max(swept, Mathf.RoundToInt(carry * Rate)), eased = Mathf.Max(3, Mathf.RoundToInt(back * Rate));
            if (Contact + swept >= Frames.Length - 1)
            {
                return this;
            }

            string key = $"{Name}|short {swept} {carried} {eased}";
            if (!scores.TryGetValue(key, out var score))
            {
                scores[key] = score = new AttackScore(this, swept, carried, eased);
            }

            return score;
        }

        private AttackScore(AttackScore whole, int swept, int carried, int eased)
        {
            Clip = whole.Clip;
            Name = $"{whole.Name}_short{swept}";
            Kind = whole.Kind;
            Thrust = whole.Thrust;
            Turns = whole.Turns;
            Rate = whole.Rate;
            Contact = whole.Contact;
            var m = BodyMeasures.OfClips;
            var hold = SwordHold.Medium;
            int last = Contact + carried, n = last + eased + 1;
            Frames = new BodyTracks[n];
            Root = new Vector3[n];
            Facing = new float[n];
            Contacts = new LocoContact[n * 2];
            Strike = new Vector3[n];
            StrikeSpeed = new float[n];
            for (int f = 0; f < n; f++)
            {
                // (The recording as far as its sweep goes; then that pose held, the arm out at the end of its sweep.)
                int from = Mathf.Min(f, Contact + swept);
                var t = whole.Frames[from];
                if (f > last)
                {
                    // (What is above the hips, back to where it was held; the hips and legs are the recording's as the
                    // carry-through ended: a cut put on other legs takes those in any case.)
                    var held = BodyTracks.Lerp(t, whole.Frames[0], Mathf.SmoothStep(0f, 1f, (f - last) / (float)eased));
                    held.Pelvis = t.Pelvis; held.PelvisTurn = t.PelvisTurn;
                    held.AnkleL = t.AnkleL; held.AnkleR = t.AnkleR; held.KneeL = t.KneeL; held.KneeR = t.KneeR;
                    held.FootL = t.FootL; held.FootR = t.FootR; held.ToeL = t.ToeL; held.ToeR = t.ToeR;
                    t = held;
                }

                Frames[f] = t;
                Root[f] = whole.Root[from];
                Facing[f] = whole.Facing[from];
                Contacts[f * 2] = whole.Contacts[from * 2];
                Contacts[f * 2 + 1] = whole.Contacts[from * 2 + 1];
                Strike[f] = StrikeOf(Frames[f], m, hold, Thrust);
            }

            Link = n - 1;
            Marks(out _, out _, out _);
            Drive = whole.Drive;
            Follow = last;
            Plain = false;
            Pivot = PlantedFoot();
        }

        /// <summary>
        /// A turn on the spot as the legs of a blow (a cut made while turning round, 17 November): the part of a
        /// turn's clip between two of its moments, with the marks a blow's legs need. The drive begins when the turn
        /// is <paramref name="driveAt"/> of the way to the landing (the share of the cut's own time its wind-up
        /// takes: so the body turns evenly through the wind-up and the drive, and does not stand winding up and then
        /// whip round); the blade is to land when the turn is <paramref name="landsAt"/> of the way through (1: as it ends;
        /// over 1: that much of 0.3 s after it has ended); the rest is the way back to the stand. The turn itself is
        /// made <paramref name="turnScale"/> of the clip's own, so the legs go as far round as is asked; and after
        /// the blade has landed <paramref name="back"/> degrees more are turned (against the turn: given back), so a
        /// body the aim has turned a little past its enemy to bring the blade onto him comes back to face him. A
        /// blow's upper body is put on it with <see cref="OnLegsOf"/>. (Made once for each.)
        /// </summary>
        public static AttackScore OfTurn(LocoClip clip, float from, float to, float landsAt, float turnScale, float back = 0f, float driveAt = 0.06f)
        {
            if (clip == null)
            {
                return null;
            }

            back = Mathf.Round(back / 5f) * 5f;
            driveAt = Mathf.Round(driveAt * 20f) / 20f;
            string key = $"{clip.Name}|turn {from:0.00}-{to:0.00} lands {landsAt:0.00} x{turnScale:0.00} back {back:0} drive {driveAt:0.00}";
            if (!scores.TryGetValue(key, out var score))
            {
                scores[key] = score = new AttackScore(clip, from, to, landsAt, turnScale, back, driveAt);
            }

            return score;
        }

        private AttackScore(LocoClip clip, float from, float to, float landsAt, float turnScale, float back, float driveAt)
        {
            Clip = clip;
            Turns = true;
            Name = $"{clip.Name.Replace("quick_", "")}_x{Mathf.RoundToInt(100f * turnScale)}_lands{Mathf.RoundToInt(100f * landsAt)}{(back != 0f ? $"_back{back:0}" : "")}_d{Mathf.RoundToInt(100f * driveAt)}";
            Kind = "";
            Rate = clip.Rate;
            var m = BodyMeasures.OfClips;
            int first = Mathf.Clamp(Mathf.RoundToInt(from * Rate), 0, clip.Frames - 3), end = Mathf.Clamp(Mathf.RoundToInt(to * Rate), first + 2, clip.Frames - 1);
            int n = end - first + 1;
            Frames = new BodyTracks[n];
            Root = new Vector3[n];
            Facing = new float[n];
            Contacts = new LocoContact[n * 2];
            Strike = new Vector3[n];
            StrikeSpeed = new float[n];
            var p = new Vector3[LocoClip.BoneCount];
            var hold = SwordHold.Medium;
            clip.Root(first / Rate, out Vector2 at0, out float yaw0);
            float facing = 0f;
            var own = new float[n];
            for (int f = 0; f < n; f++)
            {
                clip.Root((first + f) / Rate, out Vector2 at, out float yaw);
                facing += f > 0 ? Mathf.DeltaAngle(yaw0 + facing, yaw) : 0f;
                own[f] = facing;
                Root[f] = Quaternion.Euler(0f, -yaw0, 0f) * new Vector3(at.x - at0.x, 0f, at.y - at0.y);
            }

            // The marks: the drive begins as the turn does; the blade lands so far through it; then the way back.
            float all = own[n - 1];
            int begins = 1, lands = n - 2, done = n - 2;
            for (int f = n - 1; f >= 1; f--)
            {
                float share = Mathf.Abs(all) > 1f ? own[f] / all : f / (n - 1f);
                if (share >= Mathf.Max(0.06f, driveAt * Mathf.Min(landsAt, 0.97f)))
                {
                    begins = f;
                }

                if (share >= Mathf.Min(landsAt, 0.97f))
                {
                    lands = f;
                }

                if (share >= 0.97f)
                {
                    done = f;
                }
            }

            if (landsAt > 1f)
            {
                lands = done + Mathf.RoundToInt((landsAt - 1f) * 0.3f * Rate);
            }

            Contact = Mathf.Clamp(lands, Mathf.Min(begins + 2, n - 2), n - 2);
            Link = n - 1;
            for (int f = 0; f < n; f++)
            {
                float time = (first + f) / Rate;
                clip.Pose(time, p, 1f);
                // (The frame the part began in; the turn as far as it is made to go, and what is given back of it
                // after the blade has landed: the body carried a little past by the cut comes back to face him.)
                Facing[f] = own[f] * turnScale + (f > Contact ? back * Mathf.SmoothStep(0f, 1f, (f - Contact) / (float)(n - 1 - Contact)) : 0f);
                var turn = Quaternion.Euler(0f, Facing[f], 0f);
                for (int i = 0; i < p.Length; i++)
                {
                    float y = p[i].y;
                    Vector3 w = turn * new Vector3(p[i].x, 0f, p[i].z) + Root[f];
                    p[i] = new Vector3(w.x, y, w.z);
                }

                Frames[f] = m.Decompose(p, f > 0 ? Frames[f - 1] : (BodyTracks?)null);
                Contacts[f * 2] = clip.Contact(0, time);
                Contacts[f * 2 + 1] = clip.Contact(1, time);
                Strike[f] = StrikeOf(Frames[f], m, hold, false);
            }

            Marks(out _, out _, out _);
            Drive = Mathf.Clamp(begins, 1, Contact - 1);
            Follow = Mathf.Clamp((Contact + n - 1) / 2, Contact + 1, n - 1);
            Plain = false;
            Pivot = PlantedFoot();
        }

        /// <summary>
        /// Where the foot the body turns about stands as the blow begins (the frame the clip began in, on the ground):
        /// the foot that is down for most of the wind-up; of two as much down, the one behind. A body turned to bring
        /// its blow onto a line turns about this foot (<see cref="AttackMaker"/>), so it stays where it is planted.
        /// </summary>
        public readonly Vector3 Pivot;

        private Vector3 PlantedFoot()
        {
            int left = 0, right = 0;
            for (int f = 0; f <= Drive && f < Frames.Length; f++)
            {
                left += (Contacts[f * 2] & LocoContact.Down) != 0 ? 1 : 0;
                right += (Contacts[f * 2 + 1] & LocoContact.Down) != 0 ? 1 : 0;
            }

            Vector3 l = Frames[0].AnkleL, r = Frames[0].AnkleR;
            Vector3 foot = left > right + 1 ? l : right > left + 1 ? r : l.z < r.z ? l : r;
            return new Vector3(foot.x, 0f, foot.z);
        }

        /// <summary>The marks read off the blade's speed and the feet: when the drive begins, when the blow is carried through, and whether it is a plain blow.</summary>
        private void Marks(out int driveAt, out int followAt, out bool plain)
        {
            int n = Frames.Length;
            float fastest = 0f;
            MostHipTurn = MostTwist = 0f;
            for (int f = 0; f < n; f++)
            {
                MostHipTurn = Mathf.Max(MostHipTurn, Mathf.Abs(Frames[f].PelvisTurn.y - Frames[0].PelvisTurn.y));
                MostTwist = Mathf.Max(MostTwist, Mathf.Abs(Frames[f].ChestTurn.y));
            }

            for (int f = 0; f < n; f++)
            {
                int a = Mathf.Max(0, f - 1), b = Mathf.Min(n - 1, f + 1);
                StrikeSpeed[f] = Vector3.Distance(Strike[b], Strike[a]) * Rate / Mathf.Max(1, b - a);
                fastest = Mathf.Max(fastest, StrikeSpeed[f]);
            }

            // The drive begins where the blade last was slow before it lands (under a fifth of its fastest), and no
            // nearer the landing than a tenth of a second nor further than half a second.
            int least = Mathf.Max(1, Contact - Mathf.RoundToInt(0.5f * Rate)), most = Mathf.Max(1, Contact - Mathf.RoundToInt(0.1f * Rate));
            driveAt = least;
            for (int f = most; f >= least; f--)
            {
                if (StrikeSpeed[f] < 0.2f * fastest)
                {
                    driveAt = f;
                    break;
                }
            }

            bool inTheAir = false;
            for (int f = 0; f < n; f++)
            {
                inTheAir |= (Contacts[f * 2] & LocoContact.Down) == 0 && (Contacts[f * 2 + 1] & LocoContact.Down) == 0;
            }

            plain = !inTheAir && Mathf.Abs(Facing[n - 1]) < 100f && Mathf.Abs(Frames[Contact].PelvisTurn.y - Frames[0].PelvisTurn.y) < 150f && n / Rate >= 0.6f;

            // Carried through: the blade slow again after it has landed.
            followAt = n - 1;
            for (int f = Contact + 1; f < n; f++)
            {
                if (StrikeSpeed[f] < 0.2f * fastest)
                {
                    followAt = f;
                    break;
                }
            }
        }

        /// <summary>
        /// One recording's blow on another's legs (<see cref="OnLegsOf"/>): the chest, the head and the arms of
        /// <paramref name="upper"/>, as they sit on the hips; the hips' place and turn, the legs, the feet and the
        /// travel over the ground of <paramref name="legs"/>, each phase of them (the wind-up, the drive, the
        /// carry-through, the way back) stretched to the blow's own.
        /// </summary>
        private AttackScore(AttackScore upper, AttackScore legs)
        {
            Clip = upper.Clip;
            Name = upper.Name + "_on_" + legs.Name.Replace("sns_", "");
            Kind = upper.Kind;
            Thrust = upper.Thrust;
            Turns = legs.Turns;
            Rate = upper.Rate;
            Contact = upper.Contact;
            Link = upper.Link;
            Legs = legs;
            Upper = upper;
            var m = BodyMeasures.OfClips;
            var hold = SwordHold.Medium;
            int n = upper.Frames.Length;
            // (The way back, from the blade slow again to the end: the legs go back at their own pace where the blow's
            // own way back is the shorter. A cut's legs step back over 0.6 s; squeezed into a thrust's 0.1 s they
            // threw the body back at 6 m/s. The blow's own parts hold their last pose, its guard, while the legs finish.)
            int legsBack = legs.Frames.Length - 1 - legs.Follow, upperBack = n - 1 - upper.Follow;
            bool ownPace = LegsGoBackAtTheirPace && legsBack > upperBack;
            if (ownPace)
            {
                n = upper.Follow + legsBack + 1;
            }

            Frames = new BodyTracks[n];
            Root = new Vector3[n];
            Facing = new float[n];
            Contacts = new LocoContact[n * 2];
            Strike = new Vector3[n];
            StrikeSpeed = new float[n];
            for (int f = 0; f < n; f++)
            {
                // The moment of the legs' recording this frame is: the same share of the way through the same phase.
                float g = f <= upper.Drive ? Share(f, 0, upper.Drive, 0, legs.Drive)
                    : f <= upper.Contact ? Share(f, upper.Drive, upper.Contact, legs.Drive, legs.Contact)
                    : f <= upper.Follow ? Share(f, upper.Contact, upper.Follow, legs.Contact, legs.Follow)
                    : ownPace ? legs.Follow + (f - upper.Follow)
                    : Share(f, upper.Follow, n - 1, legs.Follow, legs.Frames.Length - 1);
                int a = Mathf.Clamp(Mathf.FloorToInt(g), 0, legs.Frames.Length - 2);
                float share = Mathf.Clamp01(g - a);
                var under = BodyTracks.Lerp(legs.Frames[a], legs.Frames[a + 1], share);
                var t = upper.Frames[Mathf.Min(f, upper.Frames.Length - 1)];
                t.Pelvis = under.Pelvis; t.PelvisTurn = under.PelvisTurn;
                t.AnkleL = under.AnkleL; t.AnkleR = under.AnkleR; t.KneeL = under.KneeL; t.KneeR = under.KneeR;
                t.FootL = under.FootL; t.FootR = under.FootR; t.ToeL = under.ToeL; t.ToeR = under.ToeR;
                // (The chest's twist on the hips is the blow's own: the spine is bent no further than it was recorded. Where
                // these hips turn more or less than the blow's did, the blow goes that much round, and the aim allows for it.)
                Frames[f] = t;
                Root[f] = Vector3.Lerp(legs.Root[a], legs.Root[a + 1], share);
                Facing[f] = Mathf.Lerp(legs.Facing[a], legs.Facing[a + 1], share);
                int near = Mathf.Clamp(Mathf.RoundToInt(g), 0, legs.Frames.Length - 1);
                Contacts[f * 2] = legs.Contacts[near * 2];
                Contacts[f * 2 + 1] = legs.Contacts[near * 2 + 1];
                Strike[f] = StrikeOf(Frames[f], m, hold, Thrust);
            }

            Marks(out _, out _, out Plain);
            // (The blow's own marks: its arm's.)
            Drive = upper.Drive;
            Follow = upper.Follow;
            Plain &= upper.Plain && legs.Plain;
            Pivot = PlantedFoot();
        }

        private static float Share(int f, int from, int to, int legsFrom, int legsTo) => to <= from ? legsTo : Mathf.Lerp(legsFrom, legsTo, (f - from) / (float)(to - from));

        /// <summary>
        /// On another recording's legs, the way back to the guard goes at the legs' own pace where the blow's own is
        /// the shorter (the scene's Combat Feel, "Borrowed Legs Go Back At Their Pace"). Off: squeezed into the blow's
        /// own, as before.
        /// </summary>
        public static bool LegsGoBackAtTheirPace = true;

        /// <summary>The recordings a grafted score was made of (null for a recording's own).</summary>
        public readonly AttackScore Upper, Legs;

        /// <summary>
        /// This blow on another recording's legs: its chest, head and arms; the other's hips, legs, steps and travel,
        /// each phase stretched to this blow's. A thrust recorded standing gets a cut's lunge under it; a cut recorded
        /// lunging can be made standing. (Made once for each pair.)
        /// </summary>
        /// <summary>
        /// This blow entered in the middle of its wind-up, out of the carry-through of the blow before it (the combos
        /// plan's step 2, the join; 20 November): over <paramref name="bend"/> frames the body goes from how the blow
        /// before had it (from its frame <paramref name="leave"/> on) to how this one has it (from half the bend
        /// before its frame <paramref name="enter"/> to half the bend after it), both recordings playing on at their
        /// own pace meanwhile, and from there this blow goes on as it was recorded. So the first frame is the blow before, as
        /// it was being carried through, and the blade is never stopped. The travel over the ground is this blow's.
        /// Where the one is left and the other entered is <see cref="BladeFlow.Join(BladeFlow, BladeFlow, float)"/>'s
        /// to say. (Made once for each.)
        /// </summary>
        /// <summary>
        /// At a join both blows are put into the joined score's own frame (where the blow before has him as it is
        /// left), this one laid on so that his hips carry on; and what is left of the wind-up is no shorter than the
        /// bend (Combat Feel "At The Join His Body Carries On"; <c>FIGHT_JOINTURNS=0</c> for the first build of 20
        /// November, 22:08, in which his body jumped at the join).
        /// </summary>
        public static bool JoinTurnsRoot = true, JoinWindsOverBend = true;

        /// <summary>For the reports: the last join put together, in numbers.</summary>
        public static string JoinNote = "";

        /// <param name="overWalk">For a blow laid over his walk: the joined score is in the frame the blow before began in, not where its root is as it is left. Over a walk the root is the walk's (he faces his man as he goes), and a blow is laid on it by how it differs from the guard the first blow began in.</param>
        public AttackScore EnteredFrom(AttackScore before, int leave, int enter, int bend, bool overWalk = false)
        {
            if (before == null || Frames.Length < 6 || before.Frames.Length < 3)
            {
                return this;
            }

            string key = Name + "|from|" + before.Name + "|" + leave + "|" + enter + "|" + bend + (JoinTurnsRoot ? "|t" : "") + (JoinWindsOverBend ? "|w" : "") + (overWalk ? "|walk" : "");
            if (!scores.TryGetValue(key, out var score))
            {
                scores[key] = score = new AttackScore(before, this, leave, enter, bend, overWalk);
            }

            return score;
        }

        private AttackScore(AttackScore before, AttackScore next, int leave, int enter, int bend, bool overWalk)
        {
            Clip = next.Clip;
            Name = next.Name + "_out_of_" + before.Name.Replace("sns_", "");
            Kind = next.Kind;
            Thrust = next.Thrust;
            Turns = next.Turns;
            Rate = next.Rate;
            var m = BodyMeasures.OfClips;
            var hold = SwordHold.Medium;
            // (Entered no later than two frames before it lands, and left no later than the blow before lasts.)
            enter = Mathf.Clamp(enter, 0, next.Contact - 2);
            bend = Mathf.Clamp(bend, 2, 2 * enter);
            int half = bend / 2, first = enter - half, n = next.Frames.Length - first;
            Frames = new BodyTracks[n];
            Root = new Vector3[n];
            Facing = new float[n];
            Contacts = new LocoContact[n * 2];
            Strike = new Vector3[n];
            StrikeSpeed = new float[n];
            // (The blow before, played on at its own pace: its frames a second against this one's.)
            float pace = before.Rate / Mathf.Max(1f, next.Rate);
            // A score's tracks are in the frame the fighter faced as its clip began, his travel and his turn since then
            // in them. This score's own frame is where the blow before has him as it is left: its root at that
            // frame. The first build (20 November, 22:08) took each recording's tracks as they were, as if they were
            // against its root: the blow before came in turned 68 degrees from where he stood and this one 79, and
            // the maker, fitting what it took for the blow to its aim, turned him 43 to 50 degrees, dropped his hips
            // 17 cm and leaned him 28 degrees, all it may: Renier's "the body itself seems to jump to a different
            // pose between the 2 hits".
            leave = Mathf.Clamp(leave, 0, before.Frames.Length - 1);
            // (Off, for comparing: each recording's tracks as they are, the first build's way.)
            Vector3 beforeAt = JoinTurnsRoot && !overWalk ? before.Root[leave] : Vector3.zero;
            Quaternion beforeTurn = JoinTurnsRoot && !overWalk ? Quaternion.Euler(0f, -before.Facing[leave], 0f) : Quaternion.identity;
            var points = new Vector3[LocoClip.BoneCount];
            BodyTracks Before(int f, out Vector3 root, out float facing, BodyTracks? last)
            {
                float g = Mathf.Clamp(leave + f * pace, 0f, before.Frames.Length - 1.001f);
                int a0 = Mathf.Min(Mathf.FloorToInt(g), before.Frames.Length - 2);
                m.Compose(BodyTracks.Lerp(before.Frames[a0], before.Frames[a0 + 1], g - a0), points);
                for (int i = 0; i < points.Length; i++)
                {
                    float y = points[i].y;
                    Vector3 q = beforeTurn * new Vector3(points[i].x - beforeAt.x, 0f, points[i].z - beforeAt.z);
                    points[i] = new Vector3(q.x, y, q.z);
                }

                root = beforeTurn * (Vector3.Lerp(before.Root[a0], before.Root[a0 + 1], g - a0) - beforeAt);
                facing = Mathf.LerpAngle(before.Facing[a0], before.Facing[a0 + 1], g - a0) - (JoinTurnsRoot && !overWalk ? before.Facing[leave] : 0f);
                return m.Decompose(points, last);
            }

            // This blow is laid onto that frame so that his hips, where the bend ends, are where the blow before has
            // them then and turned as it has them: against its own root at the frame it is taken from, turned by
            // `turned` and moved by `moved`.
            Quaternion nextTurn = JoinTurnsRoot ? Quaternion.Euler(0f, -next.Facing[first], 0f) : Quaternion.identity;
            Vector3 nextAt = JoinTurnsRoot ? next.Root[first] : Vector3.zero;
            var hipsBefore = Before(bend, out _, out _, null);
            var hipsNext = next.Frames[Mathf.Min(first + bend, next.Frames.Length - 1)];
            float turned = JoinTurnsRoot ? Mathf.DeltaAngle(hipsNext.PelvisTurn.y - next.Facing[first], hipsBefore.PelvisTurn.y) : 0f;
            Quaternion lay = Quaternion.Euler(0f, turned, 0f) * nextTurn;
            Vector3 hipsLaid = lay * new Vector3(hipsNext.Pelvis.x - nextAt.x, 0f, hipsNext.Pelvis.z - nextAt.z);
            Vector3 moved = JoinTurnsRoot ? new Vector3(hipsBefore.Pelvis.x - hipsLaid.x, 0f, hipsBefore.Pelvis.z - hipsLaid.z) : Vector3.zero;
            JoinNote = string.Format(System.Globalization.CultureInfo.InvariantCulture, "join: {0} left at frame {1} (its root turned {2:0} degrees by then, landing frame {3}); {4} entered at frame {5} (taken from {6}; its root turned {7:0} by then, landing frame {8}); laid on turned {9:0} degrees and moved {10:0} cm",
                before.Name, leave, before.Facing[leave], before.Contact, next.Name, enter, first, next.Facing[first], next.Contact, turned, 100f * moved.magnitude * 0.845f);
            BodyTracks? lastBefore = null, lastNext = null;
            for (int f = 0; f < n; f++)
            {
                m.Compose(next.Frames[first + f], points);
                for (int i = 0; i < points.Length; i++)
                {
                    float y = points[i].y;
                    Vector3 q = lay * new Vector3(points[i].x - nextAt.x, 0f, points[i].z - nextAt.z) + moved;
                    points[i] = new Vector3(q.x, y, q.z);
                }

                var t = m.Decompose(points, lastNext);
                lastNext = t;
                Vector3 root = JoinTurnsRoot ? lay * (next.Root[first + f] - nextAt) + moved : Quaternion.Euler(0f, -next.Facing[first], 0f) * (next.Root[first + f] - next.Root[first]);
                float facing = next.Facing[first + f] - next.Facing[first] + turned;
                if (f < bend)
                {
                    float w = f / (float)bend;
                    w = w * w * (3f - 2f * w);
                    var was = Before(f, out Vector3 rootWas, out float facingWas, lastBefore);
                    lastBefore = was;
                    t = BodyTracks.Lerp(was, t, w);
                    if (JoinTurnsRoot)
                    {
                        root = Vector3.Lerp(rootWas, root, w);
                        facing = Mathf.LerpAngle(facingWas, facing, w);
                    }
                }

                Frames[f] = t;
                Root[f] = root;
                Facing[f] = f > 0 ? Facing[f - 1] + Mathf.DeltaAngle(Facing[f - 1], facing) : facing;
                Contacts[f * 2] = next.Contacts[(first + f) * 2];
                Contacts[f * 2 + 1] = next.Contacts[(first + f) * 2 + 1];
                Strike[f] = StrikeOf(Frames[f], m, hold, Thrust);
            }

            Contact = next.Contact - first;
            Link = Mathf.Max(0, next.Link - first);
            Marks(out _, out _, out _);
            Plain = next.Plain;
            // (Its own marks: what is left of its wind-up, and its carry-through.)
            // (No shorter than the bend: what the maker does to fit the blow to its aim (the body's turn, the hips'
            // drop, the lean) comes in over the wind-up, and over one frame that is a jump.)
            Drive = Mathf.Clamp(Mathf.Max(JoinWindsOverBend ? bend : 1, next.Drive - first), 1, Contact - 1);
            Follow = Mathf.Clamp(next.Follow - first, Contact + 1, n - 1);
            Pivot = PlantedFoot();
        }

        public AttackScore OnLegsOf(AttackScore legs)
        {
            if (legs == null || legs == this || legs.Legs != null || Legs != null)
            {
                return this;
            }

            string key = Name + "|" + legs.Name + (LegsGoBackAtTheirPace ? "" : "|squeezed");
            if (!scores.TryGetValue(key, out var score))
            {
                scores[key] = score = new AttackScore(this, legs);
            }

            return score;
        }
    }
}
