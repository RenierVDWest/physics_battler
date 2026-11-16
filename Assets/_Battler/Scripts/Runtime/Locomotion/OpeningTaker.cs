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

        /// <summary>He read it and held his blow: it could not have landed in time.</summary>
        public bool Held { get; private set; }

        private float began = -1f, strikeAt = -1f, landIn;
        private bool watching, struck;

        /// <summary>The other begins a posture now, of this name: he watches the part it will open.</summary>
        public void Began(string posture, float now)
        {
            began = now;
            watching = true;
            struck = false;
            strikeAt = -1f;
            Read = Held = false;
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
                Held = true;
                How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it (open {0:0.00} to {1:0.00} s), but his blow could not land before {2:0.00} s: he held it", from, to, lands);
                return;
            }

            Read = true;
            strikeAt = now + Reaction;
            landIn = lands - Reaction;
            How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it (open {0:0.00} to {1:0.00} s): his blow started {2:0.00} s after it began, timed to land at {3:0.00} s", from, to, Reaction, lands);
        }

        /// <summary>A blow in its own time lands about this long after it starts (seconds): with time in hand he does not hurry it, and it lands truer.</summary>
        public const float OwnTime = 0.62f;

        /// <summary>
        /// He only watches now, and will react when he sees the part open (one who does not read; or one who reads
        /// with nothing to go by).
        /// </summary>
        public void Watch(float now, string how)
        {
            began = now;
            watching = true;
            struck = false;
            strikeAt = -1f;
            Read = Held = false;
            How = how;
        }

        /// <summary>
        /// He foresees an opening: the part will be open from <paramref name="opensAt"/> to
        /// <paramref name="shutsAt"/> (the game's clock), by something he has learned (<paramref name="by"/>: a
        /// posture's own tell, or the pattern the other's gestures come in). His blow starts no sooner than his
        /// reaction time from now, in its own time where there is time for that and hurried where there is not
        /// (never under <see cref="Quickest"/>), made to land <see cref="LandsInto"/> of the way into the opening.
        /// False, and he holds his blow: it could not land before the opening is all but over.
        /// </summary>
        public bool Foresee(float now, float opensAt, float shutsAt, string by)
        {
            began = now;
            struck = false;
            strikeAt = -1f;
            Read = Held = false;
            float lands = opensAt + LandsInto * (shutsAt - opensAt), latest = opensAt + LatestInto * (shutsAt - opensAt);
            float starts = Mathf.Max(now + Reaction, lands - OwnTime);
            lands = Mathf.Max(lands, starts + Quickest);
            if (lands > latest)
            {
                watching = false;
                Held = true;
                How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it by {0} (open {1:0.00} to {2:0.00} s from now), but his blow could not land before {3:0.00} s: he held it", by, opensAt - now, shutsAt - now, lands - now);
                return false;
            }

            watching = true;
            Read = true;
            strikeAt = starts;
            landIn = lands - starts;
            How = string.Format(System.Globalization.CultureInfo.InvariantCulture, "he read it by {0} (open {1:0.00} to {2:0.00} s from then): his blow started {3:0.00} s after, to land at {4:0.00} s", by, opensAt - now, shutsAt - now, starts - now, lands - now);
            return true;
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

        /// <summary>He has a blow planned for an opening he foresaw, and has not started it yet.</summary>
        public bool Pending => watching && !struck && Read && strikeAt >= 0f;

        /// <summary>He stops watching (the posture is long over).</summary>
        public void Done() => watching = false;
    }
}
