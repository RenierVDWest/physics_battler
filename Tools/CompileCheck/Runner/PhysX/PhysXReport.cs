// Checks of the real PhysX engine under the stubs (the plan "A different approach", step 1): `physx <dir>`.
#if PHYSX
using System;
using System.IO;
using PhysicsBattler.Engine;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class PhysXReport
{
    /// <summary>What the game's joint probe reports in Unity (every playtest log of 6 November 2026 opens with it).</summary>
    private const string UnityProbe = "drive standard (asked +35, got 35.0), X limit sign -1 (pushed +, stopped at 5.0), velocity signs -1 -1 -1 (turned -25 -25 -25°)";

    public static int Run(string[] args)
    {
        int failed = 0;
        void Check(string what, bool ok, string detail)
        {
            Console.WriteLine($"  {(ok ? "pass" : "FAIL")}  {what}: {detail}");
            if (!ok) failed++;
        }

        var world = new PhysXWorld();
        Physics.Backend = world;
        Console.WriteLine("PhysX under the stubs (MagicPhysX, PhysX 5; TGS solver, enhanced determinism)");

        // 1. The game's own joint probe, run on this engine: it must say what it says in Unity.
        JointConventions.EnsureProbed();
        // (Its signs and angles: the speed probe's turn within a degree, as it is printed rounded.)
        string Signs(string report) => report.Substring(0, report.IndexOf("(turned", StringComparison.Ordinal));
        bool turned = System.Text.RegularExpressions.Regex.IsMatch(JointConventions.Report, @"\(turned -2[456] -2[456] -2[456]°\)$");
        Check("the joint probe reads as in Unity", Signs(JointConventions.Report) == Signs(UnityProbe) && turned, $"here: {JointConventions.Report}");

        // 2. A box dropped on the ground comes to rest on it.
        var box = new GameObject("Box");
        box.transform.position = new Vector3(3f, 1f, 0f);
        box.AddComponent<BoxCollider>().size = Vector3.one * 0.2f;
        var boxBody = box.AddComponent<Rigidbody>();
        boxBody.mass = 5f;
        for (int i = 0; i < 120; i++) Physics.Simulate(1f / 60f);
        Check("a dropped box rests on the ground", Mathf.Abs(boxBody.position.y - 0.1f) < 0.005f && boxBody.linearVelocity.magnitude < 0.05f,
            $"centre at {boxBody.position.y:0.000} m (0.100), speed {boxBody.linearVelocity.magnitude:0.000} m/s");

        // 3. A kinematic pusher moved by MovePosition shoves it along; the box is not passed through.
        var pusher = new GameObject("Pusher");
        pusher.transform.position = new Vector3(2.5f, 0.1f, 0f);
        pusher.AddComponent<BoxCollider>().size = Vector3.one * 0.2f;
        var pushBody = pusher.AddComponent<Rigidbody>();
        pushBody.isKinematic = true;
        float deepest = 0f;
        for (int i = 0; i < 60; i++)
        {
            pushBody.MovePosition(pushBody.position + Vector3.right * (1f / 60f));   // 1 m/s
            Physics.Simulate(1f / 60f);
            deepest = Mathf.Max(deepest, world.Overlap(pusher.GetComponent<Collider>(), box.GetComponent<Collider>()));
        }

        Check("a kinematic body pushes a box ahead of it", boxBody.position.x > 3.6f && deepest < 0.02f,
            $"the box went from 3.00 to {boxBody.position.x:0.00} m, never more than {deepest * 100f:0.0} cm inside the pusher");
        UnityEngine.Object.DestroyImmediate(box);
        UnityEngine.Object.DestroyImmediate(pusher);

        // 4. A Medium's physical body, let go standing: it falls as one body, stays whole and comes to rest on the ground.
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = "Medium";
        var root = UnitBuilder.Build(rig, profile, "Ragdoll", _ => null, 0);
        var unit = root.GetComponent<UnitRig>();
        float apart = 0f, lowest = 1f, fastest = 0f, kinetic = 0f;
        foreach (var part in unit.Parts)
        {
            part.body.isKinematic = false;
            if (part.joint != null)
            {
                JointMath.ApplyLimits(part.joint, JointLimitTable.ForRagdoll(part.id));
            }
        }

        // (A shove so it topples.)
        unit.Part(PartId.Chest).body.AddForce(new Vector3(0.6f, 0f, 1f) * 60f, ForceMode.Impulse);
        for (int i = 0; i < 300; i++)
        {
            Physics.Simulate(1f / 60f);
            kinetic = 0f;
            foreach (var part in unit.Parts)
            {
                var body = part.body;
                fastest = Mathf.Max(fastest, body.linearVelocity.magnitude);
                kinetic += 0.5f * body.mass * body.linearVelocity.sqrMagnitude;
                if (float.IsNaN(body.position.x)) fastest = float.PositiveInfinity;
                if (part.partCollider != null)
                {
                    lowest = Mathf.Min(lowest, part.partCollider.ClosestPoint(new Vector3(body.position.x, -10f, body.position.z)).y);
                }

                if (PartInfo.HasParent(part.id))
                {
                    var parent = unit.Part(PartInfo.Parent(part.id)).body;
                    apart = Mathf.Max(apart, Vector3.Distance(body.position, parent.position + parent.rotation * part.restLocalPosition));
                }
            }
        }

        float pelvis = unit.Part(PartId.Pelvis).body.position.y;
        // (Unity's own ragdolls: the dead of the playtests of 6 November came 2 to 12 cm apart at a joint at their worst.)
        Check("a ragdoll's joints hold", apart < 0.05f, $"a joint came at most {apart * 100f:0.0} cm apart");
        Check("a ragdoll stays on the ground", lowest > -0.02f && pelvis < 0.35f, $"its lowest point reached {lowest * 100f:0.0} cm; the pelvis ends {pelvis:0.00} m up");
        Check("a ragdoll falls and settles", fastest < 12f && kinetic < 2f, $"fastest part {fastest:0.0} m/s; {kinetic:0.00} J of motion left after 5 s");

        Console.WriteLine(failed == 0 ? "All passed." : $"{failed} failed.");
        return failed == 0 ? 0 : 1;
    }
}
#endif
