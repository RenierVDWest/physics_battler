using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a fighter takes an opening (the plan "Fight Rhythm from Recordings", step 7, "How an attacker takes one").
    /// A blow needs time to land, so seeing a part open is not enough: it must still be open as the blade arrives.
    ///
    /// Two numbers make the difference between fighters. Whether he <see cref="Reads"/> the other's postures: a
    /// reader recognises one as it starts, knows from the times he has met it when it opens which part and for how
    /// long (its opening map, measured on the bodies, never typed in), and starts his blow early, timed to land
    /// inside the window. One who does not read only reacts to what is open now. And his <see cref="Reaction"/>
    /// time: how long after he recognises a posture, or sees a part open, he starts his blow.
    ///
    /// A posture he has not met he cannot read: the first time, a reader only reacts, and learns its map from it.
    /// </summary>
    public sealed class OpeningTaker
    {
        /// <summary>He reads postures (Combat Feel "The Swordsman Reads Openings"). Off: he only reacts to what is open now.</summary>
        public bool Reads = true;

        /// <summary>How long after he recognises a posture, or sees a part open, he starts his blow (seconds).</summary>
        public float Reaction = 0.2f;

        /// <summary>The least time a blow thrown to take an opening may take to land (seconds): a wind-up hurried more than this is no blow.</summary>
        public static float Quickest = 0.45f;

        /// <summary>How far into an opening a reader times his blade to land (0: as it opens; 1: as it shuts).</summary>
        public const float LandsInto = 0.4f;

        /// <summary>A reader does not throw a blow that would land later than this far into the opening (of its length): he would only hit what covers it again.</summary>
        public const float LatestInto = 0.9f;

        private readonly Dictionary<string, (float from, float to)> met = new Dictionary<string, (float, float)>();

        /// <summary>He has met this posture: when it opened the part and when it shut (seconds after it began).</summary>
        public bool Knows(string posture, out float from, out float to)
        {
            bool known = met.TryGetValue(posture, out var span);
            from = span.from;
            to = span.to;
            return known;
        }

        /// <summary>He has now met this posture, and how it went: its opening map is what he saw.</summary>
        public void Met(string posture, float from, float to) => met[posture] = (from, to);

        /// <summary>What he has met is forgotten.</summary>
        public void Forget() => met.Clear();

        /// <summary>What he did about the opening he is watching, or the last one: for the panel and the reports.</summary>
        public string How { get; private set; } = "";

        /// <summary>He read the posture (he knew it and timed his blow by it).</summary>
        public bool Read { get; private set; }

        private float began = -1f, strikeAt = -1f, landIn;
        private bool watching, struck;

        /// <summary>The other begins a posture now, of this name: he watches the part it will open.</summary>
        public void Began(string posture, float now)
        {
            began = now;
            watching = true;
            struck = false;
            strikeAt = -1f;
            Read = false;
            How = Reads ? "he had not met it before, so he could only react" : "he does not read: he reacts to what he sees open";
            if (!Reads || !Knows(posture, out float from, out float to))
            {
                return;
            }

            // He knows it: his blow starts after his reaction time, timed to land a way into the window.
            float lands = Mathf.Max(Reaction + Quickest, from + LandsInto * (to - from)), latest = from + LatestInto * (to - from);
            if (lands > latest)
            {
                watching = false;
                How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it (open {0:0.00} to {1:0.00} s), but his blow could not land before {2:0.00} s: he held it", from, to, lands);
                return;
            }

            Read = true;
            strikeAt = now + Reaction;
            landIn = lands - Reaction;
            How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it (open {0:0.00} to {1:0.00} s): his blow started {2:0.00} s after it began, timed to land at {3:0.00} s", from, to, Reaction, lands);
        }

        /// <summary>
        /// One step while he watches: <paramref name="openNow"/> is whether he sees the part open. True the one step
        /// he starts his blow; it is to land <paramref name="landsIn"/> seconds after.
        /// </summary>
        public bool Step(float now, bool openNow, out float landsIn)
        {
            landsIn = landIn;
            if (!watching || struck)
            {
                return false;
            }

            if (strikeAt < 0f && openNow)
            {
                // (Not read: he sees it open, and his blow starts after his reaction time, as quick as a blow can be.)
                strikeAt = now + Reaction;
                landIn = landsIn = Quickest;
                How += string.Format(System.Globalization.CultureInfo.InvariantCulture, ": he saw it open {0:0.00} s after it began", now - began);
            }

            if (strikeAt >= 0f && now >= strikeAt)
            {
                struck = true;
                return true;
            }

            return false;
        }

        /// <summary>He stops watching (the posture is long over).</summary>
        public void Done() => watching = false;
    }
}
