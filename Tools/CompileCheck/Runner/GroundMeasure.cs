using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Units;
using UnityEngine;

/// <summary>
/// The legs' opinion over a fight (the plan "Grounded legs", step 0: the table "What the playtest logs show", made by
/// the report): how long the fighters stand, how long a foot is planted, how long both feet are down, how long swings
/// last and how far the foot goes in them against the body, how much a foot on the ground slides, and whether the
/// body's weight, and the place a foot is needed, are over what the feet hold. Read from each fighter's
/// <see cref="Grounding"/>; it changes nothing.
/// </summary>
public sealed class GroundMeasure
{
    private sealed class Track
    {
        public readonly bool[] Up = new bool[2];
        public readonly float[] UpSince = new float[2], SlideRun = new float[2];
        public readonly Vector3[] FootFrom = new Vector3[2], BodyFrom = new Vector3[2];
        public readonly int[] SlideMode = new int[2];
        public bool Seen;
        public readonly float[] SlideSince = new float[2], SlideLocked = new float[2], SlideDown = new float[2], SlideScaleFrom = new float[2], SlideStepping = new float[2];
        public readonly int[] SlideSteps = new int[2];
        public readonly Vector3[] SlideTarget = new Vector3[2];
        public readonly float[] SlideFacing = new float[2], SlideYaw = new float[2];
        public int ModeWas = -1, ModeFrom = -1, DownWas = -1;
        // (A shove being caught: since when, how hard, the landings since, how the body stood when it came.)
        public int PushesSeen = -1, PushLandings, PushModeWas;
        public bool Catching, PushWalked, PushAgain;
        public float PushAt, PushSize, PushSteadyFor;
        public Vector3 PushFrom;
        public readonly bool[] PushTouch = new bool[2];
        public readonly Vector3[] TargetWas = new Vector3[2];
        public readonly float[] YawWas = new float[2], ContactPath = new float[2], ContactFor = new float[2], ContactLoose = new float[2], ContactLooseFor = new float[2];
        public readonly bool[] HeldKnown = new bool[2], InContact = new bool[2], WasLocked = new bool[2];
        public readonly float[] LockPath = new float[2], LockTurn = new float[2];
        public readonly int[] LockMode = new int[2], ForcedWas = new int[2];
        public readonly int[] ContactMode = new int[2];
        public bool StanceNoted;
        public float ModeSince;
        public float SlideModeAge0, SlideModeAge1;
    }

    /// <summary>Debugging (FIGHT_SLIDETRACE): each slide over 5 cm outside a clip is printed with what the foot's lock and the walk were doing.</summary>
    public static bool TraceSlides;

    /// <summary>Debugging (FIGHT_HELDSLIDE): a foot held still by its lock in a clip whose sole slides is printed with what moves it.</summary>
    public static bool TraceHeldSlides = Environment.GetEnvironmentVariable("FIGHT_HELDSLIDE") == "1";

    private int heldSlideLines;

    private readonly List<UnitController> units;
    private readonly Dictionary<UnitController, Track> tracks = new Dictionary<UnitController, Track>();
    private float clock;

    private int steps, both, one, none, inStance, inStanceShoved;
    private readonly int[,] held = new int[3, 7];
    private readonly float[] fadeSum = new float[3], raisedSum = new float[3];
    private readonly int[,] raisedAs = new int[3, 3];
    private readonly List<(float path, float turn, int mode)> locks = new List<(float, float, int)>();
    private readonly List<(float pull, int mode)> forced = new List<(float, int)>();
    private readonly List<(float path, float lasted, int mode)> contacts = new List<(float, float, int)>(), looseContacts = new List<(float, float, int)>();
    private readonly int[] walkWhy = new int[5];
    private readonly float[] walkSpeed = new float[5];
    private readonly int[] byMode = new int[3], bothByMode = new int[3];
    private int footSteps;
    private readonly int[] states = new int[5];
    private readonly int[,] statesByMode = new int[3, 5];
    private float slidMetres;
    private readonly float[] clipSlideBy = new float[7], clipSlideWhen = new float[10];
    private readonly Dictionary<string, float> clipSlideIn = new Dictionary<string, float>();
    private readonly List<(float length, string who, float at, int mode)> slides = new List<(float, string, float, int)>();
    private readonly List<(float lasted, float foot, float body, int mode)> swings = new List<(float, float, float, int)>();
    private readonly List<float> stanceSlowOut = new List<float>(), stanceGoingOut = new List<float>(), stanceSlowNeeded = new List<float>(), stanceGoingNeeded = new List<float>();
    private readonly float[] shiftSum = new float[2];
    private readonly List<float> landedNeeded = new List<float>();
    private readonly List<(float size, int landings, float took, float went, bool walked, int ended, int from)> catches = new List<(float, int, float, float, bool, int, int)>();
    private readonly List<float> oneFootOut = new List<float>(), standingOut = new List<float>(), stanceOff = new List<float>();
    private readonly List<float>[] neededOut = { new List<float>(), new List<float>(), new List<float>() };

