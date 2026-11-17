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

        /// <summary>Leaning to the side from the hips (degrees; above 0: to his right) and the hips carried aside, the feet where they were (leg lengths; above 0: to his right): a dodge (<see cref="ClipFighter.Dodge"/>).</summary>
        public float SideLean, HipsAside;

        /// <summary>The upper body's postures (<see cref="PostureMaker"/>; all nothing at rest): the sword hand turned on the chest so the point dips (x, degrees) or goes aside (y); the shield arm thrust out before the chest (0 to 1); the sword arm carried higher (degrees; lower below 0).</summary>
        public Vector3 BladeTurn;
        public float ShieldOut, SwordArmLift;

        /// <summary>The shield hand carried this far from where the recording has it (the chest's own frame, leg lengths: x to his right, y up, z forward), the elbow bending or straightening for it and the board facing as it did: an uncovering (<see cref="ClipFighter.Uncover"/>).</summary>
        public Vector3 ShieldShift;

        public static MoveLevers Plain => new MoveLevers { Bounce = 1f, Sway = 1f, HipTurn = 1f, ChestTurn = 1f, ArmSwing = 1f, StepLift = 1f };

        public bool AtRest => Bounce == 1f && Sway == 1f && HipTurn == 1f && ChestTurn == 1f && ArmSwing == 1f && StepLift == 1f && Lean == 0f && Crouch == 0f && SideLean == 0f && HipsAside == 0f && BladeTurn == Vector3.zero && ShieldOut == 0f && SwordArmLift == 0f && ShieldShift == Vector3.zero;
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

        // ---- One carriage (the plan "Convincing walking in the dance", step 2) ----------------------------------------

        /// <summary>
        /// How much of the stand's own places the feet keep in a step carried on its carriage (0: each recording's own
        /// places; 1: the stand's: its feet as far apart and its left foot as far ahead). Combat Feel "Guard Keeps Its
        /// Stance".
        /// </summary>
        public static float StanceKept = 0.5f;

        /// <summary>Changes whenever what a carried recording's feet are given changes: what is worked out from one is worked out again.</summary>
        public static int CarryStamp => (MoveStyle.OneCarriage ? 1 : 0) + 2 * Mathf.RoundToInt(100f * StanceKept);

        private bool carried;
        private float carryHips;
        private Vector3 carryAnkleL, carryAnkleR;

        /// <summary>
        /// This recording is carried as a stand is (<see cref="MoveStyle.OneCarriage"/>): its hips at the stand's
        /// height on average, and each foot's own place moved toward where the stand keeps that foot. Only its step is
        /// its own: how the hips rise, sway and turn about their middle, and how each foot goes about its place.
        /// </summary>
        public void CarryAs(in BodyTracks stand)
        {
            carried = true;
            carryHips = stand.Pelvis.y - mid.Pelvis.y;
            carryAnkleL = new Vector3(stand.AnkleL.x - mid.AnkleL.x, 0f, stand.AnkleL.z - mid.AnkleL.z);
            carryAnkleR = new Vector3(stand.AnkleR.x - mid.AnkleR.x, 0f, stand.AnkleR.z - mid.AnkleR.z);
        }

        /// <summary>For the reports: how far this recording's hips are moved to the stand's height (leg lengths; 0: it is not carried).</summary>
        public float CarriedHips => carried && MoveStyle.OneCarriage ? carryHips : 0f;

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
                Carry(ref t);
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
            if (levers.SideLean != 0f || levers.HipsAside != 0f)
            {
                // (Leaning aside: the hips go that way and tip a little, the back the rest, and the head comes half-way back up.)
                t.Pelvis.x += levers.HipsAside;
                t.PelvisTurn.z -= 0.35f * levers.SideLean;
                t.ChestTurn.z -= 0.65f * levers.SideLean;
                t.HeadTurn.z += 0.5f * levers.SideLean;
            }
            if (levers.ArmSwing != 1f)
            {
                t.ClavL = BodyTracks.Way(mid.ClavL, t.ClavL, levers.ArmSwing); t.ClavR = BodyTracks.Way(mid.ClavR, t.ClavR, levers.ArmSwing);
                t.UpperL = BodyTracks.Way(mid.UpperL, t.UpperL, levers.ArmSwing); t.UpperR = BodyTracks.Way(mid.UpperR, t.UpperR, levers.ArmSwing);
                t.ForeL = BodyTracks.Way(mid.ForeL, t.ForeL, levers.ArmSwing); t.ForeR = BodyTracks.Way(mid.ForeR, t.ForeR, levers.ArmSwing);
            }

            if (levers.SwordArmLift != 0f)
            {
                // (The sword arm carried higher or lower: the whole arm turned at the shoulder, on the chest.)
                var lift = Quaternion.Euler(-levers.SwordArmLift, 0f, 0f);
                t.UpperR = lift * t.UpperR;
                t.ForeR = lift * t.ForeR;
                t.HandR = lift * t.HandR;
            }

            if (levers.BladeTurn != Vector3.zero)
            {
                // (The point dipped or sent aside: the wrist does it, and the forearm a third of it.)
                t.HandR = Quaternion.Euler(levers.BladeTurn.x, levers.BladeTurn.y, 0f) * t.HandR;
                t.ForeR = Quaternion.Euler(0.33f * levers.BladeTurn.x, 0.33f * levers.BladeTurn.y, 0f) * t.ForeR;
            }

            if (levers.ShieldOut > 0f)
            {
                // (The shield thrust out: its arm straightened forward of the chest.)
                float w = Mathf.Clamp01(levers.ShieldOut);
                t.UpperL = BodyTracks.Way(t.UpperL, new Vector3(-0.15f, -0.3f, 1f).normalized, 0.85f * w);
                t.ForeL = BodyTracks.Way(t.ForeL, new Vector3(0.1f, 0.1f, 1f).normalized, w);
            }

            if (levers.ShieldShift != Vector3.zero)
            {
                // (The shield carried away: its hand goes straight there, the elbow bending or straightening for it on
                // the side it was, so the board is not swung in an arc about the shoulder, into his own body.)
                var m = BodyMeasures.OfClips;
                float upper = m.UpperLen[0], fore = m.ForeLen[0];
                Vector3 elbow = t.UpperL.normalized * upper, want = elbow + t.ForeL.normalized * fore + levers.ShieldShift;
                float far = Mathf.Clamp(want.magnitude, Mathf.Abs(upper - fore) + 0.02f, upper + fore - 0.01f);
                Vector3 to = want.normalized;
                float along = (upper * upper + far * far - fore * fore) / (2f * far);
                Vector3 side = elbow - to * Vector3.Dot(elbow, to);
                side = side.sqrMagnitude > 1e-8f ? side.normalized : Vector3.Cross(to, Vector3.forward).normalized;
                Vector3 bent = to * along + side * Mathf.Sqrt(Mathf.Max(0f, upper * upper - along * along));
                t.UpperL = bent / upper;
                t.ForeL = (to * far - bent) / fore;
            }

            t.AnkleL.y = floorL + (t.AnkleL.y - floorL) * levers.StepLift;
            t.AnkleR.y = floorR + (t.AnkleR.y - floorR) * levers.StepLift;
            Carry(ref t);
            return t;
        }

        /// <summary>The parts carried as the stand is, after the levers have had their say about each part's own middle.</summary>
        private void Carry(ref BodyTracks t)
        {
            if (carried && MoveStyle.OneCarriage)
            {
                t.Pelvis.y += carryHips;
                t.AnkleL += carryAnkleL * StanceKept;
                t.AnkleR += carryAnkleR * StanceKept;
            }
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
        public static void Pose(LocoClip clip, float time, Vector3[] into, float scale, LocoBone[] bones = null) => Pose(Levers, clip, time, into, scale, bones);

        /// <summary>The same with levers of the asker's own (a fighter's, with what a posture of his adds to them).</summary>
        public static void Pose(in MoveLevers levers, LocoClip clip, float time, Vector3[] into, float scale, LocoBone[] bones = null)
        {
            if (On)
            {
                Of(clip).Pose(time, into, scale, levers, bones);
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
