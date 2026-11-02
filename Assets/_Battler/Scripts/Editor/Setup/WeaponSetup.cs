using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// The weapons: a move asset per ready-made move (Data/Moves/MD_*), a weapon asset per weapon
    /// (Data/Weapons/WD_*) and a prefab per weapon (Prefabs/Weapons/P_Weapon_*).
    ///
    /// Move and weapon assets are created once and then yours to tune; only a weapon's shape (its
    /// length, mass and strike points, which come from the art) is refreshed from weapons.json on
    /// every run. Prefabs are rebuilt every run: the mesh, and colliders from weapons.json on the
    /// prefab's root, where the Weapon component hears what they touch.
    /// </summary>
    internal static class WeaponSetup
    {
        public static void Apply()
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(BattlerPaths.WeaponsFile);
            if (text == null)
            {
                Debug.LogWarning($"[Battler Setup] {BattlerPaths.WeaponsFile} is missing: no weapons. Rebuild it with SourceArt/Weapons/Scripts/build_weapons.py.");
                return;
            }

            var file = WeaponFile.Parse(text.text);
            var moves = new MoveDefinition[Enum.GetValues(typeof(MoveStyle)).Length];
            // Setup 11 reworked the ready-made moves (clear of the wielder's own body, follow-throughs
            // out to the side, the thrust in line with the shoulder): moves made by setup 10, not yet
            // tuned, are refreshed once.
            bool refresh = SetupVersion.Installed == 10;   // (not again from 11 on)
            // Setup 14 rebuilt the ready-made moves as big swings (the Swing and Thrust shapes) and
            // added the sword cleave and the club's rising blow: moves still on the old hand-drawn
            // path are refreshed once, and the new moves go on the weapons.
            bool swings = SetupVersion.Installed >= 10 && SetupVersion.Installed < 14;
            // Setup 13 (milestone 3) gives weapons damage and a hit-detection radius: weapons made
            // earlier have the sword's figures, so each gets its kind's defaults once.
            bool hits = SetupVersion.Installed >= 10 && SetupVersion.Installed < 13;
            // Setup 21 (milestone 10): moves say which bodies throw them; the fan strikes, made by
            // setup 20 for every body, are the small body's.
            bool bodies = SetupVersion.Installed >= 10 && SetupVersion.Installed < 21;
            // Setup 22 (the arm rebuild): each move's elbow nulls and wrist keys, each weapon's grip roll.
            bool elbows = SetupVersion.Installed >= 10 && SetupVersion.Installed < 22;
            foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
            {
                moves[(int)style] = EnsureMove(style, refresh);
                var m = moves[(int)style];
                if (elbows)
                {
                    MoveDefinition.DefaultElbows(style, out m.elbowWindUp, out m.elbowContact, out m.elbowFollow);
                    MoveDefinition.DefaultWrists(style, out m.wristWindUp, out m.wristContact, out m.wristFollow);
                    MoveDefinition.DefaultHands(style, out m.handWindUp, out m.handFollow);
                    EditorUtility.SetDirty(m);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Move(style.ToString())}: elbow nulls, wrist and hand keys.");
                }

                if (bodies && moves[(int)style].bodies != MoveDefinition.DefaultBodies(style))
                {
                    moves[(int)style].bodies = MoveDefinition.DefaultBodies(style);
                    EditorUtility.SetDirty(moves[(int)style]);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Move(style.ToString())}: thrown by {moves[(int)style].bodies}.");
                }

                if (swings && moves[(int)style].shape == MoveShape.Path)
                {
                    moves[(int)style].ApplyDefaults(style);
                    EditorUtility.SetDirty(moves[(int)style]);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Move(style.ToString())} rebuilt as a big swing.");
                }
            }

            int weaponLayer = Layers.Index(Layers.Weapon);   // (milestone 9c: weapons on their own layer)
            foreach (var name in BattlerPaths.Weapons)
            {
                var entry = file.Find(name);
                if (entry == null)
                {
                    continue;
                }

                var kind = (WeaponKind)Enum.Parse(typeof(WeaponKind), entry.kind);
                var definition = EnsureWeapon(name, kind, moves);
                if (elbows && definition.gripRoll == 0f && kind != WeaponKind.Shield)
                {
                    definition.gripRoll = WeaponDefinition.DefaultGripRoll(kind);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Weapon(name)}: grip roll {definition.gripRoll}.");
                }

                // (Setup 20, milestone 10: the club's backhand and fan strikes go on the club.)
                if (swings || (SetupVersion.Installed >= 10 && SetupVersion.Installed < 20))
                {
                    AddMoves(definition, moves);
                }

                if (hits)
                {
                    var defaults = WeaponDefinition.CreateDefault(kind);
                    definition.damage = defaults.damage;
                    definition.sweepRadius = defaults.sweepRadius;
                    definition.minHitSpeed = defaults.minHitSpeed;
                    UnityEngine.Object.DestroyImmediate(defaults);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Weapon(name)}: damage {definition.damage}, hit radius {definition.sweepRadius} m.");
                }

                ApplyShape(definition, entry);
                var prefab = BuildPrefab(name, definition, entry, weaponLayer);
                if (definition.prefab != prefab)
                {
                    definition.prefab = prefab;
                }

                EditorUtility.SetDirty(definition);
            }

            AssetDatabase.SaveAssets();
        }

        private static MoveDefinition EnsureMove(MoveStyle style, bool refresh)
        {
            string path = BattlerPaths.Move(style.ToString());
            var move = AssetDatabase.LoadAssetAtPath<MoveDefinition>(path);
            if (move != null)
            {
                if (refresh)
                {
                    move.ApplyDefaults(style);
                    EditorUtility.SetDirty(move);
                    Debug.Log($"[Battler Setup] {path} refreshed to the reworked default.");
                }

                return move;
            }

            EditorFolders.EnsureParent(path);
            move = ScriptableObject.CreateInstance<MoveDefinition>();
            move.ApplyDefaults(style);
            AssetDatabase.CreateAsset(move, path);
            return move;
        }

        private static WeaponDefinition EnsureWeapon(string name, WeaponKind kind, MoveDefinition[] moves)
        {
            string path = BattlerPaths.Weapon(name);
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (weapon != null)
            {
                return weapon;
            }

            EditorFolders.EnsureParent(path);
            weapon = WeaponDefinition.CreateDefault(kind);
            switch (kind)
            {
                case WeaponKind.Sword:
                case WeaponKind.Club:
                    weapon.moves = DefaultMoves(kind, moves);
                    break;
            }

            AssetDatabase.CreateAsset(weapon, path);
            return weapon;
        }

        private static MoveDefinition[] DefaultMoves(WeaponKind kind, MoveDefinition[] moves)
        {
            return kind == WeaponKind.Sword
                ? new[] { moves[(int)MoveStyle.SwordCut], moves[(int)MoveStyle.SwordBackhand], moves[(int)MoveStyle.SwordCleave], moves[(int)MoveStyle.SwordThrust] }
                : new[]
                {
                    moves[(int)MoveStyle.ClubSwing], moves[(int)MoveStyle.ClubSmash], moves[(int)MoveStyle.ClubRising],
                    moves[(int)MoveStyle.ClubBackhand], moves[(int)MoveStyle.ClubFan], moves[(int)MoveStyle.ClubFanBackhand],
                };
        }

        /// <summary>The ready-made moves a weapon of its kind should have, added if missing (yours stay).</summary>
        private static void AddMoves(WeaponDefinition weapon, MoveDefinition[] moves)
        {
            if (weapon.kind == WeaponKind.Shield)
            {
                return;
            }

            var list = new System.Collections.Generic.List<MoveDefinition>(weapon.moves ?? Array.Empty<MoveDefinition>());
            foreach (var move in DefaultMoves(weapon.kind, moves))
            {
                if (move != null && !list.Contains(move))
                {
                    list.Add(move);
                    Debug.Log($"[Battler Setup] {weapon.name} gets {move.name}.");
                }
            }

            weapon.moves = list.ToArray();
        }

        /// <summary>The weapon's shape comes from the art: refreshed every run.</summary>
        private static void ApplyShape(WeaponDefinition weapon, WeaponFileEntry entry)
        {
            weapon.length = entry.lengthM;
            weapon.massKg = entry.massKg;
            if (entry.points != null)
            {
                weapon.tip = RigMath.ToVector(entry.points.tip);
                weapon.edge = RigMath.ToVector(entry.points.edge);
                weapon.head = RigMath.ToVector(entry.points.head);
            }

            if (entry.faceCenter != null && entry.faceCenter.Length >= 3)
            {
                weapon.faceCenter = RigMath.ToVector(entry.faceCenter);
            }

            weapon.radius = entry.radius;
        }

        private static GameObject BuildPrefab(string name, WeaponDefinition definition, WeaponFileEntry entry, int layer)
        {
            var root = new GameObject($"P_Weapon_{name}") { layer = layer };
            try
            {
                var weapon = root.AddComponent<Weapon>();
                weapon.definition = definition;

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(BattlerPaths.WeaponModel(name));
                var filter = model != null ? model.GetComponentInChildren<MeshFilter>(true) : null;
                if (filter != null && filter.sharedMesh != null)
                {
                    var visual = new GameObject("Mesh") { layer = layer };
                    visual.transform.SetParent(root.transform, false);
                    visual.transform.localPosition = filter.transform.position;
                    visual.transform.localRotation = filter.transform.rotation;
                    visual.transform.localScale = filter.transform.lossyScale;
                    visual.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                    var source = filter.GetComponent<MeshRenderer>();
                    visual.AddComponent<MeshRenderer>().sharedMaterials = source != null ? source.sharedMaterials : new Material[0];
                }
                else
                {
                    Debug.LogWarning($"[Battler Setup] Weapon model {BattlerPaths.WeaponModel(name)} not found; P_Weapon_{name} has no mesh.");
                }

                // Colliders on the root (triggers: hits are found by sweeping the striking part, see UnitCombat).
                foreach (var c in entry.colliders ?? Array.Empty<ColliderEntry>())
                {
                    AddCollider(root, c);
                }

                string path = BattlerPaths.WeaponPrefab(name);
                EditorFolders.EnsureParent(path);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AddCollider(GameObject go, ColliderEntry entry)
        {
            switch (entry.type)
            {
                case "box":
                {
                    var box = go.AddComponent<BoxCollider>();
                    box.center = RigMath.ToVector(entry.center);
                    box.size = RigMath.ToVector(entry.size);
                    box.isTrigger = true;
                    break;
                }
                case "sphere":
                {
                    var sphere = go.AddComponent<SphereCollider>();
                    sphere.center = RigMath.ToVector(entry.center);
                    sphere.radius = entry.radius;
                    sphere.isTrigger = true;
                    break;
                }
                case "capsule":
                {
                    // Capsules in weapons.json lie along the weapon (Z).
                    Vector3 a = RigMath.ToVector(entry.start), b = RigMath.ToVector(entry.end);
                    var capsule = go.AddComponent<CapsuleCollider>();
                    capsule.direction = 2;
                    capsule.center = 0.5f * (a + b);
                    capsule.radius = entry.radius;
                    capsule.height = Vector3.Distance(a, b) + 2f * entry.radius;
                    capsule.isTrigger = true;
                    break;
                }
            }
        }
    }
}