    public GroundMeasure(List<UnitController> units)
    {
        this.units = units;
    }

    public void Step(float dt)
    {
        clock += dt;
        foreach (var u in units)
        {
            var clip = u.Clip;
            if (clip == null)
            {
                continue;
            }

            if (!tracks.TryGetValue(u, out var t))
            {
                tracks[u] = t = new Track();
            }

            var g = clip.Ground;
            // (Not its first second: it is put on its tile, and turned to its enemy, at once.)
            if (!u.IsAlive || clip.Fallen || !clip.Snapped || !g.Known || clock < 1f)
            {
                t.Seen = false;
                continue;
            }

            int mode = Mathf.Clamp((int)clip.Loco.Current, 0, 2);
            float size = clip.Size;
            if (mode != t.ModeWas)
            {
                t.ModeFrom = t.ModeWas;
                t.ModeWas = mode;
                t.ModeSince = clock;
                t.StanceNoted = false;
            }

            if (mode == 0 && t.ModeFrom == 1 && !t.StanceNoted && clock - t.ModeSince > 0.05f)
            {
                t.StanceNoted = true;
                stanceOff.Add(clip.Loco.StanceOff / clip.Loco.Leg);
            }

            steps++;
            byMode[mode]++;
            if (mode == 1)
            {
                int why = !clip.Loco.Guarding ? 0 : clip.Pacer.Walking ? 1 : clip.Loco.Shoved ? 2 : clip.Pacer.Stepping ? 3 : 4;
                walkWhy[why]++;
                walkSpeed[why] += clip.Loco.Velocity.magnitude;
            }

            if (mode == 0 && clip.Loco.StepsInStance && clip.Loco.Going.sqrMagnitude > 0.02f)
            {
                inStance++;
                if (clip.Loco.Shoved)
                {
                    inStanceShoved++;
                }
            }

            int down = g.FeetDown;
            if (down == 2)
            {
                both++;
                bothByMode[mode]++;
            }
            else if (down == 1)
            {
                one++;
            }
            else
            {
                none++;
            }

            Vector3 body = new Vector3(g.Weight.x, 0f, g.Weight.z);
            float speed = new Vector2(g.WeightSpeed.x, g.WeightSpeed.z).magnitude;
            for (int f = 0; f < 2; f++)
            {
                var foot = g.Feet[f];
                if (float.IsNaN(foot.Lowest))
                {
                    continue;
                }

                int whatNow = -1;
                // (Planted or stepping, the plan's step 2: what the foot's hold is doing, and how far a foot on the
                // ground goes over it from touching it to leaving it.)
                {
                    var lk = clip.Poser.Feet[f];
                    float yaw = clip.Poser.FootRotation(f).eulerAngles.y;
                    float placeMoved = t.HeldKnown[f] ? new Vector2(lk.Target.x - t.TargetWas[f].x, lk.Target.z - t.TargetWas[f].z).magnitude : 0f;
                    float turned = t.HeldKnown[f] ? Mathf.Abs(Mathf.DeltaAngle(t.YawWas[f], yaw)) : 0f;
                    int what = clip.Poser.Lifted(f) > 0.002f ? 3
                        : lk.Locked ? (placeMoved >= 0.001f ? 1 : turned > 0.3f ? 2 : 0)
                        : lk.Stepping ? 4
                        : clip.Loco.Down[f] < 0.5f ? 5 : 6;
                    held[mode, what]++;
                    whatNow = what;
                    if (what == 3)
                    {
                        raisedAs[mode, lk.Locked ? 0 : lk.Stepping ? 1 : 2]++;
                        raisedSum[mode] += clip.Poser.Lifted(f);
                    }

                    // (While it is held: how far its place goes, and how far it turns, from being held to being let go.)
                    if (lk.Locked)
                    {
                        if (!t.WasLocked[f])
                        {
                            t.LockPath[f] = t.LockTurn[f] = 0f;
                            t.LockMode[f] = mode;
                            t.ForcedWas[f] = lk.ForcedReleases;
                        }
                        else
                        {
                            t.LockPath[f] += placeMoved;
                            t.LockTurn[f] += turned;
                        }
                    }
                    else if (t.WasLocked[f] && t.Seen)
                    {
                        locks.Add((t.LockPath[f], t.LockTurn[f], t.LockMode[f]));
                        if (lk.ForcedReleases > t.ForcedWas[f])
                        {
                            forced.Add((lk.Released / size, mode));
                        }
                    }

                    t.WasLocked[f] = lk.Locked;
                    if (what == 5)
                    {
                        fadeSum[mode] += new Vector2(lk.Fade.x, lk.Fade.z).magnitude;
                    }

                    t.TargetWas[f] = lk.Target;
                    t.YawWas[f] = yaw;
                    t.HeldKnown[f] = true;
                    if (foot.Touching)
                    {
                        if (!t.InContact[f])
                        {
                            t.InContact[f] = true;
                            t.ContactPath[f] = t.ContactFor[f] = t.ContactLoose[f] = t.ContactLooseFor[f] = 0f;
                            t.ContactMode[f] = mode;
                        }

                        // (Held by its lock, or on the ground without one: landing, leaving, let go.)
                        if (lk.Locked)
                        {
                            t.ContactPath[f] += foot.Speed * dt;
                            t.ContactFor[f] += dt;
                        }
                        else
                        {
                            t.ContactLoose[f] += foot.Speed * dt;
                            t.ContactLooseFor[f] += dt;
                        }
                    }
                    else if (t.InContact[f])
                    {
                        t.InContact[f] = false;
                        if (t.Seen)
                        {
                            contacts.Add((t.ContactPath[f], t.ContactFor[f], t.ContactMode[f]));
                            looseContacts.Add((t.ContactLoose[f], t.ContactLooseFor[f], t.ContactMode[f]));
                        }
                    }
                }

                footSteps++;
                states[(int)foot.State]++;
                statesByMode[mode, (int)foot.State]++;
                // (Slides: each run of a foot on the ground and moving, its length.)
                // (Not the first step after it was down or not yet there: the foot's place a step before is where it stood
                // before it fell, and the whole way it was thrown counted as one slide, in whatever clip it got up into:
                // a turn, as often as not, which made the turns look like half of all the sliding in a fight.)
                if (foot.State == Grounding.FootState.Sliding && t.Seen)
                {
                    float moved = foot.Speed * dt;
                    slidMetres += moved;
                    if (mode == 2 && whatNow >= 0)
                    {
                        // (The clips' feet, the plan's step 5: what a foot sliding in a clip was doing, and in which clip.)
                        clipSlideBy[whatNow] += moved;
                        if (TraceHeldSlides && whatNow == 0 && heldSlideLines < 80)
                        {
                            // (FIGHT_HELDSLIDE=1: a foot held still by its lock, in a clip, and its sole moving: what moves it.)
                            heldSlideLines++;
                            var lk2 = clip.Poser.Feet[f];
                            var part = u.Rig.Part(f == 0 ? PhysicsBattler.Rig.PartId.FootL : PhysicsBattler.Rig.PartId.FootR).transform;
                            Console.WriteLine($"[heldslide] {clock:0.00} {u.name} {(f == 0 ? "L" : "R")} {(clip.Loco.Shot != null ? clip.Loco.Shot.Name : "?")} {clip.Loco.ShotTime:0.00}: sole {100f * foot.Speed:0} cm/s ({(foot.ToeDown ? "toe" : "heel")}); "
                                + $"ankle {100f * (part.position - lk2.Target).magnitude:0.0} cm from the lock's place, on toe {lk2.OnToe} ball {lk2.BallHolds}, yaw hold {lk2.YawHold:0.0}, foot turned to {part.eulerAngles.y:0.0} pitch {Mathf.DeltaAngle(0f, part.eulerAngles.x):0.0} roll {Mathf.DeltaAngle(0f, part.eulerAngles.z):0.0}, facing {clip.Loco.Facing:0.0}, heel {100f * foot.Heel.y:0.0} toe {100f * foot.Toe.y:0.0} cm up");
                        }
                        string playing = clip.Loco.Shot != null ? clip.Loco.Shot.Name : "?";
                        clipSlideIn[playing] = (clipSlideIn.TryGetValue(playing, out float was) ? was : 0f) + moved;
                        clipSlideWhen[Mathf.Clamp((int)(10f * clip.Loco.ShotTime / Mathf.Max(0.05f, clip.Loco.Shot != null ? clip.Loco.Shot.Duration : 1f)), 0, 9)] += moved;
                    }
                    if (t.SlideRun[f] <= 0f)
                    {
                        t.SlideMode[f] = mode;
                        t.SlideSince[f] = clock;
                        t.SlideLocked[f] = t.SlideDown[f] = t.SlideStepping[f] = 0f;
                        t.SlideSteps[f] = 0;
                        t.SlideScaleFrom[f] = clip.Loco.StrideScale;
                        t.SlideTarget[f] = clip.Poser.Feet[f].Target;
                        t.SlideFacing[f] = clip.Loco.Facing;
                        t.SlideYaw[f] = clip.Poser.Feet[f].YawHold;
                        if (f == 0) { t.SlideModeAge0 = clock - t.ModeSince; } else { t.SlideModeAge1 = clock - t.ModeSince; }
                    }

                    t.SlideRun[f] += moved;
                    t.SlideSteps[f]++;
                    t.SlideLocked[f] += clip.Poser.Feet[f].Locked ? 1f : 0f;
                    t.SlideStepping[f] += clip.Poser.Feet[f].Stepping ? 1f : 0f;
                    t.SlideDown[f] += clip.Loco.Down[f] >= 0.5f ? 1f : 0f;
                }
                else if (t.SlideRun[f] > 0f)
                {
                    slides.Add((t.SlideRun[f], u.name, clock, t.SlideMode[f]));
                    if (TraceSlides && t.SlideRun[f] > 0.05f && t.SlideMode[f] != 2)
                    {
                        int n = Mathf.Max(1, t.SlideSteps[f]);
                        Console.WriteLine($"[slide] {u.name} {(f == 0 ? "L" : "R")} {t.SlideSince[f]:0.00}-{clock:0.00} s, {100f * t.SlideRun[f]:0} cm, began {(t.SlideMode[f] == 0 ? "standing" : "walking")} {(f == 0 ? t.SlideModeAge0 : t.SlideModeAge1):0.00} s into it, now {mode}; locked {100f * t.SlideLocked[f] / n:0}%, the walk has it down {100f * t.SlideDown[f] / n:0}%, stepping off a lock {100f * t.SlideStepping[f] / n:0}%; stride x{t.SlideScaleFrom[f]:0.00} to x{clip.Loco.StrideScale:0.00}, speed {clip.Loco.Velocity.magnitude:0.00}, paced {clip.Loco.Paced}, stepping {clip.Pacer.Stepping}, height {100f * (foot.Lowest):0.0} cm; the lock's place moved {100f * new Vector2(clip.Poser.Feet[f].Target.x - t.SlideTarget[f].x, clip.Poser.Feet[f].Target.z - t.SlideTarget[f].z).magnitude:0.0} cm, body turned {Mathf.DeltaAngle(t.SlideFacing[f], clip.Loco.Facing):0} deg, foot's turn held {t.SlideYaw[f]:0} to {clip.Poser.Feet[f].YawHold:0}, on toe {clip.Poser.Feet[f].OnToe}");
                    }

                    t.SlideRun[f] = 0f;
                }

                // (Swings: from leaving the ground to touching it again.)
                Vector3 at = new Vector3(0.5f * (foot.Heel.x + foot.Toe.x), 0f, 0.5f * (foot.Heel.z + foot.Toe.z));
                if (!foot.Touching)
                {
                    if (!t.Up[f] && t.Seen)
                    {
                        t.Up[f] = true;
                        t.UpSince[f] = clock;
                        t.FootFrom[f] = at;
                        t.BodyFrom[f] = body;
                    }
                }
                else
                {
                    if (t.Up[f])
                    {
                        swings.Add((clock - t.UpSince[f], (at - t.FootFrom[f]).magnitude, (body - t.BodyFrom[f]).magnitude, mode));
                    }

                    t.Up[f] = false;
                }
            }

            // (The weight and the place a foot is needed, against the support.)
            if (down == 1 && mode != 2 && speed < 0.3f && !float.IsNaN(g.WeightOut))
            {
                oneFootOut.Add(g.WeightOut / size);
            }

            // (Steps where they are needed, the plan's step 4: each shove, and how the legs caught it: how many
            // landings and how long until the body stands on both feet with the place it needs a foot on what they hold.)
            {
                if (t.PushesSeen < 0)
                {
                    t.PushesSeen = clip.Loco.Pushes;
                }

                if (clip.Loco.Pushes != t.PushesSeen)
                {
                    t.PushesSeen = clip.Loco.Pushes;
                    if (t.Catching)
                    {
                        t.PushAgain = true;
                        t.PushSize = Mathf.Max(t.PushSize, clip.Loco.LastPush.magnitude / Mathf.Sqrt(size));
                    }
                    else
                    {
                        t.Catching = true;
                        t.PushAgain = false;
                        t.PushAt = clock;
                        t.PushSize = clip.Loco.LastPush.magnitude / Mathf.Sqrt(size);
                        t.PushLandings = 0;
                        t.PushWalked = false;
                        t.PushSteadyFor = 0f;
                        t.PushFrom = clip.Loco.Position;
                        t.PushModeWas = t.ModeWas >= 0 ? t.ModeWas : mode;
                        t.PushTouch[0] = g.Feet[0].Touching;
                        t.PushTouch[1] = g.Feet[1].Touching;
                    }
                }

                if (t.Catching)
                {
                    for (int f = 0; f < 2; f++)
                    {
                        if (g.Feet[f].Touching && !t.PushTouch[f])
                        {
                            t.PushLandings++;
                        }

                        t.PushTouch[f] = g.Feet[f].Touching;
                    }

                    t.PushWalked |= mode == 1;
                    bool steady = down == 2 && !float.IsNaN(g.NeededOut) && g.NeededOut <= 0.05f * size;
                    t.PushSteadyFor = steady ? t.PushSteadyFor + dt : 0f;
                    int ended = t.PushSteadyFor >= 0.1f ? 0 : mode == 2 ? 1 : clock - t.PushAt > 4f ? 2 : -1;
                    if (ended >= 0)
                    {
                        t.Catching = false;
                        catches.Add((t.PushSize, t.PushLandings, clock - t.PushAt - (ended == 0 ? 0.1f : 0f), (clip.Loco.Position - t.PushFrom).magnitude / size, t.PushWalked, t.PushAgain ? 3 : ended, t.PushModeWas));
                    }
                }
            }

            // (As a stepping foot comes down in the stance: is the body where its two feet can stop it?)
            if (down == 2 && t.DownWas == 1 && mode == 0 && t.Seen && !float.IsNaN(g.NeededOut))
            {
                landedNeeded.Add(g.NeededOut / size);
            }

            t.DownWas = down;
            if (down == 1 && mode == 0 && !float.IsNaN(g.WeightOut))
            {
                (speed < 0.3f ? stanceSlowOut : stanceGoingOut).Add(g.WeightOut / size);
                (speed < 0.3f ? stanceSlowNeeded : stanceGoingNeeded).Add(float.IsNaN(g.NeededOut) ? 0f : g.NeededOut / size);
                shiftSum[speed < 0.3f ? 0 : 1] += clip.Poser.HipShift.magnitude / size;
            }

            if (down == 2 && speed < 0.15f && !float.IsNaN(g.WeightOut))
            {
                standingOut.Add(g.WeightOut / size);
            }

            if (down == 2 && !float.IsNaN(g.NeededOut))
            {
                neededOut[mode].Add(g.NeededOut / size);
            }

            t.Seen = true;
        }
    }

