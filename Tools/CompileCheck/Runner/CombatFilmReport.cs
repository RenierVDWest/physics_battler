// `combatfilm <dir> [body] [clips]`: films the combat clips (Tools/Mocap/Combat/combat_bake.py) on one of our
// bodies, each played as it is (its root moving the body, its feet locked where they land), holding the
// body's weapons the way the clip's hands hold them. One film per clip, <dir>/<clip>.json, for
// Tools/Mocap/Locomotion/locofilm.py; prints how each went (planted feet, the weapon's fastest moment).
// clips: a comma list, or a prefix ("sns_", "lg_"), or all of that body's (the default).
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class CombatFilmReport
{
    public static int Run(string dir, string body, string which)
    {
        Directory.CreateDirectory(dir);
        body ??= "Medium";
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        LocoLibrary.Load();
        string prefix = body == "Large" ? "lg_" : "sns_";
        var names = LocoLibrary.Clips.Keys.Where(n => which == null ? n.StartsWith(prefix) : which.Contains(",") ? which.Split(',').Contains(n) : n.StartsWith(which)).OrderBy(n => n).ToList();
        foreach (string name in names)
        {
            var clip = LocoLibrary.Get(name);
            var root = UnitBuilder.Build(rig, profile, "Film " + body, _ => null, 0);
            var unitRig = root.GetComponent<UnitRig>();
            var anim = new AnimRig(root.transform, unitRig);
            float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
            var loco = new Locomotor(leg);
            var poser = new LocoBody(anim, loco);
            poser.SoleFrom(unitRig);
            // (The weapons as the game grips them; the clip's own hands hold them.)
            anim.SlotWeaponR.localRotation = UnitController.MainGrip;
            if (body == "Medium")
            {
                anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
            }

            const float dt = 1f / 60f;
            var film = new LocoFilm(dt, 2) { BladeLength = 0.9f * leg / 0.845f, ShieldRadius = body == "Medium" ? 0.3f * leg / 0.845f : 0f };
            var measures = new LocoMeasures(anim, loco, poser);
            loco.Place(Vector3.zero, 0f);
            float t = 0f;
            for (int i = 0; i < 30; i++, t += dt)
            {
                loco.Step(dt, Vector3.zero, 0f);
                poser.Pose(dt);
            }

            loco.Play(clip);
            float fastest = 0f, fastestAt = 0f;
            Vector3 lastTip = Vector3.zero;
            bool hasTip = false;
            for (int i = 0; i < 2000 && (loco.Current == Locomotor.Mode.OneShot || i < 10); i++, t += dt)
            {
                loco.Step(dt, Vector3.zero, loco.Facing);
                poser.Pose(dt);
                measures.Sample(dt, t, name);
                film.Add(t, name, loco, poser, anim);
                var slot = anim.SlotWeaponR;
                Vector3 tip = slot.position + slot.rotation * Vector3.forward * film.BladeLength;
                if (hasTip && (tip - lastTip).magnitude / dt > fastest)
                {
                    fastest = (tip - lastTip).magnitude / dt;
                    fastestAt = loco.ShotTime;
                }

                lastTip = tip;
                hasTip = true;
            }

            for (int i = 0; i < 30; i++, t += dt)
            {
                loco.Step(dt, Vector3.zero, loco.Facing);
                poser.Pose(dt);
                film.Add(t, name, loco, poser, anim);
            }

            File.WriteAllText(Path.Combine(dir, name + ".json"), film.ToString());
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0,-20} {1,5:0.00} s ({2}): planted feet moved at most {3:0.0} cm in {4} plants; the weapon's tip fastest {5:0.0} m/s at {6:0.00} s (contact mark {7:0.00} s); {8} forced steps",
                name, clip.Duration, clip.Role, measures.PlantDriftWorst * 100f, measures.PlantsDone, fastest, fastestAt,
                clip.ContactFrame >= 0 ? clip.ContactFrame / clip.Rate : -1f, measures.ForcedReleases));
            UnityEngine.Object.DestroyImmediate(root);
        }

        return 0;
    }
}
