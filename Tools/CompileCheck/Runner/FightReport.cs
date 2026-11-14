// `fight [dir] [seed]`: a whole 3 v 3 fight on the board with no help (milestone 6's gate), stepped
// as the smoke run is (the stand-in integrator for PhysX), teams starting at opposite ends. Records
// a playtest session in <dir> (analyze it with Tools/Playtest/analyze.py) and prints how it went.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PhysicsBattler.Battle;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class FightReport
{
    private static string[] paceTrace = new string[0];
    private static string[] blowUp = new string[0];
    private static PhysicsBattler.Sandbox.TubeTest tubes;
    private static readonly Dictionary<UnitController, (Vector3, Vector3, Vector3)> tubeFrom = new Dictionary<UnitController, (Vector3, Vector3, Vector3)>();
    private static float tubeAt;
    private static bool tubeSent;
    private static string tubeMet = "";
    private static readonly int[] paceHist = new int[16];
    private static Vector3 paceHistSum;
    private static float paceHistSpeed;
    private static readonly Dictionary<PhysicsBattler.Locomotion.ClipFighter, (Vector3 asked, Vector3 want)> paceLast = new Dictionary<PhysicsBattler.Locomotion.ClipFighter, (Vector3 asked, Vector3 want)>();

    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>Called after every step of a fight (the `arms` runner measures the poses here).</summary>
    public static Action<List<UnitController>, float> OnStep;

    public static int Run(string output, int seed)
    {
        Directory.CreateDirectory(output);
        var rigs = new Dictionary<string, RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        string assets = Application.dataPath;
        Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
        var recorder = new GameObject("Recorder").AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");
        bool engine = false;
#if PHYSX
        // (PHYSX=1: the real engine under the fight (the plan "A different approach", step 1): the clip fighters'
        // bodies physical as in Unity, every step simulated by PhysX; PHYSX_SUBSTEPS physics steps in each, 1 as Unity.)
        if (Environment.GetEnvironmentVariable("PHYSX") != null)
        {
            engine = true;
            // (The joint probe first, in a world of its own: its bodies must not meet the fight's ground.)
            Physics.Backend = new PhysicsBattler.Engine.PhysXWorld(false);
            PhysicsBattler.Muscles.JointConventions.EnsureProbed();
            PhysicsBattler.Engine.PhysXWorld.Install(BodyPart.GroundLayer, int.TryParse(Environment.GetEnvironmentVariable("PHYSX_SUBSTEPS"), out int sub) ? sub : 1);
        }
#endif
        if (!engine)
        {
            // (No physics headless: the clip fighters' bodies are the lab runners', posed only.)
            UnitController.ClipPhysical = false;
        }

        // (FIGHT_PHYSICAL=1, with PHYSX=1: the clip fighters' upper bodies physical, the game stepping the engine itself
        // (PHYSX_SUBSTEPS, 4), a blade limited to 30 m/s unless FIGHT_BLADE says otherwise.)
        if (engine && Environment.GetEnvironmentVariable("FIGHT_PHYSICAL") != null)
        {
            PhysicsBattler.Locomotion.ClipFighter.PhysicalUpper = true;
            PhysicsBattler.Locomotion.CombatFeel.MostBladeSpeed = PhysicsBattler.Locomotion.CombatFeel.PhysicalBladeSpeed;
            PhysicsBattler.Authority.PhysicsSteps.Manual = true;
            PhysicsBattler.Authority.PhysicsSteps.Substeps = int.TryParse(Environment.GetEnvironmentVariable("PHYSX_SUBSTEPS"), out int subs) ? subs : 4;
            ((PhysicsBattler.Engine.PhysXWorld)Physics.Backend).Substeps = 1;
        }

        // (The Combat Feel's feet, for a fight played with them set: FIGHT_PULL=3 (m/s), FIGHT_LIFT=0 (cm).)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_PULL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pull))
        {
            PhysicsBattler.Locomotion.CombatFeel.ComboPullSpeed = pull;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_LIFT"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float footLift))
        {
            PhysicsBattler.Locomotion.CombatFeel.MovingFootLift = footLift;
        }

        // (How much of its strength a blocked arm presses on with: FIGHT_GIVE=1 for all of it.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_GIVE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float giveShare))
        {
            PhysicsBattler.Locomotion.ClipFighter.BlockedStrength = giveShare;
        }

        // (From how far off its pose an arm starts to give, and by where it has given all it will, metres: FIGHT_GIVE_FROM=0.1,0.22. A body falls with its chest FIGHT_BORNE metres off.)
        var giveFrom = (Environment.GetEnvironmentVariable("FIGHT_GIVE_FROM") ?? "").Split(',');
        if (giveFrom.Length == 2 && float.TryParse(giveFrom[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float gf)
            && float.TryParse(giveFrom[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float gt))
        {
            PhysicsBattler.Locomotion.ClipFighter.BlockedFrom = gf;
            PhysicsBattler.Locomotion.ClipFighter.BlockedTo = gt;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_BORNE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float borne))
        {
            PhysicsBattler.Locomotion.ClipFighter.BorneDownFrom = borne;
        }

        // (UPPER_ALLOW=0: no muscle is allowed more than its strength for what the pose itself does.)
        if (float.TryParse(Environment.GetEnvironmentVariable("UPPER_ALLOW"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float allow))
        {
            PhysicsBattler.Authority.UpperBodyArticulation.AllowShare = allow;
        }

        // (UPPER_FF: how much of the pose's speed the motors are told to turn at; -1 or 57.3 to see what an engine that took it otherwise would do.)
        if (float.TryParse(Environment.GetEnvironmentVariable("UPPER_FF"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ff))
        {
            PhysicsBattler.Authority.UpperBodyArticulation.FeedForward = ff;
        }

        // (UPPER_RETURN=0: a joint off its pose is sent the pose itself, as it was; UPPER_MOSTRETURN, UPPER_BRAKE, UPPER_JOINTSPEED, UPPER_PUSHOUT: its numbers.)
        PhysicsBattler.Authority.UpperBodyArticulation.Returns = Environment.GetEnvironmentVariable("UPPER_RETURN") != "0";
        // (UPPER_AXES=parent: a joint's speed of turn given on its parent's axes, to see which the engine counts it on.)
        PhysicsBattler.Authority.UpperBodyArticulation.OtherAxes = Environment.GetEnvironmentVariable("UPPER_AXES") == "parent";
        // (UPPER_SNAP=0: a joint the pose turns at once is brought there by its muscle, not put there.)
        PhysicsBattler.Authority.UpperBodyArticulation.PutFlips = Environment.GetEnvironmentVariable("UPPER_SNAP") != "0";
        float Env(string name, float or) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : or;
        PhysicsBattler.Authority.UpperBodyArticulation.MostReturn = Env("UPPER_MOSTRETURN", PhysicsBattler.Authority.UpperBodyArticulation.MostReturn);
        // (UPPER_SLACK="15,1,40,0.3": the slack's angle and time, the taut angle and time; UPPER_WEIGHT=1: the parts weigh on their joints.)
        {
            var slack = (Environment.GetEnvironmentVariable("UPPER_SLACK") ?? "").Split(',');
            if (slack.Length == 4)
            {
                PhysicsBattler.Authority.UpperBodyArticulation.SlackAngle = float.Parse(slack[0], System.Globalization.CultureInfo.InvariantCulture) * Mathf.Deg2Rad;
                PhysicsBattler.Authority.UpperBodyArticulation.SlackTime = float.Parse(slack[1], System.Globalization.CultureInfo.InvariantCulture);
                PhysicsBattler.Authority.UpperBodyArticulation.TautAngle = float.Parse(slack[2], System.Globalization.CultureInfo.InvariantCulture) * Mathf.Deg2Rad;
                PhysicsBattler.Authority.UpperBodyArticulation.TautTime = float.Parse(slack[3], System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        PhysicsBattler.Authority.UpperBodyArticulation.DampTimes = Env("UPPER_DAMP", PhysicsBattler.Authority.UpperBodyArticulation.DampTimes);
        // (FIGHT_MUSCLE=6000: a Medium's muscle strength, N m.)
        PhysicsBattler.Locomotion.ClipFighter.MuscleStrength = Env("FIGHT_MUSCLE", PhysicsBattler.Locomotion.ClipFighter.MuscleStrength);
        PhysicsBattler.Authority.UpperBodyArticulation.SolverSteps = (int)Env("UPPER_ITER", PhysicsBattler.Authority.UpperBodyArticulation.SolverSteps);
        PhysicsBattler.Authority.UpperBodyArticulation.SolverSpeedSteps = (int)Env("UPPER_VITER", PhysicsBattler.Authority.UpperBodyArticulation.SolverSpeedSteps);
        // (UPPER_GIVEWAY=8: how far, degrees, a joint is pushed before it gives way; UPPER_FOLLOW=60.)
        PhysicsBattler.Authority.UpperBodyArticulation.GiveWay = Env("UPPER_GIVEWAY", PhysicsBattler.Authority.UpperBodyArticulation.GiveWay * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        PhysicsBattler.Authority.UpperBodyArticulation.GiveLag = Env("UPPER_GIVELAG", PhysicsBattler.Authority.UpperBodyArticulation.GiveLag);
        PhysicsBattler.Authority.UpperBodyArticulation.Follow = Env("UPPER_FOLLOW", PhysicsBattler.Authority.UpperBodyArticulation.Follow);
        PhysicsBattler.Authority.UpperBodyArticulation.EvenDamping = Environment.GetEnvironmentVariable("UPPER_EVEN") != "0";
        PhysicsBattler.Authority.UpperBodyArticulation.Weightless = Environment.GetEnvironmentVariable("UPPER_WEIGHT") != "1";
        PhysicsBattler.Authority.UpperBodyArticulation.BrakeShare = Env("UPPER_BRAKE", PhysicsBattler.Authority.UpperBodyArticulation.BrakeShare);
        PhysicsBattler.Authority.UpperBodyArticulation.MostJointSpeed = Env("UPPER_JOINTSPEED", PhysicsBattler.Authority.UpperBodyArticulation.MostJointSpeed);
        PhysicsBattler.Authority.UpperBodyArticulation.MostPushOut = Env("UPPER_PUSHOUT", PhysicsBattler.Authority.UpperBodyArticulation.MostPushOut);
        // (UPPER_GHOST=1: the physical upper bodies touch nothing, to see how the muscles alone follow the pose.)
        PhysicsBattler.Authority.UpperBodyArticulation.Ghost = Environment.GetEnvironmentVariable("UPPER_GHOST") == "1";

        // (How much of their quickness a struck body's muscles keep at the least: FIGHT_EASE=1 for all of it.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_EASE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float leastQuick))
        {
            PhysicsBattler.Locomotion.ClipFighter.LeastQuick = leastQuick;
        }

        // (How fast a pressed body's feet give way, m/s for each metre its chest is held off: FIGHT_PRESSED=0 for not at all.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_PRESSED"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pressedGain))
        {
            PhysicsBattler.Locomotion.ClipFighter.PressedGain = pressedGain;
        }

        // (The Combat Feel's limit on a blade's speed, for a fight played with it: FIGHT_BLADE=30.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_BLADE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float blade))
        {
            PhysicsBattler.Locomotion.CombatFeel.MostBladeSpeed = blade;
        }

        // (The shield: FIGHT_SHIELD="fixed,wrist,reads" (1 or 0: fixed in the fist; the wrist's most turn, degrees; the share
        // of a blow that must pass before the shield moves); "0,15,0" is as it was before.)
        {
            var shield = (Environment.GetEnvironmentVariable("FIGHT_SHIELD") ?? "").Split(',');
            if (shield.Length >= 3 && float.TryParse(shield[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float wrist)
                && float.TryParse(shield[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float reads))
            {
                PhysicsBattler.Locomotion.CombatFeel.ShieldFixed = shield[0].Trim() != "0";
                PhysicsBattler.Locomotion.CombatFeel.ShieldWristTurn = wrist;
                PhysicsBattler.Locomotion.CombatFeel.ShieldReadsFrom = reads;
            }
        }

        // (The gate's follow-up: FIGHT_ARMSTEADY="most,bones,hand" (the fastest it follows the arm sent, degrees a second, 0: no
        // limit; 1 or 0: the bones set as a body's; 1 or 0: the free hand with its arm); "0,0,0" is as it was before.)
        {
            var steady = (Environment.GetEnvironmentVariable("FIGHT_ARMSTEADY") ?? "").Split(',');
            if (steady.Length >= 2 && float.TryParse(steady[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float mostSent))
            {
                PhysicsBattler.Motion.ArmGate.MostSent = mostSent;
                PhysicsBattler.Motion.ArmLimits.BonesAsABody = steady[1].Trim() != "0";
                if (steady.Length >= 3)
                {
                    PhysicsBattler.Locomotion.SwordParry.FreeHandWithArm = steady[2].Trim() != "0";
                }
            }
        }

        // (The physical arms' stops: FIGHT_ARMSTOPS=0 runs a fight without them, any other number is how many times its
        // muscle's strength a stopped joint is held with.)
        {
            string stops = Environment.GetEnvironmentVariable("FIGHT_ARMSTOPS");
            if (stops != null && float.TryParse(stops, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float times))
            {
                PhysicsBattler.Locomotion.ClipFighter.ArmStops = times > 0f;
                if (times > 0f)
                {
                    PhysicsBattler.Authority.UpperBodyArticulation.StopTimes = times;
                }
            }
        }

        // (The gate at the end of the pose: FIGHT_ARMGATE=0 turns it off, arms posed wherever their hands are sent, as before.)
        if (Environment.GetEnvironmentVariable("FIGHT_ARMGATE") == "0")
        {
            PhysicsBattler.Motion.ArmLimits.On = false;
        }

        if (Environment.GetEnvironmentVariable("FIGHT_STANDTRACE") == "1") { PhysicsBattler.Locomotion.Locomotor.StandTrace = m => Console.WriteLine("[stand] " + m); }
        // (The weight over the feet: FIGHT_SHIFT=0 keeps the hips where the clip has them; "share,waits" sets how far they
        // go toward the carrying foot and how long a step from standing waits for them (seconds).)
        {
            var sh = (Environment.GetEnvironmentVariable("FIGHT_SHIFT") ?? "").Split(',');
            if (sh.Length >= 1 && float.TryParse(sh[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float share))
            {
                PhysicsBattler.Locomotion.LocoBody.WeightShifts = share > 0f;
                if (share > 0f)
                {
                    PhysicsBattler.Locomotion.LocoBody.ShiftShare = share;
                }

                if (sh.Length >= 2 && float.TryParse(sh[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float waits))
                {
                    PhysicsBattler.Locomotion.LocoBody.ShiftWaits = waits;
                }
            }
        }

        // (Steps where they are needed: FIGHT_SHOVETO=1 steps a shove in the stance only up to a walk's speed, as before step 4.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_SHOVETO"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float shoveTo))
        {
            PhysicsBattler.Locomotion.Locomotor.ShoveSteppedTo = shoveTo;
        }

        // (What the legs can catch: FIGHT_CATCH=0 never knocks a fighter down for the speed a blow throws it at, as before; a
        // number sets that speed (m/s at a Medium's size).)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_CATCH"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float catches))
        {
            PhysicsBattler.Locomotion.CombatFeel.LegsDecide = catches > 0f;
            if (catches > 0f)
            {
                PhysicsBattler.Locomotion.CombatFeel.LegsCatch = catches;
            }
        }

        GroundMeasure.TraceSlides = Environment.GetEnvironmentVariable("FIGHT_SLIDETRACE") == "1";

        // (A body the engine loses: FIGHT_LOST=0 puts it back only after half a second away, as before.
        // FIGHT_BLOWUP="A Medium,10,200000": from that second, for three steps, that fighter's sword hand is pulled with
        // that many newtons, as a leg that nothing stops pulls an arm it has hooked.)
        if (Environment.GetEnvironmentVariable("FIGHT_LOST") == "0")
        {
            PhysicsBattler.Authority.UpperBodyArticulation.PutsBackAtOnce = false;
        }

        blowUp = (Environment.GetEnvironmentVariable("FIGHT_BLOWUP") ?? "").Split(',');
        // (FIGHT_PACETRACE="A Swordsman,17,21": each step of that fighter's pacing between those seconds, written out.)
        paceTrace = (Environment.GetEnvironmentVariable("FIGHT_PACETRACE") ?? "").Split(',');
        if (paceTrace.Length >= 3)
        {
            PhysicsBattler.Locomotion.ClipFighter.PaceTrace = (f, asked, want) => paceLast[f] = (asked, want);
        }

        // (FIGHT_PACEHIST=1: what the steering asks of a fighter in guard with no clip playing, by speed.)
        if (Environment.GetEnvironmentVariable("FIGHT_PACEHIST") == "1")
        {
            PhysicsBattler.Locomotion.ClipFighter.PaceTrace = (f, asked, want) =>
            {
                paceLast[f] = (asked, want);
                if (f.Loco.Current != PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && f.Loco.Guarding)
                {
                    float a = asked.magnitude / Mathf.Sqrt(f.Size);
                    paceHist[Mathf.Clamp((int)(a / 0.1f), 0, paceHist.Length - 1)]++;
                    paceHistSum += asked;
                    paceHistSpeed += asked.magnitude;
                }
            };
        }

        // (Stand and step: FIGHT_STEPS=0 runs a fight with the steering's velocity walked as it is; "worth,pace,eases,stance,held,shoves"
        // sets how much ground is asked before a step (metres), the pace it is stepped at (m/s), how quickly the body picks
        // it up (seconds), whether it is stepped in the stance (0: walked), how far off its distance a fighter stands (metres), and whether a shove is stepped in the stance (0: walked off).)
        {
            var st = (Environment.GetEnvironmentVariable("FIGHT_STEPS") ?? "").Split(',');
            if (st.Length >= 1 && float.TryParse(st[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float worth))
            {
                PhysicsBattler.Locomotion.StepPacer.On = worth > 0f;
                if (worth > 0f)
                {
                    PhysicsBattler.Locomotion.StepPacer.StepWorth = worth;
                }

                if (st.Length >= 2 && float.TryParse(st[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pace))
                {
                    PhysicsBattler.Locomotion.StepPacer.StepPace = pace;
                }

                if (st.Length >= 3 && float.TryParse(st[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float eases))
                {
                    PhysicsBattler.Locomotion.Locomotor.PacedHalfLife = eases;
                }

                if (st.Length >= 4)
                {
                    PhysicsBattler.Locomotion.Locomotor.StepsInStanceOn = st[3].Trim() != "0";
                }

                if (st.Length >= 5 && float.TryParse(st[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ring))
                {
                    PhysicsBattler.Battle.FightLoop.RingHeldWithin = ring;
                }

                if (st.Length >= 6)
                {
                    PhysicsBattler.Locomotion.Locomotor.ShovesInStance = st[5].Trim() != "0";
                }
            }
        }

        // (Real weapon weight: FIGHT_WMASS="3,1" makes the weapons three times their own weight in the physical body and
        // takes that much off what the script adds to a blow (",0": the script as it was); "0" is the weapons as before,
        // their weight as written whatever their bearer's size.)
        {
            var wm = (Environment.GetEnvironmentVariable("FIGHT_WMASS") ?? "").Split(',');
            if (wm.Length >= 1 && float.TryParse(wm[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float times))
            {
                PhysicsBattler.Locomotion.CombatFeel.WeaponsHaveMass = times > 0f;
                if (times > 0f)
                {
                    PhysicsBattler.Locomotion.CombatFeel.WeaponMass = times;
                }

                if (wm.Length >= 2)
                {
                    PhysicsBattler.Locomotion.CombatFeel.MassTakesFromFelt = wm[1].Trim() != "0";
                }

                // (And ",0" after that: the weapon arm no stronger for a heavier weapon.)
                if (wm.Length >= 3)
                {
                    PhysicsBattler.Locomotion.CombatFeel.ArmFollowsMass = wm[2].Trim() != "0";
                }
            }
        }

        // (Heavy weapons: FIGHT_HEFT="2.5,0.7,3" sets how much heavier a blow is felt, how much of its speed a weapon
        // keeps through a body, and how much stronger the weapon arm is through a strike; "1,0,1" is as it was before.)
        {
            var heft = (Environment.GetEnvironmentVariable("FIGHT_HEFT") ?? "").Split(',');
            if (heft.Length >= 3 && float.TryParse(heft[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float hw)
                && float.TryParse(heft[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float hk)
                && float.TryParse(heft[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ha))
            {
                PhysicsBattler.Locomotion.CombatFeel.WeaponWeight = hw;
                PhysicsBattler.Locomotion.CombatFeel.SwingKept = hk;
                PhysicsBattler.Locomotion.CombatFeel.StrikeArmStrength = ha;
                // (And, after them, the fastest a blow drives a body back, the fastest it throws it back stumbling, and the furthest it bends it.)
                if (heft.Length >= 6)
                {
                    PhysicsBattler.Locomotion.CombatFeel.MostDrive = float.Parse(heft[3], System.Globalization.CultureInfo.InvariantCulture);
                    PhysicsBattler.Locomotion.CombatFeel.MostStumble = float.Parse(heft[4], System.Globalization.CultureInfo.InvariantCulture);
                    PhysicsBattler.Locomotion.CombatFeel.MostBend = float.Parse(heft[5], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
        float seconds = float.TryParse(Environment.GetEnvironmentVariable("FIGHT_SECONDS"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fs) ? fs : 240f;
        var result = Play(rigs, seed, seconds, recorder, out var stats);
        Call(recorder, "OnDisable");
        Application.dataPath = assets;
        Console.WriteLine(result);
        Console.WriteLine(stats);
        Console.WriteLine("fight session: " + recorder.SessionPath);
        return 0;
    }

    /// <summary>Plays a fight to its end (or the time limit); the result line, and per-unit figures.</summary>
    private static readonly bool Smalls = Environment.GetEnvironmentVariable("FIGHT_SMALLS") != null;

#if PHYSX
    private static Overlaps overlaps;
#endif

    public static string Play(Dictionary<string, RigFile> rigs, int seed, float limit, PlaytestRecorder recorder, out string stats)
    {
        var grid = new BoardGrid();
        var lineup = new[]
        {
            // (The small units left the battle on 6 November: a medium with a sword only in their place, "Swordsman";
            // FIGHT_SMALLS=1 has them back.)
            new[] { (Smalls ? "Small" : "Swordsman", new Tile(2, 1)), ("Medium", new Tile(3, 1)), ("Large", new Tile(4, 1)) },
            new[] { ("Medium", new Tile(4, 6)), ("Large", new Tile(2, 6)), (Smalls ? "Small" : "Swordsman", new Tile(3, 6)) },
        };
        // (The plan "Fighting off several attackers": FIGHT_LINEUP=gang2 or gang3 sets one medium with sword and shield
        // in the middle on 2 or 3 swordsmen coming from in front, the side and behind.)
        string gang = Environment.GetEnvironmentVariable("FIGHT_LINEUP");
        if (gang == "gang2" || gang == "gang3")
        {
            lineup = new[]
            {
                new[] { ("Medium", new Tile(3, 3)) },
                gang == "gang2"
                    ? new[] { ("Swordsman", new Tile(3, 6)), ("Swordsman", new Tile(6, 3)) }
                    : new[] { ("Swordsman", new Tile(3, 6)), ("Swordsman", new Tile(6, 3)), ("Swordsman", new Tile(3, 0)) },
            };
        }

        if (gang == "duel")
        {
            // (One on one: a Medium with sword and shield against a swordsman.)
            lineup = new[] { new[] { ("Medium", new Tile(3, 2)) }, new[] { ("Swordsman", new Tile(3, 5)) } };
        }

        if (gang == "mediums")
        {
            // (Mediums only, six of them: the new plan's battle. A swordsman and two with sword and shield a side.)
            lineup = new[]
            {
                new[] { ("Swordsman", new Tile(2, 1)), ("Medium", new Tile(3, 1)), ("Medium", new Tile(4, 1)) },
                new[] { ("Medium", new Tile(4, 6)), ("Medium", new Tile(2, 6)), ("Swordsman", new Tile(3, 6)) },
            };
        }

        // (The sandbox's target test: FIGHT_LINEUP=targets sets three attackers each on a target that stands there (one
        // open, one with sword and shield, one with a sword); FIGHT_TARGETS="LM" makes the attackers Larges and leaves
        // the targets Mediums, "ML" the other way, "LL" both.)
        bool targetTest = gang == "targets";
        if (targetTest)
        {
            string who = Environment.GetEnvironmentVariable("FIGHT_TARGETS") ?? "MM";
            bool la = who.Length > 0 && who[0] == 'L', lt = who.Length > 1 && who[1] == 'L';
            lineup = new[]
            {
                new[] { (la ? "Large" : "Swordsman", new Tile(1, 2)), (la ? "Large" : "Swordsman", new Tile(3, 2)), (la ? "Large" : "Swordsman", new Tile(6, 2)) },
                new[] { (lt ? "Large open" : "Open", new Tile(1, 5)), (lt ? "Large shield" : "Medium", new Tile(3, 5)), (lt ? "Large sword" : "Swordsman", new Tile(6, 5)) },
            };
        }

        // (The sandbox's tube test: FIGHT_LINEUP=tubes sets the target test's three targets alone; FIGHT_TUBE="6,60,8,1.2,0.15,0,0"
        // sends a tube across each at 6 s: 60 N·s at 8 m/s, 1.2 m up, its end 0.15 m past the target's middle, the targets
        // turned 0 degrees, the physics only (1: taken as blows too). FIGHT_TARGETS="ML" makes the targets Larges.)
        bool tubeLineup = gang == "tubes";
        if (tubeLineup)
        {
            string who = Environment.GetEnvironmentVariable("FIGHT_TARGETS") ?? "MM";
            bool lt = who.Length > 1 && who[1] == 'L';
            lineup = new[]
            {
                new (string, Tile)[0],
                new[] { (lt ? "Large open" : "Open", new Tile(1, 5)), (lt ? "Large shield" : "Medium", new Tile(3, 5)), (lt ? "Large sword" : "Swordsman", new Tile(6, 5)) },
            };
        }

        if (Environment.GetEnvironmentVariable("FIGHT_MIRROR") != null)
        {
            // (Each team in the other's places, to see whether a side or a slot wins, not the units.)
            var swap = new[] { lineup[1], lineup[0] };
            lineup = new[]
            {
                Array.ConvertAll(swap[0], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
                Array.ConvertAll(swap[1], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
            };
        }

        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising), MoveDefinition.Create(MoveStyle.ClubBackhand), MoveDefinition.Create(MoveStyle.ClubFan), MoveDefinition.Create(MoveStyle.ClubFanBackhand) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        List<UnitController> Raise()
        {
            var raised = new List<UnitController>();
            for (int team = 0; team < 2; team++)
            {
                foreach (var (named, tile) in lineup[team])
                {
                    string body = named.StartsWith("Large") ? "Large" : named == "Small" ? "Small" : "Medium";
                    var profile = UnitProfile.CreateDefault();
                    profile.name = "UP_" + body;
                    profile.body = body;
                    profile.style = CombatStyle.CreateFor(body);
                    if (body == "Small") { profile.maxHealth = 60f; profile.maxPoise = 25f; }
                    if (body == "Large") { profile.maxHealth = 220f; profile.maxPoise = 120f; }
                    var root = UnitBuilder.Build(rigs[body], profile, $"{(team == 0 ? "A" : "B")} {named}", _ => null, 8);
                    root.transform.position = grid.Center(tile);
                    root.transform.rotation = Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f);
                    var unit = root.GetComponent<UnitController>();
                    unit.Team = team;
                    unit.Profile = profile;
                    Call(unit, "Awake");
                    if (named.EndsWith("pen")) unit.Equip(null, null);
                    else if (named == "Swordsman" || named.EndsWith("sword")) unit.Equip(sword, null);
                    else if (named == "Medium" || named.EndsWith("shield")) unit.Equip(sword, shield);
                    else unit.Equip(club, null);
                    raised.Add(unit);
                }
            }

            return raised;
        }

        // (FIGHT_FALSESTART=0.55: as in the sandbox when a fresh set is asked for half a second after the first, which
        // is what the tube test's button does: a first set raised, fought for that long, and destroyed as the sandbox
        // destroys them, before the set the fight is for. What matters is that the first set is then gone from the
        // engine's steps: left told of them in Unity, they threw and the engine was never stepped again.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_FALSESTART"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float falseStart) && falseStart > 0f)
        {
            var first = Raise();
            var firstLoop = new FightLoop(seed) { Grid = grid, Endless = true };
            const float firstDt = 1f / 60f;
            for (float t = 0f; t < falseStart; t += firstDt)
            {
                firstLoop.Step(first, firstDt, t);
                foreach (var unit in first)
                {
                    Call(unit, "FixedUpdate");
                }

                if (PhysicsBattler.Authority.PhysicsSteps.Manual)
                {
                    PhysicsBattler.Authority.PhysicsSteps.Run(firstDt);
                }
                else
                {
                    Physics.Simulate(firstDt);
                }
            }

            int toldWith = PhysicsBattler.Authority.PhysicsSteps.Listeners;
            foreach (var unit in first)
            {
                UnityEngine.Object.Destroy(unit.gameObject);
            }

            Console.WriteLine($"FALSESTART a first set of {first.Count} fought {falseStart:0.00} s and was destroyed: {toldWith} were told of the engine's steps with it, {PhysicsBattler.Authority.PhysicsSteps.Listeners} after (0: none of them is left told)");
        }

        var units = Raise();
        if (Environment.GetEnvironmentVariable("FIGHT_THROWER") != null)
        {
            // (One told of the engine's steps that throws, as a destroyed fighter left told did in Unity: it must be taken off, and the engine go on.)
            PhysicsBattler.Authority.PhysicsSteps.BeforeSubstep += (k, n, sub) => throw new InvalidOperationException("the thrower, told of a physics step");
        }

        if (targetTest)
        {
            // (Laid out as the sandbox lays them out: the pairs side by side, each attacker facing its target.)
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * PhysicsBattler.Sandbox.BattleSandbox.TargetPairsApart;
                var attacker = units[i];
                var target = units[3 + i];
                Vector3 spot = grid.origin + new Vector3(x, 0f, 0.5f * PhysicsBattler.Sandbox.BattleSandbox.TargetFromAttacker);
                target.Teleport(spot, BoardGrid.TeamYaw(1));
                attacker.Teleport(grid.origin + new Vector3(x, 0f, -0.5f * PhysicsBattler.Sandbox.BattleSandbox.TargetFromAttacker), BoardGrid.TeamYaw(0));
                target.TargetDummy = true;
                target.DummySpot = spot;
                target.Unharmed = Environment.GetEnvironmentVariable("FIGHT_TARGETS_HARMED") == null;
                attacker.OnlyTarget = target;
            }
        }

        var legWas = new Dictionary<(UnitController, int), Quaternion>();
        tubes = null;
        if (tubeLineup)
        {
            string[] set = (Environment.GetEnvironmentVariable("FIGHT_TUBE") ?? "6,60,8,1.2,0.15,0,0").Split(',');
            float Set(int i, float otherwise) => i < set.Length && float.TryParse(set[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
            tubes = new PhysicsBattler.Sandbox.TubeTest { Impulse = Set(1, 60f), Speed = Set(2, 8f), Height = Set(3, 1.2f), Reach = Set(4, 0.15f), Turned = Set(5, 0f), AsBlow = Set(6, 0f) > 0.5f, Fells = Set(7, 0f) > 0.5f };
            tubeAt = Set(0, 6f);
            tubeSent = false;
            Vector3 faces = tubes.Facing(Quaternion.Euler(0f, BoardGrid.TeamYaw(0), 0f) * Vector3.forward);
            for (int i = 0; i < units.Count; i++)
            {
                float x = (i - 1) * PhysicsBattler.Sandbox.BattleSandbox.TargetPairsApart;
                Vector3 spot = grid.origin + new Vector3(x, 0f, 0.5f * PhysicsBattler.Sandbox.BattleSandbox.TargetFromAttacker);
                units[i].Teleport(spot, Mathf.Atan2(faces.x, faces.z) * Mathf.Rad2Deg);
                units[i].TargetDummy = true;
                units[i].DummySpot = spot;
                units[i].DummyFace = spot + faces * 5f;
                units[i].Unharmed = true;
            }
        }

        var loop = new FightLoop(seed) { Grid = grid, Endless = targetTest || tubeLineup };
        // (The plan "Bringing the lab into the fight", step 4: CLIP_FIGHT=1 moves the units as clip fighters, their
        // walk measured as the lab's is.)
        var feet = new Dictionary<UnitController, PhysicsBattler.Locomotion.LocoMeasures>();
        var lastRoot = new Dictionary<UnitController, Vector3>();
        // (Feet sliding on the ground: each foot's ankle, while it is down at its standing height, and how far it moves there.)
        var slide = new Dictionary<UnitController, GroundSlide>();
        float kickAt = -1f, kickSpeed = 150f;
        {
            var kick = (Environment.GetEnvironmentVariable("FIGHT_KICK") ?? "").Split(',');
            if (kick.Length == 2 && float.TryParse(kick[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ka)
                && float.TryParse(kick[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ks))
            {
                kickAt = ka;
                kickSpeed = ks;
            }
        }
        var kicked = new HashSet<UnitController>();
        float[] kickFastest = new float[20], kickHead = new float[20];
        var upperSteps = new Dictionary<UnitController, (int on, int all)>();
        var headOffs = new Dictionary<UnitController, List<float>>();
        var upperOffs = new[] { new List<float>(), new List<float>(), new List<float>(), new List<float>(), new List<float>() };
        // (Physical legs: how far each foot is from where the pose has it, held by its lock and not; and each knee.)
        var legOffs = new[] { new List<float>(), new List<float>(), new List<float>() };
        Array.Clear(PhysicsBattler.Locomotion.ClipFighter.RootShoves, 0, 4);
        PhysicsBattler.Locomotion.ClipFighter.RetireSeconds = PhysicsBattler.Locomotion.ClipFighter.RetireFastest = 0f;
        // (FIGHT_HOP=1: a step back is a hop. Off by default since 13 November: its feet are let go as the body leaves them.)
        if (Environment.GetEnvironmentVariable("FIGHT_HOP") != null)
        {
            PhysicsBattler.Locomotion.ClipFighter.StepBackHops = Environment.GetEnvironmentVariable("FIGHT_HOP") == "1";
        }

        // (FIGHT_LEGS=0: the legs go where the pose puts them, as before 14 November. On by default: the plan's step 6.)
        if (Environment.GetEnvironmentVariable("FIGHT_LEGS") != null)
        {
            PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs = Environment.GetEnvironmentVariable("FIGHT_LEGS") == "1";
        }

        // (FIGHT_PINS=0: physical legs without their planted feet pinned. FIGHT_PINSPRING: the pin's spring, N/m.)
        if (Environment.GetEnvironmentVariable("FIGHT_PINS") == "0")
        {
            PhysicsBattler.Authority.UpperBodyArticulation.PinsFeet = false;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_PINSPRING"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pinSpring))
        {
            PhysicsBattler.Authority.UpperBodyArticulation.PinSpring = pinSpring;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_LEGQUICK"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float legQuick))
        {
            PhysicsBattler.Locomotion.ClipFighter.LegQuick = legQuick;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_LEGSTRONG"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float legStrong))
        {
            PhysicsBattler.Locomotion.ClipFighter.LegStrength = legStrong;
        }

        // (FIGHT_HIPS=0: the hips locked to the pose, as before 14 November. FIGHT_HIPQUICK, FIGHT_HIPMOST: how quick the hold
        // is (Hz) and the most it holds with (times the weight). FIGHT_HITFORCE: the blow's force, times its momentum.)
        {
            float Env(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
            PhysicsBattler.Locomotion.ClipFighter.HeldHips = Environment.GetEnvironmentVariable("FIGHT_HIPS") != "0";
            PhysicsBattler.Authority.UpperBodyArticulation.HipQuick = Env("FIGHT_HIPQUICK", 5f);
            PhysicsBattler.Authority.UpperBodyArticulation.HipMost = Env("FIGHT_HIPMOST", 3f);
            PhysicsBattler.Locomotion.CombatFeel.HitForce = Env("FIGHT_HITFORCE", 1f);
        }

        // (FIGHT_FALLS=0: a fall as before 14 November, the body thrown away for a ragdoll. FIGHT_FALLMUSCLE: the share of a
        // person's strength a falling body works with. FIGHT_HEAD=0, FIGHT_REACH=0: the head not kept off the ground, the arms not reaching for it.)
        PhysicsBattler.Locomotion.ClipFighter.OneBodyFalls = Environment.GetEnvironmentVariable("FIGHT_FALLS") != "0";
        PhysicsBattler.Locomotion.ClipFighter.FallMuscle = float.TryParse(Environment.GetEnvironmentVariable("FIGHT_FALLMUSCLE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fallMuscle) ? fallMuscle : 1f;
        PhysicsBattler.Locomotion.FallPose.ProtectsHead = Environment.GetEnvironmentVariable("FIGHT_HEAD") != "0";
        PhysicsBattler.Locomotion.FallPose.Reaches = Environment.GetEnvironmentVariable("FIGHT_REACH") != "0";

        // (FIGHT_FREEARM=feet: a swordsman's carried hand placed by the way its feet face, as it first was; back: behind its hip, as before.)
        PhysicsBattler.Locomotion.SwordParry.FreeArmWithChest = Environment.GetEnvironmentVariable("FIGHT_FREEARM") != "feet";
        PhysicsBattler.Locomotion.SwordParry.FreeArmInFront = Environment.GetEnvironmentVariable("FIGHT_FREEARM") != "back";
        PhysicsBattler.Locomotion.ClipFighter.MeasuresArms = true;

        // (FIGHT_LEGSCLEAR=0: the get-up from its back as before 14 November: the planted foot under the hip, the other swept back through it.)
        PhysicsBattler.Motion.GetUp.LegsClearTheHips = Environment.GetEnvironmentVariable("FIGHT_LEGSCLEAR") != "0";

        // (FIGHT_SWORDUP=0: a sword taken up after a fall from where in the world it was last held, as before 14 November. FIGHT_TURNWAY=0: a guard's turning round played the wrong way, as before.)
        PhysicsBattler.Locomotion.ClipFighter.SwordTakenUpWhereItIs = Environment.GetEnvironmentVariable("FIGHT_SWORDUP") != "0";
        PhysicsBattler.Locomotion.Locomotor.GuardTurnsTheWayAsked = Environment.GetEnvironmentVariable("FIGHT_TURNWAY") != "0";

        // (FIGHT_SETTLE=0: a body that is down goes on reaching, as before.)
        PhysicsBattler.Locomotion.ClipFighter.FallenBodiesSettle = Environment.GetEnvironmentVariable("FIGHT_SETTLE") != "0";

        // (FIGHT_HANDHOLD=0.5: how hard a landed hand is held, in times the body's weight.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_HANDHOLD"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float handHold))
        {
            PhysicsBattler.Authority.UpperBodyArticulation.HandPinMost = handHold * 785f;
        }

        // (FIGHT_HANDSTAY=0: a hand that lands in a fall is not held where it landed.)
        PhysicsBattler.Locomotion.ClipFighter.HandsStayWhereTheyLand = Environment.GetEnvironmentVariable("FIGHT_HANDSTAY") != "0";

        // (FIGHT_CAUGHT=0: no fall is caught.)
        PhysicsBattler.Locomotion.ClipFighter.FallsCanBeCaught = Environment.GetEnvironmentVariable("FIGHT_CAUGHT") != "0";

        // (FIGHT_GETUP=0: the get-ups as they were before 14 November.)
        PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps = Environment.GetEnvironmentVariable("FIGHT_GETUP") != "0";

        // (FIGHT_PIVOT=0: a planted foot that turns with the body goes round about its ankle, as before.)
        if (Environment.GetEnvironmentVariable("FIGHT_PIVOT") == "0")
        {
            PhysicsBattler.Locomotion.LocoBody.PivotsOnBall = false;
        }

        if (int.TryParse(Environment.GetEnvironmentVariable("FIGHT_PIVOTWHEN"), out int pivotsWhen))
        {
            PhysicsBattler.Locomotion.LocoBody.PivotsWhen = pivotsWhen;
        }

        // (FIGHT_RETIRE=0: ground given is snapped in at 4 m/s, as before.)
        if (Environment.GetEnvironmentVariable("FIGHT_RETIRE") == "0")
        {
            PhysicsBattler.Locomotion.ClipFighter.RetiresInTime = false;
        }
        var rootJump = new Dictionary<UnitController, (float most, float at)>();
        var reachedAt = new Dictionary<UnitController, float>();
        var overlapPairs = new Dictionary<string, (int steps, float nearest)>();
        var mateGaps = new List<float>();
        var moving = new Dictionary<UnitController, int>();
        var running = new Dictionary<UnitController, int>();
        var cameAt = new Dictionary<UnitController, float>();
        int targetSteps = 0, aloneSteps = 0;
        var offs = new Dictionary<UnitController, List<float>>();
        foreach (var u in units)
        {
            feet[u] = new PhysicsBattler.Locomotion.LocoMeasures(u.Clip.Anim, u.Clip.Loco, u.Clip.Poser);
            lastRoot[u] = u.Root.position;
            offs[u] = new List<float>();
        }

        float dt = Time.fixedDeltaTime;
        int shared = 0, attacks = 0;
        var started = new Dictionary<UnitController, int>();
        var blowsAt = new List<(float t, UnitController by, UnitController at, float duration)>();
        var tempoTime = new Dictionary<string, float>();
        void OnMove(UnitController u, MoveState s)
        {
            started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1;
            attacks++;
            blowsAt.Add((Time.time, u, s.Target as UnitController, s.Duration));
        }
        UnitController.MoveStarted += OnMove;
        // (Clip fighters' blows, which have no MoveState, and what every blow did: step 5 of the plan.)
        var clipBlows = new Dictionary<UnitController, List<string>>();
        var startedFrom = new Dictionary<string, int>();
        var turning = new List<(UnitController who, UnitController to, float since)>();
        var turnTimes = new List<float>();
        int turnsMissed = 0, outnumberedSteps = 0, surroundedSteps = 0;
        void OnClipStrike(UnitController u, string what, UnitController at)
        {
            started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1;
            attacks++;
            blowsAt.Add((Time.time, u, at, 1f));
            if (!clipBlows.TryGetValue(u, out var l)) clipBlows[u] = l = new List<string>();
            l.Add(what);
            if (at != null)
            {
                Vector3 from = u.Root.position - at.Root.position;
                from.y = 0f;
                float angle = Vector3.Angle(Quaternion.Euler(0f, at.FacingYaw, 0f) * Vector3.forward, from);
                string sector = angle < 45f ? "front" : angle < 135f ? "side" : "back";
                startedFrom[sector] = (startedFrom.TryGetValue(sector, out int sn) ? sn : 0) + 1;
                if (angle > 45f) turning.Add((at, u, Time.time));
            }
        }
        var hitKinds = new Dictionary<UnitController, Dictionary<string, int>>();
        var sectors = new Dictionary<string, int>();
        float damageAll = 0f;
        void OnHit(HitReport h)
        {
            if (h.Attacker == null) return;
            if (h.Struck != null && h.Result.Impulse.sqrMagnitude > 1e-6f)
            {
                // (Where the blow came from, against the way the one struck faced: in front, beside, behind.)
                Vector3 from = h.Attacker.Root.position - h.Struck.Root.position;
                from.y = 0f;
                float angle = Vector3.Angle(Quaternion.Euler(0f, h.Struck.FacingYaw, 0f) * Vector3.forward, from);
                string sector = angle < 45f ? "front" : angle < 135f ? "side" : "back";
                string what = h.Parried ? "parried" : h.Shield ? "blocked" : "landed";
                var seen = h.Struck.ClipThreats != null ? ((List<UnitController.ClipThreat>)null) : null;
                bool sawIt = false;
                foreach (var th in h.Struck.ClipThreats) if (th.Attacker == h.Attacker && th.Seen) sawIt = true;
                what += sawIt ? " (seen)" : " (unseen)";
                string key = sector + " " + what;
                sectors[key] = (sectors.TryGetValue(key, out int sn) ? sn : 0) + 1;
            }

            if (!hitKinds.TryGetValue(h.Attacker, out var k)) hitKinds[h.Attacker] = k = new Dictionary<string, int>();
            string kind = h.Kind ?? "?";
            k[kind] = (k.TryGetValue(kind, out int n) ? n : 0) + 1;
            damageAll += h.Result.Damage;
            if (Environment.GetEnvironmentVariable("FIGHT_HITS") != null)
            {
                Console.WriteLine($"  {Time.time:0.00} {h.Attacker.name} > {h.Struck.name}: {kind}, {h.Part}, {h.Result.Damage:0} damage, {h.Result.Reaction}; {h.Struck.Health:0} hp left");
            }
        }
        UnitController.ClipStrikeStarted += OnClipStrike;
        UnitController.Hit += OnHit;
        // Milestone 10: blows inside combos, and the time from one link's start to the next.
        int comboBlows = 0, combos = 0;
        var linkGaps = new List<float>();
        var contactGaps = new List<float>();
        var comboEnds = new Dictionary<string, int>();
        // (Clip fighters' combos, the plan "Bringing the lab into the fight", step 7: each counted as it starts, with the
        // tempo's reason, and each of its blows inside it.)
        var comboWhy = new Dictionary<string, int>();
        var lastClipContact = new Dictionary<UnitController, float>();
        void OnClipCombo(UnitController u, string what, UnitController at)
        {
            if (!what.StartsWith("a combo")) return;
            combos++;
            comboBlows++;
            int open = what.IndexOf('('), close = what.IndexOf(')');
            string why = open > 0 && close > open ? what.Substring(open + 1, close - open - 1) : "the lab's";
            comboWhy[why] = (comboWhy.TryGetValue(why, out int n) ? n : 0) + 1;
        }
        void OnClipBlow(UnitController u, bool next)
        {
            if (next)
            {
                comboBlows++;
                attacks++;
                if (lastClipContact.TryGetValue(u, out float t0)) linkGaps.Add(Time.time - t0);
            }
            lastClipContact[u] = Time.time;
        }
        UnitController.ClipStrikeStarted += OnClipCombo;
        UnitController.ClipBlowStarted += OnClipBlow;
        void OnCombo(UnitController u, ComboRun c, string what)
        {
            if (what == "started")
            {
                combos++;
                comboBlows++;
            }
            else if (what.StartsWith("link"))
            {
                comboBlows++;
                if (c.LinkStarts.Count >= 2)
                {
                    linkGaps.Add(c.LinkStarts[c.LinkStarts.Count - 1] - c.LinkStarts[c.LinkStarts.Count - 2]);
                }
            }
            else
            {
                // (The strikes' rhythm: from one link's striking moment to the next's.)
                for (int i = 1; i < c.Contacts.Count; i++)
                {
                    contactGaps.Add(c.Contacts[i] - c.Contacts[i - 1]);
                }

                string why = what.Replace("ended: ", "");
                why = why.StartsWith("stopped by") ? (why.EndsWith("'s shield") ? "blocked" : "stopped in a body") : why;
                comboEnds[why] = (comboEnds.TryGetValue(why, out int k) ? k : 0) + 1;
            }
        }
        UnitController.ComboChanged += OnCombo;
        var heft = new HeftMeasure(units);
        var grounds = new GroundMeasure(units);
        var legs = new LegMeasure(units);
        // (Each kind of unit on its own: in the target test, the attackers and each target; else by what it carries.)
        var limits = new LimitMeasure(units, u => targetTest ? (u.TargetDummy ? "the target " + (u.Clip != null ? u.Clip.Stance.ToString().ToLowerInvariant() : "?") : "the attackers") : u.name.Substring(2));
        var dummyOff = new Dictionary<UnitController, (float most, float last)>();
        int steps = (int)(limit * 60f) + 600;
        float startTime = Time.time;
        try
        {
            for (int step = 0; step < steps && !loop.Over; step++)
            {
                Time.time += dt;
                if (recorder != null)
                {
                    Call(recorder, "FixedUpdate");
                }

                loop.TimeLimit = limit;
                loop.Step(units, dt, Time.time);
                heft.Step(dt);
                limits.Step();
                grounds.Step(dt);
                float paceAt = Time.time - startTime;
                if (paceTrace.Length >= 3 && float.TryParse(paceTrace[1], out float paceFrom) && float.TryParse(paceTrace[2], out float paceTo) && paceAt >= paceFrom && paceAt <= paceTo)
                {
                    foreach (var u in units)
                    {
                        if (u.name == paceTrace[0] && u.Clip != null && paceLast.TryGetValue(u.Clip, out var p))
                        {
                            var f = u.Clip;
                            Console.WriteLine($"[pace] {paceAt:0.00} asked {p.asked.magnitude:0.00} walk asked {p.want.magnitude:0.00} {(f.Pacer.Walking ? "walking" : f.Pacer.Stepping ? "stepping" : "standing")} owed {f.Pacer.Owed.magnitude:0.00} | speed {f.Loco.Velocity.magnitude:0.00} {f.Loco.Current} paced {f.Loco.Paced} rate {f.Loco.PhaseRate:0.00} stride {f.Loco.StrideScale:0.00} cadence {f.Loco.Cadence:0.00} down {f.Loco.Down[0]:0}{f.Loco.Down[1]:0}");
                            for (int ft = 0; ft < 2; ft++)
                            {
                                var lk = f.Poser.Feet[ft];
                                var gf = f.Ground.Feet[ft];
                                Console.WriteLine($"[foot] {paceAt:0.00} {(ft == 0 ? "L" : "R")} {gf.State} speed {gf.Speed:0.00} locked {lk.Locked} toe {lk.OnToe} stepping {lk.Stepping} target ({lk.Target.x:0.000},{lk.Target.y:0.000},{lk.Target.z:0.000}) posed ({lk.Posed.x:0.000},{lk.Posed.z:0.000}) sole heel ({gf.Heel.x:0.000},{gf.Heel.y:0.000},{gf.Heel.z:0.000}) toe ({gf.Toe.x:0.000},{gf.Toe.y:0.000},{gf.Toe.z:0.000}) yawhold {lk.YawHold:0.0} phase {f.Loco.Phase:0.00} facing {f.Loco.Facing:0.0} at ({f.Loco.Position.x:0.000},{f.Loco.Position.z:0.000}) down {f.Loco.Down[ft]:0} fade {100f * new Vector2(lk.Fade.x, lk.Fade.z).magnitude:0} cm {100f * lk.Fade.y:0} up, {lk.FadeTime:0.00}/{lk.FadeOver:0.00} lifted {100f * f.Poser.Lifted(ft):0.0} {f.Loco.Current} {(f.Loco.Shot != null ? f.Loco.Shot.Name : "")} {f.Loco.ShotTime:0.00}");
                            }
                        }
                    }
                }
                legs.Step(dt);
                foreach (var u in units)
                {
                    if (u.TargetDummy)
                    {
                        float off = Vector3.Distance(new Vector3(u.Root.position.x, 0f, u.Root.position.z), new Vector3(u.DummySpot.x, 0f, u.DummySpot.z));
                        dummyOff[u] = (Mathf.Max(dummyOff.TryGetValue(u, out var was) ? was.most : 0f, off), off);
                    }
                }
                foreach (var u in units)
                {
                    if (u.IsAlive && u.State == UnitState.Standing && u.Tempo != null && u.TempoOn)
                    {
                        string k = u.Tempo.State.ToString();
                        tempoTime[k] = (tempoTime.TryGetValue(k, out float sec) ? sec : 0f) + dt;
                    }
                }

                foreach (var unit in units)
                {
                    Call(unit, "FixedUpdate");
                }

                // (FIGHT_TURN="3,180": at 3 s every unit turns 180 degrees on the spot, as the debug panel's button has them do.)
                {
                    string[] turn = (Environment.GetEnvironmentVariable("FIGHT_TURN") ?? "").Split(',');
                    if (turn.Length == 2 && float.TryParse(turn[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float turnAt) && paceAt >= turnAt && paceAt < turnAt + dt
                        && float.TryParse(turn[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float turnBy))
                    {
                        foreach (var u in units)
                        {
                            Console.WriteLine($"TURN {paceAt:0.000} {u.name}: {(u.Clip != null && u.Clip.TurnNow(turnBy) ? "turning" : "not turned")}");
                        }
                    }
                }

                // (FIGHT_FACE="4,-170": at 4 s what every target stands facing is turned 170 degrees to its left about its spot, so that it turns to it when it next stands idle: after getting up, as a target does that got up facing another way.)
                {
                    string[] face = (Environment.GetEnvironmentVariable("FIGHT_FACE") ?? "").Split(',');
                    if (face.Length == 2 && float.TryParse(face[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float faceAt) && paceAt >= faceAt && paceAt < faceAt + dt
                        && float.TryParse(face[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float faceBy))
                    {
                        foreach (var u in units)
                        {
                            if (u.DummyFace.HasValue)
                            {
                                u.DummyFace = u.DummySpot + Quaternion.Euler(0f, faceBy, 0f) * (u.DummyFace.Value - u.DummySpot);
                            }
                        }
                    }
                }

                // (FIGHT_LEGTRACE="3,6,sword": from 3 s to 6 s, each step, where the pose has the knees and feet of every unit so named, in its hips' own frame, and how fast each thigh turned.)
                {
                    string[] legt = (Environment.GetEnvironmentVariable("FIGHT_LEGTRACE") ?? "").Split(',');
                    if (legt.Length >= 2 && float.TryParse(legt[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float legFrom) && float.TryParse(legt[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float legTo) && paceAt >= legFrom && paceAt <= legTo)
                    {
                        foreach (var u in units)
                        {
                            if (u.Clip == null || (legt.Length > 2 && !u.name.ToLowerInvariant().Contains(legt[2])))
                            {
                                continue;
                            }

                            var an = u.Clip.Anim;
                            var hips = an.Part(PartId.Pelvis);
                            var hipsIn = Quaternion.Inverse(hips.rotation);
                            string P(Vector3 v) => $"{v.x:0.00} {v.y:0.00} {v.z:0.00}";
                            string line = $"LEG {paceAt:0.000} {u.name}: {u.Clip.Footing} {u.Clip.Loco.Current} root {u.Clip.Loco.Position.x:0.00} {u.Clip.Loco.Position.z:0.00} facing {u.Clip.Loco.Facing:0} pelvis {hips.position.x:0.00} {hips.position.z:0.00} feet {an.Part(PartId.FootL).position.x:0.00} {an.Part(PartId.FootL).position.z:0.00} / {an.Part(PartId.FootR).position.x:0.00} {an.Part(PartId.FootR).position.z:0.00} hips {hips.position.y:0.00} up {hips.up.y:0.00}";
                            for (int f = 0; f < 2; f++)
                            {
                                var thigh = an.Part(f == 0 ? PartId.ThighL : PartId.ThighR);
                                var shin = an.Part(f == 0 ? PartId.ShinL : PartId.ShinR);
                                var foot = an.Part(f == 0 ? PartId.FootL : PartId.FootR);
                                var key = (u, f);
                                float turned = legWas.TryGetValue(key, out var was) ? Quaternion.Angle(was, hipsIn * thigh.rotation) * Mathf.Deg2Rad / dt : 0f;
                                legWas[key] = hipsIn * thigh.rotation;
                                var tid = f == 0 ? PartId.ThighL : PartId.ThighR;
                                float past = PhysicsBattler.Muscles.JointMath.PastLimits(Quaternion.Inverse(an.RestLocalRotation(tid)) * thigh.localRotation, PhysicsBattler.Muscles.JointLimitTable.For(tid), out float pitch, out float aboutY, out float aboutZ);
                                line += $" | {(f == 0 ? "L" : "R")} knee {P(hipsIn * (shin.position - thigh.position))} foot {P(hipsIn * (foot.position - thigh.position))} thigh {turned:0.0} rad/s, raised {-pitch:0} twist {aboutY:0} out {aboutZ:0} (past {past:0})";
                            }

                            Console.WriteLine(line);
                        }
                    }
                }

                // (FIGHT_ARMTRACE="3,5,sword": from 3 s to 5 s, each step, where the pose has the hands of every unit so named, in the frame of the way its feet face.)
                {
                    string[] arm = (Environment.GetEnvironmentVariable("FIGHT_ARMTRACE") ?? "").Split(',');
                    if (arm.Length >= 2 && float.TryParse(arm[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float armFrom) && float.TryParse(arm[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float armTo) && paceAt >= armFrom && paceAt <= armTo)
                    {
                        foreach (var u in units)
                        {
                            if (u.Clip == null || (arm.Length > 2 && !u.name.ToLowerInvariant().Contains(arm[2])))
                            {
                                continue;
                            }

                            var an = u.Clip.Anim;
                            var back = Quaternion.Inverse(Quaternion.Euler(0f, u.Clip.Loco.Facing, 0f));
                            Vector3 In(Vector3 world) => back * (world - u.Clip.Loco.Position);
                            string P(Vector3 v) => $"{v.x:0.00} {v.y:0.00} {v.z:0.00}";
                            var chestIn = Quaternion.Inverse(an.Part(PartId.Chest).rotation);
                            Vector3 blade = an.SlotWeaponR != null ? an.SlotWeaponR.forward : Vector3.zero;
                            float chestYaw = Mathf.DeltaAngle(u.Clip.Loco.Facing, Mathf.Atan2(an.Part(PartId.Chest).forward.x, an.Part(PartId.Chest).forward.z) * Mathf.Rad2Deg);
                            var up = u.Clip.Upper;
                            Console.WriteLine($"ARM {paceAt:0.000} {u.name}: facing {u.Clip.Loco.Facing:0} {u.Clip.Loco.Current} chest {chestYaw:0} | pelvis {P(In(an.Part(PartId.Pelvis).position))} | handR {P(In(an.Part(PartId.HandR).position))} by chest {P(chestIn * (an.Part(PartId.HandR).position - an.Part(PartId.Chest).position))} blade {P(chestIn * blade)} | handL by chest {P(chestIn * (an.Part(PartId.HandL).position - an.Part(PartId.Chest).position))}{(up != null ? $" | off {100f * up.Off(PartId.HandL):0}/{100f * up.Off(PartId.HandR):0} cm" : "")}");
                        }
                    }
                }

                // (FIGHT_STEPBACK="5,7,9": at those seconds every unit steps back from its opponent, as the debug panel's button has them do.)
                foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_STEPBACK") ?? "").Split(','))
                {
                    if (float.TryParse(at, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float backAt) && paceAt >= backAt && paceAt < backAt + dt)
                    {
                        int did = 0;
                        foreach (var u in units)
                        {
                            if (u.Clip != null && u.IsAlive && u.Opponent != null && u.Opponent.Clip != null && u.Clip.StepBackNow(u.Opponent.Clip.Loco.Position, 0.35f, 0.1f))
                            {
                                did++;
                                Console.WriteLine($"STEPBACK {paceAt:0.000} {u.name}");
                            }
                        }
                    }
                }

                if (tubes != null)
                {
                    // (The tube test: the tubes set a second before, sent at their time, and for a second and a half after
                    // each target's body against its pose, and how far it has gone from its spot.)
                    if (!tubeSent && !tubes.Armed && paceAt >= tubeAt - 1f)
                    {
                        tubes.Arm(units, Quaternion.Euler(0f, BoardGrid.TeamYaw(0), 0f) * Vector3.forward);
                    }

                    if (!tubeSent && paceAt >= tubeAt)
                    {
                        tubeSent = true;
                        tubeFrom.Clear();
                        foreach (var u in units)
                        {
                            var up0 = u.Clip?.Upper;
                            if (up0 != null)
                            {
                                tubeFrom[u] = (up0.Link(PartId.Pelvis).transform.position, up0.Link(PartId.Chest).transform.position, up0.Link(PartId.Head).transform.position);
                            }
                        }
                        Console.WriteLine($"TUBE {paceAt:0.000} sent {tubes.Launch()}: {tubes.Mass:0.0} kg at {tubes.Speed:0.0} m/s ({tubes.Impulse:0} N·s), {tubes.Height:0.00} m up, reach {tubes.Reach:0.00} m, at {tubes.Side} (turned {tubes.Turned:0}), {(tubes.AsBlow ? "as blows" : "physics only")}, legs {(PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs ? "physical" : "posed")}");
                    }

                    tubes.Step(dt);
                    int since = Mathf.RoundToInt((paceAt - tubeAt) / dt);
                    if (tubeSent && since >= 0 && since <= 30 && since % 2 == 0 && Environment.GetEnvironmentVariable("FIGHT_TUBETRACE") == "1")
                    {
                        Console.WriteLine($"TUBE at +{paceAt - tubeAt:0.000}: {tubes.State}; the first target at ({units[0].Clip.Loco.Position.x:0.00},{units[0].Clip.Loco.Position.z:0.00})");
                    }

                    if (tubeSent && since >= 0 && since <= 90 && since % 6 == 0)
                    {
                        foreach (var u in units)
                        {
                            var up = u.Clip != null ? u.Clip.Upper : null;
                            string body = up == null ? (u.Clip != null && u.Clip.Fallen ? "down" : "no physical body")
                                : $"hips {(up.Supported ? 100f * up.HipsOff : 0f):0.0} chest {100f * up.Off(PartId.Chest):0.0} head {100f * up.Off(PartId.Head):0.0} hands {100f * up.Off(PartId.HandL):0.0}/{100f * up.Off(PartId.HandR):0.0}"
                                    + (up.HasLegs ? $" knees {100f * up.Off(PartId.ShinL):0.0}/{100f * up.Off(PartId.ShinR):0.0} feet {100f * up.Off(PartId.FootL):0.0}/{100f * up.Off(PartId.FootR):0.0}" : "") + " cm off its pose";
                            Console.WriteLine($"TUBE +{paceAt - tubeAt:0.00} {u.name}: {body}; {100f * Vector3.Distance(u.Clip.Loco.Position, u.DummySpot):0} cm from its spot; {(u.Clip.Upper != null && tubeFrom.TryGetValue(u, out var from0) ? $"moved since sent: hips {100f * Vector3.Distance(u.Clip.Upper.Link(PartId.Pelvis).transform.position, from0.Item1):0.0} chest {100f * Vector3.Distance(u.Clip.Upper.Link(PartId.Chest).transform.position, from0.Item2):0.0} head {100f * Vector3.Distance(u.Clip.Upper.Link(PartId.Head).transform.position, from0.Item3):0.0} cm; up: hips {100f * u.Clip.Upper.Link(PartId.Pelvis).transform.position.y:0} chest {100f * u.Clip.Upper.Link(PartId.Chest).transform.position.y:0} head {100f * u.Clip.Upper.Link(PartId.Head).transform.position.y:0} hands {100f * u.Clip.Upper.Link(PartId.HandL).transform.position.y:0}/{100f * u.Clip.Upper.Link(PartId.HandR).transform.position.y:0} feet {100f * u.Clip.Upper.Link(PartId.FootL).transform.position.y:0}/{100f * u.Clip.Upper.Link(PartId.FootR).transform.position.y:0} cm; " : "")}{u.Clip.LastReaction}");
                        }
                    }

                    if (tubeSent && tubes.Report != tubeMet)
                    {
                        tubeMet = tubes.Report;
                        Console.WriteLine($"TUBE met at +{paceAt - tubeAt:0.00}: {tubeMet}");
                    }
                }

                // (FIGHT_LEGKNOCK="5,7,9": at those seconds every unit's right shin is knocked sideways at 6 m/s, as the debug panel's button does.)
                foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_LEGKNOCK") ?? "").Split(','))
                {
                    if (float.TryParse(at, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float knockAt) && paceAt >= knockAt && paceAt < knockAt + dt)
                    {
                        foreach (var u in units)
                        {
                            if (u.Clip != null && u.IsAlive && u.Clip.KnockLegNow(6f))
                            {
                                Console.WriteLine($"LEGKNOCK {paceAt:0.000} {u.name}");
                            }
                        }
                    }
                }

                if (blowUp.Length >= 2 && float.TryParse(blowUp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float blowAt)
                    && paceAt >= blowAt && paceAt < blowAt + 3.5f * dt)
                {
                    float pull = blowUp.Length >= 3 && float.TryParse(blowUp[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float n) ? n : 200000f;
                    foreach (var u in units)
                    {
                        var hand = u.name == blowUp[0] && u.Clip != null && u.Clip.Upper != null ? u.Clip.Upper.Link(PartId.HandR) : null;
                        if (hand != null)
                        {
                            hand.AddForce(new Vector3(0.6f, -0.2f, -0.8f).normalized * pull);
                            Console.WriteLine($"BLOWUP {paceAt:0.000} {u.name}: its sword hand pulled with {pull:0} N; pelvis at {u.Clip.Upper.Link(PartId.Pelvis).transform.position}");
                            break;
                        }
                    }
                }

                // (The engine under the stubs: every script's step, then the physics step, as in Unity.)
                if (PhysicsBattler.Authority.PhysicsSteps.Manual)
                {
                    PhysicsBattler.Authority.PhysicsSteps.Run(dt);
                }
                else
                {
                    Physics.Simulate(dt);
                }
#if PHYSX
                if (Physics.Backend is PhysicsBattler.Engine.PhysXWorld px)
                {
                    overlaps ??= new Overlaps(px);
                    overlaps.Step(units);
                }
#endif

                // (How long a unit took to face an attacker that started a blow from its side or behind.)
                for (int k = turning.Count - 1; k >= 0; k--)
                {
                    var (who, to, since) = turning[k];
                    Vector3 from = to.Root.position - who.Root.position;
                    from.y = 0f;
                    float off = Vector3.Angle(Quaternion.Euler(0f, who.FacingYaw, 0f) * Vector3.forward, from);
                    if (off < 30f) { turnTimes.Add(Time.time - since); turning.RemoveAt(k); }
                    else if (!who.IsAlive || !to.IsAlive || Time.time - since > 3f) { turnsMissed++; turning.RemoveAt(k); }
                }

                // (How spread round a unit its attackers stood: the plan "Fighting off several attackers", step 5.)
                foreach (var u in units)
                {
                    if (!u.IsAlive || u.IsDown) continue;
                    var on = FightLoop.OnIt(units, u, UnitController.ClipEngagedWithin * u.Body.SizeFactor);
                    if (on.Count < 2) continue;
                    outnumberedSteps++;
                    if (FightLoop.Spread(u.Root.position, on) > 120f) surroundedSteps++;
                }

                // (How they spread: each standing one's nearest teammate, and whether its target has others of its side on it.)
                foreach (var u in units)
                {
                    if (!u.IsAlive || u.State != UnitState.Standing) continue;
                    float nearestMate = 99f;
                    foreach (var o in units)
                    {
                        if (o != u && o.IsAlive && o.Team == u.Team && o.State == UnitState.Standing)
                            nearestMate = Mathf.Min(nearestMate, Vector3.Distance(FightLoop.BodyAt(u), FightLoop.BodyAt(o)));
                    }
                    if (nearestMate < 90f) mateGaps.Add(nearestMate);
                    if (u.Opponent != null && u.Opponent.IsAlive)
                    {
                        int sharing = 0;
                        foreach (var o in units)
                        {
                            if (o != u && o.IsAlive && o.Team == u.Team && o.Opponent == u.Opponent) sharing++;
                        }
                        targetSteps++;
                        if (sharing == 0) aloneSteps++;
                    }
                }

                float t = Time.time - startTime;
                foreach (var u in units)
                {
                    if (!feet.TryGetValue(u, out var m))
                    {
                        continue;
                    }

                    if (u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.Cycle && u.Clip.Loco.Velocity.magnitude > 0.3f)
                    {
                        moving[u] = (moving.TryGetValue(u, out var mv) ? mv : 0) + 1;
                        if (u.Clip.Loco.Running) running[u] = (running.TryGetValue(u, out var rn) ? rn : 0) + 1;
                    }

                    // (The side it first came at its target from: the angle between its way in and the line between the teams.)
                    if (!cameAt.ContainsKey(u) && u.Opponent != null && Vector3.Distance(u.Root.position, u.Opponent.Root.position) < 2.2f * u.Body.SizeFactor)
                    {
                        Vector3 axis = (u.Team == 0 ? 1f : -1f) * Vector3.forward;
                        Vector3 way = u.Opponent.Root.position - u.Root.position;
                        way.y = 0f;
                        cameAt[u] = Vector3.Angle(axis, way);
                    }

                    if (!slide.TryGetValue(u, out var gs)) slide[u] = gs = new GroundSlide(u.Clip.Anim, u.Clip.Loco) { Poser = u.Clip.Poser, Name = u.name };
                    gs.Sample(u.IsAlive && !u.IsDown, dt, t);
                    if (u.IsAlive && !u.IsDown)
                    {
                        upperSteps.TryGetValue(u, out var us);
                        upperSteps[u] = (us.on + (u.Clip.Upper != null ? 1 : 0), us.all + 1);
                        if (u.Clip.Upper != null)
                        {
                            var up = u.Clip.Upper;
                            // (FIGHT_KICK="4,150": at 4 s every joint of every physical upper body is set turning at 150 rad/s,
                            // as a body thrown out of hand is; and for 2 s after, how fast its fastest joint still turns.)
                            if (kickAt >= 0f && t >= kickAt && t < kickAt + 2f)
                            {
                                if (!kicked.Contains(u))
                                {
                                    kicked.Add(u);
                                    up.Kick(kickSpeed);
                                }
                                else
                                {
                                    float fastest = up.FastestJoint(out var which);
                                    int slot = Mathf.Clamp(Mathf.FloorToInt((t - kickAt) * 10f), 0, 19);
                                    kickFastest[slot] = Mathf.Max(kickFastest[slot], fastest);
                                    kickHead[slot] = Mathf.Max(kickHead[slot], up.Holds(PhysicsBattler.Rig.PartId.Head) ? up.Off(PhysicsBattler.Rig.PartId.Head) * 100f : 0f);
                                }
                            }

                            bool striking = u.Clip.Striking;
                            if (up.Holds(PhysicsBattler.Rig.PartId.HandR)) (striking ? upperOffs[0] : upperOffs[1]).Add(up.Off(PhysicsBattler.Rig.PartId.HandR) * 100f);
                            if (up.Holds(PhysicsBattler.Rig.PartId.HandL)) upperOffs[2].Add(up.Off(PhysicsBattler.Rig.PartId.HandL) * 100f);
                            if (up.HasLegs)
                            {
                                for (int ft = 0; ft < 2; ft++)
                                {
                                    var footId = ft == 0 ? PhysicsBattler.Rig.PartId.FootL : PhysicsBattler.Rig.PartId.FootR;
                                    var shinId = ft == 0 ? PhysicsBattler.Rig.PartId.ShinL : PhysicsBattler.Rig.PartId.ShinR;
                                    if (up.Holds(footId)) (u.Clip.Poser.Feet[ft].Locked ? legOffs[0] : legOffs[1]).Add(up.Off(footId) * 100f);
                                    if (up.Holds(shinId)) legOffs[2].Add(up.Off(shinId) * 100f);
                                }
                            }
                            if (up.Holds(PhysicsBattler.Rig.PartId.Head))
                            {
                                upperOffs[3].Add(up.Off(PhysicsBattler.Rig.PartId.Head) * 100f);
                                if (!headOffs.TryGetValue(u, out var ho)) headOffs[u] = ho = new List<float>();
                                ho.Add(up.Off(PhysicsBattler.Rig.PartId.Head) * 100f);
                            }
                            if (up.Turns(PhysicsBattler.Rig.PartId.HandL)) upperOffs[4].Add(up.GripOff(PhysicsBattler.Rig.PartId.HandL));
                            string watch = Environment.GetEnvironmentVariable("FIGHT_WATCH");
                            if (up.Holds(PhysicsBattler.Rig.PartId.Head) && (watch != null ? u.name == watch
                                : Environment.GetEnvironmentVariable("FIGHT_STRAY") == "1"
                                && (up.Off(PhysicsBattler.Rig.PartId.Head) > 0.25f || up.GripOff(PhysicsBattler.Rig.PartId.HandL) > 60f || (up.Holds(PhysicsBattler.Rig.PartId.HandR) && up.Off(PhysicsBattler.Rig.PartId.HandR) > 0.8f))))
                            {
                                UnitController near = null;
                                foreach (var o in units)
                                {
                                    if (o != u && o.IsAlive && (near == null || (o.Root.position - u.Root.position).sqrMagnitude < (near.Root.position - u.Root.position).sqrMagnitude)) near = o;
                                }

                                string touching = "";
#if PHYSX
                                if (Physics.Backend is PhysicsBattler.Engine.PhysXWorld pw)
                                {
                                    var mine = new HashSet<Collider>(up.Colliders);
                                    var seen = new HashSet<string>();
                                    pw.KeepTouched = true;
                                    foreach (var c in watch != null ? pw.Touched : pw.Contacts)
                                    {
                                        if (c.A == null || c.B == null || mine.Contains(c.A) == mine.Contains(c.B)) continue;
                                        var own = mine.Contains(c.A) ? c.A : c.B;
                                        var other = mine.Contains(c.A) ? c.B : c.A;
                                        var top = other.transform;
                                        while (top.parent != null && top.parent.parent != null) top = top.parent;
                                        seen.Add($"{own.transform.parent.name}/{own.name} on {top.name}/{other.name}{(other.attachedRigidbody != null ? (other.attachedRigidbody.isKinematic ? " (kinematic)" : " (free)") : "")}");
                                    }

                                    touching = "; touching: " + string.Join(", ", seen);
                                    if (watch != null)
                                    {
                                        pw.Touched.Clear();
                                        touching += "\n      " + up.Describe(PhysicsBattler.Rig.PartId.Spine) + "; " + up.Describe(PhysicsBattler.Rig.PartId.Chest) + "; " + up.Describe(PhysicsBattler.Rig.PartId.UpperArmR)
                                            + "; " + up.Describe(PhysicsBattler.Rig.PartId.ForearmR) + "; " + up.Describe(PhysicsBattler.Rig.PartId.HandR) + "\n      " + up.Describe(PhysicsBattler.Rig.PartId.UpperArmL)
                                            + "; " + up.Describe(PhysicsBattler.Rig.PartId.ForearmL) + "; " + up.Describe(PhysicsBattler.Rig.PartId.HandL) + "; " + up.DescribeGrips() + "; " + up.Describe(PhysicsBattler.Rig.PartId.Head);
                                    }
                                }
#endif
                                Console.WriteLine($"STRAY {t:0.000} {u.name} at {u.Root.position.x:0.0},{u.Root.position.z:0.0}: head {up.Off(PhysicsBattler.Rig.PartId.Head) * 100f:0} cm, chest {up.Off(PhysicsBattler.Rig.PartId.Chest) * 100f:0} cm, "
                                    + $"hands {(up.Holds(PhysicsBattler.Rig.PartId.HandR) ? up.Off(PhysicsBattler.Rig.PartId.HandR) * 100f : -1f):0} / {(up.Holds(PhysicsBattler.Rig.PartId.HandL) ? up.Off(PhysicsBattler.Rig.PartId.HandL) * 100f : -1f):0} cm, grip {up.GripOff(PhysicsBattler.Rig.PartId.HandL):0}°; jumps {up.Jumps}; "
                                    + $"{u.Clip.Loco.Current} {(u.Clip.Loco.Shot != null ? u.Clip.Loco.Shot.Name : "")} {u.Clip.LastReaction}; torso ease pin {u.Clip.Authority.Pin(PhysicsBattler.Authority.BodyRegion.Torso):0.00}; "
                                    + $"nearest {(near != null ? $"{near.name} {Vector3.Distance(near.Root.position, u.Root.position):0.00} m off, {(near.Team == u.Team ? "same side" : "enemy")}, {near.Clip.Loco.Current} {(near.Clip.Loco.Shot != null ? near.Clip.Loco.Shot.Name : "")}" : "none")}{touching}");
                            }
                        }
                    }
                    m.Sample(dt, t, u.Clip.Loco.Current + (u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && u.Clip.Loco.Shot != null ? " " + u.Clip.Loco.Shot.Name : "") + (u.Clip.Fallen ? " fallen" : "") + (u.Clip.Striking ? " striking" : "") + (u.Clip.Stumbling ? " stumbling" : ""));
                    float jump = Vector3.Distance(u.Root.position, lastRoot[u]);
                    lastRoot[u] = u.Root.position;
                    if (!rootJump.TryGetValue(u, out var j) || jump > j.most)
                    {
                        rootJump[u] = (jump, t);
                    }

                    // (How far it is from where it would strike its opponent from: the fight's engage distance.)
                    if (u.Opponent != null)
                    {
                        Vector3 d = u.Opponent.Root.position - u.Root.position;
                        float off = new Vector2(d.x, d.z).magnitude - (u.Style.engageDistance * u.Body.SizeFactor + 0.25f * u.Opponent.Body.SizeFactor);
                        if (off < 0.35f * u.Body.SizeFactor && !reachedAt.ContainsKey(u))
                        {
                            reachedAt[u] = t;
                        }

                        if (reachedAt.ContainsKey(u))
                        {
                            offs[u].Add(off);
                        }
                    }
                }

                OnStep?.Invoke(units, dt);

                // Two standing bodies overlapping (free roam: the steering should keep them apart).
                int overlapping = FightLoop.Overlapping(units);
                if (overlapping > 0)
                {
                    for (int i = 0; i < units.Count; i++)
                    {
                        for (int j = i + 1; j < units.Count; j++)
                        {
                            float apart = Vector3.Distance(FightLoop.BodyAt(units[i]), FightLoop.BodyAt(units[j]));
                            if (units[i].IsAlive && units[j].IsAlive && apart < 0.8f * (FightLoop.BodyRadius(units[i]) + FightLoop.BodyRadius(units[j])))
                            {
                                string key = units[i].name + " & " + units[j].name;
                                if (Environment.GetEnvironmentVariable("FIGHT_OVERLAP_TRACE") != null && step % 6 == 0)
                                {
                                    string W(UnitController u) => $"{u.name} want {u.ClipWant.magnitude:0.00} ({Vector3.Dot(u.ClipWant, (FightLoop.BodyAt(u) - FightLoop.BodyAt(u == units[i] ? units[j] : units[i])).normalized):+0.00;-0.00} away) walks {u.Clip.Loco.Velocity.magnitude:0.00} {u.Clip.Loco.Current} circling {u.CircleOpponent}";
                                    Console.WriteLine($"  {Time.time - startTime:0.00} {key} {apart:0.00} m: {W(units[i])} | {W(units[j])}");
                                }

                                overlapPairs[key] = (overlapPairs.TryGetValue(key, out var op) ? op.steps : 0) + 1 is int k ? (k, Mathf.Min(op.nearest == 0f ? 9f : op.nearest, apart)) : op;
                            }
                        }
                    }
                }
                if (overlapping > 0 && shared++ % 120 == 0 && Environment.GetEnvironmentVariable("FIGHT_DEBUG") != null)
                {
                    Console.WriteLine($"  t {Time.time - startTime:0.0}: {overlapping} pairs of bodies overlapping");
                }
            }
        }
        finally
        {
            UnitController.MoveStarted -= OnMove;
            UnitController.ClipStrikeStarted -= OnClipStrike;
            UnitController.ClipStrikeStarted -= OnClipCombo;
            UnitController.ClipBlowStarted -= OnClipBlow;
            UnitController.Hit -= OnHit;
            UnitController.ComboChanged -= OnCombo;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"  clip fighters ({(UnitController.ClipBlows ? "step 5: striking" : "step 4: moving only")}): {damageAll:0} damage dealt in all; bodies moved straight out of each other {UnitController.ClipDeepMoves} times, {UnitController.ClipDeepMoved:0.00} m in all");
        if (sectors.Count > 0)
        {
            var keys = new List<string>(sectors.Keys);
            keys.Sort();
            sb.AppendLine("    blows met, by where they came from (against the facing of the one struck): " + string.Join(", ", keys.ConvertAll(k => $"{k} {sectors[k]}")));
            sb.Append(heft.Report());
            sb.Append(limits.Report());
            sb.Append(grounds.Report());
            if (paceHist.Sum() > 0)
            {
                int n = paceHist.Sum();
                sb.AppendLine("      the steering's velocity asked in guard with no clip playing, by speed (m/s at a Medium's size, share of the time): " + string.Join(", ", paceHist.Select((c, i) => $"{0.1f * i:0.0}: {100f * c / n:0}%")));
            }
            sb.Append(legs.Report());
            if (dummyOff.Count > 0)
            {
                sb.AppendLine("    the target test's targets, off their spots: " + string.Join("; ", units.Where(u => u.TargetDummy).Select(u =>
                    $"{u.name} ({(u.Clip != null ? u.Clip.Stance.ToString().ToLowerInvariant() : "?")}) at most {dummyOff[u].most:0.00} m, {dummyOff[u].last:0.00} m at the end, health {u.Health:0} of {u.Profile.maxHealth:0}{(u.IsAlive ? "" : ", dead")}")));
            }
            sb.AppendLine($"    outnumbered (two or more on it): {outnumberedSteps * dt:0.0} s in all; with them more than 120 degrees apart round it {100f * surroundedSteps / Mathf.Max(1, outnumberedSteps):0}% of that");
            turnTimes.Sort();
            sb.AppendLine($"    turning to an attacker starting a blow from beside or behind: {turnTimes.Count} faced it (median {(turnTimes.Count > 0 ? turnTimes[turnTimes.Count / 2] : 0f):0.00} s, 90% {(turnTimes.Count > 0 ? turnTimes[turnTimes.Count * 9 / 10] : 0f):0.00} s), {turnsMissed} not within 3 s");
            sb.AppendLine("    blows started, by where the attacker stood (against its target's facing): " + string.Join(", ", new List<string>(startedFrom.Keys).ConvertAll(k => $"{k} {startedFrom[k]}")));
        }

        mateGaps.Sort();
        if (mateGaps.Count > 0)
        {
            sb.AppendLine($"    spread: nearest teammate median {mateGaps[mateGaps.Count / 2]:0.00} m (10% {mateGaps[mateGaps.Count / 10]:0.00}, least {mateGaps[0]:0.00}); a unit alone on its target {100f * aloneSteps / Mathf.Max(1, targetSteps):0}% of the time");
        }
        foreach (var u in units)
        {
            if (hitKinds.TryGetValue(u, out var k) || clipBlows.ContainsKey(u))
            {
                var parts = new List<string>();
                if (k != null) foreach (var pair in k) parts.Add($"{pair.Key} {pair.Value}");
                int combosThrown = clipBlows.TryGetValue(u, out var l) ? l.FindAll(x => x.StartsWith("a combo")).Count : 0;
                sb.AppendLine($"    {u.name}: {(l != null ? l.Count : 0)} blows ({combosThrown} combos); its hits: {(parts.Count > 0 ? string.Join(", ", parts) : "none")}; it fell {u.Clip.Falls - (u.IsAlive ? 0 : 1)} times alive ({u.Clip.ThrownDown} of them thrown faster than its legs can catch), was thrown back stumbling {u.Clip.Stumbles} times; concentration at most {u.MostConcentration:0.00}; combo blows turned on another {u.ClipComboTurns}{(u.IsAlive ? "" : ", killed")}");
            }
        }

        if (PhysicsBattler.Locomotion.ClipFighter.PhysicalUpper)
        {
            string Spread(List<float> l, string unit)
            {
                if (l.Count == 0) return "none";
                l.Sort();
                return $"half the time under {l[l.Count / 2]:0.0}, 95% under {l[Mathf.Min(l.Count - 1, (int)(l.Count * 0.95f))]:0.0}, 99% under {l[Mathf.Min(l.Count - 1, (int)(l.Count * 0.99f))]:0.0}, at most {l[l.Count - 1]:0.0} {unit}";
            }

            sb.AppendLine("    borne down (a body pressed so far off its pose that it fell): " + string.Join(", ", units.Select(u => $"{u.name} {u.Clip.BorneDown}")));
            sb.AppendLine("    planted feet that began to pivot on the ball of the foot: " + string.Join(", ", units.Select(u => $"{u.name} {u.Clip.Poser.Pivots}")));
            var fell = units.Where(u => u.Clip != null).SelectMany(u => u.Clip.FallsMeasured.Select(f => (u, f))).ToList();
            if (fell.Count > 0)
            {
                var living = fell.Where(x => !x.f.Dead).Select(x => x.f).ToList();
                sb.AppendLine($"    falls {(PhysicsBattler.Locomotion.ClipFighter.OneBodyFalls ? $"in the one body, muscle {PhysicsBattler.Locomotion.ClipFighter.FallMuscle:0.00}" : "as ragdolls, as before")}: in the one body {units.Sum(u => u.Clip != null ? u.Clip.OneBodyFallen : 0)}; a joint furthest from the fall's pose: " + string.Join(", ", units.Where(u => u.Clip?.Upper != null).Select(u => $"{u.Clip.Upper.FallOffMostPart} {u.Clip.Upper.FallOffMost:0}")) + " degrees");
                sb.AppendLine($"    hands held where they landed in a fall: {units.Sum(u => u.Clip != null ? u.Clip.HandsHeld : 0)}; came away again: {units.Sum(u => u.Clip != null ? u.Clip.HandsCameAway : 0)}; dragged over the ground {100f * units.Sum(u => u.Clip != null ? u.Clip.HandsDragged : 0f):0} cm in all");
                sb.AppendLine($"    falls caught on a knee and a hand: {units.Sum(u => u.Clip != null ? u.Clip.Caught : 0)}; begun so and fallen after all: {units.Sum(u => u.Clip != null ? u.Clip.CaughtAndLost : 0)}");
                sb.AppendLine($"    falls measured: {fell.Count} ({living.Count} of the living). Of the living: the head reached the ground in {living.Count(f => f.HeadTouched)}{(living.Any(f => f.HeadTouched) ? $" (at {living.Where(f => f.HeadTouched).Average(f => f.HeadSpeed):0.0} m/s on average, {living.Where(f => f.HeadTouched).Max(f => f.HeadSpeed):0.0} at most)" : "")}; landed on a hand or knee before the hips, trunk or head in {living.Count(f => f.Braced)}; the hips landed at {(living.Any(f => f.HipsAt >= 0f) ? living.Where(f => f.HipsAt >= 0f).Average(f => f.HipsSpeed) : 0f):0.0} m/s on average; slid {(living.Count > 0 ? 100f * living.Average(f => f.Slid) : 0f):0} cm on average; their hands went {(living.Count > 0 ? 100f * living.Average(f => f.HandsSlid) : 0f):0} cm over the ground on average and left it again {living.Sum(f => f.HandsLifted)} times in all");
                foreach (var way in fell.Where(x => !x.f.Dead).GroupBy(x => x.f.Way))
                {
                    sb.AppendLine($"      {way.Key}: {way.Count()}, the head down in {way.Count(x => x.f.HeadTouched)}, on a hand or knee first in {way.Count(x => x.f.Braced)}");
                }

                foreach (var (u, f) in fell.Take(40))
                {
                    sb.AppendLine($"      {u.name} {f.Text}");
                }
            }

            sb.AppendLine($"    hips {(PhysicsBattler.Locomotion.ClipFighter.HeldHips ? $"held ({PhysicsBattler.Authority.UpperBodyArticulation.HipQuick:0.0} Hz, at most {PhysicsBattler.Authority.UpperBodyArticulation.HipMost:0.0} weights)" : "locked")}: off the pose on average / at most (cm): " + string.Join(", ", units.Where(u => u.Clip != null).Select(u => $"{u.name} {(u.Clip.HipsOffSteps > 0 ? 100f * u.Clip.HipsOffSum / u.Clip.HipsOffSteps : 0f):0.0} / {100f * u.Clip.HipsOffMost:0.0}")));
            sb.AppendLine($"    a blow's force ({PhysicsBattler.Locomotion.CombatFeel.HitForce:0.0} times its momentum): " + string.Join(", ", units.Where(u => u.Clip != null).Select(u => $"{u.name} {u.Clip.HitForces} blows, {u.Clip.HitForceGiven:0} N·s, a joint turned aside {(u.Clip.Upper != null ? u.Clip.Upper.HitBentMost : 0f):0} degrees at most")));
            sb.AppendLine("    standing, as seen: a forearm or hand inside its own body (s) / what its right hand holds (s) / the longest stretch (s): " + string.Join(", ", units.Where(u => u.Clip != null).Select(u => $"{u.name} {u.Clip.ArmSeenIn:0.00} / {u.Clip.BladeSeenIn:0.00} / {u.Clip.ArmSeenInLongest:0.00}")));
            sb.AppendLine("    swordsmen: the pose had the sword hand behind its own chest, on its feet and not striking (s) / swords taken up afresh after the guard was not held a while: " + string.Join(", ", units.Where(u => u.Clip != null && u.Clip.Parry != null).Select(u => $"{u.name} {u.Clip.SwordBehindFor:0.00} / {u.Clip.SwordsTakenUp}")));
            var rose = units.Where(u => u.Clip != null).SelectMany(u => u.Clip.GetUpLog.Select(g => (u, g))).ToList();
            if (rose.Count > 0)
            {
                sb.AppendLine($"    get-ups: {rose.Count} ({(PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps ? "the new ones" : "as before")})");
                foreach (var (u, g) in rose.Take(30))
                {
                    sb.AppendLine($"      {u.name} {g}");
                }
            }

            sb.AppendLine("    physical bodies put where the pose jumped to (a pose that moves at once): " + string.Join(", ", units.Where(u => u.Clip?.Upper != null).Select(u => $"{u.name} {u.Clip.Upper.Jumps}")));
            sb.AppendLine($"    told of the engine's steps at the end: {PhysicsBattler.Authority.PhysicsSteps.Listeners} (the fighters with physical bodies: {units.Count(u => u.Clip != null && u.Clip.Upper != null)}); thrown and taken off: {PhysicsBattler.Authority.PhysicsSteps.Faults}");
            sb.AppendLine("    lost by the engine and put back on its pose at once: " + string.Join(", ", units.Select(u => $"{u.name} {u.Clip.LostBody}")));
            if (kickAt >= 0f)
            {
                sb.AppendLine($"    kicked at {kickAt:0.0} s (every joint set turning at {kickSpeed:0} rad/s): the fastest joint of any body (rad/s) / the furthest head from its pose (cm), each tenth of a second after:");
                var row = new System.Text.StringBuilder("     ");
                for (int i = 0; i < 20; i++) row.Append($" {kickFastest[i]:0}/{kickHead[i]:0}");
                sb.AppendLine(row.ToString());
            }

            sb.AppendLine("    the physical body against the pose, units on their feet:");
            sb.AppendLine($"      the weapon hand in a strike: {Spread(upperOffs[0], "cm")}");
            sb.AppendLine($"      the weapon hand otherwise: {Spread(upperOffs[1], "cm")}");
            sb.AppendLine($"      the shield hand: {Spread(upperOffs[2], "cm")}");
            sb.AppendLine($"      the head: {Spread(upperOffs[3], "cm")}");
            if (legOffs[0].Count + legOffs[1].Count > 0)
            {
                sb.AppendLine($"      physical legs, a foot held by its lock: {Spread(legOffs[0], "cm")}");
                sb.AppendLine($"      physical legs, a foot in the air or stepping: {Spread(legOffs[1], "cm")}");
                sb.AppendLine($"      physical legs, a knee: {Spread(legOffs[2], "cm")}");
            }
            sb.AppendLine($"      the shield in its hand: {Spread(upperOffs[4], "°")}");
            foreach (var u in units)
            {
                if (headOffs.TryGetValue(u, out var ho)) sb.AppendLine($"      {u.name}'s head: {Spread(ho, "cm")}; joints put where the pose turned them at once: {u.Clip.SnappedBack}; put back on its pose after staying away: {u.Clip.RanAway}; joints gave way: {(u.Clip.Upper != null ? u.Clip.Upper.GaveWay : 0)} times; an arm held to its pose for being in its own body: {u.Clip.HeldForBody} steps, for a joint past its range: {u.Clip.HeldForRange}");
            }
            sb.AppendLine("    physical upper bodies, the share of each unit's time on its feet with one: " + string.Join(", ", upperSteps.Select(p => $"{p.Key.name} {100f * p.Value.on / Mathf.Max(1, p.Value.all):0}%"
                + (p.Value.on < p.Value.all ? $" (now {p.Key.Clip.Footing}, torso {p.Key.Clip.Authority.Level(PhysicsBattler.Authority.BodyRegion.Torso)}, lost: {string.Join(" ", PhysicsBattler.Rig.PartInfo.All.Where(id => p.Key.Rig.IsSevered(id)))})" : ""))));
        }

        var shoved = PhysicsBattler.Locomotion.ClipFighter.RootShoves;
        sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "    ground given (all units): {0:0.00} m in {1:0.00} s, {2:0.0} m/s on average, {3:0.0} at the fastest; steps back {4}, hopped {5}",
            shoved[1], PhysicsBattler.Locomotion.ClipFighter.RetireSeconds, shoved[1] / Mathf.Max(1e-3f, PhysicsBattler.Locomotion.ClipFighter.RetireSeconds), PhysicsBattler.Locomotion.ClipFighter.RetireFastest, units.Sum(u => u.Clip != null ? u.Clip.Retires : 0), units.Sum(u => u.Clip != null ? u.Clip.Poser.Hops : 0)));
        sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "    roots moved at once, not walked (all units): kept apart {0:0.00} m, ground given {1:0.00} m, strikes snapped onto targets {2:0.00} m, pressed back {3:0.00} m",
            shoved[0], shoved[1], shoved[2], shoved[3]));
        var slides = slide.Values.Where(g => g.Seconds > 0f).ToList();
        if (slides.Count > 0)
        {
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "    feet sliding on the ground (all units): {0:0.0} cm a second on their feet ({3:0.0} m in strikes, {4:0.0} m otherwise), the worst single slide {1:0} cm, {2:0.0}% of the time with a foot sliding faster than 0.3 m/s",
                100f * slides.Sum(g => g.Slid) / slides.Sum(g => g.Seconds), 100f * slides.Max(g => g.Worst), 100f * slides.Sum(g => g.FastSeconds) / slides.Sum(g => g.Seconds), slides.Sum(g => g.SlidStriking), slides.Sum(g => g.SlidOtherwise)));
        }

        foreach (var pair in overlapPairs)
        {
            sb.AppendLine($"    overlapping: {pair.Key}, {pair.Value.steps} steps, as near as {pair.Value.nearest:0.00} m");
        }

        foreach (var u in units)
        {
            if (!feet.TryGetValue(u, out var m))
            {
                continue;
            }

            var o = offs[u];
            o.Sort();
            string near = reachedAt.TryGetValue(u, out float at)
                ? $"reached its target at {at:0.0} s, then off its striking distance median {(o.Count > 0 ? o[o.Count / 2] : 0f):+0.00;-0.00} m (10% {(o.Count > 0 ? o[o.Count / 10] : 0f):+0.00;-0.00}, 90% {(o.Count > 0 ? o[o.Count * 9 / 10] : 0f):+0.00;-0.00})"
                : "never reached its target";
            if (Environment.GetEnvironmentVariable("FIGHT_FEET") != null)
            {
                foreach (var line in m.Report().Split('\n'))
                {
                    if (line.Contains("while planted")) sb.AppendLine($"      {u.name}: {line.Trim()}");
                }
            }

            sb.AppendLine($"    {u.name}: ran {(moving.TryGetValue(u, out var mvn) && mvn > 0 ? 100f * (running.TryGetValue(u, out var rnn) ? rnn : 0) / mvn : 0f):0}% of its walking; came at its first target {(cameAt.TryGetValue(u, out var ca) ? $"{ca:0}° off head-on" : "never")}");
            if (slide.TryGetValue(u, out var gs))
            {
                if (Environment.GetEnvironmentVariable("FIGHT_SLIDES") != null)
                {
                    foreach (var note in gs.Notes) sb.AppendLine($"      {u.name}: {note}");
                }

                sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "    {0}: feet slid on the ground {1:0.00} m in all ({2:0.0} cm a second on its feet), the worst single slide {3:0} cm, {4:0.0}% of its time with a foot sliding faster than 0.3 m/s",
                    u.name, gs.Slid, gs.Seconds > 0f ? 100f * gs.Slid / gs.Seconds : 0f, gs.Worst * 100f, gs.Seconds > 0f ? 100f * gs.FastSeconds / gs.Seconds : 0f));
            }

            sb.AppendLine($"    {u.name}: {near}; planted feet moved at most {m.PlantDriftWorst * 100f:0.0} cm ({m.PlantsDone} plants); the root's biggest step {rootJump[u].most:0.00} m at {rootJump[u].at:0.0} s; feet travelled {m.FeetTravel:0.0} m for the body's {m.BodyTravel:0.0}");
        }

        foreach (var u in units)
        {
            sb.AppendLine($"  {u.name}: {(u.IsAlive ? $"alive, {u.Health:0}/{u.Profile.maxHealth:0} hp" : "dead")}{(u.Loadout?.Main == null ? ", unarmed" : "")}{(u.LegLost ? ", legless" : "")}, "
                          + $"{(started.TryGetValue(u, out int n) ? n : 0)} blows, {u.SeveredLimbs.Count} limbs lost");
        }

        // The tempo's headline (milestone 7a; Tools/Playtest/tempo.py has the rest): bursts, gaps, taking turns.
        var gaps = new List<float>();
        var afterEnd = new List<float>();
        var lastBy = new Dictionary<UnitController, float>();
        var lastEnd = new Dictionary<UnitController, float>();
        var lastInPair = new Dictionary<(UnitController, UnitController), UnitController>();
        int alternate = 0, same = 0;
        foreach (var (t, by, at, duration) in blowsAt)
        {
            if (lastBy.TryGetValue(by, out float before))
            {
                gaps.Add(t - before);
                afterEnd.Add(t - lastEnd[by]);
            }

            lastBy[by] = t;
            lastEnd[by] = t + duration;
            if (at == null)
            {
                continue;
            }

            var key = units.IndexOf(by) < units.IndexOf(at) ? (by, at) : (at, by);
            if (lastInPair.TryGetValue(key, out var previous))
            {
                if (previous == by) same++; else alternate++;
            }

            lastInPair[key] = by;
        }

        gaps.Sort();
        if (gaps.Count > 0)
        {
            int bursts = afterEnd.FindAll(g => g < 0.6f).Count;
            float total = 0f;
            foreach (var v in tempoTime.Values) total += v;
            var shares = new List<string>();
            foreach (var pair in tempoTime) shares.Add($"{pair.Key.ToLowerInvariant()} {100f * pair.Value / Mathf.Max(1e-3f, total):0}%");
            if (shares.Count > 0) sb.AppendLine("  tempo (time on their feet): " + string.Join(", ", shares));
            sb.AppendLine($"  gaps between a unit's own blows: median {gaps[gaps.Count / 2]:0.0} s; in bursts (next within 0.6 s of the last ending): {100f * bursts / afterEnd.Count:0}%; "
                          + $"within pairs the next blow was the other's {100f * alternate / Mathf.Max(1, alternate + same):0}% of the time");
        }

        linkGaps.Sort();
        var ends = new List<string>();
        foreach (var pair in comboEnds) ends.Add($"{pair.Key} {pair.Value}");
        var whys = new List<string>();
        foreach (var pair in comboWhy) whys.Add($"{pair.Key} {pair.Value}");
        contactGaps.Sort();
        sb.AppendLine($"  combos: {combos}, {100f * comboBlows / Mathf.Max(1, attacks):0}% of blows inside them (target 25-40%); striking moment to the next: median {(contactGaps.Count > 0 ? contactGaps[contactGaps.Count / 2] : 0f):0.00} s "
                      + $"(target 0.4-0.7, {contactGaps.Count}); link start to the next: median {(linkGaps.Count > 0 ? linkGaps[linkGaps.Count / 2] : 0f):0.00} s; ended: {string.Join(", ", ends)}{(whys.Count > 0 ? "; clip combos for: " + string.Join(", ", whys) : "")}");
        sb.Append($"  steps with standing bodies overlapping: {shared}; times a unit made no headway and went round: {loop.Stalls}");
#if PHYSX
        if (overlaps != null)
        {
            sb.AppendLine();
            sb.Append(overlaps.Report());
        }
#endif
        stats = sb.ToString();
        string result = loop.Over ? $"fight over: {loop.Result}" : $"not over after {Time.time - startTime:0} s";
        foreach (var u in units)
        {
            Call(u, "OnDestroy");
            UnityEngine.Object.DestroyImmediate(u.gameObject);
        }

        return result + $"; {attacks} blows in all";
    }

    private static void Call(object target, string method)
    {
        target?.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}

/// <summary>
/// A unit's feet sliding on the ground: each ankle while it is down at its standing height, and how far it moves
/// there (flat). A planted foot doesn't move; one that does is being dragged by a root something moved under it.
/// </summary>
internal sealed class GroundSlide
{
    private readonly PhysicsBattler.Rig.AnimRig anim;
    private readonly Vector3[] was = new Vector3[2];
    private readonly bool[] down = new bool[2];
    private readonly float[] run = new float[2];
    private static readonly PhysicsBattler.Rig.PartId[] Feet = { PhysicsBattler.Rig.PartId.FootL, PhysicsBattler.Rig.PartId.FootR };

    /// <summary>Metres slid in all, the furthest in one stay on the ground, the seconds sampled and those with a foot sliding faster than 0.3 m/s.</summary>
    public float Slid, Worst, Seconds, FastSeconds;

    private readonly PhysicsBattler.Locomotion.Locomotor loco;

    public PhysicsBattler.Locomotion.LocoBody Poser;
    public string Name;
    private static readonly bool TraceSlides = Environment.GetEnvironmentVariable("FIGHT_SLIDE_TRACE") != null;

    /// <summary>Metres slid while a strike's clip played, and otherwise.</summary>
    public float SlidStriking, SlidOtherwise;

    public GroundSlide(PhysicsBattler.Rig.AnimRig anim, PhysicsBattler.Locomotion.Locomotor loco)
    {
        this.anim = anim;
        this.loco = loco;
    }

    private float lowest = float.MaxValue;

    /// <summary>Each slide of over 5 cm: when, what was playing, how far.</summary>
    public readonly List<string> Notes = new List<string>();
    private readonly string[] during = new string[2];

    public void Sample(bool onItsFeet, float dt, float time)
    {
        if (!onItsFeet)
        {
            down[0] = down[1] = false;
            return;
        }

        Seconds += dt;
        bool fast = false;
        for (int f = 0; f < 2; f++)
        {
            Vector3 at = anim.Part(Feet[f]).position;
            // (Its standing ankle height: the lowest an ankle has been, found as it goes.)
            lowest = Mathf.Min(lowest, at.y);
            // (Down: where its motion says the foot is on the ground, and it is.)
            bool now = at.y <= lowest + 0.015f && loco.Down[f] >= 0.5f;
            if (now && down[f])
            {
                Vector3 moved = at - was[f];
                moved.y = 0f;
                float far = moved.magnitude;
                Slid += far;
                if (loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.ContactFrame >= 0) SlidStriking += far; else SlidOtherwise += far;
                run[f] += far;
                if (TraceSlides && far > 0.004f && Poser != null)
                {
                    var lk = Poser.Feet[f];
                    Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "  slide {0} {1:0.00} s {2}: {3:0.0} cm this step (run {4:0.0}); locked {5}, stepping {6}, fade {7:0.0} cm, height {8:0.0} cm (asked {12:0.0} cm up, {13:0.0} cm from its place); {9} {10} t {11:0.00}",
                        Name, time, f == 0 ? "L" : "R", far * 100f, (run[f] + far) * 100f, lk.Locked, lk.Stepping, lk.Fade.magnitude * 100f, (at.y - lowest) * 100f, loco.Current, loco.Shot != null ? loco.Shot.Name : "", loco.ShotTime, (lk.Target.y - lowest) * 100f, Vector3.Distance(lk.Target, at) * 100f));
                }

                if (far > 0.002f)
                {
                    string what = loco.Current + (loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && loco.Shot != null ? " " + loco.Shot.Name : "");
                    if (during[f] == null || !during[f].Contains(what)) during[f] = (during[f] == null ? "" : during[f] + ", ") + what;
                }

                Worst = Mathf.Max(Worst, run[f]);
                fast |= far > 0.3f * dt;
            }
            else
            {
                if (run[f] > 0.05f && Notes.Count < 30)
                {
                    Notes.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "until {0:0.00} s the {1} foot slid {2:0} cm ({3})", time, f == 0 ? "left" : "right", run[f] * 100f, during[f]));
                }

                run[f] = 0f;
                during[f] = null;
            }

            down[f] = now;
            was[f] = at;
        }

        if (fast)
        {
            FastSeconds += dt;
        }
    }
}