    private static float At(List<float> sorted, float share) => sorted.Count == 0 ? 0f : sorted[Mathf.Clamp((int)(share * sorted.Count), 0, sorted.Count - 1)];

    private static string Pct(int part, int of) => of > 0 ? $"{100f * part / of:0}%" : "-";

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        if (steps == 0)
        {
            return "";
        }

        float seconds = steps / 60f, minutes = seconds / 60f;
        sb.AppendLine($"    the legs' opinion (the plan \"Grounded legs\", step 0; {seconds:0} unit-seconds on their feet; a sole's end within 2 cm of the ground is on it):");
        sb.AppendLine($"      a fighter is standing (not walking, not in a clip) {Pct(byMode[0], steps)} of the time, walking {Pct(byMode[1], steps)}, in a clip (a strike, a reaction, a turn) {Pct(byMode[2], steps)}");
        sb.AppendLine($"      a foot is planted (on the ground, not moving) {Pct(states[0], footSteps)} of the time, on the ground and moving over 10 cm/s {Pct(states[1], footSteps)}, landing {Pct(states[2], footSteps)}, in the air under 9 cm {Pct(states[3], footSteps)}, in the air higher {Pct(states[4], footSteps)}");
        sb.AppendLine($"      of the standing: stepping in the stance (the body going over 0.15 m/s, the feet stepping after it) {Pct(inStance, steps)} of all the time (after a shove {Pct(inStanceShoved, steps)}), standing still {Pct(byMode[0] - inStance, steps)}");
        {
            string[] whys = { "out of its guard", "asked to walk (as fast as a walk)", "walking off a shove", "a step under way out of a walk", "coming to a stand" };
            sb.AppendLine("      of the walking: " + string.Join(", ", whys.Select((w, i) => $"{w} {Pct(walkWhy[i], steps)} (at {walkSpeed[i] / Mathf.Max(1, walkWhy[i]):0.00} m/s)")));
        }

