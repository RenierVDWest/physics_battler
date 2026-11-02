// `armfilm`: a blow played (a body with its weapon at a medium's part) and every step's pose written
// out as JSON (the animation rig's parts, the weapon's grip and tip, the target's parts, the aim and
// the key moments), for Tools/CompileCheck/armfilm.py to draw as stick figures.
//   FILM_MOVE=SwordCut (MoveStyle; all swings and the thrust when unset), FILM_BODY=Medium,
//   FILM_PART=Chest, FILM_OUT=<directory> (default: the current directory)
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ArmFilmReport
{
    public static int Run()
    {
        string only = Environment.GetEnvironmentVariable("FILM_MOVE");
        PhysicsBattler.Motion.PosePipeline.DebugNoCorrection = Environment.GetEnvironmentVariable("FILM_NOCORR") != null;
        string body = Environment.GetEnvironmentVariable("FILM_BODY") ?? "Medium";
        string outDir = Environment.GetEnvironmentVariable("FILM_OUT") ?? ".";
        Enum.TryParse(Environment.GetEnvironmentVariable("FILM_PART") ?? "Chest", out AimPart wanted);
        foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
        {
            if (only != null && style.ToString() != only)
            {
                continue;
            }

            var move = MoveDefinition.Create(style);
            if (move == null || move.shape == MoveShape.Path)
            {
                continue;
            }

            var weapon = style.ToString().StartsWith("Sword") ? SwingReport.Sword() : SwingReport.Club();
            if (Environment.GetEnvironmentVariable("KEY_SEED") != null)
            {
                KeyFitReport.Seed(move, weapon, body, wanted);
            }

            // (Keys from a keyfit run's printout: FILM_KEYS=<file>.)
            string keysFile = Environment.GetEnvironmentVariable("FILM_KEYS");
            if (keysFile != null && File.Exists(keysFile))
            {
                string text = File.ReadAllText(keysFile);
                Vector3 V3(string v)
                {
                    var n = System.Text.RegularExpressions.Regex.Matches(v, @"-?[\d.]+");
                    return new Vector3(float.Parse(n[0].Value, CultureInfo.InvariantCulture), float.Parse(n[1].Value, CultureInfo.InvariantCulture), float.Parse(n[2].Value, CultureInfo.InvariantCulture));
                }

                var h = System.Text.RegularExpressions.Regex.Match(text, $@"  {style}: hand windUp = new Vector3\(([^)]*)\); follow = new Vector3\(([^)]*)\)");
                var w = System.Text.RegularExpressions.Regex.Match(text, $@"  {style}: wrist windUp = new Vector3\(([^)]*)\); contact = new Vector3\(([^)]*)\); follow = new Vector3\(([^)]*)\)");
                var e = System.Text.RegularExpressions.Regex.Match(text, $@"  {style}: elbow windUp = ([-\d.]+)f; contact = ([-\d.]+)f; follow = ([-\d.]+)f");
                if (h.Success && w.Success && e.Success)
                {
                    move.handWindUp = V3(h.Groups[1].Value);
                    move.handFollow = V3(h.Groups[2].Value);
                    move.wristWindUp = V3(w.Groups[1].Value);
                    move.wristContact = V3(w.Groups[2].Value);
                    move.wristFollow = V3(w.Groups[3].Value);
                    move.elbowWindUp = float.Parse(e.Groups[1].Value, CultureInfo.InvariantCulture);
                    move.elbowContact = float.Parse(e.Groups[2].Value, CultureInfo.InvariantCulture);
                    move.elbowFollow = float.Parse(e.Groups[3].Value, CultureInfo.InvariantCulture);
                }
            }

            weapon.moves = new[] { move };
            string who = body;
            var a = AimMatrix.Build(who, Vector3.zero, 0f, 0);
            if (!move.UsableBy(a.Body.SizeFactor))
            {
                UnityEngine.Object.DestroyImmediate(a.gameObject);
                who = "Small";
                a = AimMatrix.Build(who, Vector3.zero, 0f, 0);
            }

            var target = AimMatrix.Build("Medium", Vector3.forward * 5f, 180f, 1);
            try
            {
                float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                target.Place(Vector3.forward * distance, 180f);
                a.DealsHits = false;
                a.Equip(weapon, null);
                for (int i = 0; i < 20; i++)
                {
                    a.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                }

                MoveState state = null;
                foreach (var part in new[] { wanted, AimPart.Chest, AimPart.Belly, AimPart.Head, AimPart.Neck, AimPart.KneeL, AimPart.ElbowR })
                {
                    if (state == null && move.targets.Contains(part))
                    {
                        state = MovePlanner.Prepare(a, target, move, part);
                        if (state != null)
                        {
                            wanted = part;
                        }
                    }
                }

                if (state == null || !a.StartMove(state))
                {
                    Console.WriteLine($"{style}: not planned");
                    continue;
                }

                var json = new StringBuilder();
                string V(Vector3 v) => string.Format(CultureInfo.InvariantCulture, "[{0:0.###},{1:0.###},{2:0.###}]", v.x, v.y, v.z);
                string Pose(UnitController u)
                {
                    var sb = new StringBuilder("{");
                    foreach (PartId id in Enum.GetValues(typeof(PartId)))
                    {
                        sb.Append($"\"{id}\":{V(u.Anim.Part(id).position)},");
                    }

                    sb.Append($"\"HeadTop\":{V(u.Anim.Part(PartId.Head).position + u.Anim.Part(PartId.Head).rotation * Vector3.up * 0.22f * u.Body.SizeFactor)}");
                    return sb.Append('}').ToString();
                }

                json.Append($"{{\"move\":\"{style}\",\"body\":\"{who}\",\"part\":\"{wanted}\",\"aim\":{V(state.Aim)},");
                json.Append(string.Format(CultureInfo.InvariantCulture, "\"windUp\":{0:0.###},\"contact\":{1:0.###},\"follow\":{2:0.###},", move.WindUpEnd, state.ContactTime, move.SwingFollowEnd));
                json.Append($"\"target\":{Pose(target)},\"frames\":[");
                bool first = true;
                for (int i = 0; i < 600 && a.IsAttacking; i++)
                {
                    a.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                    var slot = a.Anim.SlotWeaponR;
                    Vector3 tip = slot.position + slot.rotation * (Vector3.forward * weapon.length * a.Loadout.Scale);
                    var arm = a.Pose.LastWeaponArm;
                    json.Append(first ? "" : ",");
                    json.Append(string.Format(CultureInfo.InvariantCulture, "{{\"t\":{0:0.####},\"pose\":{1},\"grip\":{2},\"tip\":{3},\"strike\":{4},\"wrist\":[{5:0},{6:0},{7:0}],\"swivel\":{8:0},\"keyed\":{9},\"cleared\":{10}}}",
                        state.T, Pose(a), V(slot.position), V(tip), V(a.Pose.StrikePoint), arm.twist, arm.flexion, arm.deviation, arm.swivel, V(a.Pose.HandKeyed), V(a.Pose.HandCleared)));
                    first = false;
                }

                json.Append(string.Format(CultureInfo.InvariantCulture, "],\"arc\":{0:0},\"contactSpeed\":{1:0.0}}}", state.Arc, state.ContactSpeed));
                File.WriteAllText(Path.Combine(outDir, $"film_{style}.json"), json.ToString());
                Console.WriteLine($"{style}: {wanted}, arc {state.Arc:0}, contact {state.ContactSpeed:0.0} m/s");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a.gameObject);
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }

        return 0;
    }
}
