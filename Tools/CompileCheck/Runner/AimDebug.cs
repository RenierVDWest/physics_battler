using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class AimDebug
{
    public static int Run(string a, string b, string moveName, string partName, float distance)
    {
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        var style = (MoveStyle)Enum.Parse(typeof(MoveStyle), moveName);
        var move = MoveDefinition.Create(style);
        var weapon = moveName.StartsWith("Sword") ? sword : club;
        var part = (AimPart)Enum.Parse(typeof(AimPart), partName);
        var attacker = AimMatrix.Build(a, Vector3.zero, 0f, 0);
        var target = AimMatrix.Build(b, new Vector3(0f, 0f, distance), 180f, 1);
        attacker.Equip(weapon, null);
        for (int i = 0; i < 20; i++) { attacker.PreviewPose(1f / 60f); target.PreviewPose(1f / 60f); }
        var state = MovePlanner.Prepare(attacker, target, move, part);
        if (state == null) { Console.WriteLine("out of lunge range"); return 0; }
        Console.WriteLine($"aim {state.Aim} lunge {state.Lunge.magnitude:0.00} dur {state.Duration:0.00} tc {state.ContactTime:0.00}");
        float err = attacker.TryOut(state, true);
        var anim = attacker.Anim;
        float arm = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;
        Console.WriteLine($"tryout err {err * 100:0.0} cm; spine bend {attacker.Pose.SpineBend:0} crouch {attacker.Pose.MoveCrouch:0.00} pelvis drop {attacker.Pose.PelvisDrop:0.00}");
        Console.WriteLine($"feet L {attacker.Stance.Position(0)} R {attacker.Stance.Position(1)}");
        Console.WriteLine($"shoulder {anim.Part(PartId.UpperArmR).position} hand {anim.Part(PartId.HandR).position} dist {Vector3.Distance(anim.Part(PartId.UpperArmR).position, anim.Part(PartId.HandR).position):0.000} arm {arm:0.000}");
        Console.WriteLine($"chest {anim.Part(PartId.Chest).position} strike {attacker.Pose.StrikePoint} pelvis {anim.Part(PartId.Pelvis).position}");
        return 0;
    }
}
