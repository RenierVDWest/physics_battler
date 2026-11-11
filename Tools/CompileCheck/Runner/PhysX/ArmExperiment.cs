// The plan "A different approach", step 2: one physical sword arm. `physx-arm`: a Medium's strikes with the weapon arm
// held three ways (kinematic as today; the pin's servo; on its joints' motors), at 60 and 240 physics steps a second,
// swung at nothing, at a fixed post and at a free weight, on the real engine.
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

internal static class ArmExperiment
{
    private static readonly bool Trace = Environment.GetEnvironmentVariable("ARM_TRACE") != null;

    private sealed class Setup
    {
        public string Name;
        public AuthorityLevel Arm;
        public float Strength = 1f, MostHz = 24f, MostForce = float.MaxValue;
        public int Substeps = 1;
        public AuthorityLevel? Torso;
        public float MostLoad = 8f;
        public bool ByInertia;
        public int Iters;
    }

    private struct Result
    {
        public float TipOff, TipOffAtContact, PeakSpeed, Deepest, InPostSeconds, FastestAfter, OffAtEnd, WeightSpeed, Fastest;
        public bool Blew;
    }

    public static int Run(string[] args)
    {
        var ci = CultureInfo.InvariantCulture;
        Physics.Backend = new PhysXWorld(false);
        JointConventions.EnsureProbed();
        float force = float.TryParse(Environment.GetEnvironmentVariable("ARM_FORCE"), NumberStyles.Float, ci, out float f) ? f : 600f;
        float fast = float.TryParse(Environment.GetEnvironmentVariable("ARM_HZ"), NumberStyles.Float, ci, out float hz) ? hz : 40f;
        var setups = new List<Setup>
        {
            new Setup { Name = "kinematic (today), 60 Hz", Arm = AuthorityLevel.Kinematic },
            new Setup { Name = "pin servo 0.7, 60 Hz", Arm = AuthorityLevel.Pinned, Strength = 0.7f },
            new Setup { Name = "muscles, torso on its pins, 240 Hz", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = 60f },
            new Setup { Name = "muscles, torso carried, 60 Hz", Arm = AuthorityLevel.Muscled, Torso = AuthorityLevel.Kinematic, MostLoad = 200f },
            new Setup { Name = "muscles, torso carried, 240 Hz", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = 60f, Torso = AuthorityLevel.Kinematic, MostLoad = 200f },
            new Setup { Name = "muscles by inertia, torso carried, 60 Hz", Arm = AuthorityLevel.Muscled, MostHz = 20f, Torso = AuthorityLevel.Kinematic, ByInertia = true },
            new Setup { Name = "muscles by inertia, torso carried, 240 Hz", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = fast, Torso = AuthorityLevel.Kinematic, ByInertia = true },
            new Setup { Name = "muscles by inertia, torso carried, 240 Hz, 40 solver passes", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = fast, Torso = AuthorityLevel.Kinematic, ByInertia = true, Iters = 40 },
            new Setup { Name = "muscles by inertia, torso carried, 480 Hz", Arm = AuthorityLevel.Muscled, Substeps = 8, MostHz = 2f * fast, Torso = AuthorityLevel.Kinematic, ByInertia = true },
            new Setup { Name = "muscles by inertia, torso on muscles too, 240 Hz", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = fast, Torso = AuthorityLevel.Muscled, ByInertia = true },
            new Setup { Name = $"muscles by inertia of limited strength ({force:0}), torso carried, 240 Hz", Arm = AuthorityLevel.Muscled, Substeps = 4, MostHz = fast, MostForce = force, Torso = AuthorityLevel.Kinematic, ByInertia = true },
        };
        string only = Environment.GetEnvironmentVariable("ARM_ONLY");
        var clips = ClipFighter.StrikesOf("Medium").Where(c => c.ContactFrame >= 0 && (only == null || c.Name.Contains(only))).ToList();
        int most = int.TryParse(Environment.GetEnvironmentVariable("ARM_CLIPS"), out int n) ? n : 8;
        clips = clips.Take(most).ToList();
        Console.WriteLine($"One physical sword arm: {clips.Count} strikes of the Medium's ({string.Join(", ", clips.Select(c => c.Name))})");
        Console.WriteLine("  tip: how far the physical blade's point is from the posed one (cm), swung at nothing; post: how deep the blade went into a fixed post (cm);");
        Console.WriteLine("  weight: the speed a free 20 kg ball is left with (m/s).");
        string which = Environment.GetEnvironmentVariable("ARM_SETUP");
        foreach (var setup in setups)
        {
            if (which != null && !setup.Name.Contains(which)) continue;
            var free = new List<Result>();
            var post = new List<Result>();
            var weight = new List<Result>();
            foreach (var clip in clips)
            {
                free.Add(Strike(setup, clip, 0));
                post.Add(Strike(setup, clip, 1));
                weight.Add(Strike(setup, clip, 2));
            }

            Console.WriteLine($"{setup.Name}:");
            Console.WriteLine(string.Format(ci, "  at nothing: tip off its pose through the blow at most {0:0.0} cm (median of strikes {1:0.0}), at contact {2:0.0} cm; the posed tip's top speed {3:0.0} m/s; back on its pose after: {4:0.0} cm off",
                free.Max(r => r.TipOff) * 100f, Median(free.Select(r => r.TipOff)) * 100f, Median(free.Select(r => r.TipOffAtContact)) * 100f, free.Max(r => r.PeakSpeed), free.Max(r => r.OffAtEnd) * 100f));
            Console.WriteLine(string.Format(ci, "  at a post: deepest {0:0.0} cm (median {1:0.0}); in it {2:0.00} s a strike; the hand's top speed after meeting it {3:0.0} m/s; blow-ups {4}",
                post.Max(r => r.Deepest) * 100f, Median(post.Select(r => r.Deepest)) * 100f, post.Average(r => r.InPostSeconds), post.Max(r => r.FastestAfter), post.Count(r => r.Blew) + free.Count(r => r.Blew)));
            Console.WriteLine(string.Format(ci, "  at a weight: left at {0:0.0} m/s (median; least {1:0.0}, most {2:0.0}); deepest in it {3:0.0} cm",
                Median(weight.Select(r => r.WeightSpeed)), weight.Min(r => r.WeightSpeed), weight.Max(r => r.WeightSpeed), weight.Max(r => r.Deepest) * 100f));
        }

        return 0;
    }