        string[] names = { "standing", "walking", "in a clip" };
        for (int m = 0; m < 3; m++)
        {
            int n = 0;
            for (int s = 0; s < 5; s++)
            {
                n += statesByMode[m, s];
            }

            if (n > 0)
            {
                sb.AppendLine($"        {names[m]}: a foot planted {Pct(statesByMode[m, 0], n)}, sliding {Pct(statesByMode[m, 1], n)}, in the air under 9 cm {Pct(statesByMode[m, 3], n)}, higher {Pct(statesByMode[m, 4], n)}; both feet on the ground {Pct(bothByMode[m], byMode[m])} of the time");
            }
        }

        sb.AppendLine($"      both feet are on the ground {Pct(both, steps)} of the time, one {Pct(one, steps)}, neither {Pct(none, steps)}; while walking, a foot is in the air {Pct(byMode[1] - bothByMode[1], byMode[1])} of the time");
        if (swings.Count > 0)
        {
            var lasted = swings.Select(s => s.lasted).OrderBy(x => x).ToList();
            var longOnes = swings.Where(s => s.lasted > 0.6f).ToList();
            float airTime = swings.Sum(s => s.lasted);
            sb.AppendLine($"      swings (a foot off the ground until it is on it again): {swings.Count} ({swings.Count / seconds:0.0} a second for each fighter), lasting {At(lasted, 0.5f):0.00} s at the middle, a quarter over {At(lasted, 0.75f):0.00} s, a tenth over {At(lasted, 0.9f):0.00} s");
            if (longOnes.Count > 0)
            {
                var foot = longOnes.Select(s => s.foot).OrderBy(x => x).ToList();
                var body = longOnes.Select(s => s.body).OrderBy(x => x).ToList();
                sb.AppendLine($"        swings lasting over 0.6 s: {longOnes.Count} ({100f * longOnes.Count / swings.Count:0}% of them), {100f * longOnes.Sum(s => s.lasted) / airTime:0}% of all the time feet are in the air; in them the foot goes {100f * At(foot, 0.5f):0} cm while the body goes {100f * At(body, 0.5f):0} cm (at the middle)");
            }
            else
            {
                sb.AppendLine("        swings lasting over 0.6 s: none");
            }
        }

