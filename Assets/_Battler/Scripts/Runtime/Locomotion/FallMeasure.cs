using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// What a fall did to a body, measured from the step it went down to the step it began to get up (or, for one
    /// that stays down, until it had lain a while): the plan "Controlled falls", step 1. A person going down keeps
    /// the head off the ground and lands on the hands and knees before the trunk; these are the numbers that say
    /// whether a fighter does. Nothing here changes a fall: it is read off the body's parts as they are.
    /// </summary>
    public class FallMeasure
    {
        /// <summary>Which way it went, against the way it faced: backward, forward, to its left, to its right.</summary>
        public string Way = "";

        /// <summary>Why it went down, as the fight said it.</summary>
        public string Why = "";

        /// <summary>It was killed (a body that does not protect itself is no fault in the dead).</summary>
        public bool Dead;

        /// <summary>How long it was measured for (seconds).</summary>
        public float Lasted;

        /// <summary>The first part to reach the ground after it went down, and when (seconds after; -1: none did).</summary>
        public string First = "";
        public float FirstAt = -1f;

        /// <summary>When a hand or forearm, a knee, the hips, the spine or chest, and the head first reached the ground (seconds after it went down; -1: never).</summary>
        public float HandAt = -1f, KneeAt = -1f, HipsAt = -1f, TrunkAt = -1f, HeadAt = -1f;

        /// <summary>How fast the head and the hips were going when they reached the ground (m/s; 0: they did not).</summary>
        public float HeadSpeed, HipsSpeed;

        /// <summary>How fast the head was coming down when it reached the ground (m/s; a head that rolls onto the ground goes fast along it and comes down slowly, one that is slammed comes down fast).</summary>
        public float HeadDownSpeed;

        private float headWas = float.MaxValue;

        /// <summary>The lowest the head came (its lowest point over the ground, metres).</summary>
        public float HeadLowest = float.MaxValue;

        /// <summary>How far the hips went over the ground after they reached it (metres).</summary>
        public float Slid;

        /// <summary>How far its hands went over the ground while they were on it (metres, both together), and how many times a hand that had reached the ground left it again.</summary>
        public float HandsSlid;
        public int HandsLifted;

        /// <summary>A hand that has reached the ground and is this far over it again (metres at a Medium's size) has left it.</summary>
        public const float HandLeftAt = 0.1f;

        /// <summary>How far the hips turned over in all, whichever way (degrees): a body that tumbles turns a long way.</summary>
        public float Turned;

        private Quaternion hipsTurnWas;

        private readonly bool[] handDown = new bool[2];
        private readonly Vector3[] handWas = new Vector3[2];

        /// <summary>A hand, a forearm or a knee was down before the hips, the trunk or the head: it landed on what can take it.</summary>
        public bool Braced
        {
            get
            {
                float limb = HandAt >= 0f && KneeAt >= 0f ? Mathf.Min(HandAt, KneeAt) : Mathf.Max(HandAt, KneeAt);
                float body = float.MaxValue;
                foreach (float at in new[] { HipsAt, TrunkAt, HeadAt })
                {
                    if (at >= 0f)
                    {
                        body = Mathf.Min(body, at);
                    }
                }

                return limb >= 0f && limb < body;
            }
        }

        /// <summary>The head reached the ground.</summary>
        public bool HeadTouched => HeadAt >= 0f;

        /// <summary>A part this near the ground has reached it (its lowest point, metres at a Medium's size).</summary>
        public const float TouchWithin = 0.03f;

        /// <summary>A knee this near the ground (the joint itself, metres at a Medium's size) is down.</summary>
        public const float KneeWithin = 0.12f;

        private Vector3 hipsWas;
        private bool begun;

        /// <summary>The way a body thrown along <paramref name="way"/> goes, for one facing <paramref name="facing"/> (degrees about up).</summary>
        public static string WayOf(Vector3 way, float facing)
        {
            way.y = 0f;
            if (way.sqrMagnitude < 1e-6f)
            {
                return "down where it stood";
            }

            way.Normalize();
            Vector3 ahead = Quaternion.Euler(0f, facing, 0f) * Vector3.forward;
            float along = Vector3.Dot(way, ahead);
            if (along > 0.707f)
            {
                return "forward";
            }

            if (along < -0.707f)
            {
                return "backward";
            }

            return Vector3.Dot(way, Quaternion.Euler(0f, facing, 0f) * Vector3.right) > 0f ? "to its right" : "to its left";
        }

        /// <summary>A step of the fall: where the fallen body's parts are against the ground at <paramref name="ground"/> (its height).</summary>
        /// <param name="speedOf">How fast a part is going (m/s), where the rig's own bodies do not say (a body that falls as one physical body); null: the rig's.</param>
        public void Step(UnitRig rig, float ground, float size, float dt, System.Func<PartId, float> speedOf = null)
        {
            Lasted += dt;
            float within = TouchWithin * size;
            float hand = Mathf.Min(Mathf.Min(Low(rig, PartId.HandL, ground), Low(rig, PartId.HandR, ground)), Mathf.Min(Low(rig, PartId.ForearmL, ground), Low(rig, PartId.ForearmR, ground)));
            float knee = Mathf.Min(Joint(rig, PartId.ShinL, ground), Joint(rig, PartId.ShinR, ground)) - (KneeWithin - TouchWithin) * size;
            float hips = Low(rig, PartId.Pelvis, ground);
            float trunk = Mathf.Min(Low(rig, PartId.Spine, ground), Low(rig, PartId.Chest, ground));
            float head = Low(rig, PartId.Head, ground);
            HeadLowest = Mathf.Min(HeadLowest, head);
            Reach(ref HandAt, hand, within, "a hand");
            Reach(ref KneeAt, knee, within, "a knee");
            if (Reach(ref HipsAt, hips, within, "the hips"))
            {
                HipsSpeed = speedOf != null ? speedOf(PartId.Pelvis) : Speed(rig, PartId.Pelvis);
            }

            Reach(ref TrunkAt, trunk, within, "the trunk");
            if (Reach(ref HeadAt, head, within, "the head"))
            {
                HeadSpeed = speedOf != null ? speedOf(PartId.Head) : Speed(rig, PartId.Head);
                HeadDownSpeed = headWas < float.MaxValue && dt > 0f ? Mathf.Max(0f, (headWas - head) / dt) : 0f;
            }

            headWas = head;

            // (Each hand: how far it goes over the ground while it is on it, and whether it leaves it again.)
            for (int h = 0; h < 2; h++)
            {
                var id = h == 0 ? PartId.HandL : PartId.HandR;
                var part = rig.Part(id);
                if (part == null || rig.IsSevered(id))
                {
                    continue;
                }

                float over = Low(rig, id, ground);
                Vector3 where = part.transform.position;
                where.y = 0f;
                if (over <= within)
                {
                    if (handDown[h])
                    {
                        HandsSlid += Vector3.Distance(where, handWas[h]);
                    }

                    handDown[h] = true;
                    handWas[h] = where;
                }
                else if (handDown[h] && over > HandLeftAt * size)
                {
                    handDown[h] = false;
                    HandsLifted++;
                }
                else if (handDown[h])
                {
                    HandsSlid += Vector3.Distance(where, handWas[h]);
                    handWas[h] = where;
                }
            }

            var pelvis = rig.Part(PartId.Pelvis);
            if (pelvis != null)
            {
                if (begun)
                {
                    Turned += Quaternion.Angle(hipsTurnWas, pelvis.transform.rotation);
                }

                hipsTurnWas = pelvis.transform.rotation;
                Vector3 at = pelvis.transform.position;
                at.y = 0f;
                if (begun && HipsAt >= 0f)
                {
                    Slid += Vector3.Distance(at, hipsWas);
                }

                hipsWas = at;
                begun = true;
            }
        }

        private bool Reach(ref float at, float height, float within, string what)
        {
            if (at >= 0f || height > within)
            {
                return false;
            }

            at = Lasted;
            if (FirstAt < 0f)
            {
                FirstAt = Lasted;
                First = what;
            }

            return true;
        }

        /// <summary>How high a part's lowest point is over the ground (metres; far up if it is not there).</summary>
        private static float Low(UnitRig rig, PartId id, float ground)
        {
            var part = rig.Part(id);
            if (part == null || part.partCollider == null || rig.IsSevered(id))
            {
                return float.MaxValue;
            }

            // (The point of it nearest a point well under it: its underside.)
            Vector3 centre = part.transform.position;
            Vector3 under = part.partCollider.ClosestPoint(new Vector3(centre.x, ground - 1f, centre.z));
            return under.y - ground;
        }

        /// <summary>How high a part's own joint is over the ground (a shin's is its knee).</summary>
        private static float Joint(UnitRig rig, PartId id, float ground)
        {
            var part = rig.Part(id);
            return part == null || rig.IsSevered(id) ? float.MaxValue : part.transform.position.y - ground;
        }

        private static float Speed(UnitRig rig, PartId id)
        {
            var part = rig.Part(id);
            return part != null && part.body != null ? part.body.linearVelocity.magnitude : 0f;
        }

        private static string When(float at) => at < 0f ? "never" : $"{at:0.00} s";

        /// <summary>The fall in a line, for the logs.</summary>
        public string Text =>
            $"fell {Way}{(Dead ? ", killed" : "")}: first down {(FirstAt < 0f ? "nothing" : $"{First} at {FirstAt:0.00} s")}; a hand {When(HandAt)}, a knee {When(KneeAt)}, the hips {When(HipsAt)}{(HipsAt >= 0f ? $" at {HipsSpeed:0.0} m/s" : "")}, the trunk {When(TrunkAt)}, the head {(HeadAt < 0f ? $"never (lowest {100f * HeadLowest:0} cm up)" : $"{HeadAt:0.00} s at {HeadSpeed:0.0} m/s ({HeadDownSpeed:0.0} downward)")}; {(Braced ? "landed on a hand or knee first" : "not on a hand or knee first")}; slid {100f * Slid:0} cm; its hips turned over {Turned:0} degrees in all; its hands went {100f * HandsSlid:0} cm over the ground and left it again {HandsLifted} times; measured {Lasted:0.0} s ({Why})";
    }
}
