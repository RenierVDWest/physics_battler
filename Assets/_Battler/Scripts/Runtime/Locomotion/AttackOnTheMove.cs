using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a blow thrown on the move sits on the walk under it (the plan "Fight Rhythm from Recordings": the body in
    /// layers, the legs from one motion and the blow from another).
    /// </summary>
    public struct MoveBlowStyle
    {
        /// <summary>
        /// How far the hips go from where the walk has them to where the blow has its own (0 to 1); the spine takes
        /// the rest, so the chest and the blade go round as far as they did in the recording either way. The
        /// recorded blows turn the hips 40 to 70 degrees over feet that pivot for it; a walk's feet do not, so its
        /// hips take a part.
        /// </summary>
        public float HipsTurn;

        /// <summary>How much of the blow's sinking and rising the hips take (1: as recorded): the knees bend for it over the walk's feet.</summary>
        public float HipsDrop;

        /// <summary>How far the hips go toward the target as the blow lands (leg lengths): his weight goes into the blow, and the legs reach back for it.</summary>
        public float HipsInto;

        /// <summary>
        /// How much the walk is checked through the blow (0: not at all; 0.45: at the landing he goes at 55% of his
        /// pace): he slows as the blow is driven, slowest as it lands, and picks up again. The steps under him
        /// shorten and slow with it: a catch in his stride, not a stop.
        /// </summary>
        public float Check;

        /// <summary>
        /// How far the walk may carry him toward the target, or back from it, on top of where it was going, for the
        /// blade to reach (leg lengths): a step in under the blow, taken by the walk's own legs over the wind-up and
        /// the drive. 0: he strikes from wherever the walk has him.
        /// </summary>
        public float StepIn;

        /// <summary>How much of the hips' turn the knees go round with (0: they keep pointing the way he walks; 1: they turn with the hips).</summary>
        public float KneesFollow;

        public static MoveBlowStyle Plain => new MoveBlowStyle { HipsTurn = 0.5f, HipsDrop = 1f, HipsInto = 0.07f, Check = 0.45f, KneesFollow = 0.4f, StepIn = 0.75f };
    }

    /// <summary>
    /// A blow thrown on the move: the walk goes on under it. The legs, the feet's steps and the body's travel are the
    /// walk's; the chest, the head and the arms are the blow's (a made attack's clip, <see cref="AttackMaker"/>);
    /// the hips are both: the walk's, turned, sunk and carried toward the target as the blow drives them.
    ///
    /// <para><b>The legs answer the blow</b> three ways. The hips over them turn and sink with it, so the knees bend
    /// and the thighs go round. The walk is checked through the drive (<see cref="MoveBlowStyle.Check"/>), so the
    /// steps shorten and slow as the blade comes round and lengthen again after. And the blade is timed to land as
    /// a foot comes down (<see cref="LandsWithAFoot"/>), as a blow is thrown off a step.</para>
    /// </summary>
    public static class AttackOnTheMove
    {
        /// <summary>
        /// A blow asked for while he walks is thrown on the move (the scene's Combat Feel, "Attacks Are Thrown On The
        /// Move"). Off: he plants himself to strike, his speed carried into the blow.
        /// </summary>
        public static bool On = true;

        /// <summary>How a blow on the move sits on the walk (the scene's Combat Feel, "Moving Blow ...").</summary>
        public static MoveBlowStyle Style = MoveBlowStyle.Plain;

        /// <summary>Set (the headless run): each blow on the move tells how its landing went against its plan.</summary>
        public static System.Action<string> Trace;

        /// <summary>The weapon hand is pulled by what the walk has gone off the blow's plan (off: for finding what it does).</summary>
        public static bool Pulls = true;

        /// <summary>With <see cref="Trace"/>: every step of the layering is told as well.</summary>
        public static bool TraceSteps;

        /// <summary>Under this pace he is not walking (m/s at a Medium's size): a blow is thrown planted.</summary>
        public const float WalksFrom = 0.8f;

        /// <summary>A blow that would still fall this far short thrown on the move (metres at a Medium's size) is thrown planted instead, with its lunge.</summary>
        public const float ShortOfReach = 0.1f;

        /// <summary>The check begins this long before the hips start in, and is gone this long after the landing (seconds of the blow's clip).</summary>
        public const float CheckLeads = 0.12f, CheckLasts = 0.35f;

        /// <summary>The spine twists no further than this between the hips and the chest (degrees).</summary>
        public const float MostTwist = 85f;

        /// <summary>The blow's upper body is given back to the walk over this long at its end (seconds).</summary>
        public const float HandsBack = 0.25f;

        private static float Smooth(float from, float to, float x)
        {
            float u = to > from ? Mathf.Clamp01((x - from) / (to - from)) : x >= to ? 1f : 0f;
            return u * u * (3f - 2f * u);
        }

        /// <summary>How much the blow has hold of the walk at a time of its clip (0 to 1): in over the drive, all at the landing, gone after.</summary>
        public static float Hold(float time, float driveAt, float landAt)
        {
            return Smooth(driveAt - CheckLeads, landAt, time) * (1f - Smooth(landAt, landAt + CheckLasts, time));
        }

        /// <summary>The share of his pace he goes at, at a time of the blow's clip.</summary>
        public static float Goes(float time, float driveAt, float landAt, float check)
        {
            return 1f - Mathf.Clamp01(check) * Hold(time, driveAt, landAt);
        }

        /// <summary>How much of the step in has been taken by a time of the blow's clip (0 to 1): eased in from the start, all of it as the blade lands.</summary>
        public static float SteppedIn(float time, float landAt) => Smooth(0f, landAt, time);

        /// <summary>
        /// Where the walk will have taken him by a time of the blow (the frame he is in as it begins, leg lengths;
        /// and how far he will have turned, degrees): going as he goes now, at the same speed against his own
        /// facing, checked as the blow checks him, with the blow's step in on top. His turning: with
        /// <paramref name="faces"/>, he keeps facing that place as he has it now (a fighter faces his enemy, and
        /// turns the faster the nearer he passes); without, at the rate he turns now (a straight line, or the circle
        /// he is on).
        /// </summary>
        /// <param name="speed">How he walks as the blow begins (his own frame, leg lengths a second).</param>
        /// <param name="turning">How fast he turns (degrees a second).</param>
        /// <param name="faces">The place he keeps facing (the frame the blow begins in), or null.</param>
        /// <param name="stepIn">The blow's step in (the frame the blow begins in, leg lengths).</param>
        public static void Walked(Vector3 speed, float turning, Vector3? faces, Vector3 stepIn, float time, float driveAt, float landAt, float check, out Vector3 at, out float turned)
        {
            Vector3 walked = Vector3.zero;
            at = Vector3.zero;
            turned = 0f;
            float bearing = faces.HasValue ? Mathf.Atan2(faces.Value.x, faces.Value.z) * Mathf.Rad2Deg : 0f;
            const float step = 1f / 60f;
            for (float t = 0f; t < time - 1e-5f; t += step)
            {
                float dt = Mathf.Min(step, time - t), goes = Goes(t + 0.5f * dt, driveAt, landAt, check);
                walked += Quaternion.Euler(0f, turned, 0f) * speed * (goes * dt);
                at = walked + stepIn * SteppedIn(t + dt, landAt);
                if (faces.HasValue)
                {
                    Vector3 to = faces.Value - at;
                    // (Too near to say which way it is: he turns no further.)
                    turned = to.x * to.x + to.z * to.z > 0.04f ? Mathf.DeltaAngle(bearing, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg) : turned;
                }
                else
                {
                    turned += turning * goes * dt;
                }
            }
        }

        /// <summary>
        /// When a blade can land so that it comes down with a foot: the walk's coming footfalls (its stride's phase
        /// and how fast it goes on, cycles a second; the left foot lands at phase 0, the right at a half) between
        /// <paramref name="soonest"/> and <paramref name="latest"/> seconds from now, into <paramref name="times"/>.
        /// With no stride to go by, or none of its footfalls in that time, none.
        /// </summary>
        public static void Footfalls(float phase, float phaseRate, float soonest, float latest, System.Collections.Generic.List<float> times)
        {
            times.Clear();
            if (phaseRate < 0.2f)
            {
                return;
            }

            float first = Mathf.Repeat(0.5f - Mathf.Repeat(phase, 0.5f), 0.5f) / phaseRate;
            for (int n = 0; n < 12; n++)
            {
                float t = first + n * 0.5f / phaseRate;
                if (t >= soonest && t <= latest)
                {
                    times.Add(t);
                }
            }
        }

        /// <summary>The step in is taken no faster than this over the time to the landing (leg lengths a second, on top of the walk's own pace): a step, not a dart.</summary>
        public const float MostStepInPace = 1.6f;

        /// <summary>
        /// The body in layers: the walk's legs and hips with the blow's chest, head and arms on them, and the hips
        /// turned, sunk and carried as the blow drives them. All against the root (the walk's for the walk, the
        /// clip's for the blow), leg lengths.
        /// </summary>
        /// <param name="guard">The blow's clip at its start: what its hips do is counted from there.</param>
        /// <param name="turned">How far the blow's clip has turned its root since it began (degrees): the recorded hips' turn through the blow is in the root's facing.</param>
        /// <param name="weight">How much of the blow's upper body is on (1; down to 0 as it is given back to the walk).</param>
        /// <param name="into">The hips' way toward the target, times how far they go there now (against the root, leg lengths).</param>
        public static BodyTracks Layer(in BodyTracks walk, in BodyTracks blow, in BodyTracks guard, float turned, float weight, Vector3 into, in MoveBlowStyle style)
        {
            var r = walk;
            // How the blow has its hips turned against the way he faces: its clip's own, and what its root has turned
            // (the recorded hips' turn through a blow is mostly in its root's facing). The walk's hips go part of the
            // way there from where the walk has them, and the spine takes the rest: the chest, and so the blade, is
            // turned against the way he faces just as the recording has it, whatever the walk's hips are doing. (A
            // guard walks forward with its hips square and stands with them turned 50 degrees: a blow laid on the
            // walk's hips as they are would be thrown 50 degrees off to one side.)
            float blowYaw = blow.PelvisTurn.y + turned;
            float hips = weight * Mathf.Clamp01(style.HipsTurn) * Mathf.DeltaAngle(walk.PelvisTurn.y, blowYaw);
            r.PelvisTurn.y += hips;
            r.PelvisTurn.x += weight * (blow.PelvisTurn.x - guard.PelvisTurn.x);
            r.PelvisTurn.z += weight * (blow.PelvisTurn.z - guard.PelvisTurn.z);
            r.Pelvis.x += weight * (blow.Pelvis.x - guard.Pelvis.x) + into.x;
            r.Pelvis.z += weight * (blow.Pelvis.z - guard.Pelvis.z) + into.z;
            r.Pelvis.y += weight * style.HipsDrop * (blow.Pelvis.y - guard.Pelvis.y);
            // (The chest as the recording has it against the way he faces, whole: its lean goes the way it went, not
            // round with the hips. Against the hips as they are now, that is what the spine has to make.)
            Quaternion want = Quaternion.Euler(blow.PelvisTurn.x, blowYaw, blow.PelvisTurn.z) * Quaternion.Euler(blow.ChestTurn);
            Vector3 chest = BodyMeasures.EulerOf(Quaternion.Inverse(Quaternion.Euler(r.PelvisTurn)) * want, blow.ChestTurn.y + Mathf.DeltaAngle(r.PelvisTurn.y, blowYaw));
            chest.y = Mathf.Clamp(chest.y, -MostTwist, MostTwist);
            r.ChestTurn = new Vector3(Mathf.LerpUnclamped(walk.ChestTurn.x, chest.x, weight), walk.ChestTurn.y + weight * Mathf.DeltaAngle(walk.ChestTurn.y, chest.y), Mathf.LerpUnclamped(walk.ChestTurn.z, chest.z, weight));
            r.HeadTurn = Vector3.LerpUnclamped(walk.HeadTurn, blow.HeadTurn, weight);
            r.ClavL = BodyTracks.Way(walk.ClavL, blow.ClavL, weight); r.ClavR = BodyTracks.Way(walk.ClavR, blow.ClavR, weight);
            r.UpperL = BodyTracks.Way(walk.UpperL, blow.UpperL, weight); r.UpperR = BodyTracks.Way(walk.UpperR, blow.UpperR, weight);
            r.ForeL = BodyTracks.Way(walk.ForeL, blow.ForeL, weight); r.ForeR = BodyTracks.Way(walk.ForeR, blow.ForeR, weight);
            r.HandL = Quaternion.Slerp(walk.HandL, blow.HandL, weight);
            r.HandR = Quaternion.Slerp(walk.HandR, blow.HandR, weight);
            // (The knees are kept against the hips: turned back by what the hips turned and the knees do not follow,
            // they go on pointing the way he walks.)
            var back = Quaternion.Euler(0f, -hips * (1f - Mathf.Clamp01(style.KneesFollow)), 0f);
            r.KneeL = back * walk.KneeL;
            r.KneeR = back * walk.KneeR;
            return r;
        }
    }
}