        var over5 = slides.Where(s => s.length > 0.05f).OrderByDescending(s => s.length).ToList();
        sb.AppendLine($"      a foot on the ground and moving over 10 cm/s: {Pct(states[1], footSteps)} of the time, {slidMetres / Mathf.Max(1e-3f, minutes):0.0} m a minute for each fighter; slides over 5 cm: {over5.Count} ({over5.Count / Mathf.Max(1e-3f, minutes):0} a minute for each fighter)"
            + (over5.Count > 0 ? $", the worst {100f * over5[0].length:0} cm ({over5[0].who} at {over5[0].at:0.0} s, {names[over5[0].mode]}); by what was playing: {string.Join(", ", over5.GroupBy(s => s.mode).OrderBy(x => x.Key).Select(x => $"{names[x.Key]} {x.Count()}"))}" : ""));
        float clipSlid = clipSlideBy.Sum();
        if (clipSlid > 0f)
        {
            // (The clips' feet, the plan's step 5.)
            string[] doing = { "held still", "held, its place moving", "held, turning", "lifted to hover", "let go and stepping to its place", "in the clip's swing", "let go, the clip has it down" };
            sb.AppendLine($"      sliding in a clip, {clipSlid:0.0} m in all, by what the foot's hold was doing: " + string.Join(", ", Enumerable.Range(0, 7).Select(i => $"{doing[i]} {100f * clipSlideBy[i] / clipSlid:0}%")));
            sb.AppendLine("        by the clip: " + string.Join(", ", clipSlideIn.OrderByDescending(p => p.Value).Take(10).Select(p => $"{p.Key} {p.Value:0.0} m")));
            sb.AppendLine("        by how far through the clip (tenths): " + string.Join(" ", clipSlideWhen.Select(v => $"{100f * v / clipSlid:0}%")));
        }

