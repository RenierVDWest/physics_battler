// The plan "A different approach", step 3, the experiment: a hit on a physical body. A Medium stands in its guard,
// its legs the pose's, its upper body (pelvis to head, both arms) one articulation on muscles of limited strength
// (ArticulatedBody), and takes blows of a known weight: a ball of so many kilograms at so many metres a second, at
// the chest, the head, an arm. Measured for each: which part moved most and how the rest followed, whether it went
// the blow's way, how long until the body is back on its pose, whether it wobbled, and what push the pelvis passed on
// to the legs. The same blows on today's body (held to the pose by its pins) for the before.
#if PHYSX
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PhysicsBattler.Authority;
using PhysicsBattler.Engine;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class HitExperiment
{
    private static readonly bool Trace = Environment.GetEnvironmentVariable("HIT_TRACE") != null;

    private sealed class Setup
    {
        public string Name;
        public bool Articulated;
        public int Substeps = 4;
        /// <summary>The torso's motors (spine, chest), the neck's and the arms': how quick (Hz).</summary>
        public float Torso = 6f, Neck = 8f, Arms = 10f;
        /// <summary>The most the spine and chest turn with (N m); the neck a quarter, the shoulder half, the elbow a third, the wrist an eighth.</summary>
        public float Strength = 600f;
        /// <summary>A struck body's muscles let go to this share of their quickness for <see cref="LooseFor"/> seconds (1: they don't).</summary>
        public float Loose = 1f, LooseFor = 0.12f;
        /// <summary>What the blow gives the body goes to its walk as a change of speed (its weight over the body's mass): the legs take it, stepping.</summary>
        public bool Legs;
        /// <summary>Built by the game's own class (<see cref="UpperBodyArticulation"/>, on the stubs' ArticulationBody, the game stepping the engine), not straight in the engine.</summary>
        public bool Game;
    }

    private sealed class Blow
    {
        public string Name;
        public PartId At;
        /// <summary>The way the ball travels, against the body's facing (x its right, z its front).</summary>
        public Vector3 Way;
        public float Mass, Speed;
    }

    private sealed class Result
    {
        public PartId Struck;
        public float Delivered, BackAfter, Along, PassedOn, MostHold, StruckPeakAt, Driven, DrivenSpeed;
        public int Wobbles;
        public bool Blew, Met;
        public readonly Dictionary<PartId, float> Moved = new Dictionary<PartId, float>();
    }

    public static int Run(string[] args)
    {
        var ci = CultureInfo.InvariantCulture;
        Physics.Backend = new PhysXWorld(false);
        JointConventions.EnsureProbed();
        ArticulationConventions.EnsureProbed();
        float F(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Float, ci, out float v) ? v : otherwise;
        var setups = new List<Setup>
        {
            new Setup { Name = "today's body (held to the pose by its pins), 60 Hz", Articulated = false, Substeps = 1 },
            new Setup { Name = "muscles: torso 3 Hz", Articulated = true, Torso = 3f, Neck = 5f },
            new Setup { Name = "muscles: torso 6 Hz", Articulated = true, Torso = 6f, Neck = 8f },
            new Setup { Name = "muscles: torso 12 Hz", Articulated = true, Torso = 12f, Neck = 14f },
            new Setup { Name = "muscles: torso 12 Hz, let go to a quarter for 0.12 s when struck", Articulated = true, Torso = 12f, Neck = 14f, Loose = 0.25f },
            new Setup { Name = "the game's own upper body: torso 6 Hz", Articulated = true, Game = true, Torso = 6f, Neck = 8f },
            new Setup { Name = "muscles: torso 6 Hz, and the legs take the blow", Articulated = true, Torso = 6f, Neck = 8f, Legs = true },
            new Setup { Name = "muscles: torso 6 Hz, let go to a quarter for 0.12 s, and the legs take the blow", Articulated = true, Torso = 6f, Neck = 8f, Loose = 0.25f, Legs = true },
            new Setup { Name = "muscles: as set", Articulated = true, Legs = Environment.GetEnvironmentVariable("HIT_LEGS") != null, Torso = F("HIT_TORSO", 6f), Neck = F("HIT_NECK", 8f), Arms = F("HIT_ARMS", 10f), Strength = F("HIT_STRENGTH", 600f), Loose = F("HIT_LOOSE", 1f), LooseFor = F("HIT_LOOSE_FOR", 0.12f) },
        };
        var blows = new List<Blow>
        {
            new Blow { Name = "the chest from the front, light (3 kg at 3 m/s)", At = PartId.Chest, Way = Vector3.back, Mass = 3f, Speed = 3f },
            new Blow { Name = "the chest from the front (3 kg at 8 m/s)", At = PartId.Chest, Way = Vector3.back, Mass = 3f, Speed = 8f },
            new Blow { Name = "the chest from the front, heavy (6 kg at 10 m/s)", At = PartId.Chest, Way = Vector3.back, Mass = 6f, Speed = 10f },
            new Blow { Name = "the chest from its right (3 kg at 8 m/s)", At = PartId.Chest, Way = Vector3.left, Mass = 3f, Speed = 8f },
            new Blow { Name = "the head from its right (2 kg at 8 m/s)", At = PartId.Head, Way = Vector3.left, Mass = 2f, Speed = 8f },
            new Blow { Name = "the head from the front (2 kg at 8 m/s)", At = PartId.Head, Way = Vector3.back, Mass = 2f, Speed = 8f },
            new Blow { Name = "the weapon arm from its right (3 kg at 8 m/s)", At = PartId.UpperArmR, Way = Vector3.left, Mass = 3f, Speed = 8f },
            new Blow { Name = "the back from behind (3 kg at 8 m/s)", At = PartId.Chest, Way = Vector3.forward, Mass = 3f, Speed = 8f },
            new Blow { Name = "the chest from the front, a Large's club (20 kg at 8 m/s)", At = PartId.Chest, Way = Vector3.back, Mass = 20f, Speed = 8f },
        };
        string which = Environment.GetEnvironmentVariable("HIT_SETUP"), only = Environment.GetEnvironmentVariable("HIT_ONLY");
        Console.WriteLine("A hit on a physical body: a Medium in its guard, struck by a ball. For each blow: the part struck, how far each part");
        Console.WriteLine("  went from its pose (cm, most first), the share of the struck part's move that was the blow's way, how long until every");
        Console.WriteLine("  part was back within 2 cm, how often the struck part swung back out, and the push passed on to the legs.");
        foreach (var setup in setups)
        {
            if (which != null ? !setup.Name.Contains(which) : setup.Name == "muscles: as set") continue;
            Console.WriteLine($"{setup.Name}:");
            foreach (var blow in blows)
            {
                if (only != null && !blow.Name.Contains(only)) continue;
                var r = Strike(setup, blow);
                if (!r.Met)
                {
                    Console.WriteLine($"  {blow.Name}: the ball never met the body");
                    continue;
                }

                string moved = string.Join(", ", r.Moved.OrderByDescending(m => m.Value).Take(6).Select(m => string.Format(ci, "{0} {1:0.0}", m.Key, m.Value * 100f)));
                Console.WriteLine(string.Format(ci, "  {0}: {1:0} N s given; struck {2}; moved: {3}; {4:0}% the blow's way, at its most after {5:0.00} s; back after {6:0.00} s; swung back out {7} times; passed on {8:0} N s (most {9:0} N){10}{11}",
                    blow.Name, r.Delivered, r.Struck, moved, r.Along * 100f, r.StruckPeakAt, r.BackAfter, r.Wobbles, r.PassedOn, r.MostHold,
                    setup.Legs ? string.Format(ci, "; its feet took it {0:0} cm (at most {1:0.00} m/s)", r.Driven * 100f, r.DrivenSpeed) : "", r.Blew ? "; BLEW UP" : ""));
            }
        }

        return 0;
    }

    private static Result Strike(Setup setup, Blow blow)
    {
        const float dt = 1f / 60f;
        foreach (var body in Physics.Bodies.ToArray()) UnityEngine.Object.DestroyImmediate(body.gameObject);
        foreach (var collider in Physics.Colliders.ToArray()) UnityEngine.Object.DestroyImmediate(collider.gameObject);
        var world = PhysXWorld.Install(BodyPart.GroundLayer, setup.Game ? 1 : setup.Substeps);
        PhysicsSteps.Manual = setup.Game;
        PhysicsSteps.Substeps = setup.Substeps;
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = "Medium";
        var root = UnitBuilder.Build(rig, profile, "Struck", _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var fighter = new ClipFighter(root, unitRig, profile, Vector3.zero, 0f, 0f, LocoLibrary.CombatPrefix("Medium"), physical: true) { Defends = false };
        fighter.Loco.Place(Vector3.zero, 0f);
        var parts = ArticulatedBody.UpperBody;
        ArticulatedBody body2 = null;
        UpperBodyArticulation upper = null;
        void Before(int k, int n, float sub)
        {
            fighter.Authority.Substep(k, n);
            upper?.Substep(k, n, dt);
        }

        void Step()
        {
            fighter.Move(dt, Vector3.zero, 0f, new Vector3(0f, 1.3f, 2f), false);
            fighter.Apply(dt);
            if (setup.Game)
            {
                upper?.BeginStep(dt);
                PhysicsSteps.Run(dt);
                upper?.MoveShadows();
            }
            else
            {
                Physics.Simulate(dt);
            }
        }

        if (setup.Game) PhysicsSteps.BeforeSubstep += Before;

        for (int i = 0; i < 5; i++) Step();
        (float hz, float most) Muscle(PartId id, float loose)
        {
            switch (id)
            {
                case PartId.Spine:
                case PartId.Chest: return (setup.Torso * loose, setup.Strength);
                case PartId.Head: return (setup.Neck * loose, 0.25f * setup.Strength);
                case PartId.UpperArmL:
                case PartId.UpperArmR: return (setup.Arms * loose, 0.5f * setup.Strength);
                case PartId.ForearmL:
                case PartId.ForearmR: return (setup.Arms * loose, 0.33f * setup.Strength);
                default: return (setup.Arms * loose, 0.125f * setup.Strength);
            }
        }

        if (setup.Articulated)
        {
            foreach (var id in parts)
            {
                var c = unitRig.Part(id).partCollider;
                if (c != null) c.enabled = false;
            }

            if (setup.Game)
            {
                foreach (var id in parts)
                {
                    fighter.Authority.Skipped[(int)id] = true;
                    unitRig.Part(id).body.isKinematic = true;
                }

                upper = new UpperBodyArticulation(unitRig, fighter.Anim, null, id => Muscle(id, 1f));
            }
            else
            {
                body2 = new ArticulatedBody(world, fighter.Anim, unitRig, parts, id => Muscle(id, 1f));
            }
        }

        Vector3 Off(PartId id) => upper != null ? upper.Link(id).transform.position - fighter.Anim.Part(id).position
            : body2 != null ? body2.Off(id) : unitRig.Part(id).body.position - fighter.Anim.Part(id).position;
        Vector3 At(PartId id) => upper != null ? upper.Link(id).transform.position : body2 != null ? body2.PartAt(id, out _) : unitRig.Part(id).body.position;
        // (A part's middle: its collider's, as it sits on the part.)
        var middle = new Dictionary<PartId, Vector3>();
        foreach (var id in parts)
        {
            var part = unitRig.Part(id);
            Vector3 c = part.partCollider is BoxCollider box ? box.center : part.partCollider is CapsuleCollider capsule ? capsule.center : part.partCollider is SphereCollider ball0 ? ball0.center : Vector3.zero;
            middle[id] = part.partCollider != null ? Quaternion.Inverse(part.transform.rotation) * (part.partCollider.transform.TransformPoint(c) - part.transform.position) : Vector3.zero;
        }

        Vector3 Middle(PartId id)
        {
            if (upper != null)
            {
                var t = upper.Link(id).transform;
                return t.position + t.rotation * middle[id];
            }

            if (body2 != null)
            {
                Vector3 at = body2.PartAt(id, out var turn);
                return at + turn * middle[id];
            }

            return unitRig.Part(id).body.position + unitRig.Part(id).body.rotation * middle[id];
        }

        var rest = new Dictionary<PartId, float>();
        foreach (var id in parts) rest[id] = 0f;
        for (int i = 0; i < 150; i++)
        {
            Step();
            if (i >= 110)
            {
                foreach (var id in parts) rest[id] = Mathf.Max(rest[id], Off(id).magnitude);
            }
        }

        if (Trace)
        {
            if (body2 != null) Console.WriteLine("    what hangs from each joint (kg m2): " + string.Join(", ", parts.Skip(1).Select(id => $"{id} {body2.Hangs(id)}")));
            if (upper != null) Console.WriteLine("    what hangs from each joint (kg m2): " + string.Join(", ", parts.Skip(1).Select(id => $"{id} {upper.Hangs(id)}")));
            if (body2 != null) Console.WriteLine("    joints off (deg): " + string.Join(", ", parts.Skip(1).Select(id => $"{id} {body2.JointOff(id):0.0}")));
        }

        if (Trace) Console.WriteLine("    standing, off its pose: " + string.Join(", ", parts.Select(id => $"{id} {rest[id] * 100f:0.0}")));

        // The blow: a ball on its way to where the part is now.
        var result = new Result();
        Vector3 way = blow.Way.normalized, target = Middle(blow.At);
        float unitMass = unitRig.Parts.Sum(p => p.body.mass);
        Vector3 ballWas = way * blow.Speed, owed = Vector3.zero, stoodAt = fighter.Loco.Position, passedBefore = Vector3.zero;
        var ballObject = new GameObject("Ball");
        ballObject.transform.position = target - way * 0.6f;
        var sphere = ballObject.AddComponent<SphereCollider>();
        sphere.radius = 0.06f;
        var ball = ballObject.AddComponent<Rigidbody>();
        ball.mass = blow.Mass;
        ball.useGravity = false;
        ball.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        ball.linearVelocity = way * blow.Speed;
        // (HIT_FILM=<folder>: where every part and the ball are, each step, for hitfilm.py to draw.)
        string filmTo = Environment.GetEnvironmentVariable("HIT_FILM");
        StreamWriter film = null;
        if (filmTo != null)
        {
            Directory.CreateDirectory(filmTo);
            string Clean(string name) => new string(name.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            film = new StreamWriter(Path.Combine(filmTo, Clean(setup.Name) + "+" + Clean(blow.Name) + ".csv"));
            film.WriteLine("step,met,thing,x,y,z");
        }

        int metAt = -1, lastOut = -1, struckPeak = -1;
        Vector3 passedAfter = Vector3.zero;
        float struckMost = 0f;
        var series = new List<float>();
        foreach (var id in parts) result.Moved[id] = 0f;
        for (int i = 0; i < 240; i++)
        {
            if (metAt < 0 && body2 != null)
            {
                passedBefore = body2.PassedOn;
                body2.MostHold = 0f;
            }

            Step();
            // (What the ball gave the body this step, flat: to its walk as a change of speed, once it is worth a step.)
            if (sphere.enabled)
            {
                Vector3 gave = blow.Mass * (ballWas - ball.linearVelocity);
                ballWas = ball.linearVelocity;
                if (setup.Legs)
                {
                    owed += Vector3.ProjectOnPlane(gave, Vector3.up) / unitMass;
                }
            }

            if (setup.Legs && owed.magnitude > 0.05f && metAt >= 0 && i - metAt >= 1)
            {
                fighter.Loco.Push(owed);
                owed = Vector3.zero;
            }

            if (film != null)
            {
                foreach (var id in PartInfo.All)
                {
                    Vector3 at = upper != null && Array.IndexOf(parts, id) >= 0 ? upper.Link(id).transform.position
                        : body2 != null && Array.IndexOf(parts, id) >= 0 ? body2.PartAt(id, out _) : unitRig.Part(id).body.position;
                    film.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3:0.0000},{4:0.0000},{5:0.0000}", i, metAt >= 0 ? i - metAt : -1, id, at.x, at.y, at.z));
                }

                if (sphere.enabled) film.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},Ball,{2:0.0000},{3:0.0000},{4:0.0000}", i, metAt >= 0 ? i - metAt : -1, ball.position.x, ball.position.y, ball.position.z));
            }

            result.Driven = Mathf.Max(result.Driven, Vector3.ProjectOnPlane(fighter.Loco.Position - stoodAt, Vector3.up).magnitude);
            result.DrivenSpeed = Mathf.Max(result.DrivenSpeed, fighter.Loco.Velocity.magnitude);
            if (metAt < 0)
            {
                if ((ball.linearVelocity - way * blow.Speed).magnitude > 0.02f * blow.Speed)
                {
                    metAt = i;
                    result.Met = true;
                    // (The part struck: the nearest to the ball as it met the body.)
                    result.Struck = parts.OrderBy(id => (Middle(id) - ball.position).sqrMagnitude).First();
                    if (setup.Loose < 1f)
                    {
                        foreach (var id in parts) { var m = Muscle(id, setup.Loose); body2?.SetMuscle(id, m.hz, m.most); upper?.SetMuscle(id, m.hz, m.most); }
                    }
                }
                else if (i > 60)
                {
                    break;
                }

                continue;
            }

            int since = i - metAt;
            if (since == 30 && body2 != null) passedAfter = body2.PassedOn;   // (the half second after it met the body)
            if (since == 9)
            {
                // (The ball has given what it had: taken away, so it doesn't drift back into the body.)
                result.Delivered = blow.Mass * (ball.linearVelocity - way * blow.Speed).magnitude;
                sphere.enabled = false;
                ball.linearVelocity = Vector3.zero;
                ballObject.transform.position = new Vector3(0f, -50f, 0f);
                ball.position = ballObject.transform.position;
            }

            if (setup.Loose < 1f && since == Mathf.RoundToInt(setup.LooseFor / dt))
            {
                foreach (var id in parts) { var m = Muscle(id, 1f); body2?.SetMuscle(id, m.hz, m.most); upper?.SetMuscle(id, m.hz, m.most); }
            }

            bool anyOut = false;
            foreach (var id in parts)
            {
                Vector3 off = Off(id);
                float far = off.magnitude;
                if (float.IsNaN(far) || far > 3f)
                {
                    result.Blew = true;
                }

                result.Moved[id] = Mathf.Max(result.Moved[id], far);
                anyOut |= far > rest[id] + 0.02f;
                if (id == result.Struck)
                {
                    series.Add(far);
                    if (far > struckMost)
                    {
                        struckMost = far;
                        struckPeak = since;
                        result.Along = far > 1e-4f ? Mathf.Abs(Vector3.Dot(off, way)) / far : 0f;
                    }
                }
            }

            if (anyOut) lastOut = since;
            if (Trace && since % 3 == 0 && since < 90)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    {0:0.00} s: {1}", since * dt, string.Join(", ", parts.Select(id => $"{id} {Off(id).magnitude * 100f:0.0}"))));
            }

            if (result.Blew) break;
        }

        result.BackAfter = (lastOut + 1) * dt;
        result.StruckPeakAt = Mathf.Max(0, struckPeak) * dt;
        // (Swung back out: after its most, each time the struck part's distance from its pose rises again by over a centimetre.)
        float low = struckMost;
        bool rising = false;
        for (int k = Mathf.Max(0, struckPeak); k < series.Count; k++)
        {
            if (!rising)
            {
                low = Mathf.Min(low, series[k]);
                if (series[k] > low + 0.01f)
                {
                    rising = true;
                    result.Wobbles++;
                }
            }
            else if (k > 0 && series[k] < series[k - 1])
            {
                rising = false;
                low = series[k];
            }
        }

        if (body2 != null)
        {
            result.PassedOn = Vector3.ProjectOnPlane(passedAfter - passedBefore, Vector3.up).magnitude;
            result.MostHold = body2.MostHold;
            body2.Release();
        }

        film?.Dispose();
        if (setup.Game) PhysicsSteps.BeforeSubstep -= Before;
        PhysicsSteps.Manual = false;
        upper?.Destroy();
        fighter.Destroy();
        return result;
    }
}
#endif
