using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The momentum ledger for two bodies that run into each other (the plan "Impacts that carry weight", step 3). The
    /// speeds the two leave with are worked out, not written in: what goes in comes out. In: both weights and both
    /// speeds along the line from the one who deals it to the one who takes it. While the two are pressed together
    /// they go as one, and the feet of a man who has set himself pass some of it to the ground (never more than his
    /// grip allows over the time they are in contact); then they spring apart by <see cref="Bounce"/>. Nothing is
    /// drawn by lot: the same meeting gives the same speeds.
    /// </summary>
    public static class ImpactLedger
    {
        /// <summary>The speeds after a bash are worked out by the ledger (Combat Feel "A Bash's Speeds Are Worked Out"). Off: written in, as before.</summary>
        public static bool On = true;

        /// <summary>How much the two spring apart, of the speed they closed at (0: they stay together; 1: all of it comes back).</summary>
        public static float Bounce = DefaultBounce;

        /// <summary>How much of what his grip allows a man who has set himself (a counter) passes to the ground through his feet (0 to 1).</summary>
        public static float FeetTake = DefaultFeetTake;

        public const float DefaultBounce = 0.2f, DefaultFeetTake = 1f;

        /// <summary>A foot's grip on the ground; how long two bodies are pressed together (seconds); gravity.</summary>
        public const float Grip = 0.8f, ContactTime = 0.25f, Gravity = 9.81f;

        /// <summary>What the ledger worked out for one meeting.</summary>
        public struct Result
        {
            /// <summary>Each one's speed along the line afterwards (m/s; + is the way the impact goes).</summary>
            public float Dealer, Taker;

            /// <summary>What the taker's feet passed to the ground (N·s), and what went through the point they met at, from the dealer to the taker (N·s).</summary>
            public float Ground, Through;

            /// <summary>What the dealer's own legs put in through the ground as he pushed on through the meeting (N·s): his "pushes through".</summary>
            public float Drive;
        }

        /// <summary>The most a man's feet can pass to the ground in one meeting (N·s): his grip times his weight times the time of contact, by how many of his feet are down (0 to 1).</summary>
        public static float GroundCanTake(float mass, float feetDown) => Grip * mass * Gravity * ContactTime * Mathf.Clamp01(feetDown);

        /// <summary>
        /// One meeting worked out. <paramref name="dealerSpeed"/> and <paramref name="takerSpeed"/> are along the line
        /// from the dealer to the taker (a taker driving into it has a speed below 0). <paramref name="takerSet"/>:
        /// how set the taker is to take it on his feet (0: not at all; 1: braced, both feet down).
        /// </summary>
        /// <param name="pushesThrough">How much speed the dealer's arm and legs add to his board while the two are pressed together (m/s): he drives on off the ground, so this much is not taken out of his own going.</param>
        public static Result Work(float dealerMass, float dealerSpeed, float takerMass, float takerSpeed, float takerSet, float pushesThrough = 0f)
        {
            pushesThrough = Mathf.Max(0f, pushesThrough);
            dealerSpeed += pushesThrough;
            dealerMass = Mathf.Max(1f, dealerMass);
            takerMass = Mathf.Max(1f, takerMass);
            float both = dealerMass + takerMass;
            float closing = Mathf.Max(0f, dealerSpeed - takerSpeed);
            float momentum = dealerMass * dealerSpeed + takerMass * takerSpeed;
            // (The ground only resists: it takes no more than would stop the two, and never sends them the other way.)
            float ground = Mathf.Clamp(FeetTake * GroundCanTake(takerMass, takerSet), 0f, Mathf.Max(0f, momentum));
            float together = (momentum - ground) / both;
            var r = new Result
            {
                Dealer = together - Bounce * closing * takerMass / both,
                Taker = together + Bounce * closing * dealerMass / both,
                Ground = ground,
                Drive = Mathf.Max(1f, dealerMass) * pushesThrough,
            };
            // (What the taker was handed at the point they met: what changed his speed, and what his feet passed on.)
            r.Through = takerMass * (r.Taker - takerSpeed) + ground;
            return r;
        }
    }
}