        if (oneFootOut.Count > 0)
        {
            oneFootOut.Sort();
            sb.AppendLine($"      on one foot, standing or walking, the body moving under 0.3 m/s ({oneFootOut.Count / 60f:0} unit-seconds): the weight is outside that foot by {100f * At(oneFootOut, 0.5f):0} cm at the middle, a quarter of the time over {100f * At(oneFootOut, 0.75f):0} cm");
        }

        if (catches.Count > 0)
        {
            sb.AppendLine($"      shoves and how the legs caught them (the plan's step 4; {catches.Count}, {catches.Count / Mathf.Max(1e-3f, minutes):0.0} a minute for each fighter; caught: standing on both feet with the place a foot is needed within 5 cm of what they hold):");
            float[] bands = { 0f, 0.5f, 1f, 1.6f, 99f };
            for (int b = 0; b + 1 < bands.Length; b++)
            {
                var of = catches.Where(c => c.size >= bands[b] && c.size < bands[b + 1]).ToList();
                if (of.Count == 0)
                {
                    continue;
                }

                var caught = of.Where(c => c.ended == 0).ToList();
                var steps = caught.Select(c => (float)c.landings).OrderBy(x => x).ToList();
                var took = caught.Select(c => c.took).OrderBy(x => x).ToList();
                var went = caught.Select(c => c.went).OrderBy(x => x).ToList();
                sb.AppendLine($"        {(b == bands.Length - 2 ? $"over {bands[b]:0.0}" : $"{bands[b]:0.0} to {bands[b + 1]:0.0}")} m/s: {of.Count}; caught {Pct(caught.Count, of.Count)} (in {At(steps, 0.5f):0} landings at the middle, {At(steps, 0.9f):0} for one in ten; within three landings {Pct(caught.Count(c => c.landings <= 3), of.Count)}; {At(took, 0.5f):0.00} s, the body gone {100f * At(went, 0.5f):0} cm), shoved again first {Pct(of.Count(c => c.ended == 3), of.Count)}, a clip began first {Pct(of.Count(c => c.ended == 1), of.Count)}, not in 4 s {Pct(of.Count(c => c.ended == 2), of.Count)}; walked {Pct(of.Count(c => c.walked), of.Count)}; it was in a clip when shoved {Pct(of.Count(c => c.from == 2), of.Count)}");
            }
        }