    private static float Median(IEnumerable<float> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted.Count == 0 ? 0f : sorted[sorted.Count / 2];
    }

    /// <summary>One strike: at nothing (0), at a fixed post where it lands (1), at a free 20 kg ball there (2).</summary>
    private static Result Strike(Setup setup, LocoClip clip, int scene)
    {
        const float dt = 1f / 60f;
        // (A fresh world each: nothing left over from the last strike.)
        foreach (var body in Physics.Bodies.ToArray()) UnityEngine.Object.DestroyImmediate(body.gameObject);
        foreach (var collider in Physics.Colliders.ToArray()) UnityEngine.Object.DestroyImmediate(collider.gameObject);
        var world = PhysXWorld.Install(BodyPart.GroundLayer, setup.Substeps);
        AuthorityController.MostMuscleFrequency = setup.MostHz;
        AuthorityController.MuscleMostForce = setup.MostForce;
        AuthorityController.MaxMuscleLoad = setup.MostLoad;
        AuthorityController.MusclesByInertia = setup.ByInertia;

        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = "Medium";
        if (setup.Iters > 0)
        {
            profile.solverIterations = setup.Iters;
            profile.solverVelocityIterations = Mathf.Max(4, setup.Iters / 4);
        }

        var root = UnitBuilder.Build(rig, profile, "Striker", _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var fighter = new ClipFighter(root, unitRig, profile, Vector3.zero, 0f, 0f, LocoLibrary.CombatPrefix("Medium"), physical: true) { Defends = false };
        var poser = fighter.Poser;
        poser.BodyKey = "Medium";
        fighter.Anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        unitRig.SlotWeaponR.localRotation = UnitController.MainGrip;
        poser.BladeR = UnitController.MainGrip * Vector3.forward;
        poser.CarryR = LocoBody.Carry.Weapon;
        float length = poser.WeaponLength;
        fighter.EnableStrikes(null);
        fighter.StrikeArm = setup.Arm;
        fighter.StrikeArmStrength = setup.Strength;
        fighter.TorsoHold = setup.Torso;

        // Its sword: a solid blade on the physical hand, its weight on the hand.
        var blade = new GameObject("Blade");
        blade.transform.SetParent(unitRig.SlotWeaponR, false);
        var capsule = blade.AddComponent<CapsuleCollider>();
        capsule.direction = 2;
        capsule.radius = 0.012f;
        capsule.height = length - 0.12f;
        capsule.center = new Vector3(0f, 0f, 0.5f * (length + 0.12f));
        foreach (var part in unitRig.Parts)
        {
            if (part.partCollider != null) Physics.IgnoreCollision(capsule, part.partCollider, true);
        }

        unitRig.Part(PartId.HandR).body.mass += 1.2f;
        unitRig.Part(PartId.HandR).body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        fighter.Loco.Place(Vector3.zero, 0f);
        void Step()
        {
            fighter.Move(dt, Vector3.zero, 0f, new Vector3(0f, 1.3f, 2f), false);
            fighter.Apply(dt);
            Physics.Simulate(dt);
        }

        for (int i = 0; i < 60; i++) Step();

        // Where the strike lands: its own sweet spot at contact.
        Vector3 target = new Vector3(0f, 1.2f, 1.1f);
        if (StrikeReach.TryGet("Medium", clip.Name, out var grip, out var way))
        {
            target = grip + way * (length * poser.SweetSpot);
        }

        Collider thing = null;
        Rigidbody ball = null;
        if (scene == 1)
        {
            var post = new GameObject("Post");
            post.transform.position = new Vector3(target.x, 1f, target.z);
            var box = post.AddComponent<BoxCollider>();
            box.size = new Vector3(0.1f, 2f, 0.1f);
            thing = box;
        }
        else if (scene == 2)
        {
            var weight = new GameObject("Weight");
            weight.transform.position = target;
            var sphere = weight.AddComponent<SphereCollider>();
            sphere.radius = 0.12f;
            ball = weight.AddComponent<Rigidbody>();
            ball.mass = 20f;
            ball.useGravity = false;
            thing = sphere;
        }

        var result = new Result();
        poser.PlayStrike(clip, target);
        Vector3 lastPosed = Vector3.zero, lastHand = fighter.Anim.Part(PartId.HandR).position;
        bool met = false, known = false;
        float contactAt = clip.ContactFrame / 30f;
        int after = 0;
        for (int i = 0; i < 400 && after < 30; i++)
        {
            Step();
            bool playing = fighter.Loco.Current == Locomotor.Mode.OneShot && fighter.Loco.Shot == clip;
            if (!playing) after++;
            Vector3 posedTip = fighter.Anim.SlotWeaponR.position + fighter.Anim.SlotWeaponR.forward * length;
            Vector3 realTip = unitRig.SlotWeaponR.position + unitRig.SlotWeaponR.forward * length;
            float off = Vector3.Distance(posedTip, realTip);
            if (float.IsNaN(off) || off > 3f)
            {
                result.Blew = true;
                break;
            }

            // (The blow itself: from a quarter of a second before its contact to a tenth after.)
            if (playing && fighter.Loco.ShotTime > contactAt - 0.25f && fighter.Loco.ShotTime < contactAt + 0.1f)
            {
                result.TipOff = Mathf.Max(result.TipOff, off);
                if (known) result.PeakSpeed = Mathf.Max(result.PeakSpeed, (posedTip - lastPosed).magnitude / dt);
                if (Mathf.Abs(fighter.Loco.ShotTime - contactAt) < 0.6f * dt) result.TipOffAtContact = off;
            }

            if (Trace && scene == 0 && playing)
            {
                Vector3 posedHand = fighter.Anim.Part(PartId.HandR).position, realHand = unitRig.Part(PartId.HandR).body.position;
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    t {0:0.000} (contact {1:0.000}): posed tip {2,5:0.0} m/s, posed hand {3,5:0.0} m/s; tip off {4,6:0.0} cm, hand off {5,5:0.0} cm, blade's way off {6,5:0.0} deg",
                    fighter.Loco.ShotTime, contactAt, known ? (posedTip - lastPosed).magnitude / dt : 0f, known ? (posedHand - lastHand).magnitude / dt : 0f, off * 100f,
                    Vector3.Distance(posedHand, realHand) * 100f, Vector3.Angle(fighter.Anim.SlotWeaponR.forward, unitRig.SlotWeaponR.forward)));
                lastHand = posedHand;
                float Off(PartId id)
                {
                    var parent = PartInfo.Parent(id);
                    var posed = Quaternion.Inverse(fighter.Anim.Part(parent).rotation) * fighter.Anim.Part(id).rotation;
                    var real = Quaternion.Inverse(unitRig.Part(parent).body.rotation) * unitRig.Part(id).body.rotation;
                    return Quaternion.Angle(posed, real);
                }

                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "        joints off: shoulder {0:0.0}, elbow {1:0.0}, wrist {2:0.0} deg; chest off {3:0.0} deg", Off(PartId.UpperArmR), Off(PartId.ForearmR), Off(PartId.HandR),
                    Quaternion.Angle(fighter.Anim.Part(PartId.Chest).rotation, unitRig.Part(PartId.Chest).body.rotation)));
            }

            lastPosed = posedTip;
            known = true;
            if (thing != null)
            {
                float deep = world.Overlap(capsule, thing);
                result.Deepest = Mathf.Max(result.Deepest, deep);
                if (deep > 0.005f) result.InPostSeconds += dt;
                bool touching = deep > 0f || world.Contacts.Any(c => (c.A == capsule && c.B == thing) || (c.B == capsule && c.A == thing));
                met |= touching;
                if (met) result.FastestAfter = Mathf.Max(result.FastestAfter, unitRig.Part(PartId.HandR).body.linearVelocity.magnitude);
            }

            result.OffAtEnd = off;
        }

        if (ball != null) result.WeightSpeed = ball.linearVelocity.magnitude;
        fighter.Destroy();
        return result;
    }
}
#endif
