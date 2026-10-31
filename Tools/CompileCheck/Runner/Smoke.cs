// A dry run of the playtest recorder outside Unity: two orcs built from orc_rig.json, the controllers
// stepped by hand, and the bodies faked as a lagging puppet of their ghosts (there is no PhysX here).
// It exercises the recorder end to end so Tools/Playtest/analyze.py can be tested on real output.
//
//   dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll smoke <output folder>
using System;
using System.IO;
using System.Reflection;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class Smoke
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static int Run(string output)
    {
        string assets = UnityEngine.Application.dataPath;
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(assets, "_Battler/Data/Rigs/orc_rig.json")));
        Directory.CreateDirectory(output);
        UnityEngine.Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");

        var recorderObject = new GameObject("Recorder");
        var recorder = recorderObject.AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");

        var profile = UnitProfile.CreateDefault();
        profile.name = "UP_Orc";
        var a = Spawn(rig, profile, "Orc A (club)", new Vector3(-1f, 0f, 0f), Vector3.right, 0);
        var b = Spawn(rig, profile, "Orc B", new Vector3(1f, 0f, 0f), Vector3.left, 1);
        a.Equip(MakeClub(rig));

        var attack = AttackDefinition.CreateDefault();
        attack.name = "AD_ClubLowSwing";
        float dt = Time.fixedDeltaTime;
        for (int step = 0; step < 60 * 12; step++)
        {
            Time.time += dt;
            Call(recorder, "FixedUpdate");
            foreach (var unit in new[] { a, b })
            {
                Call(unit, "FixedUpdate");
                Puppet(unit, dt);
            }

            if (step == 20)
            {
                a.TryAttack(b, attack, out _);
            }

            if (step == 100)
            {
                var chest = b.Rig.Part(PartId.Chest);
                ImpactSystem.ResolveWeaponContact(a, a.Weapon, chest, new ContactPoint { point = chest.body.position, normal = Vector3.left });
                recorder.Mark("smoke test mark");
            }

            if (step == 140)
            {
                b.State.LoseStability(500f);   // straight to the floor
                Application.RaiseLog("A warning from the smoke test", LogType.Warning);
            }

            if (step == 300)
            {
                Time.timeScale = 0.5f;
                Call(recorder, "Update");
            }
        }

        Call(recorder, "OnDisable");
        Console.WriteLine("smoke session: " + recorder.SessionPath);
        return 0;
    }

    private static UnitController Spawn(RigFile rig, UnitProfile profile, string name, Vector3 position, Vector3 facing, int team)
    {
        var root = UnitBuilder.Build(rig, profile, name, _ => null, 8);
        root.name = name;
        root.transform.position = position;
        root.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
        var unit = root.GetComponent<UnitController>();
        unit.Team = team;
        unit.Profile = profile;
        Call(unit, "Awake");
        return unit;
    }

    private static Weapon MakeClub(RigFile rig)
    {
        var root = new GameObject("P_Weapon_ClubBig(Clone)");
        var weapon = root.AddComponent<Weapon>();
        weapon.definition = WeaponDefinition.CreateDefault();
        weapon.definition.name = "WD_ClubBig";
        weapon.definition.twoHanded = false;
        var collider = new GameObject("Collider_0");
        collider.transform.SetParent(root.transform, false);
        weapon.colliders = new Collider[] { collider.AddComponent<CapsuleCollider>() };
        var tip = new GameObject("Tip").transform;
        tip.SetParent(root.transform, false);
        tip.localPosition = new Vector3(0f, 0f, 0.95f);
        weapon.tip = tip;
        var sweet = new GameObject("SweetSpot").transform;
        sweet.SetParent(root.transform, false);
        sweet.localPosition = new Vector3(0f, 0f, 0.75f);
        weapon.sweetSpot = sweet;
        return weapon;
    }

    /// <summary>Stands in for PhysX: each part turns a share of the way to its ghost every step.</summary>
    private static void Puppet(UnitController unit, float dt)
    {
        var before = new Vector3[PartInfo.Count];
        foreach (var id in PartInfo.All)
        {
            before[(int)id] = unit.Rig.Part(id).transform.position;
        }

        foreach (var id in PartInfo.All)
        {
            var part = unit.Rig.Part(id);
            var ghost = unit.Ghost.Part(id);
            if (id == PartId.Pelvis)
            {
                part.transform.position = Vector3.Lerp(part.transform.position, ghost.position, 0.2f);
            }

            float follow = PartInfo.Group(id) == MuscleGroup.ArmR ? 0.12f : 0.3f;
            part.transform.rotation = Quaternion.Slerp(part.transform.rotation, ghost.rotation, follow);
        }

        foreach (var id in PartInfo.All)
        {
            var part = unit.Rig.Part(id);
            part.body.linearVelocity = (part.transform.position - before[(int)id]) / dt;
        }
    }

    private static void Call(object target, string method)
    {
        target.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}
