using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A strike's combo core (the plan "Combo feel: Bam! Bam! Bam! in one second"): the short stretch of it a combo
    /// shows when it follows on. It is entered 0.18 to 0.30 s before contact, where the blade has turned out of its
    /// chamber and is already on its way into the blow (and the body turning and stepping into it), and left 0.06 s
    /// after contact, before the recovery begins. Worked out once per clip, from its own tracks.
    /// </summary>
    public readonly struct StrikeCore
    {
        /// <summary>Where the core starts and is cut (seconds in the clip), and the clip's contact.</summary>
        public readonly float Entry, Exit, Contact;

        /// <summary>The chamber's turnaround (the blade slowest before the blow, seconds; it may lie before the earliest entry).</summary>
        public readonly float Turnaround;

        /// <summary>The blade tip's speed at the entry against its fastest about contact (0 to 1), and the body's turn and travel left from the entry to contact (degrees; leg lengths).</summary>
        public readonly float EntrySpeedShare, TurnLeft, TravelLeft;

        /// <summary>The tip's fastest about contact (leg lengths a second), the way it sweeps over the core (leg lengths, turning round the shoulder), and the blow's weight from them (<see cref="StrikeCores.WeightOf"/>): a combo ends on its heaviest.</summary>
        public readonly float Peak, Sweep, Weight;

        /// <summary>Entered with both feet off the ground (no takeoff found); a leap: entered at its takeoff, longer before contact than a beat allows (only ever a combo's last blow, its wind-up quickened).</summary>
        public readonly bool Airborne, Leap;

        public StrikeCore(float entry, float exit, float contact, float turnaround, float entryShare, float turnLeft, float travelLeft, bool airborne, bool leap,
            float peak, float sweep)
        {
            Peak = peak;
            Sweep = sweep;
            Weight = StrikeCores.WeightOf(peak, sweep, travelLeft, leap);
            Airborne = airborne;
            Leap = leap;
            Entry = entry;
            Exit = exit;
            Contact = contact;
            Turnaround = turnaround;
            EntrySpeedShare = entryShare;
            TurnLeft = turnLeft;
            TravelLeft = travelLeft;
        }

        /// <summary>From the entry to contact (s).</summary>
        public float Lead => Contact - Entry;
    }

    public static class StrikeCores
    {
        /// <summary>The core's entry: no earlier and no later than this before contact (s).</summary>
        public const float MostLead = 0.30f, LeastLead = 0.18f;

        /// <summary>
        /// Where the core is cut: once the slash has swept on through its contact (the playtest: cut at contact, the
        /// tail of the slash was lost, and every combo read as short blows). The first time after contact the blade's
        /// tip turns slower than <see cref="SweptOutBelow"/> of its speed about contact, no sooner than
        /// <see cref="ExitAfter"/> and no later than <see cref="MostFollowThrough"/> after it (s).
        /// </summary>
        public const float ExitAfter = 0.18f, MostFollowThrough = 0.35f, SweptOutBelow = 0.12f;

        /// <summary>Entered a little after the chamber's turnaround, once the blade is on its way (s).</summary>
        public const float PastTurnaround = 0.03f;

        /// <summary>A jumping strike is entered on the ground, as its last foot leaves it, looked for this far before contact (s).</summary>
        public const float MostTakeoffLead = 1f;

        /// <summary>The blade's length used to find its tip from the weapon hand (leg lengths).</summary>
        public const float BladeLength = 1f;

        private const float Step = 1f / 120f;

        private static readonly Dictionary<LocoClip, StrikeCore> found = new Dictionary<LocoClip, StrikeCore>();

        /// <summary>A strike's core (false for a clip without a contact).</summary>
        public static bool TryGet(LocoClip clip, out StrikeCore core)
        {
            core = default;
            if (clip == null || clip.ContactFrame < 0)
            {
                return false;
            }

            if (!found.TryGetValue(clip, out core))
            {
                core = Find(clip);
                found[clip] = core;
            }

            return true;
        }

        /// <summary>The weapon hand's tip (the blade's end, along the fist from the little finger to the index) where the clip has it, on its own ground (leg lengths).</summary>
        public static Vector3 Tip(LocoClip clip, float time)
        {
            Vector3 hand = World(clip, LocoBone.RightHand, time);
            // (As StrikeCombos.Blade: the knuckles' line, leaned 20 degrees toward the fingers.)
            Vector3 side = (World(clip, LocoBone.RightHandIndex, time) - World(clip, LocoBone.RightHandPinky, time)).normalized;
            Vector3 along = (World(clip, LocoBone.RightHandMiddle, time) - hand).normalized;
            Vector3 way = side * Mathf.Cos(20f * Mathf.Deg2Rad) + along * Mathf.Sin(20f * Mathf.Deg2Rad);
            return hand + (way.sqrMagnitude > 1e-8f ? way.normalized : Vector3.up) * BladeLength;
        }

        /// <summary>
        /// Which way the blow lands, seen from the root at <paramref name="from"/> (degrees off its facing, + right, as
        /// the walk turns): the blade's tip at contact, carried by the root's own move and turn from then to contact.
        /// A combo turns least where each next blow lands the way the last one did.
        /// </summary>
        public static float Bearing(LocoClip clip, float from, string body = "Medium")
        {
            const float leg = 0.845f, reach = 0.9f * 0.7f;
            float contact = clip.ContactFrame / clip.Rate;
            Vector3 point;
            if (StrikeReach.TryGet(body, clip.Name, out var grip, out var way))
            {
                // (As the strike's plan has it: the weapon's sweet spot at contact, against the root at the clip's start.)
                point = grip + way * reach;
                clip.RootMotion(0f, from, out var before, out float turnedBefore);
                point = Quaternion.Euler(0f, -turnedBefore, 0f) * (point - new Vector3(before.x, 0f, before.y) * leg);
            }
            else
            {
                Vector3 hand = clip.Point(LocoBone.RightHand, contact);
                Vector3 side = (clip.Point(LocoBone.RightHandIndex, contact) - clip.Point(LocoBone.RightHandPinky, contact)).normalized;
                Vector3 along = (clip.Point(LocoBone.RightHandMiddle, contact) - hand).normalized;
                Vector3 tip = hand + (side * Mathf.Cos(20f * Mathf.Deg2Rad) + along * Mathf.Sin(20f * Mathf.Deg2Rad)).normalized * BladeLength;
                clip.RootMotion(from, contact, out var moved, out float turned);
                point = new Vector3(moved.x, 0f, moved.y) + Quaternion.Euler(0f, turned, 0f) * tip;
            }

            return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        }

        /// <summary>A bone where the clip has it on its own ground (its root's track applied; leg lengths).</summary>
        public static Vector3 World(LocoClip clip, LocoBone bone, float time)
        {
            clip.Root(time, out var place, out float facing);
            Vector3 p = clip.Point(bone, time);
            float r = facing * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector3(place.x + c * p.x - s * p.z, p.y, place.y + s * p.x + c * p.z);
        }

        /// <summary>The tip's speed, turning round the hand's shoulder rather than carried by the body (leg lengths a second).</summary>
        public static float TipSpeed(LocoClip clip, float time)
        {
            float a = Mathf.Max(0f, time - Step), b = Mathf.Min(clip.Duration, time + Step);
            if (b <= a)
            {
                return 0f;
            }

            Vector3 tipMoved = Tip(clip, b) - Tip(clip, a);
            Vector3 bodyMoved = World(clip, LocoBone.RightArm, b) - World(clip, LocoBone.RightArm, a);
            return (tipMoved - bodyMoved).magnitude / (b - a);
        }

        /// <summary>
        /// How heavy a blow reads (the showstopper of a combo is its heaviest): the tip's sweep into contact over the
        /// last <see cref="MostLead"/> (leg lengths), its speed there (a tenth a leg length a second), the body carried
        /// into it (leg lengths), and a leap's a full one more.
        /// </summary>
        public static float WeightOf(float peak, float sweep, float travel, bool leap) => sweep + 0.1f * peak + travel + (leap ? 1f : 0f);

        private static StrikeCore Find(LocoClip clip)
        {
            float contact = clip.ContactFrame / clip.Rate;
            float earliest = Mathf.Max(0f, contact - MostLead), latest = Mathf.Max(0f, contact - LeastLead);

            float peak = 0f;
            for (float t = contact - 0.06f; t <= contact + 0.06f; t += Step)
            {
                peak = Mathf.Max(peak, TipSpeed(clip, Mathf.Clamp(t, 0f, clip.Duration)));
            }

            // (The follow-through: on until the slash has swept out.)
            float exit = Mathf.Min(contact + MostFollowThrough, clip.Duration);
            for (float t = contact + ExitAfter; t <= contact + MostFollowThrough; t += Step)
            {
                if (t >= clip.Duration || TipSpeed(clip, t) < SweptOutBelow * peak)
                {
                    exit = Mathf.Min(t, clip.Duration);
                    break;
                }
            }

            // The chamber's turnaround: going back from contact, where the tip is slowest before it gathers into the
            // blow (the last time it drops under a fifth of its contact speed, or the slowest point in the last 0.5 s).
            // (Never that slow, a blow carried on from the clip's own run-up: its slowest point.)
            float turnaround = Mathf.Max(0f, contact - 0.5f), slowest = float.MaxValue;
            for (float t = contact - 0.04f; t >= Mathf.Max(0f, contact - 0.5f); t -= Step)
            {
                float v = TipSpeed(clip, t);
                if (v < 0.2f * peak)
                {
                    turnaround = t;
                    break;
                }

                if (v < slowest)
                {
                    slowest = v;
                    turnaround = t;
                }
            }

            float entry = Mathf.Clamp(turnaround + PastTurnaround, earliest, latest);
            // (A jump: entered as it leaves the ground, never in the air, where the bridge would lift the body in
            // 0.1 s. The long chops leap a leg length high from 0.5 to 0.8 s before contact: a leap's core is
            // longer than a beat: it ends a combo, its showstopper, its chamber quickened to fit.)
            bool airborne = Aloft(clip, entry);
            if (airborne)
            {
                for (float t = entry; t >= Mathf.Max(0f, contact - MostTakeoffLead); t -= Step)
                {
                    if (!Aloft(clip, t))
                    {
                        entry = t;
                        airborne = false;
                        break;
                    }
                }
            }

            bool leap = contact - entry > MostLead + 1e-3f;

            float share = peak > 1e-4f ? TipSpeed(clip, entry) / peak : 0f;
            clip.RootMotion(entry, contact, out var moved, out float turned);
            float sweep = 0f;
            for (float t = Mathf.Max(entry, contact - MostLead); t < contact; t += Step)
            {
                sweep += TipSpeed(clip, t) * Step;
            }

            return new StrikeCore(entry, exit, contact, turnaround, share, turned, moved.magnitude, airborne, leap, peak, sweep);
        }

        private static bool Aloft(LocoClip clip, float t) =>
            (clip.Contact(0, t) & LocoContact.Down) == 0 && (clip.Contact(1, t) & LocoContact.Down) == 0;
    }
}