        if (stanceSlowOut.Count + stanceGoingOut.Count > 0)
        {
            string Line(List<float> outs, List<float> needed, float shifted)
            {
                if (outs.Count == 0)
                {
                    return "never";
                }

                outs.Sort();
                needed.Sort();
                return $"{outs.Count / 60f:0} unit-seconds, the weight outside that foot by {100f * At(outs, 0.5f):0} cm at the middle (a quarter over {100f * At(outs, 0.75f):0}), the place a foot is needed {100f * At(needed, 0.5f):0} cm outside it, the hips shifted {100f * shifted / outs.Count:0} cm on average";
            }

            int waited = units.Sum(u => u.Clip != null ? u.Clip.Poser.ShiftedSteps : 0);
            if (landedNeeded.Count > 0)
            {
                sb.AppendLine($"      as a stepping foot comes down in the stance ({landedNeeded.Count} times): the place a foot is needed to stop the body is on what the two feet hold {100f * landedNeeded.Count(x => x <= 0f) / landedNeeded.Count:0}% of the time, within 5 cm of it {100f * landedNeeded.Count(x => x <= 0.05f) / landedNeeded.Count:0}%, over 10 cm outside it {100f * landedNeeded.Count(x => x > 0.10f) / landedNeeded.Count:0}%");
            }

            sb.AppendLine($"      the weight over the feet (the plan's step 3), standing in the stance on one foot: the body under 0.3 m/s: {Line(stanceSlowOut, stanceSlowNeeded, shiftSum[0])}; going faster: {Line(stanceGoingOut, stanceGoingNeeded, shiftSum[1])}; steps from standing that waited for the weight to shift: {waited} ({waited / Mathf.Max(1e-3f, minutes):0.0} a minute for each fighter)");
        }

        if (standingOut.Count > 0)
        {
            standingOut.Sort();
            sb.AppendLine($"      still on both feet (the weight moving under 0.15 m/s; {standingOut.Count / 60f:0} unit-seconds): the weight is outside the support {100f * standingOut.Count(x => x > 0f) / standingOut.Count:0}% of the time, by over 3 cm {100f * standingOut.Count(x => x > 0.03f) / standingOut.Count:0}%, the worst {100f * standingOut[standingOut.Count - 1]:0} cm");
        }

        {
            int spells = units.Sum(u => u.Clip != null ? u.Clip.Pacer.Spells : 0), back = units.Sum(u => u.Clip != null ? u.Clip.Pacer.Reversals : 0);
            float stood = units.Sum(u => u.Clip != null ? u.Clip.Pacer.Stood : 0f), stepped = units.Sum(u => u.Clip != null ? u.Clip.Pacer.Stepped : 0f);
            sb.AppendLine(PhysicsBattler.Locomotion.StepPacer.On
                ? $"      stand and step: paced {stood + stepped:0} unit-seconds (in guard, no clip playing): standing {100f * stood / Mathf.Max(1e-3f, stood + stepped):0}% of it; {spells} spells of stepping ({spells / Mathf.Max(1e-3f, minutes):0.0} a minute for each fighter), {back} of them back the way of the one before within 2 s"
                : "      stand and step: off (the steering's velocity walked as it is)");
        }

