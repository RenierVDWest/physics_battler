using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// What a fighter leaves open (the plan "Fight Rhythm from Recordings", "Openings are measured, not
    /// hand-marked"): a part of his body is open to someone when a straight line from that one's sword shoulder
    /// reaches the part without first meeting his shield or another part of him (an arm before his chest). It is
    /// looked at on the bodies as they are, so whatever he does, a posture, a step, a blow, shows in it; nobody
    /// types in what a posture opens.
    /// </summary>
    public static class Openings
    {
        /// <summary>One spot on his body as someone sees it: open, or what is before it.</summary>
        public struct Seen
        {
            public AimSpot Spot;
            public bool Open;

            /// <summary>What covers it ("its shield", or the part of him that is in the way); nothing when it is open.</summary>
            public string Behind;
        }

        /// <summary>How far before a spot the line to it is looked along (metres at a Medium's size: his own cover is no further from him than his arm reaches), how far apart it is looked at, and how thick the line is.</summary>
        public const float LooksFrom = 0.9f, Every = 0.03f, Thick = 0.02f;

        /// <summary>Every spot of <paramref name="of"/> as seen from a place (world): which are open, and what covers the others.</summary>
        public static void Look(ClipFighter of, Vector3 from, List<Seen> into)
        {
            into.Clear();
            foreach (var spot in AimSpots.All)
            {
                Vector3 at = AimSpots.Point(of, spot);
                Vector3 way = at - from;
                float far = way.magnitude;
                var seen = new Seen { Spot = spot, Open = true, Behind = "" };
                if (far > 1e-3f)
                {
                    way /= far;
                    float begins = Mathf.Max(0f, far - LooksFrom * of.Size);
                    bool done = false;
                    for (float d = begins; d <= far + 0.5f * Every && !done; d += Every * of.Size)
                    {
                        PartId? reached = null, before = null;
                        bool shield = false;
                        foreach (var collider in Physics.OverlapSphere(from + way * Mathf.Min(d, far), Thick * of.Size, ~0, QueryTriggerInteraction.Collide))
                        {
                            if (of.ShieldColliders.Contains(collider))
                            {
                                shield = true;
                            }
                            else if (of.PartOf(collider, out var id))
                            {
                                if (AimSpots.Counts(spot, id) || id == AimSpots.PartOf(spot))
                                {
                                    reached = id;
                                }
                                else
                                {
                                    before = id;
                                }
                            }
                        }

                        if (reached.HasValue)
                        {
                            // (The spot's own part reached: open, whatever else is touching it there.)
                            done = true;
                        }
                        else if (shield || before.HasValue)
                        {
                            seen.Open = false;
                            seen.Behind = shield ? "its shield" : "its " + before.Value;
                            done = true;
                        }
                    }
                }

                into.Add(seen);
            }
        }
    }
}
