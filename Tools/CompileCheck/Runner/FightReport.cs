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
        // (FIGHT_SETOFF=0: setting off from a stand as it was, two quick steps.)
        PhysicsBattler.Locomotion.Locomotor.SetsOffEasy = Environment.GetEnvironmentVariable("FIGHT_SETOFF") != "0";
        // (FIGHT_ANYPOSE=0: an attack asked for while he moves stops him in one step and begins from his guard, as it
        // was. FIGHT_COAST=0.4: how long his speed is carried into the attack, seconds.)
        PhysicsBattler.Locomotion.AttackMaker.FromAnyPose = Environment.GetEnvironmentVariable("FIGHT_ANYPOSE") != "0";
        // (FIGHT_ONTHEMOVE=0: a blow asked for while he walks is thrown planted, his speed carried into it, not over
        // the walk. FIGHT_MOVECHECK=0.45, FIGHT_MOVEHIPS=0.5, FIGHT_MOVEINTO=0.07, FIGHT_MOVESTEPIN=0.75: how a blow on the move sits on the walk.)
        PhysicsBattler.Locomotion.AttackOnTheMove.On = Environment.GetEnvironmentVariable("FIGHT_ONTHEMOVE") != "0";
        // (FIGHT_ARMPROBE=1: where the first fighter's hands are on its chest after each stage of the pose, once a second.)
        if (Environment.GetEnvironmentVariable("FIGHT_ARMPROBE") == "1")
        {
            int probed = 0;
            PhysicsBattler.Locomotion.ClipFighter first = null;
            PhysicsBattler.Locomotion.ClipFighter.ArmStageProbe = (who, stage) =>
            {
                first ??= who;
                if (who != first || (stage.StartsWith("the clip's") ? ++probed : probed) % 60 != 0)
                {
                    return;
                }

                Vector3 chest = who.Anim.Part(PartId.Chest).position;
                Vector3 r = who.Anim.Part(PartId.HandR).position - chest, l = who.Anim.Part(PartId.HandL).position - chest;
                Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ARMPROBE {0} after {1}: right hand {2:0.00} {3:0.00} {4:0.00}, left hand {5:0.00} {6:0.00} {7:0.00}", probed, stage, r.x, r.y, r.z, l.x, l.y, l.z));
            };
        }

        // (FIGHT_BLADECLEAR=0: a feint is held wherever its wind-up has the blade, as it was.)
        PhysicsBattler.Locomotion.PostureMaker.BladeClear = Env("FIGHT_BLADECLEAR", 0.18f);
        // (FIGHT_LUNGESTOPS=1: a fake lunge asked for while he walks stops him, as it was.)
        PhysicsBattler.Locomotion.PostureMaker.LungesOnTheMove = Environment.GetEnvironmentVariable("FIGHT_LUNGESTOPS") != "1";
        // (FIGHT_SHORTPLANTS=1: a blow on the move that would fall short is thrown planted, with its lunge, as it was.)
        PhysicsBattler.Locomotion.AttackOnTheMove.StaysWhenShort = Environment.GetEnvironmentVariable("FIGHT_SHORTPLANTS") != "1";
        PhysicsBattler.Locomotion.AttackOnTheMove.Trace = line => Console.WriteLine("ONTHEMOVE " + line);
        // (FIGHT_OVERTRACE=1: every step of a blow on the move: the hips' turn, the walk's check, its stride's phase and which feet are down.)
        PhysicsBattler.Locomotion.AttackOnTheMove.TraceSteps = Environment.GetEnvironmentVariable("FIGHT_OVERTRACE") == "1";
        PhysicsBattler.Locomotion.AttackOnTheMove.Pulls = Environment.GetEnvironmentVariable("FIGHT_MOVEPULL") != "0";
        {
            var move = PhysicsBattler.Locomotion.MoveBlowStyle.Plain;
            float MoveEnv(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
            move.Check = MoveEnv("FIGHT_MOVECHECK", move.Check);
            move.HipsTurn = MoveEnv("FIGHT_MOVEHIPS", move.HipsTurn);
            move.HipsInto = MoveEnv("FIGHT_MOVEINTO", move.HipsInto);
            move.StepIn = MoveEnv("FIGHT_MOVESTEPIN", move.StepIn);
            PhysicsBattler.Locomotion.AttackOnTheMove.Style = move;
        }

        // (FIGHT_LEGSBACK=0: a blow on another's legs goes back with the legs' way back squeezed into its own, as it was.)
        PhysicsBattler.Locomotion.AttackScore.LegsGoBackAtTheirPace = Environment.GetEnvironmentVariable("FIGHT_LEGSBACK") != "0";
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_COAST"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float coastFor))
        {
            PhysicsBattler.Locomotion.AttackMaker.CoastTime = coastFor;
        }

        // (FIGHT_STYLEAMOUNT=1: capoeira's manner laid over the standard set, how much; FIGHT_STYLEMOTION=0.5,
        // FIGHT_STYLESQUARE=0.5, FIGHT_STYLEARMS=1: how much livelier, how much the chest squares up, how much of the arms.
        // FIGHT_STYLESWEEP=8, with a duel: the amount goes from 0 to 1 over the run's first so many seconds.)
        PhysicsBattler.Locomotion.StyleManner.Amount = Env("FIGHT_STYLEAMOUNT", 0f);
        PhysicsBattler.Locomotion.StyleManner.Dials = new PhysicsBattler.Locomotion.MannerDials
        {
            Carriage = 1f, Motion = Env("FIGHT_STYLEMOTION", 0.5f), Turn = Env("FIGHT_STYLESQUARE", 0.5f), Arms = Env("FIGHT_STYLEARMS", 1f), HandsKeep = 1f, HeadKeeps = Env("FIGHT_STYLEEYES", 1f), InBlows = Env("FIGHT_STYLEINBLOWS", 0.5f),
        };
        // (FIGHT_GINGAMADE=0: the capoeira style stands in the recorded take, as the first pass. FIGHT_GINGASTEP=0.8,
        // FIGHT_GINGATEMPO=1, FIGHT_GINGARESTLESS=0.4, FIGHT_GINGAVARY=0.5: the made ginga's size, tempo, restlessness, variation.)
        PhysicsBattler.Locomotion.StandPattern.Generated = Environment.GetEnvironmentVariable("FIGHT_GINGAMADE") != "0";
        PhysicsBattler.Locomotion.StandPattern.Size = Env("FIGHT_GINGASTEP", 0.8f);
        PhysicsBattler.Locomotion.StandPattern.Tempo = Env("FIGHT_GINGATEMPO", 1f);
        PhysicsBattler.Locomotion.StandPattern.Restless = Env("FIGHT_GINGARESTLESS", 0.4f);
        PhysicsBattler.Locomotion.StandPattern.Vary = Env("FIGHT_GINGAVARY", 0.5f);
        // (The standard movement made of its parts, the fight rhythm plan's step A. FIGHT_MOVEMADE=0: the recordings'
        // points played, as before. The levers: FIGHT_MOVEBOUNCE, FIGHT_MOVESWAY, FIGHT_MOVEHIPTURN, FIGHT_MOVECHESTTURN,
        // FIGHT_MOVEARMS, FIGHT_MOVELIFT (1: as recorded), FIGHT_MOVELEAN (degrees), FIGHT_MOVECROUCH (cm).)
        PhysicsBattler.Locomotion.MoveScore.On = Env("FIGHT_MOVEMADE", 1f) > 0.5f;
        PhysicsBattler.Locomotion.MoveScore.Levers = new PhysicsBattler.Locomotion.MoveLevers
        {
            Bounce = Env("FIGHT_MOVEBOUNCE", 1f), Sway = Env("FIGHT_MOVESWAY", 1f), HipTurn = Env("FIGHT_MOVEHIPTURN", 1f), ChestTurn = Env("FIGHT_MOVECHESTTURN", 1f),
            ArmSwing = Env("FIGHT_MOVEARMS", 1f), StepLift = Env("FIGHT_MOVELIFT", 1f), Lean = Env("FIGHT_MOVELEAN", 0f), SwordArmLift = Env("FIGHT_MOVESWORDLIFT", 0f), ShieldOut = Env("FIGHT_MOVESHIELDOUT", 0f), Crouch = 0.01f * Env("FIGHT_MOVECROUCH", 0f) / PhysicsBattler.Locomotion.Locomotor.ClipLeg,
        };
        // (FIGHT_STYLETRACE=1: what the walk lays of the manner, each step.)
        if (Environment.GetEnvironmentVariable("FIGHT_STYLETRACE") == "1")
        {
            PhysicsBattler.Locomotion.StyleManner.Trace = line => Console.WriteLine($"[manner] {Time.time:0.000} {line}");
        }

        // (FIGHT_JOGS=0: the guard without its jog, as it was.)
        PhysicsBattler.Locomotion.MoveStyle.Jogs = Environment.GetEnvironmentVariable("FIGHT_JOGS") != "0";
        // (FIGHT_ONECARRIAGE=0: the guard's steps each as recorded, as it was. FIGHT_STANCEKEPT=0.5: how much of the stand's stance the feet keep.)
        PhysicsBattler.Locomotion.MoveStyle.OneCarriage = Environment.GetEnvironmentVariable("FIGHT_ONECARRIAGE") != "0";
        PhysicsBattler.Locomotion.MoveScore.StanceKept = Env("FIGHT_STANCEKEPT", 0.5f);
        // (FIGHT_SINKSBEFORESTEP=0.12: the hips sink all they can before a foot left behind is let go, as it was.)
        PhysicsBattler.Locomotion.LocoBody.SinksBeforeStep = Env("FIGHT_SINKSBEFORESTEP", 0.03f);
        // (FIGHT_WHOLESTEPS=0: the walk follows what is asked at every moment, as before 18 November. FIGHT_STEPPACE,
        // FIGHT_STEPTEMPO, FIGHT_STEPPAUSE: a whole step's least pace and tempo, and how far ahead he stands from.)
        PhysicsBattler.Locomotion.Locomotor.WholeSteps = Environment.GetEnvironmentVariable("FIGHT_WHOLESTEPS") != "0";
        PhysicsBattler.Locomotion.Locomotor.StepLeastPace = Env("FIGHT_STEPPACE", 0.6f);
        // (FIGHT_LEGSTURNED=0: a diagonal is two recordings mixed, as before 18 November.)
        PhysicsBattler.Locomotion.Locomotor.LegsTurned = Environment.GetEnvironmentVariable("FIGHT_LEGSTURNED") != "0";
        PhysicsBattler.Locomotion.Locomotor.StepLeastTempo = Env("FIGHT_STEPTEMPO", 0.85f);
        PhysicsBattler.Locomotion.Locomotor.StepPausesFrom = Env("FIGHT_STEPPAUSE", 0.12f);
        // (FIGHT_TURNSLASH=always: every quick turn round is made with a cut; =0: none is. FIGHT_BLADEGOES, FIGHT_BLADELEADS:
        // its levers. FIGHT_TURNCUT=spin_backhand_mid: the cut taken for a turn to the right. FIGHT_TURNSLASHANYWAY=0.25.)
        PhysicsBattler.Locomotion.TurnSlashes.On = Environment.GetEnvironmentVariable("FIGHT_TURNSLASH") != "0";
        PhysicsBattler.Locomotion.TurnSlashes.Always = Environment.GetEnvironmentVariable("FIGHT_TURNSLASH") == "always";
        PhysicsBattler.Locomotion.TurnSlashes.EndsShort = Environment.GetEnvironmentVariable("FIGHT_TURNCUTSHORT") != "0";
        // (FIGHT_TURNCUTROUND=220: the most a cut made turning round turns his body in all. FIGHT_TURNCUTAIM: the most the aim turns it.)
        PhysicsBattler.Locomotion.TurnSlashes.MostRound = Env("FIGHT_TURNCUTROUND", PhysicsBattler.Locomotion.TurnSlashes.MostRound);
        PhysicsBattler.Locomotion.TurnSlashes.AimTurns = Env("FIGHT_TURNCUTAIM", PhysicsBattler.Locomotion.TurnSlashes.AimTurns);
        // (FIGHT_TURNCUTFEET=0: the aim multiplies a turn cut's feet's travel for the distance, as before 18 November.)
        PhysicsBattler.Locomotion.TurnSlashes.FeetGoRound = Environment.GetEnvironmentVariable("FIGHT_TURNCUTFEET") != "0";
        // (FIGHT_CLIPDUMP="folder,part of a name": each attack's clip made whose name has that part, written there as it is made.)
        {
            string[] dump = (Environment.GetEnvironmentVariable("FIGHT_CLIPDUMP") ?? "").Split(',');
            if (dump.Length >= 1 && dump[0].Length > 0)
            {
                System.IO.Directory.CreateDirectory(dump[0]);
                int dumped = 0;
                PhysicsBattler.Locomotion.AttackMaker.ClipMade = d =>
                {
                    if (dumped < 40 && (dump.Length < 2 || d.name.Contains(dump[1])))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dump[0], $"{dumped++:00}_{d.name.Replace('/', '_').Replace(' ', '_')}.json"), UnityEngine.JsonUtility.ToJson(d));
                    }
                };
            }
        }
        PhysicsBattler.Locomotion.TurnSlashes.SweepsFree = Environment.GetEnvironmentVariable("FIGHT_TURNCUTFREE") != "0";
        PhysicsBattler.Locomotion.TurnSlashes.SweepsOn = Env("FIGHT_TURNCUTSWEEPS", 0.1f);
        PhysicsBattler.Locomotion.TurnSlashes.CarriesOn = Env("FIGHT_TURNCUTCARRIES", 0.2f);
        PhysicsBattler.Locomotion.TurnSlashes.BladeGoes = Env("FIGHT_BLADEGOES", 1f);
        PhysicsBattler.Locomotion.TurnSlashes.BladeLeads = Env("FIGHT_BLADELEADS", 0f);
        PhysicsBattler.Locomotion.TurnSlashes.Anyway = Env("FIGHT_TURNSLASHANYWAY", 0.25f);
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_TURNCUTLEFT")))
        {
            PhysicsBattler.Locomotion.TurnSlashes.LeftCut = Environment.GetEnvironmentVariable("FIGHT_TURNCUTLEFT");
        }

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_TURNCUT")))
        {
            PhysicsBattler.Locomotion.TurnSlashes.RightCut = Environment.GetEnvironmentVariable("FIGHT_TURNCUT");
        }

        // (FIGHT_QUICKTURN=0: a fighter turns round as he did. FIGHT_QUICKPACE, FIGHT_QUICKFROM, FIGHT_QUICKWITHIN, FIGHT_TURNDRAWSIN, FIGHT_TURNSINKS: the quick turn's levers.)
        PhysicsBattler.Locomotion.Locomotor.QuickTurns = Environment.GetEnvironmentVariable("FIGHT_QUICKTURN") != "0";
        PhysicsBattler.Locomotion.Locomotor.QuickTurnPace = Env("FIGHT_QUICKPACE", 1.5f);
        PhysicsBattler.Locomotion.Locomotor.QuickTurnFrom = Env("FIGHT_QUICKFROM", 75f);
        PhysicsBattler.Locomotion.Locomotor.QuickTurnWithin = Env("FIGHT_QUICKWITHIN", 4f);
        PhysicsBattler.Locomotion.MoveStyle.TurnDrawsIn = Env("FIGHT_TURNDRAWSIN", 0.5f);
        PhysicsBattler.Locomotion.MoveStyle.TurnSinks = Env("FIGHT_TURNSINKS", 0.4f);
        // (FIGHT_TRAVEL=0: the clips' travel as it was, turned the wrong way round.)
        PhysicsBattler.Locomotion.LocoClip.TravelsTrue = Environment.GetEnvironmentVariable("FIGHT_TRAVEL") != "0";
        if (Environment.GetEnvironmentVariable("FIGHT_ARMGATE") == "0")
        {
            PhysicsBattler.Motion.ArmLimits.On = false;
        }

        // (FIGHT_TURNTRACE=1: each turn on the spot, as it begins.)
        if (Environment.GetEnvironmentVariable("FIGHT_TURNTRACE") == "1") { PhysicsBattler.Locomotion.Locomotor.TurnTrace = (l, m) =>
            {
                string who = "someone";
                foreach (var u in PhysicsBattler.Units.UnitController.All)
                {
                    if (u != null && u.Clip != null && u.Clip.Loco == l)
                    {
                        who = u.name;
                    }
                }

                Console.WriteLine($"[turn] {who} at {l.Position.x:0.0} {l.Position.z:0.0}: {m}");
            }; }
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
            // (FIGHT_TUBEROWS=4: four rows of the three targets, twelve bodies sent down together: what a fall costs with many on the field.)
            int rows = int.TryParse(Environment.GetEnvironmentVariable("FIGHT_TUBEROWS"), out int tubeRows) ? Mathf.Clamp(tubeRows, 1, 8) : 1;
            // ("MC": a Large with its club, a Small with its club, and a swordsman: the bodies as the battle arms them.)
            bool battle = who.Length > 1 && who[1] == 'C';
            var targets = new List<(string, Tile)>();
            for (int r = 0; r < rows; r++)
            {
                targets.Add((battle ? "Large" : lt ? "Large open" : "Open", new Tile(1, 5)));
                targets.Add((battle ? "Small" : lt ? "Large shield" : "Medium", new Tile(3, 5)));
                targets.Add((lt ? "Large sword" : "Swordsman", new Tile(6, 5)));
            }

            lineup = new[] { new (string, Tile)[0], targets.ToArray() };
        }

        // (The sandbox's aim test: FIGHT_LINEUP=aim sets one swordsman before one target that stands there, and asks him
        // for one blow at each of the nineteen spots in turn. FIGHT_AIMKIT=open|shield|sword is what the target holds,
        // FIGHT_AIMDIST=1.4 how far apart they stand, FIGHT_AIMTURN=90 how the target is turned (90: its left side to him),
        // FIGHT_AIMROUNDS=3 goes through the spots three times.)
        bool aimLineup = gang == "aim";
        string aimKit = Environment.GetEnvironmentVariable("FIGHT_AIMKIT") ?? "open";
        if (aimLineup)
        {
            lineup = new[]
            {
                // (FIGHT_AIMWHO=shield: the attacker has sword and shield, as the strikes were recorded.)
                new[] { (Environment.GetEnvironmentVariable("FIGHT_AIMWHO") == "shield" ? "Medium" : Environment.GetEnvironmentVariable("FIGHT_AIMWHO") == "open" ? "Open" : "Swordsman", new Tile(3, 2)) },
                new[] { (aimKit == "shield" ? "Medium" : aimKit == "sword" ? "Swordsman" : "Open", new Tile(3, 5)) },
            };
        }

        if (gang == "crowd")
        {
            // (Many on the field: eight a side, of every kind. What the fight costs a step with sixteen.)
            string[] kinds = { "Swordsman", "Medium", "Large", "Medium", "Swordsman", "Medium", "Large", "Swordsman" };
            lineup = new[]
            {
                kinds.Select((k, i) => (k, new Tile(i, 1))).ToArray(),
                kinds.Select((k, i) => (k, new Tile(7 - i, 6))).ToArray(),
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

        PhysicsBattler.Sandbox.AimTest aim = null;
        // (FIGHT_DUEL="WalkTo:6,CircleLeft:6,BackAway:5": the duel lab's movement orders given in turn, each for so
        // many seconds, in the aim lineup; no blows are asked for. FIGHT_DUELAPART=3: how far apart the two begin.)
        PhysicsBattler.Sandbox.DuelLab duel = null;
        PhysicsBattler.Sandbox.ShieldTest shieldTest = null;
        int shieldGo = 0;
        // (FIGHT_SHIELDCOVER=0: the shield arm is the recording's, as it was. FIGHT_BASH=0: no shield bash. In any fight.)
        PhysicsBattler.Locomotion.ClipFighter.ShieldCover = Environment.GetEnvironmentVariable("FIGHT_SHIELDCOVER") != "0";
        PhysicsBattler.Locomotion.ClipFighter.ShieldBash = Environment.GetEnvironmentVariable("FIGHT_BASH") != "0";
        // (FIGHT_FAKESDRAW=1: a guard goes to meet a fake, as it did.)
        PhysicsBattler.Locomotion.ClipFighter.FakesDrawNoGuard = Environment.GetEnvironmentVariable("FIGHT_FAKESDRAW") != "1";
        // (FIGHT_CLASSES=0: every blow is the plain one.)
        PhysicsBattler.Locomotion.AttackClasses.On = Environment.GetEnvironmentVariable("FIGHT_CLASSES") != "0";
        PhysicsBattler.Sandbox.FakeTest fakeTest = null;
        PhysicsBattler.Sandbox.Dance dance = null;
        string[] danceLearned = { "", "" };
        int fakeGo = 0;
        string fakeLearned = "";
        float styleSweep = float.TryParse(Environment.GetEnvironmentVariable("FIGHT_STYLESWEEP"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float sweepOver) ? sweepOver : 0f;
        var duelOrders = new List<(PhysicsBattler.Sandbox.DuelLab.Order order, float until)>();
        bool courseBegun = false, courseTold = false;
        var danceMeterA = new PhysicsBattler.Sandbox.FootworkMeter();
        var danceMeterB = new PhysicsBattler.Sandbox.FootworkMeter();
        int duelAt = -1;
        float duelFrom = 0f, duelGapFrom = 0f, duelNote = 0f;
        // (FIGHT_DUELBLOW="3.2:Chest,6:Belly": a blow asked for at those seconds, whatever he is doing then: the
        // plan "Fight Rhythm from Recordings", step 3, attacks from any pose. How its start went is told: the biggest
        // change of speed from one step to the next, of the root, the hips, each foot and the sword hand, as posed
        // and as seen, over its first 0.4 s. A body going 1.4 m/s that stops in a step has a change of 1.4 m/s.)
        var duelBlows = new List<(float at, PhysicsBattler.Locomotion.AimSpot spot)>();
        int[] ownN = new int[2]; float[] ownHips = new float[2], ownLean = new float[2], ownLow = { 9f, 9f }, ownHigh = { -9f, -9f }, ownFoot = new float[2];
        float ownDropAt = -1f, ownDropFrom = 0f, ownDropLow = 0f;
        int[,] coverN = new int[2, 4], coverLooks = new int[2, 4];
        bool shieldDumped = false;
        float bashGapWas = 0f;
        Vector3 bashOtherWas = Vector3.zero, bashMoverWas = Vector3.zero;
        Vector3[,] coverSum = new Vector3[2, 4], coverSq = new Vector3[2, 4];
        float[,] coverOff = new float[2, 4], coverOpen = new float[2, 4], coverBelly = new float[2, 4];
        string openedTold = "none watched yet";
        bool openTold = false;
        bool startWatched = false;
        int aimRounds = 0;
        if (aimLineup)
        {
            float Env(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
            float apart = Env("FIGHT_AIMDIST", PhysicsBattler.Sandbox.BattleSandbox.AimDistanceAtFirst), turned = Env("FIGHT_AIMTURN", 0f);
            aimRounds = Mathf.Max(1, (int)Env("FIGHT_AIMROUNDS", 1f));
            string duelAsked = Environment.GetEnvironmentVariable("FIGHT_DUEL");
            if (!string.IsNullOrEmpty(duelAsked))
            {
                apart = Env("FIGHT_DUELAPART", PhysicsBattler.Sandbox.BattleSandbox.DuelApartAtFirst);
                aimRounds = 0;
                float until = 1.5f;
                foreach (string part in duelAsked.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split(':');
                    if (Enum.TryParse(kv[0].Trim(), true, out PhysicsBattler.Sandbox.DuelLab.Order order))
                    {
                        until += kv.Length > 1 && float.TryParse(kv[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float secs) ? secs : 5f;
                        duelOrders.Add((order, until));
                    }
                }

                // (FIGHT_DUELPACE=2.6: the pace he is asked to go at, m/s; without it, his guard's own walk.)
                // (FIGHT_DUELLEAP=0.8: how far a leap goes, metres; FIGHT_DUELLEAPTIME=0.3: how long it takes.)
                duel = new PhysicsBattler.Sandbox.DuelLab { Pace = Env("FIGHT_DUELPACE", 0f), LeapFar = Env("FIGHT_DUELLEAP", 0.5f), LeapTime = Env("FIGHT_DUELLEAPTIME", 0.25f) };
                // (FIGHT_DUELSTYLE=capoeira: he moves in the capoeira style, standing in the ginga. FIGHT_GINGASIZE=0.67,
                // FIGHT_GINGAARMS=1, FIGHT_GINGATAKE=0..3: the ginga's size, its arms' swings and which take of it.)
                if ((Environment.GetEnvironmentVariable("FIGHT_DUELSTYLE") ?? "").ToLowerInvariant().StartsWith("cap"))
                {
                    duel.Style = PhysicsBattler.Locomotion.MoveStyle.Capoeira;
                }

                // (FIGHT_WALKREBUILT=1: the guard's walk forward and back is the plain walk with the guard's manner laid on;
                // FIGHT_REBUILTCARRIAGE=1, FIGHT_REBUILTMOTION=1: how much of each.)
                PhysicsBattler.Locomotion.MoveStyle.GuardWalkRebuilt = Environment.GetEnvironmentVariable("FIGHT_WALKREBUILT") == "1";
                PhysicsBattler.Locomotion.MoveStyle.RebuiltCarriage = Env("FIGHT_REBUILTCARRIAGE", 1f);
                PhysicsBattler.Locomotion.MoveStyle.RebuiltMotion = Env("FIGHT_REBUILTMOTION", 1f);
                if (PhysicsBattler.Locomotion.MoveStyle.GuardWalkRebuilt && duel.Style == null)
                {
                    duel.Style = PhysicsBattler.Locomotion.MoveStyle.Standard;
                }

                PhysicsBattler.Locomotion.MoveStyle.StandSize = Env("FIGHT_GINGASIZE", PhysicsBattler.Locomotion.MoveStyle.StandSize);
                PhysicsBattler.Locomotion.MoveStyle.StandArms = Env("FIGHT_GINGAARMS", PhysicsBattler.Locomotion.MoveStyle.StandArms);
                PhysicsBattler.Locomotion.MoveStyle.GingaTake = (int)Env("FIGHT_GINGATAKE", 0f);
                // (FIGHT_GINGASWORD=0: the ginga's blade along the recorded fingers, as recorded. FIGHT_GINGAPOINTS=0.35, FIGHT_GINGALAG=0.1.)
                PhysicsBattler.Locomotion.LocoBody.GingaSword = Environment.GetEnvironmentVariable("FIGHT_GINGASWORD") != "0";
                PhysicsBattler.Locomotion.LocoBody.GingaSwordPoints = Env("FIGHT_GINGAPOINTS", PhysicsBattler.Locomotion.LocoBody.GingaSwordPoints);
                PhysicsBattler.Locomotion.LocoBody.GingaSwordLag = Env("FIGHT_GINGALAG", PhysicsBattler.Locomotion.LocoBody.GingaSwordLag);
                // (FIGHT_DUELNEAR=1: how near he walks, metres.)
                duel.Near = Env("FIGHT_DUELNEAR", duel.Near);
                foreach (string part in (Environment.GetEnvironmentVariable("FIGHT_DUELBLOW") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split(':');
                    if (kv.Length > 1 && float.TryParse(kv[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float blowAt) && Enum.TryParse(kv[1].Trim(), true, out PhysicsBattler.Locomotion.AimSpot blowSpot))
                    {
                        duelBlows.Add((blowAt, blowSpot));
                    }
                }
            }
            var attacker = units[0];
            var target = units[1];
            Vector3 targetSpot = grid.origin + new Vector3(0f, 0f, 0.5f * apart), attackerSpot = grid.origin + new Vector3(0f, 0f, -0.5f * apart);
            Vector3 faces = Quaternion.Euler(0f, turned, 0f) * (attackerSpot - targetSpot).normalized;
            target.Teleport(targetSpot, Mathf.Atan2(faces.x, faces.z) * Mathf.Rad2Deg);
            attacker.Teleport(attackerSpot, BoardGrid.TeamYaw(0));
            target.TargetDummy = attacker.TargetDummy = true;
            target.Unharmed = attacker.Unharmed = true;
            target.DummySpot = targetSpot;
            attacker.DummySpot = attackerSpot;
            target.DummyHolds = attacker.DummyHolds = PhysicsBattler.Sandbox.AimTest.HeldWithin;
            // (Built blows may be tried from nearer than the fight lets enemies come; the clips from where they are.)
            target.StandsAsNearAs = attacker.StandsAsNearAs = Environment.GetEnvironmentVariable("FIGHT_AIMBUILT") != "0" || Environment.GetEnvironmentVariable("FIGHT_AIMMADE") != "0" ? PhysicsBattler.Sandbox.AimTest.NearestApart : 0f;
            target.DummyFace = Mathf.Abs(turned) > 0.5f ? targetSpot + faces * 5f : (Vector3?)null;
            attacker.OnlyTarget = target;
            // (FIGHT_AIMBUILT=0: today's strike clips, not built blows. FIGHT_AIMLINE=Down and FIGHT_AIMSIZE=Wrist set a
            // built blow's line and size by hand, for every spot.)
            // (FIGHT_AIMMADE=0: not attacks made of the scores. FIGHT_AIMATTACK=sns_forehand names the score; FIGHT_AIMLAND=0.8
            // lands the blade so long after it is asked.)
            PhysicsBattler.Locomotion.AttackMaker.On = Environment.GetEnvironmentVariable("FIGHT_AIMMADE") != "0";
            PhysicsBattler.Locomotion.BuiltBlow.On = Environment.GetEnvironmentVariable("FIGHT_AIMBUILT") != "0";
            PhysicsBattler.Locomotion.BuiltBlow.Pushed = Env("FIGHT_AIMPUSH", 1f);
            PhysicsBattler.Locomotion.BuiltBlow.UsesTrunk = Environment.GetEnvironmentVariable("FIGHT_AIMTRUNK") != "0";
            aim = new PhysicsBattler.Sandbox.AimTest { Attacker = attacker, Target = target };

            aim.Attack = Environment.GetEnvironmentVariable("FIGHT_AIMATTACK");
            aim.Legs = Environment.GetEnvironmentVariable("FIGHT_AIMLEGS");
            PhysicsBattler.Locomotion.ClipFighter.MadeBorrowsLegs = Environment.GetEnvironmentVariable("FIGHT_AIMBORROW") != "0";
            {
                // (FIGHT_STYLE="WindUp=1.5,Lunge=1.2": the style the attacks are made in.)
                // (FIGHT_STYLEPRESET=Heavy: one of the styles to start from, the levers of FIGHT_STYLE on top of it.)
                object style = PhysicsBattler.Locomotion.AttackStyle.Plain;
                foreach (var preset in PhysicsBattler.Locomotion.AttackStyle.Presets)
                {
                    if (preset.name == Environment.GetEnvironmentVariable("FIGHT_STYLEPRESET"))
                    {
                        style = preset.style;
                    }
                }

                foreach (string part in (Environment.GetEnvironmentVariable("FIGHT_STYLE") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split('=');
                    var field = typeof(PhysicsBattler.Locomotion.AttackStyle).GetField(kv[0].Trim());
                    if (field != null && kv.Length == 2 && float.TryParse(kv[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v))
                    {
                        field.SetValue(style, v);
                    }
                }

                PhysicsBattler.Locomotion.AttackMaker.Style = (PhysicsBattler.Locomotion.AttackStyle)style;
            }
            aim.LandIn = Env("FIGHT_AIMLAND", 0f);
            if (Enum.TryParse(Environment.GetEnvironmentVariable("FIGHT_AIMLINE"), true, out PhysicsBattler.Locomotion.CutLine aimLine))
            {
                aim.Line = aimLine;
            }

            if (Enum.TryParse(Environment.GetEnvironmentVariable("FIGHT_AIMSIZE"), true, out PhysicsBattler.Locomotion.CutSize aimSize))
            {
                aim.Size = aimSize;
            }

            aim.Ended += blow => Console.WriteLine($"AIM {Time.time:0.00} {blow.Text}");
        }

        var legWas = new Dictionary<(UnitController, int), Quaternion>();
        var filmLines = new List<string>();
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
                float x = (i % 3 - 1) * PhysicsBattler.Sandbox.BattleSandbox.TargetPairsApart;
                Vector3 spot = grid.origin + new Vector3(x, 0f, 0.5f * PhysicsBattler.Sandbox.BattleSandbox.TargetFromAttacker + 6f * (i / 3));
                units[i].Teleport(spot, Mathf.Atan2(faces.x, faces.z) * Mathf.Rad2Deg);
                units[i].TargetDummy = true;
                units[i].DummySpot = spot;
                units[i].DummyFace = spot + faces * 5f;
                units[i].Unharmed = true;
            }
        }

        var loop = new FightLoop(seed) { Grid = grid, Endless = targetTest || tubeLineup || aimLineup };
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

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_TUCKLEGS"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float tuckLegs))
        {
            PhysicsBattler.Locomotion.FallPose.TuckLegs = tuckLegs;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_TUCKNECK"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float tuckNeck))
        {
            PhysicsBattler.Locomotion.ClipFighter.TuckedNeck = tuckNeck;
        }

        // (FIGHT_TUCK=0: a body thrown off its feet does not tuck.)
        PhysicsBattler.Locomotion.ClipFighter.ThrownBodiesTuck = Environment.GetEnvironmentVariable("FIGHT_TUCK") != "0";

        // (FIGHT_SETTLE=0: a body that is down goes on reaching, as before.)
        PhysicsBattler.Locomotion.ClipFighter.FallenBodiesSettle = Environment.GetEnvironmentVariable("FIGHT_SETTLE") != "0";

        // (FIGHT_HANDHOLD=0.5: how hard a landed hand is held, in times the body's weight.)
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_HANDHOLD"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float handHold))
        {
            PhysicsBattler.Authority.UpperBodyArticulation.HandPinMost = handHold * 785f;
        }

        // (FIGHT_HANDSTAY=0: a hand that lands in a fall is not held where it landed.)
        PhysicsBattler.Locomotion.ClipFighter.HandsStayWhereTheyLand = Environment.GetEnvironmentVariable("FIGHT_HANDSTAY") != "0";
        // (FIGHT_HELDSOLID=0: what a ragdoll holds is stopped by nothing, as before.)
        PhysicsBattler.Locomotion.ClipFighter.RagdollsHoldSolidThings = Environment.GetEnvironmentVariable("FIGHT_HELDSOLID") != "0";
        // (FIGHT_SHIELDBEFORE=0: falling, a shield arm reaches for the ground as an empty one does.)
        PhysicsBattler.Locomotion.FallPose.ShieldKeptBeforeChest = Environment.GetEnvironmentVariable("FIGHT_SHIELDBEFORE") != "0";
        // (FIGHT_ARMSOUT=0: fallen, its arms and its shield are not stopped by its own body, and its hands are not posed clear of it. "pose": only the pose's part; "body": only the body's.)
        {
            string armsOut = Environment.GetEnvironmentVariable("FIGHT_ARMSOUT");
            PhysicsBattler.Authority.UpperBodyArticulation.ArmsStoppedByBody = armsOut != "0" && armsOut != "pose";
            PhysicsBattler.Locomotion.FallPose.HandsClearOfTrunk = armsOut != "0" && armsOut != "body";
        }

        // (FIGHT_CAUGHT=0: no fall is caught.)
        PhysicsBattler.Locomotion.ClipFighter.FallsCanBeCaught = Environment.GetEnvironmentVariable("FIGHT_CAUGHT") != "0";

        // (FIGHT_GETUP=0: the get-ups as they were before 14 November.)
        PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps = Environment.GetEnvironmentVariable("FIGHT_GETUP") != "0";
        // (FIGHT_GETUPRECORDED=0: the get-ups made by hand, as before 17 November. FIGHT_GETUPTAKES=0.6: a recorded get-up's time, as a share of the recording's.)
        PhysicsBattler.Motion.GetUp.FromRecordings = Environment.GetEnvironmentVariable("FIGHT_GETUPRECORDED") != "0";
        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_GETUPTAKES"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float getUpTakes))
        {
            PhysicsBattler.Motion.GetUp.RecordedTakes = getUpTakes;
        }

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
            if (what.StartsWith("a cut as he turns") && Environment.GetEnvironmentVariable("FIGHT_TURNTRACE") == "1")
            {
                Console.WriteLine($"[turn] {u.name} {Time.time:0.00}: {what}");
            }

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
            aim?.OnHit(h, Time.time);
            dance?.OnHit(h, Time.time);
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
        // (What a step costs here (the scripts as compiled for this runner, the engine as PhysX has it): the fight's
        // step, the units' own, and the physics, by whether any body is down or getting up.)
        var costs = new Dictionary<string, (double fight, double own, double physics, int steps)>();
        var clockFight = new System.Diagnostics.Stopwatch();
        var clockOwn = new System.Diagnostics.Stopwatch();
        var clockPhysics = new System.Diagnostics.Stopwatch();
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
                clockFight.Restart();
                loop.Step(units, dt, Time.time);
                clockFight.Stop();
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

                clockOwn.Restart();
                foreach (var unit in units)
                {
                    Call(unit, "FixedUpdate");
                }

                clockOwn.Stop();

                // (FIGHT_TURN="3,180": at 3 s every unit turns 180 degrees on the spot, as the debug panel's button has them do; its turn back to its enemy is then its own.)
                {
                    string[] turn = (Environment.GetEnvironmentVariable("FIGHT_TURN") ?? "").Split(',');
                    if (turn.Length >= 2 && float.TryParse(turn[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float turnAt) && paceAt >= turnAt && paceAt < turnAt + dt
                        && float.TryParse(turn[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float turnBy))
                    {
                        foreach (var u in units)
                        {
                            // (A third part, "3,180,a m": only the units so named.)
                            if (turn.Length > 2 && !u.name.ToLowerInvariant().Contains(turn[2].ToLowerInvariant()))
                            {
                                continue;
                            }

                            Console.WriteLine($"TURN {paceAt:0.000} {u.name}: {(u.Clip != null && u.Clip.TurnNow(turnBy) ? "turning" : "not turned")}");
                        }
                    }
                }

                // (FIGHT_BEHIND="3,150": at 3 s the duel's other fighter is put 150 degrees round the mover from where the mover
                // faces (+ to his right), as far off as he was and facing him: an enemy at once at his side or behind him.)
                {
                    string[] behind = (Environment.GetEnvironmentVariable("FIGHT_BEHIND") ?? "").Split(',');
                    if (duel != null && behind.Length == 2 && float.TryParse(behind[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float behindAt) && paceAt >= behindAt && paceAt < behindAt + dt
                        && float.TryParse(behind[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float behindBy))
                    {
                        Console.WriteLine($"BEHIND {paceAt:0.000}: {(duel.PutOther(behindBy) ? $"the other put {behindBy:0} degrees round him" : "not put")}");
                    }
                }

                // (FIGHT_BEHINDOTHER="4,150": the same the other way about: at 4 s the duel's mover is put 150 degrees round the other.)
                {
                    string[] behind = (Environment.GetEnvironmentVariable("FIGHT_BEHINDOTHER") ?? "").Split(',');
                    if (duel != null && behind.Length == 2 && float.TryParse(behind[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float behindAt) && paceAt >= behindAt && paceAt < behindAt + dt
                        && float.TryParse(behind[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float behindBy))
                    {
                        Console.WriteLine($"BEHIND {paceAt:0.000}: {(duel.PutMover(behindBy) ? $"the mover put {behindBy:0} degrees round the other" : "not put")}");
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

                // (FIGHT_FILM="path.json,3,5,sword,target": from 3 s to 5 s, each step, the body as posed of every unit so
                // named, written for Tools/Mocap/Rhythm/film.py to draw as a strip of stick figures: each part's joint,
                // the feet's toes and whether each is held down, the sword. What the fight shows, feet's locks and all.)
                {
                    string[] film = (Environment.GetEnvironmentVariable("FIGHT_FILM") ?? "").Split(',');
                    if (film.Length >= 3 && float.TryParse(film[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float filmFrom) && float.TryParse(film[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float filmTo) && paceAt >= filmFrom)
                    {
                        if (paceAt <= filmTo)
                        {
                            var inv = System.Globalization.CultureInfo.InvariantCulture;
                            foreach (var u in units)
                            {
                                bool named = film.Length <= 3;
                                for (int i = 3; i < film.Length; i++)
                                {
                                    named |= u.name.ToLowerInvariant().Contains(film[i].Trim().ToLowerInvariant());
                                }

                                if (u.Clip == null || !named)
                                {
                                    continue;
                                }

                                var an = u.Clip.Anim;
                                var filmLine = new System.Text.StringBuilder();
                                // (Standing in a ginga made of its takes: which take he is in and the moment of it, so the pose can be set beside the recording's.)
                                var filmPattern = u.Clip.Loco.StandsInPattern && u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.Idle ? u.Clip.Loco.Guard.Pattern : null;
                                string filmRef = filmPattern != null && filmPattern.Begun ? "\"ref\":\"" + filmPattern.TakeName + "\",\"reft\":" + filmPattern.TakeTime.ToString("0.000", inv) + ",\"resting\":" + (filmPattern.Resting ? "1" : "0") + ",\"size\":" + PhysicsBattler.Locomotion.StandPattern.Size.ToString("0.00", inv) + ",\"hips\":[" + filmPattern.Hips.x.ToString("0.000", inv) + "," + filmPattern.Hips.y.ToString("0.000", inv) + "," + filmPattern.Hips.z.ToString("0.000", inv) + "," + filmPattern.AnkleL.x.ToString("0.000", inv) + "," + filmPattern.AnkleL.y.ToString("0.000", inv) + "," + filmPattern.AnkleL.z.ToString("0.000", inv) + "," + filmPattern.AnkleR.x.ToString("0.000", inv) + "," + filmPattern.AnkleR.y.ToString("0.000", inv) + "," + filmPattern.AnkleR.z.ToString("0.000", inv) + "]," : "";
                                // (The walk's own hands, against its root, metres: what the arms are given before the guard's own work on them.)
                                var rawHands = u.Clip.Loco.RawPoints;
                                Vector3 rawR = rawHands[(int)PhysicsBattler.Locomotion.LocoBone.RightHand], rawL = rawHands[(int)PhysicsBattler.Locomotion.LocoBone.LeftHand];
                                filmRef += "\"rawh\":[" + rawR.x.ToString("0.000", inv) + "," + rawR.y.ToString("0.000", inv) + "," + rawR.z.ToString("0.000", inv) + "," + rawL.x.ToString("0.000", inv) + "," + rawL.y.ToString("0.000", inv) + "," + rawL.z.ToString("0.000", inv) + "],\"arms\":" + u.Clip.Poser.ClipArms.ToString("0.00", inv) + ",";
                                {
                                    // (The walk's own state: which cycles it blends and how much of each, its phase, pace, the way it travels, its levers' crouch and lean, and how far the last switch still carries it off.)
                                    var wl = u.Clip.Loco;
                                    var wb = new System.Text.StringBuilder();
                                    foreach (var (bc, bw) in wl.Blend)
                                    {
                                        wb.Append(bc.Name).Append(':').Append(bw.ToString("0.00", inv)).Append(';');
                                    }

                                    var wlev = wl.LeversShown;
                                    filmRef += "\"blend\":\"" + wb + "\",\"ph\":" + wl.Phase.ToString("0.000", inv) + ",\"spd\":" + wl.Velocity.magnitude.ToString("0.00", inv) + ",\"want\":" + wl.Wanted.magnitude.ToString("0.00", inv) + ",\"ang\":" + wl.TravelAngle.ToString("0", inv) + ",\"crouch\":" + (wlev.Crouch * PhysicsBattler.Locomotion.Locomotor.ClipLeg * 100f).ToString("0.0", inv) + ",\"lean\":" + wlev.Lean.ToString("0.0", inv) + ",\"sw\":" + wl.Switches + ",\"off\":" + wl.SwitchOffsetNow.ToString("0.000", inv) + ",\"ss\":" + wl.StrideScale.ToString("0.00", inv) + ",\"cad\":" + wl.Cadence.ToString("0.00", inv) + ",\"face\":" + wl.Facing.ToString("0.0", inv) + ",";
                                }

                                filmLine.Append("{").Append(filmRef).Append("\"t\":").Append(paceAt.ToString("0.000", inv)).Append(",\"u\":\"").Append(u.name).Append("\",\"doing\":\"").Append(u.Clip.Loco.Current).Append(u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && u.Clip.Loco.Shot != null ? " " + u.Clip.Loco.Shot.Name : "").Append("\",\"p\":[");
                                foreach (var id in PartInfo.All)
                                {
                                    Vector3 at = an.Part(id).position;
                                    filmLine.Append(at.x.ToString("0.000", inv)).Append(',').Append(at.y.ToString("0.000", inv)).Append(',').Append(at.z.ToString("0.000", inv)).Append(',');
                                }

                                // (The head's top, the hands' ends and the toes: a part's joint is where it begins.)
                                Vector3[] filmEnds =
                                {
                                    an.Part(PartId.Head).position + an.Part(PartId.Head).up * 0.2f,
                                    an.Part(PartId.HandL).position + an.Part(PartId.HandL).rotation * an.RestLocalPosition(PartId.HandL).normalized * 0.08f,
                                    an.Part(PartId.HandR).position + an.Part(PartId.HandR).rotation * an.RestLocalPosition(PartId.HandR).normalized * 0.08f,
                                    an.Part(PartId.FootL).position + an.Part(PartId.FootL).forward * 0.17f - an.Part(PartId.FootL).up * 0.07f,
                                    an.Part(PartId.FootR).position + an.Part(PartId.FootR).forward * 0.17f - an.Part(PartId.FootR).up * 0.07f,
                                };
                                foreach (var at in filmEnds)
                                {
                                    filmLine.Append(at.x.ToString("0.000", inv)).Append(',').Append(at.y.ToString("0.000", inv)).Append(',').Append(at.z.ToString("0.000", inv)).Append(',');
                                }

                                filmLine.Length--;
                                filmLine.Append("],\"d\":[").Append(u.Clip.Poser.Feet[0].Locked ? 1 : 0).Append(',').Append(u.Clip.Poser.Feet[1].Locked ? 1 : 0).Append("],\"s\":[");
                                var slot = an.SlotWeaponR;
                                Vector3 grip = slot != null ? slot.position : an.Part(PartId.HandR).position, tip = slot != null ? slot.position + slot.forward * u.Clip.Poser.WeaponLength : grip;
                                filmLine.Append(grip.x.ToString("0.000", inv)).Append(',').Append(grip.y.ToString("0.000", inv)).Append(',').Append(grip.z.ToString("0.000", inv)).Append(',').Append(tip.x.ToString("0.000", inv)).Append(',').Append(tip.y.ToString("0.000", inv)).Append(',').Append(tip.z.ToString("0.000", inv));
                                filmLine.Append("],\"root\":[").Append(u.Clip.Loco.Position.x.ToString("0.000", inv)).Append(',').Append(u.Clip.Loco.Position.z.ToString("0.000", inv)).Append(',').Append(u.Clip.Loco.Facing.ToString("0.0", inv)).Append("]}");
                                filmLines.Add(filmLine.ToString());
                            }
                        }
                        else if (filmLines.Count > 0)
                        {
                            File.WriteAllText(film[0], "[\n" + string.Join(",\n", filmLines) + "\n]\n");
                            filmLines.Clear();
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

                if (duel != null && aim != null)
                {
                    // (The duel lab: each order given at its time, and how the mover went under it told as it ends.)
                    if (!duel.Running)
                    {
                        duel.Begin(aim.Attacker, aim.Target);
                    }

                    int now = duelAt;
                    while (now + 1 < duelOrders.Count && paceAt >= (now < 0 ? 1.5f : duelOrders[now].until))
                    {
                        now++;
                    }

                    if (now != duelAt || (duelAt >= 0 && duelAt == duelOrders.Count - 1 && paceAt >= duelOrders[duelAt].until && duelNote >= 0f))
                    {
                        if (duelAt >= 0)
                        {
                            float took = paceAt - duelFrom;
                            Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "DUEL {0,-11} for {1:0.0} s: went {2:0.00} m ({3:0.00} m/s over the whole of it), from {4:0.00} m apart to {5:0.00} m; ended {6}; fastest {7:0.00} m/s of {8:0.00} asked; setting off, a foot {9:0.0} m/s and the hips {10:0.0} m/s in 0.35 s ({11:0.00} m gone){12}",
                                duelOrders[duelAt].order, took, duel.Gone, duel.Gone / Mathf.Max(0.1f, took), duelGapFrom, duel.Gap, duel.Now == PhysicsBattler.Sandbox.DuelLab.Order.Stand ? "standing (the order done)" : "still at it", duel.Fastest, duel.PaceNow, duel.SetOffFoot, duel.SetOffHips, duel.SetOffWent, duelOrders[duelAt].order >= PhysicsBattler.Sandbox.DuelLab.Order.LeapIn ? "; " + duel.LastLeap : ""));
                        }

                        if (now != duelAt)
                        {
                            duelAt = now;
                            duelFrom = paceAt;
                            duelGapFrom = duel.Gap;
                            duel.Give(duelOrders[now].order);
                        }
                        else
                        {
                            duelNote = -1f;
                            duel.Give(PhysicsBattler.Sandbox.DuelLab.Order.Stand);
                        }
                    }

                    if (styleSweep > 0f)
                    {
                        PhysicsBattler.Locomotion.StyleManner.Amount = Mathf.Clamp01(paceAt / styleSweep);
                    }

                    // (FIGHT_COURSE=1: the footwork course is run from 2 s in, and what its meter made of it is said as it ends.)
                    if (Environment.GetEnvironmentVariable("FIGHT_COURSE") == "1")
                    {
                        if (!courseBegun && paceAt >= 2f)
                        {
                            courseBegun = duel.BeginCourse();
                        }
                        else if (courseBegun && !courseTold && !duel.CourseOn)
                        {
                            courseTold = true;
                            Console.WriteLine("COURSE his footwork " + duel.Meter.Text);
                        }
                    }

                    duel.Step(dt);
                    // (What he leaves open: once as he stands, and after each posture watched.)
                    if (duel.Opened != openedTold)
                    {
                        openedTold = duel.Opened;
                        Console.WriteLine($"OPENINGS {paceAt:0.00} {duel.Opened}");
                    }

                    if (!openTold && paceAt >= 1.5f && duel.Open.Count > 0)
                    {
                        openTold = true;
                        var open = new System.Text.StringBuilder();
                        var shut = new System.Text.StringBuilder();
                        foreach (var seen in duel.Open)
                        {
                            (seen.Open ? open : shut).Append(seen.Open ? PhysicsBattler.Locomotion.AimSpots.Name(seen.Spot) + ", " : PhysicsBattler.Locomotion.AimSpots.Name(seen.Spot) + " (" + seen.Behind + "), ");
                        }

                        Console.WriteLine($"OPEN as he stands at {paceAt:0.0} s: {open}| covered: {shut}");
                    }

                    if (ownDropAt >= 0f && paceAt - ownDropAt < 0.5f)
                    {
                        ownDropLow = Mathf.Min(ownDropLow, duel.Mover.Clip.Anim.Part(PartId.Pelvis).position.y);
                    }

                    // (FIGHT_WALKSON=0: a walk kept up under a walk's pace is stepped, as it was.)
                    PhysicsBattler.Locomotion.Locomotor.WalksOn = Environment.GetEnvironmentVariable("FIGHT_WALKSON") != "0";
                    // (FIGHT_SLOWSIDE=0/1/2: what a slow walk kept up to the side is walked on: the side-step, the crouched step, the upright strafe.)
                    PhysicsBattler.Locomotion.Locomotor.SlowSide = int.TryParse(Environment.GetEnvironmentVariable("FIGHT_SLOWSIDE"), out int slowSide) ? slowSide : 0;
                    // (FIGHT_SHIELDCOVER=0: the shield arm is the recording's, as it was; FIGHT_COVERHOLD, FIGHT_COVERINBLOW: how much
                    // of the arm is the cover's. Where each of the two carries his board is measured from 2.5 s on: COVER lines.)
                    // (FIGHT_BASHAT="4,9": the mover bashes the other at those seconds, whatever the distance. FIGHT_BASHWITHIN: how
                    // near an enemy must come to be bashed unasked. FIGHT_BASHKIND=small/medium/hard: which of the three. What each asked bash did is said 1.2 s after it: BASHED lines.)
                    {
                        float EnvB(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
                        PhysicsBattler.Locomotion.ClipFighter.BashWithin = EnvB("FIGHT_BASHWITHIN", PhysicsBattler.Locomotion.ClipFighter.BashWithin);
                        PhysicsBattler.Locomotion.ClipFighter.BashThrows = EnvB("FIGHT_BASHTHROWS", PhysicsBattler.Locomotion.ClipFighter.BashThrows);
                        PhysicsBattler.Locomotion.ClipFighter.BashArmCounts = EnvB("FIGHT_BASHARMCOUNTS", PhysicsBattler.Locomotion.ClipFighter.BashArmCounts);
                        string bashKindAsked = (Environment.GetEnvironmentVariable("FIGHT_BASHKIND") ?? "small").Trim().ToLowerInvariant();
                        int bashKind = bashKindAsked == "hard" ? 2 : bashKindAsked == "medium" ? 1 : 0;
                        // (FIGHT_BASHREACH: that bash's arm reach, 0 to 1.)
                        PhysicsBattler.Locomotion.ClipFighter.BashKinds[bashKind].Reach = EnvB("FIGHT_BASHREACH", PhysicsBattler.Locomotion.ClipFighter.BashKinds[bashKind].Reach);
                        foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_BASHAT") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!float.TryParse(at, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bashAt) || duel.Mover == null || duel.Mover.Clip == null || duel.Other == null || duel.Other.Clip == null)
                            {
                                continue;
                            }

                            if (paceAt >= bashAt && paceAt - dt < bashAt)
                            {
                                bashGapWas = Vector3.Distance(duel.Mover.Root.position, duel.Other.Root.position);
                                bashOtherWas = duel.Other.Root.position;
                                bashMoverWas = duel.Mover.Root.position;
                                Console.WriteLine($"BASH {paceAt:0.00}: {(duel.Mover.Clip.Bash(duel.Other.Clip, true, bashKind) ? "begun" : "he could not")}, {bashGapWas:0.00} m apart");
                            }

                            if (paceAt >= bashAt + 1.2f && paceAt - dt < bashAt + 1.2f)
                            {
                                Console.WriteLine($"BASHED {paceAt:0.00}: {duel.Mover.Clip.LastBash}. 1.2 s after: he has gone {Vector3.Distance(duel.Mover.Root.position, bashMoverWas):0.00} m, the other {Vector3.Distance(duel.Other.Root.position, bashOtherWas):0.00} m{(duel.Other.Clip.Fallen ? " and is down" : "")}; they are {Vector3.Distance(duel.Mover.Root.position, duel.Other.Root.position):0.00} m apart");
                            }
                        }
                    }

                    if (paceAt >= 2.5f && duel.Mover != null && duel.Other != null && duel.Mover.Clip != null && duel.Other.Clip != null)
                    {
                        var two = new[] { duel.Mover, duel.Other };
                        for (int k = 0; k < 2; k++)
                        {
                            var c = two[k].Clip;
                            if (c.Fallen || !c.ShieldBoard(out Vector3 centre, out Vector3 normal, out _))
                            {
                                continue;
                            }

                            if (!shieldDumped && Environment.GetEnvironmentVariable("FIGHT_SHIELDDUMP") == "1" && c.Rig.SlotWeaponL != null)
                            {
                                // (How the board sits on the hand: for reading a playtest's logged hand as a board.)
                                shieldDumped = true;
                                var handT = c.Rig.Part(PartId.HandL).transform;
                                Vector3 cLocal = handT.InverseTransformPoint(centre), nLocal = handT.InverseTransformDirection(normal);
                                Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "SHIELDDUMP board centre in the hand's frame ({0:0.000}, {1:0.000}, {2:0.000}); its normal there ({3:0.000}, {4:0.000}, {5:0.000}); slot parent {6}", cLocal.x, cLocal.y, cLocal.z, nLocal.x, nLocal.y, nLocal.z, c.Rig.SlotWeaponL.parent != null ? c.Rig.SlotWeaponL.parent.name : "none"));
                            }

                            var back = Quaternion.Inverse(Quaternion.Euler(0f, c.Loco.Facing, 0f));
                            Vector3 chest = c.Anim.Part(PartId.Chest).position, at = back * (centre - chest);
                            Vector3 toOther = two[1 - k].Clip.Anim.Part(PartId.Chest).position - centre;
                            int kind = c.ShieldArmHeld ? 3 : c.Striking && !c.Posturing ? 2 : c.Posturing ? 1 : 0;
                            coverN[k, kind]++;
                            coverSum[k, kind] += at;
                            coverSq[k, kind] += Vector3.Scale(at, at);
                            coverOff[k, kind] += Vector3.Angle(normal, toOther);
                            if (Mathf.FloorToInt(10f * paceAt) != Mathf.FloorToInt(10f * (paceAt - dt)))
                            {
                                // (What he leaves open, ten times a second: the looking is not cheap.)
                                var from = two[1 - k].Clip.PartAt(PartId.UpperArmR);
                                coverLooks[k, kind]++;
                                coverOpen[k, kind] += PhysicsBattler.Locomotion.Openings.LookAt(c, from, PhysicsBattler.Locomotion.AimSpot.Chest).Open ? 1 : 0;
                                coverBelly[k, kind] += PhysicsBattler.Locomotion.Openings.LookAt(c, from, PhysicsBattler.Locomotion.AimSpot.Belly).Open ? 1 : 0;
                            }
                        }
                    }

                    // (FIGHT_OWN="Prowler,Strutter": the mover's own levers and the other's, FighterLevers' made-up fighters;
                    // FIGHT_OWNON=0: the switch off. What each then does is measured as posed, from 2.5 s to 6 s: OWN lines; and how far
                    // the mover's hips sink in a drop asked after that (FIGHT_DROPAT), against where they were just before it.)
                    {
                        string[] own = (Environment.GetEnvironmentVariable("FIGHT_OWN") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        PhysicsBattler.Locomotion.FighterLevers.On = Environment.GetEnvironmentVariable("FIGHT_OWNON") != "0";
                        var both = new[] { duel.Mover, duel.Other };
                        for (int k = 0; k < both.Length; k++)
                        {
                            if (both[k] == null || both[k].Clip == null)
                            {
                                continue;
                            }

                            if (k < own.Length && both[k].Clip.Loco.Own == null)
                            {
                                both[k].Clip.Loco.Own = PhysicsBattler.Locomotion.FighterLevers.Named(own[k]);
                            }

                            if (own.Length > 0 && paceAt >= 2.5f && paceAt <= 6f)
                            {
                                var an = both[k].Clip.Anim;
                                float hips = an.Part(PartId.Pelvis).position.y - both[k].Root.position.y;
                                Vector3 up = an.Part(PartId.Chest).up, fwd = Quaternion.Euler(0f, both[k].Clip.Loco.Facing, 0f) * Vector3.forward;
                                float lean = Mathf.Atan2(Vector3.Dot(up, fwd), up.y) * Mathf.Rad2Deg;
                                float foot = Mathf.Max(an.Part(PartId.FootL).position.y, an.Part(PartId.FootR).position.y) - both[k].Root.position.y;
                                ownN[k]++; ownHips[k] += hips; ownLean[k] += lean;
                                ownLow[k] = Mathf.Min(ownLow[k], hips); ownHigh[k] = Mathf.Max(ownHigh[k], hips); ownFoot[k] = Mathf.Max(ownFoot[k], foot);
                            }
                        }
                    }

                    // (FIGHT_DROPAT="3,6": he drops at those seconds, PostureMaker's drop.)
                    foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_DROPAT") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        float when = float.Parse(at, System.Globalization.CultureInfo.InvariantCulture);
                        if (paceAt >= when && paceAt - dt < when)
                        {
                            Console.WriteLine($"DROP {paceAt:0.00}: {(duel.Mover.Clip.Drop() ? "begun" : "he could not")}");
                            ownDropAt = paceAt;
                            ownDropFrom = duel.Mover.Clip.Anim.Part(PartId.Pelvis).position.y;
                            ownDropLow = ownDropFrom;
                            duel.WatchOpenings("drop");
                        }
                    }

                    // (FIGHT_BLUFFAT="2:dip,3.5:flick,5:circle,6.5:press": the upper body's postures at those seconds;
                    // FIGHT_GUARDAT="4:high,7:low": a change of guard.)
                    foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_BLUFFAT") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] bits = at.Split(':');
                        float when = float.Parse(bits[0], System.Globalization.CultureInfo.InvariantCulture);
                        if (paceAt >= when && paceAt - dt < when)
                        {
                            var kind = bits[1] == "dip" ? PhysicsBattler.Locomotion.Locomotor.Pulse.BladeDip : bits[1] == "flick" ? PhysicsBattler.Locomotion.Locomotor.Pulse.BladeFlick : bits[1] == "circle" ? PhysicsBattler.Locomotion.Locomotor.Pulse.BladeCircle : PhysicsBattler.Locomotion.Locomotor.Pulse.ShieldPress;
                            Console.WriteLine($"BLUFF {paceAt:0.00} {bits[1]}: {(duel.Mover.Clip.Bluff(kind) ? "begun" : "he could not")}");
                            duel.WatchOpenings(bits[1]);
                        }
                    }

                    foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_GUARDAT") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] bits = at.Split(':');
                        float when = float.Parse(bits[0], System.Globalization.CultureInfo.InvariantCulture);
                        if (paceAt >= when && paceAt - dt < when)
                        {
                            duel.Mover.Clip.GuardAt(bits[1] == "high" ? 1 : bits[1] == "low" ? -1 : 0);
                            Console.WriteLine($"GUARD {paceAt:0.00} {bits[1]}");
                        }
                    }

                    // (FIGHT_SHIELDTEST="3:Chest,3:Chest,3:Belly": the shield test, the plan's step 7: from those seconds on, one
                    // after another as soon as both are back on their spots, the target uncovers that part and the swordsman takes it. FIGHT_READS=0: he only reacts; FIGHT_REACTION=0.2
                    // his reaction time; FIGHT_TELL=0.3 and FIGHT_OPENFOR=0.4 the uncovering's tell and how long the part is
                    // open; FIGHT_QUICKEST=0.45 the least time his blow may take to land. FIGHT_READSAT="12:0": he stops
                    // (0) or starts (1) reading at that second.)
                    if (shieldTest == null && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_SHIELDTEST")))
                    {
                        float EnvF(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
                        shieldTest = new PhysicsBattler.Sandbox.ShieldTest();
                        shieldTest.Begin(aim);
                        shieldTest.Taker.Reads = Environment.GetEnvironmentVariable("FIGHT_READS") != "0";
                        shieldTest.Taker.Reaction = EnvF("FIGHT_REACTION", 0.2f);
                        PhysicsBattler.Locomotion.PostureMaker.UncoverTell = EnvF("FIGHT_TELL", PhysicsBattler.Locomotion.PostureMaker.UncoverTell);
                        PhysicsBattler.Locomotion.PostureMaker.UncoverFor = EnvF("FIGHT_OPENFOR", PhysicsBattler.Locomotion.PostureMaker.UncoverFor);
                        PhysicsBattler.Locomotion.OpeningTaker.Quickest = EnvF("FIGHT_QUICKEST", PhysicsBattler.Locomotion.OpeningTaker.Quickest);
                        // (FIGHT_UNCOVERROOM=0.15: the room the shield leaves about the line. FIGHT_SHIELDATTACK=sns_thrust: the attack he takes it with.)
                        PhysicsBattler.Locomotion.PostureMaker.UncoverRoom = EnvF("FIGHT_UNCOVERROOM", PhysicsBattler.Locomotion.PostureMaker.UncoverRoom);
                        shieldTest.Attack = Environment.GetEnvironmentVariable("FIGHT_SHIELDATTACK");
                        if (Environment.GetEnvironmentVariable("FIGHT_SHIELDTRACE") == "1")
                        {
                            shieldTest.Trace = line => Console.WriteLine("SHIELDSTEP " + line);
                        }

                        shieldTest.Ended += trial => Console.WriteLine($"SHIELDTEST {Time.time - startTime:0.00} {(trial.Reads ? "reads" : "reacts")}, reaction {trial.Reaction:0.00} s: {trial.Verdict}");
                    }

                    if (shieldTest != null)
                    {
                        foreach (string at in (Environment.GetEnvironmentVariable("FIGHT_READSAT") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string[] bits = at.Split(':');
                            float when = float.Parse(bits[0], System.Globalization.CultureInfo.InvariantCulture);
                            if (paceAt >= when && paceAt - dt < when)
                            {
                                shieldTest.Taker.Reads = bits[1].Trim() != "0";
                            }
                        }

                        // (Each go begins at its second or as soon after as it can: both back on their spots, neither busy.)
                        string[] goes = Environment.GetEnvironmentVariable("FIGHT_SHIELDTEST").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (shieldGo < goes.Length)
                        {
                            string[] bits = goes[shieldGo].Split(':');
                            float when = float.Parse(bits[0], System.Globalization.CultureInfo.InvariantCulture);
                            if (paceAt >= when && Enum.TryParse(bits[1].Trim(), true, out PhysicsBattler.Locomotion.AimSpot uncovers))
                            {
                                var spotWas = shieldTest.Spot;
                                shieldTest.Spot = uncovers;
                                if (shieldTest.Cannot.Length > 0)
                                {
                                    shieldTest.Spot = spotWas;
                                }
                                else if (shieldTest.Uncover(Time.time))
                                {
                                    shieldGo++;
                                    Console.WriteLine($"UNCOVER {paceAt:0.00} the {uncovers}: begun");
                                }
                            }
                        }

                        shieldTest.Step(dt, Time.time);
                    }

                    // (The fakes test, the plan's step 7, second part. FIGHT_FAKES="3:short,6:long": the target throws a
                    // fake slash at those seconds. FIGHT_PATTERN=3: from that second it plays its pattern, FIGHT_PATTERNSHORTS=2
                    // short fakes then a long one, one every FIGHT_BEAT=1.5 s; FIGHT_MIXED=1 mixes the count up.
                    // FIGHT_SHORTDEPTH, FIGHT_LONGDEPTH, FIGHT_LONGHOLD: the fakes themselves. FIGHT_READS, FIGHT_REACTION,
                    // FIGHT_QUICKEST as in the shield test.)
                    if (fakeTest == null && (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_FAKES")) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_PATTERN"))))
                    {
                        float EnvG(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
                        fakeTest = new PhysicsBattler.Sandbox.FakeTest();
                        fakeTest.Begin(aim);
                        fakeTest.Taker.Reads = Environment.GetEnvironmentVariable("FIGHT_READS") != "0";
                        fakeTest.Taker.Reaction = EnvG("FIGHT_REACTION", 0.2f);
                        PhysicsBattler.Locomotion.OpeningTaker.Quickest = EnvG("FIGHT_QUICKEST", PhysicsBattler.Locomotion.OpeningTaker.Quickest);
                        fakeTest.Shorts = (int)EnvG("FIGHT_PATTERNSHORTS", fakeTest.Shorts);
                        fakeTest.Beat = EnvG("FIGHT_BEAT", fakeTest.Beat);
                        fakeTest.MixedUp = Environment.GetEnvironmentVariable("FIGHT_MIXED") == "1";
                        // (FIGHT_FAKECOMMITS=1: in a fake of his own his shield does not go to meet a blow.)
                        PhysicsBattler.Locomotion.ClipFighter.PostureCommitsShield = Environment.GetEnvironmentVariable("FIGHT_FAKECOMMITS") == "1";
                        PhysicsBattler.Sandbox.FakeTest.ShortDepth = EnvG("FIGHT_SHORTDEPTH", PhysicsBattler.Sandbox.FakeTest.ShortDepth);
                        PhysicsBattler.Sandbox.FakeTest.LongDepth = EnvG("FIGHT_LONGDEPTH", PhysicsBattler.Sandbox.FakeTest.LongDepth);
                        PhysicsBattler.Sandbox.FakeTest.LongHold = EnvG("FIGHT_LONGHOLD", PhysicsBattler.Sandbox.FakeTest.LongHold);
                        fakeTest.Shown += fake => Console.WriteLine($"FAKE {fake.Began - startTime:0.00} {fake.Text(fakeTest.Spot)}");
                        fakeTest.Struck += struck => Console.WriteLine($"FAKEBLOW {struck.AskedAt - startTime:0.00} {(fakeTest.Taker.Reads ? "reads" : "reacts")}: {struck.Verdict}");
                    }

                    if (fakeTest != null)
                    {
                        string[] fakes = (Environment.GetEnvironmentVariable("FIGHT_FAKES") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (fakeGo < fakes.Length)
                        {
                            string[] bits = fakes[fakeGo].Split(':');
                            if (paceAt >= float.Parse(bits[0], System.Globalization.CultureInfo.InvariantCulture) && fakeTest.Throw(bits[1].Trim() == "long", Time.time))
                            {
                                fakeGo++;
                            }
                        }

                        if (float.TryParse(Environment.GetEnvironmentVariable("FIGHT_PATTERN"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float patternFrom) && paceAt >= patternFrom && !fakeTest.Pattern)
                        {
                            fakeTest.Pattern = true;
                            Console.WriteLine($"PATTERN {paceAt:0.00} begun: {fakeTest.Shorts} short fakes then a long one, every {fakeTest.Beat:0.00} s{(fakeTest.MixedUp ? ", mixed up" : "")}");
                        }

                        fakeTest.Step(dt, Time.time);
                        if (fakeTest.Learned != fakeLearned)
                        {
                            fakeLearned = fakeTest.Learned;
                            Console.WriteLine($"LEARNED {paceAt:0.00} {fakeLearned}");
                        }
                    }

                    // (The dance, the plan's step 8, second part. FIGHT_DANCE="Strutter,Prowler": the two gesture in those
                    // patterns from 2 s on (the mover's, the other's); FIGHT_DANCEREADS="1,0": which of them reads;
                    // FIGHT_DANCEMIXED="0,1": whose pattern is mixed up; FIGHT_DANCEREADING=0: the switch off.)
                    if (dance == null && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIGHT_DANCE")) && paceAt >= 2f && aim != null && aim.Attacker.Clip != null && aim.Target.Clip != null)
                    {
                        string[] names = Environment.GetEnvironmentVariable("FIGHT_DANCE").Split(',');
                        string[] reads = (Environment.GetEnvironmentVariable("FIGHT_DANCEREADS") ?? "1,1").Split(',');
                        string[] mixed = (Environment.GetEnvironmentVariable("FIGHT_DANCEMIXED") ?? "0,0").Split(',');
                        dance = new PhysicsBattler.Sandbox.Dance();
                        dance.Begin(aim.Attacker, aim.Target);
                        PhysicsBattler.Sandbox.Dance.Reading = Environment.GetEnvironmentVariable("FIGHT_DANCEREADING") != "0";
                        // (FIGHT_DANCEANSWERS=1: a blow stopped is answered.)
                        PhysicsBattler.Sandbox.Dance.Answers = Environment.GetEnvironmentVariable("FIGHT_DANCEANSWERS") == "1";
                        // (FIGHT_DANCEMOVES=0: they stand on their spots. With FIGHT_OWN their restless and aggressive are their own.
                        // FIGHT_DANCEGAP=1: the gap and what each is about, twice a second.)
                        PhysicsBattler.Sandbox.Dance.Moves = Environment.GetEnvironmentVariable("FIGHT_DANCEMOVES") != "0";
                        // (FIGHT_DANCECALM=0: the dancers' feet as before 18 November.)
                        PhysicsBattler.Sandbox.Dance.CalmFeet = Environment.GetEnvironmentVariable("FIGHT_DANCECALM") != "0";
                        PhysicsBattler.Locomotion.ClipFighter.PostureCommitsShield = Environment.GetEnvironmentVariable("FIGHT_FAKECOMMITS") == "1";
                        var two = new[] { dance.A, dance.B };
                        for (int k = 0; k < 2; k++)
                        {
                            two[k].Pattern = PhysicsBattler.Sandbox.GesturePattern.Named(names[Math.Min(k, names.Length - 1)]);
                            two[k].Pattern.MixedUp = mixed[Math.Min(k, mixed.Length - 1)].Trim() == "1";
                            two[k].Reads = reads[Math.Min(k, reads.Length - 1)].Trim() != "0";
                            Console.WriteLine($"DANCER {two[k].Unit.name} ({(two[k].Reads ? "reads" : "only reacts")}): {two[k].Pattern.Text}");
                        }

                        string Who(PhysicsBattler.Sandbox.Dancer d) => d == dance.A ? "A" : "B";
                        dance.Shown += (d, g) => Console.WriteLine($"GESTURE {g.Began - startTime:0.00} {Who(d)}: {g.Text}{(g.StruckIn ? " (struck in it)" : "")}");
                        dance.Struck += b => Console.WriteLine($"DANCEBLOW {b.AskedAt - startTime:0.00} {Who(b.By)}: {b.Verdict}");
                        dance.Told += (d, what) => Console.WriteLine($"DANCEREAD {Time.time - startTime:0.00} {Who(d)}: {what}");
                        dance.On = dance.Cannot.Length == 0;
                        Console.WriteLine(dance.On ? $"DANCE {paceAt:0.00} begun" : "DANCE cannot: " + dance.Cannot);
                    }

                    if (dance != null)
                    {
                        dance.Step(dt, Time.time);
                        danceMeterA.Step(dance.A.Unit, dt);
                        danceMeterB.Step(dance.B.Unit, dt);
                        if (Environment.GetEnvironmentVariable("FIGHT_DANCEGAP") == "1" && dance.On && Mathf.FloorToInt(2f * paceAt) != Mathf.FloorToInt(2f * (paceAt - dt)))
                        {
                            Console.WriteLine($"DANCEGAP {paceAt:0.0} {dance.Gap:0.00} m | A {dance.A.Tempo}{(dance.A.Unit.DummyGo.HasValue ? $" going {dance.A.Unit.DummyGo.Value.magnitude:0.0}" : " standing")} {dance.A.Note} | B {dance.B.Tempo}{(dance.B.Unit.DummyGo.HasValue ? $" going {dance.B.Unit.DummyGo.Value.magnitude:0.0}" : " standing")} {dance.B.Note}");
                        }
                        var two = new[] { dance.A, dance.B };
                        for (int k = 0; k < 2; k++)
                        {
                            if (two[k].Learned != danceLearned[k])
                            {
                                danceLearned[k] = two[k].Learned;
                                Console.WriteLine($"DANCELEARNED {paceAt:0.00} {(k == 0 ? "A" : "B")}: {danceLearned[k]}");
                            }
                        }
                    }

                    for (int i = 0; i < duelBlows.Count; i++)
                    {
                        if (paceAt >= duelBlows[i].at)
                        {
                            // (FIGHT_POSTURE=feint | lunge | both: each blow asked for is a posture instead, PostureMaker.)
                            string posture = (Environment.GetEnvironmentVariable("FIGHT_POSTURE") ?? "").ToLowerInvariant();
                            if (posture.Length > 0)
                            {
                                PhysicsBattler.Locomotion.PostureMaker.Ask(posture == "feint" || posture == "both" ? 1f : 0f, posture == "lunge" || posture == "both" ? 1f : 0f);
                            }

                            bool asked = duel.Strike(aim, duelBlows[i].spot, Time.time);
                            duel.WatchOpenings(posture.Length > 0 ? posture : "a blow");
                            Console.WriteLine($"DUELBLOW {paceAt:0.00} at the {duelBlows[i].spot} while {duel.BlowBegan}: {(asked ? "begun, " + duel.BlowThrown : "he could not")}");
                            startWatched = asked;
                            duelBlows.RemoveAt(i);
                            break;
                        }
                    }

                    if (startWatched && duel.BlowStart.Length > 0)
                    {
                        Console.WriteLine("DUELSTART " + duel.BlowStart);
                        startWatched = false;
                    }

                    // (FIGHT_DUELTRACE=1: the first order's setting off, step by step.)
                    if (Environment.GetEnvironmentVariable("FIGHT_DUELTRACE") == "1")
                    {
                        duel.Trace ??= new List<string>();
                        if (duelAt == 0 && duel.Trace.Count > 0)
                        {
                            foreach (string line in duel.Trace)
                            {
                                Console.WriteLine("SETOFF " + line);
                            }
                        }

                        duel.Trace.Clear();
                    }
                }

                if (aim != null)
                {
                    // (The aim test: the spots asked in turn once the two have stood a moment, each blow read as it goes.)
                    if (aimRounds > 0 && paceAt >= 1.5f && aim.Waiting == 0 && !aim.Following)
                    {
                        aimRounds--;
                        aim.OrderAll();
                    }

                    aim.Step(dt, Time.time);
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
                clockPhysics.Restart();
                if (PhysicsBattler.Authority.PhysicsSteps.Manual)
                {
                    PhysicsBattler.Authority.PhysicsSteps.Run(dt);
                }
                else
                {
                    Physics.Simulate(dt);
                }

                clockPhysics.Stop();
                {
                    int down = units.Count(u => u.Clip != null && u.Clip.Footing == PhysicsBattler.Locomotion.ClipFighter.Footings.Down && u.IsAlive);
                    int rising = units.Count(u => u.Clip != null && u.Clip.Footing == PhysicsBattler.Locomotion.ClipFighter.Footings.GettingUp && u.IsAlive);
                    string phase = down > 0 ? (2 * down >= units.Count ? "half or more down" : "some down") : rising > 0 ? "some getting up" : "all on their feet";
                    var c = costs.TryGetValue(phase, out var had) ? had : default;
                    costs[phase] = (c.fight + clockFight.Elapsed.TotalMilliseconds, c.own + clockOwn.Elapsed.TotalMilliseconds, c.physics + clockPhysics.Elapsed.TotalMilliseconds, c.steps + 1);
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

        if (duel != null && duel.Mover != null && duel.Other != null && duel.Mover.Clip != null && duel.Other.Clip != null)
        {
            Console.WriteLine($"BASHES {duel.Mover.name}: {duel.Mover.Clip.Bashes} thrown, {duel.Mover.Clip.BashesLanded} shoved the other; {duel.Other.name}: {duel.Other.Clip.Bashes} thrown, {duel.Other.Clip.BashesLanded} shoved the other");
        }

        if (duel != null)
        {
            string[] kinds = { "standing and walking", "in a fake", "in a blow of his own", "meeting a blow" };
            var two = new[] { duel.Mover, duel.Other };
            for (int k = 0; k < 2; k++)
            {
                for (int kind = 0; kind < 4; kind++)
                {
                    int n = coverN[k, kind];
                    if (n < 30 || two[k] == null)
                    {
                        continue;
                    }

                    Vector3 mean = coverSum[k, kind] / n, var = coverSq[k, kind] / n - Vector3.Scale(mean, mean);
                    float moves = Mathf.Sqrt(Mathf.Max(0f, var.x) + Mathf.Max(0f, var.y) + Mathf.Max(0f, var.z));
                    Console.WriteLine($"COVER {two[k].name} {kinds[kind]} ({n / 60f:0.0} s): the board's middle from his chest {100f * mean.x:0} cm to his right, {100f * mean.y:0} up, {100f * mean.z:0} ahead; it moves about {100f * moves:0.0} cm (rms); it looks {coverOff[k, kind] / n:0} deg off the other; his chest is open to the other {100f * coverOpen[k, kind] / Mathf.Max(1, coverLooks[k, kind]):0}% of the time, his belly {100f * coverBelly[k, kind] / Mathf.Max(1, coverLooks[k, kind]):0}%");
                }
            }
        }

        if (dance != null)
        {
            Console.WriteLine("DANCEFOOTWORK A his footwork " + danceMeterA.Text);
            Console.WriteLine("DANCEFOOTWORK B his footwork " + danceMeterB.Text);
            Console.WriteLine($"DANCESETOFFS the two set off from a stand {dance.SetOffs} times in {danceMeterA.Seconds:0} s (once every {2f * danceMeterA.Seconds / Mathf.Max(1, dance.SetOffs):0.0} s each); whole steps begun {dance.A.Unit.Clip.Loco.WholeStepsBegun} and {dance.B.Unit.Clip.Loco.WholeStepsBegun}, stands to let the asking catch up {dance.A.Unit.Clip.Loco.StepPauses} and {dance.B.Unit.Clip.Loco.StepPauses}");
            Console.WriteLine($"DANCETALLY A {dance.A.Unit.name} ({dance.A.Pattern.Name}, {(dance.A.Reads ? "reads" : "only reacts")}): {dance.Tally(dance.A)}");
            Console.WriteLine($"DANCEFEET {dance.Feet()}");
            Console.WriteLine($"DANCECLASS A: {dance.Classes(dance.A)}");
            Console.WriteLine($"DANCECLASS B: {dance.Classes(dance.B)}");
            Console.WriteLine($"DANCETALLY B {dance.B.Unit.name} ({dance.B.Pattern.Name}, {(dance.B.Reads ? "reads" : "only reacts")}): {dance.Tally(dance.B)}");
        }

        if (duel != null && ownDropAt >= 0f)
        {
            Console.WriteLine($"OWNDROP {duel.Mover.name}: in his drop his hips sank {100f * (ownDropFrom - ownDropLow):0.0} cm");
        }

        if (duel != null)
        {
            var both = new[] { duel.Mover, duel.Other };
            for (int k = 0; k < 2; k++)
            {
                if (ownN[k] > 0 && both[k] != null)
                {
                    var own = both[k].Clip.Loco.Own;
                    Console.WriteLine($"OWN {both[k].name} ({(own != null ? own.Name : "none")}{(PhysicsBattler.Locomotion.FighterLevers.On ? "" : ", switch off")}): hips carried {100f * ownHips[k] / ownN[k]:0.0} cm up, rising and falling {100f * (ownHigh[k] - ownLow[k]):0.0} cm; chest leans {ownLean[k] / ownN[k]:0.0} deg; feet lifted to {100f * ownFoot[k]:0.0} cm");
                }
            }
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
            if (aim != null)
            {
                sb.AppendLine($"    the aim test (a swordsman {Vector3.Distance(aim.Attacker.DummySpot, aim.Target.DummySpot):0.00} m from a target with {(aimKit == "shield" ? "sword and shield" : aimKit == "sword" ? "a sword" : "open hands")}): {aim.Tally()}");
                if (fakeTest != null)
                {
                    sb.AppendLine($"    the fakes test: {fakeTest.Tally()}");
                    Console.WriteLine($"FAKETALLY {fakeTest.Tally()}");
                }

                if (shieldTest != null)
                {
                    sb.AppendLine($"    the shield test: {shieldTest.Tally()}");
                }

                foreach (var blow in aim.Blows)
                {
                    sb.AppendLine("        " + (blow.Done ? blow.Text : $"asked for the {PhysicsBattler.Locomotion.AimSpots.Name(blow.Spot)} with {blow.Strike}: not over when the fight ended"));
                }
            }

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
            foreach (var c in costs.OrderBy(c => c.Key))
            {
                double all = (c.Value.fight + c.Value.own + c.Value.physics) / c.Value.steps;
                sb.AppendLine($"    a step's cost here with {units.Count} units, {c.Key} ({c.Value.steps} steps): {all:0.00} ms ({all / units.Count:0.00} a unit): the fight and the poses {c.Value.fight / c.Value.steps:0.00}, the units' own steps {c.Value.own / c.Value.steps:0.00}, the physics and the muscles {c.Value.physics / c.Value.steps:0.00}");
            }

            var fell = units.Where(u => u.Clip != null).SelectMany(u => u.Clip.FallsMeasured.Select(f => (u, f))).ToList();
            if (fell.Count > 0)
            {
                var living = fell.Where(x => !x.f.Dead).Select(x => x.f).ToList();
                sb.AppendLine($"    falls {(PhysicsBattler.Locomotion.ClipFighter.OneBodyFalls ? $"in the one body, muscle {PhysicsBattler.Locomotion.ClipFighter.FallMuscle:0.00}" : "as ragdolls, as before")}: in the one body {units.Sum(u => u.Clip != null ? u.Clip.OneBodyFallen : 0)}; a joint furthest from the fall's pose: " + string.Join(", ", units.Where(u => u.Clip?.Upper != null).Select(u => $"{u.Clip.Upper.FallOffMostPart} {u.Clip.Upper.FallOffMost:0}")) + " degrees");
                sb.AppendLine($"    falls it tucked in (thrown off its feet): {units.Sum(u => u.Clip != null ? u.Clip.Tucked : 0)}");
                sb.AppendLine($"    hands held where they landed in a fall: {units.Sum(u => u.Clip != null ? u.Clip.HandsHeld : 0)}; came away again: {units.Sum(u => u.Clip != null ? u.Clip.HandsCameAway : 0)}; dragged over the ground {100f * units.Sum(u => u.Clip != null ? u.Clip.HandsDragged : 0f):0} cm in all; let go because the body had come down over them: {units.Sum(u => u.Clip != null ? u.Clip.HandsFromUnder : 0)}");
                sb.AppendLine($"    falls caught on a knee and a hand: {units.Sum(u => u.Clip != null ? u.Clip.Caught : 0)}; begun so and fallen after all: {units.Sum(u => u.Clip != null ? u.Clip.CaughtAndLost : 0)}");
                sb.AppendLine($"    falls measured: {fell.Count} ({living.Count} of the living). Of the living: the head reached the ground in {living.Count(f => f.HeadTouched)}{(living.Any(f => f.HeadTouched) ? $" (at {living.Where(f => f.HeadTouched).Average(f => f.HeadSpeed):0.0} m/s on average, {living.Where(f => f.HeadTouched).Max(f => f.HeadSpeed):0.0} at most)" : "")}; landed on a hand or knee before the hips, trunk or head in {living.Count(f => f.Braced)}; the hips landed at {(living.Any(f => f.HipsAt >= 0f) ? living.Where(f => f.HipsAt >= 0f).Average(f => f.HipsSpeed) : 0f):0.0} m/s on average; slid {(living.Count > 0 ? 100f * living.Average(f => f.Slid) : 0f):0} cm on average; their hands went {(living.Count > 0 ? 100f * living.Average(f => f.HandsSlid) : 0f):0} cm over the ground on average and left it again {living.Sum(f => f.HandsLifted)} times in all");
                // (What they held, by how they fell: in the one body, whose pose and solids keep it, and as ragdolls (a limb off), the dead with them.)
                foreach (var (kind, falls) in new[] { ("in the one body, the living", living.Where(f => !f.Ragdoll).ToList()), ("as ragdolls, the living and the dead", fell.Select(x => x.f).Where(f => f.Ragdoll).ToList()) })
                {
                    if (falls.Count == 0)
                    {
                        continue;
                    }

                    var armedFalls = falls.Where(f => f.Armed).ToList();
                    var shieldFalls = falls.Where(f => f.Shielded).ToList();
                    sb.AppendLine($"    what they held as they fell and lay ({kind}: {falls.Count}; as seen): an arm in its own body {falls.Average(f => f.ArmInFor):0.00} s a fall ({falls.Max(f => f.ArmInFor):0.00} at most); {armedFalls.Count} with a weapon: in its own body {(armedFalls.Count > 0 ? armedFalls.Average(f => f.BladeInFor) : 0f):0.00} s a fall ({(armedFalls.Count > 0 ? armedFalls.Max(f => f.BladeInFor) : 0f):0.00} at most), its end at most {(armedFalls.Count > 0 ? armedFalls.Max(f => f.BladeFastest) : 0f):0.0} m/s, lowest {(armedFalls.Count > 0 ? 100f * armedFalls.Min(f => f.BladeLowest) : 0f):0} cm over the ground; {shieldFalls.Count} with a shield: in its own body {(shieldFalls.Count > 0 ? shieldFalls.Average(f => f.ShieldInFor) : 0f):0.00} s a fall ({(shieldFalls.Count > 0 ? shieldFalls.Max(f => f.ShieldInFor) : 0f):0.00} at most), at most {(shieldFalls.Count > 0 ? 100f * shieldFalls.Max(f => f.ShieldDeepest) : 0f):0} cm in, lowest {(shieldFalls.Count > 0 ? 100f * shieldFalls.Min(f => f.ShieldLowest) : 0f):0} cm over the ground");
                }
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