        {
            string[] whats = { "held still", "held, its place moving", "held, turning", "lifted to hover over the ground", "let go and stepping to its place", "in the clip's swing", "let go, the clip has it down, not yet held" };
            sb.AppendLine("      planted or stepping (the plan's step 2), what each foot's hold is doing, of the time:");
            for (int m = 0; m < 3; m++)
            {
                int n = 0;
                for (int w = 0; w < 7; w++)
                {
                    n += held[m, w];
                }

                if (n > 0)
                {
                    sb.AppendLine($"        {names[m]}: " + string.Join(", ", Enumerable.Range(0, 7).Select(w => $"{whats[w]} {100f * held[m, w] / n:0.0}%")) + $"; in a swing the foot is still {100f * fadeSum[m] / Mathf.Max(1, held[m, 5]):0} cm from the clip's path on average");
                }
            }

            sb.AppendLine("      lifted to hover, by what the foot was then: " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
            {
                int n = raisedAs[m, 0] + raisedAs[m, 1] + raisedAs[m, 2];
                return n == 0 ? $"{names[m]} never" : $"{names[m]} held {100f * raisedAs[m, 0] / n:0}%, stepping to its place {100f * raisedAs[m, 1] / n:0}%, in the clip's swing {100f * raisedAs[m, 2] / n:0}%, by {100f * raisedSum[m] / n:0.0} cm on average";
            })));
            if (locks.Count > 0)
            {
                sb.AppendLine("      a held foot's own place, from being held to being let go: " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
                {
                    var of = locks.Where(c => c.mode == m).ToList();
                    return of.Count == 0 ? $"{names[m]} none" : $"{names[m]} ({of.Count}) moved over 1 cm {100f * of.Count(x => x.path > 0.01f) / of.Count:0}%, over 5 cm {100f * of.Count(x => x.path > 0.05f) / of.Count:0}%; turned over 5 degrees {100f * of.Count(x => x.turn > 5f) / of.Count:0}%, over 20 {100f * of.Count(x => x.turn > 20f) / of.Count:0}%";
                })));
            }

            if (forced.Count > 0)
            {
                float minutes2 = steps / 3600f;
                sb.AppendLine("      a held foot let go because the body had gone too far from it (it then steps to its place): " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
                {
                    var of = forced.Where(c => c.mode == m).Select(c => c.pull).OrderBy(x => x).ToList();
                    return of.Count == 0 ? $"{names[m]} never" : $"{names[m]} {of.Count / Mathf.Max(1e-3f, minutes2):0.0} a minute for each fighter, the body {100f * At(of, 0.5f):0} cm from it at the middle, {100f * At(of, 0.9f):0} cm for one in ten";
                })));
            }

            if (contacts.Count > 0)
            {
                sb.AppendLine("      a foot on the ground and held, from touching the ground to leaving it, how far it goes over it: " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
                {
                    var of = contacts.Where(c => c.mode == m && c.lasted > 0.05f).Select(c => c.path).OrderBy(x => x).ToList();
                    return of.Count == 0 ? $"{names[m]} none" : $"{names[m]} ({of.Count}) {100f * At(of, 0.5f):0.0} cm at the middle, over 1 cm {100f * of.Count(x => x > 0.01f) / of.Count:0}%, over 3 cm {100f * of.Count(x => x > 0.03f) / of.Count:0}%, over 10 cm {100f * of.Count(x => x > 0.10f) / of.Count:0}%";
                })));
                sb.AppendLine("      and on the ground without being held (landing, leaving, let go): " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
                {
                    var of = looseContacts.Where(c => c.mode == m).Select(c => c.path).OrderBy(x => x).ToList();
                    float time = looseContacts.Where(c => c.mode == m).Sum(c => c.lasted), all = contacts.Where(c => c.mode == m).Sum(c => c.lasted) + time;
                    return of.Count == 0 ? $"{names[m]} none" : $"{names[m]} {100f * time / Mathf.Max(1e-3f, all):0}% of its time on the ground, {100f * At(of, 0.5f):0.0} cm at the middle, over 3 cm {100f * of.Count(x => x > 0.03f) / of.Count:0}%, over 10 cm {100f * of.Count(x => x > 0.10f) / of.Count:0}%";
                })));
            }
        }

        if (stanceOff.Count > 0)
        {
            stanceOff.Sort();
            sb.AppendLine($"      come to a stand out of a walk ({stanceOff.Count} times): the foot furthest from where the standing clip has it is {100f * At(stanceOff, 0.25f):0}% of a leg's length off at a quarter, {100f * At(stanceOff, 0.5f):0}% at the middle, {100f * At(stanceOff, 0.75f):0}% at three quarters, {100f * At(stanceOff, 0.9f):0}% at nine tenths");
        }

        if (swings.Count > 0)
        {
            sb.AppendLine($"      swings lasting over 0.45 s: {100f * swings.Count(s => s.lasted > 0.45f) / swings.Count:0}% of them; over 1 s: {100f * swings.Count(s => s.lasted > 1f) / swings.Count:0}%; walking or standing only: over 0.45 s {100f * swings.Count(s => s.mode != 2 && s.lasted > 0.45f) / Mathf.Max(1, swings.Count(s => s.mode != 2)):0}% of {swings.Count(s => s.mode != 2)}");
        }

        if (swings.Count > 0)
        {
            sb.AppendLine("      swings by what was playing as the foot came down: " + string.Join("; ", Enumerable.Range(0, 3).Select(m =>
            {
                var of = swings.Where(x => x.mode == m).Select(x => x.lasted).OrderBy(x => x).ToList();
                return of.Count == 0 ? $"{names[m]} none" : $"{names[m]} {of.Count}, {At(of, 0.5f):0.00} s at the middle, {100f * of.Count(x => x > 0.45f) / of.Count:0}% over 0.45 s";
            })));
        }

        for (int m = 0; m < 3; m++)
        {
            var l = neededOut[m];
            if (l.Count > 0)
            {
                sb.AppendLine($"      both feet down, {names[m]} ({l.Count / 60f:0} unit-seconds): the place a foot is needed to stop the body is outside the support {100f * l.Count(x => x > 0f) / l.Count:0}% of the time, by over 10 cm {100f * l.Count(x => x > 0.10f) / l.Count:0}%");
            }
        }

        return sb.ToString();
    }
}
