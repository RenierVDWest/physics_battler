using System.Runtime.CompilerServices;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The levers of movement made of its parts (<see cref="MoveScore"/>): how much each part does of what the
    /// recording has it do, about the middle of what it does in that recording. At rest (<see cref="Plain"/>)
    /// the body is the recording's.
    /// </summary>
    public struct MoveLevers
    {
        /// <summary>The hips' rise and fall, their sway over the feet, their turning; the chest's turning on the hips; the arms' swing; the feet's lift in their swing (1: as recorded).</summary>
        public float Bounce, Sway, HipTurn, ChestTurn, ArmSwing, StepLift;

        /// <summary>Leaning forward from the hips, added (degrees); the hips carried lower, the feet where they were (leg lengths).</summary>
        public float Lean, Crouch;

        public static MoveLevers Plain => new MoveLevers { Bounce = 1f, Sway = 1f, HipTurn = 1f, ChestTurn = 1f, ArmSwing = 1f, StepLift = 1f };

        public bool AtRest => Bounce == 1f && Sway == 1f && HipTurn == 1f && ChestTurn == 1f && ArmSwing == 1f && StepLift == 1f && Lean == 0f && Crouch == 0f;
    }

    /// <summary>
    /// A movement recording taken apart into its parts, as <see cref="AttackScore"/> is a strike taken apart (the
    /// plan "Fight Rhythm from Recordings", step A: the standard movement made of its parts).
    ///
    /// For every frame of a walk, a jog, a stand or a step: what each part of the body does against the part it
    /// sits on (<see cref="BodyTracks"/>: the hips over the ground, the chest on the hips, the head on the chest,
    /// each arm on the chest, each leg under the hips) and how the back is bent (<see cref="SpineShape"/>). The
    /// walk asks it for the body at a moment (<see cref="Pose"/>) where it asked the recording for its points, and
    /// the body is put together from the parts (<see cref="BodyMeasures.Compose(in BodyTracks, Vector3[], in SpineShape)"/>).
    /// With the levers at rest that is the recording's body, to under a millimetre; a lever changes one part's
    /// doing about the middle of what it does in that recording, and everything that sits on the part goes with it.
    /// </summary>
    public sealed class MoveScore
    {
        /// <summary>The standard movement is made of its parts (Combat Feel "Standard Movement Is Made Of Its Parts"). Off: the recordings' points are played.</summary>
        public static bool On = true;

        /// <summary>The levers every fighter's made movement is put together with (Combat Feel).</summary>
        public static MoveLevers Levers = MoveLevers.Plain;

        private static readonly ConditionalWeakTable<LocoClip, MoveScore> Scores = new ConditionalWeakTable<LocoClip, MoveScore>();

        private readonly LocoClip clip;
        private readonly BodyTracks[] tracks;
        private readonly SpineShape[] spines;
        private readonly BodyTracks mid;
        private readonly float floorL, floorR;
        private readonly Vector3[] whole = new Vector3[LocoClip.BoneCount];

        /// <summary>A recording's score: made the first time it is asked for, and kept with the recording.</summary>
        public static MoveScore Of(LocoClip clip) => Scores.GetValue(clip, c => new MoveScore(c));

        private MoveScore(LocoClip clip)
        {
            this.clip = clip;
            var m = BodyMeasures.OfClips;
            var p = new Vector3[LocoClip.BoneCount];
            int n = clip.Frames;
            tracks = new BodyTracks[n];
            spines = new SpineShape[n];
            BodyTracks? last = null;
            floorL = floorR = float.MaxValue;
            for (int f = 0; f < n; f++)
            {
                clip.Pose(f / clip.Rate, p, 1f);
                tracks[f] = m.Decompose(p, last);
                spines[f] = BodyMeasures.SpineOf(p);
                last = tracks[f];
                floorL = Mathf.Min(floorL, tracks[f].AnkleL.y);
                floorR = Mathf.Min(floorR, tracks[f].AnkleR.y);
            }

            mid = Carriage.Of(tracks).Mid;
        }

        /// <summary>The parts at a time of the recording, with the levers laid on them, and how the back is bent then.</summary>
        public BodyTracks Parts(float time, in MoveLevers levers, out SpineShape spine)
        {
            clip.FrameAt(time, out int a, out int b, out float u);
            var t = BodyTracks.Lerp(tracks[a], tracks[b], u);
            spine = SpineShape.Lerp(spines[a], spines[b], u);
            if (levers.AtRest)
            {
                return t;
            }

            // Each part about the middle of what it does in this recording.
            t.Pelvis = new Vector3(
                mid.Pelvis.x + (t.Pelvis.x - mid.Pelvis.x) * levers.Sway,
                mid.Pelvis.y + (t.Pelvis.y - mid.Pelvis.y) * levers.Bounce - levers.Crouch,
                mid.Pelvis.z + (t.Pelvis.z - mid.Pelvis.z) * levers.Sway);
            t.PelvisTurn = mid.PelvisTurn + (t.PelvisTurn - mid.PelvisTurn) * levers.HipTurn;
            t.ChestTurn = mid.ChestTurn + (t.ChestTurn - mid.ChestTurn) * levers.ChestTurn;
            // (Leaning: the hips tip a little, the back the rest, and the head comes back up most of the way.)
            t.PelvisTurn.x += 0.35f * levers.Lean;
            t.ChestTurn.x += 0.65f * levers.Lean;
            t.HeadTurn.x -= 0.7f * levers.Lean;
            if (levers.ArmSwing != 1f)
            {
                t.ClavL = BodyTracks.Way(mid.ClavL, t.ClavL, levers.ArmSwing); t.ClavR = BodyTracks.Way(mid.ClavR, t.ClavR, levers.ArmSwing);
                t.UpperL = BodyTracks.Way(mid.UpperL, t.UpperL, levers.ArmSwing); t.UpperR = BodyTracks.Way(mid.UpperR, t.UpperR, levers.ArmSwing);
                t.ForeL = BodyTracks.Way(mid.ForeL, t.ForeL, levers.ArmSwing); t.ForeR = BodyTracks.Way(mid.ForeR, t.ForeR, levers.ArmSwing);
            }

            t.AnkleL.y = floorL + (t.AnkleL.y - floorL) * levers.StepLift;
            t.AnkleR.y = floorR + (t.AnkleR.y - floorR) * levers.StepLift;
            return t;
        }

        /// <summary>The body at a time of the recording, put together from its parts (leg lengths, scaled): every bone, or only those asked for.</summary>
        public void Pose(float time, Vector3[] into, float scale, in MoveLevers levers, LocoBone[] bones = null)
        {
            var t = Parts(time, levers, out var spine);
            BodyMeasures.OfClips.Compose(t, whole, spine);
            if (bones == null)
            {
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    into[i] = whole[i] * scale;
                }
            }
            else
            {
                foreach (var bone in bones)
                {
                    into[(int)bone] = whole[(int)bone] * scale;
                }
            }
        }

        /// <summary>A movement recording's body at a time, as the walk asks for it: made of its parts when that is on, the recording's points when it is off.</summary>
        public static void Pose(LocoClip clip, float time, Vector3[] into, float scale, LocoBone[] bones = null)
        {
            if (On)
            {
                Of(clip).Pose(time, into, scale, Levers, bones);
            }
            else if (bones == null)
            {
                clip.Pose(time, into, scale);
            }
            else
            {
                clip.Pose(time, into, scale, bones);
            }
        }
    }
}
