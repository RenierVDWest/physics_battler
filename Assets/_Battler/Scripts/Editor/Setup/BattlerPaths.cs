namespace PhysicsBattler.Editor.Setup
{
    /// <summary>Every asset path the project setup reads or writes.</summary>
    internal static class BattlerPaths
    {
        public const string Root = "Assets/_Battler";

        public const string OrcArtFolder = Root + "/Art/Orc";
        public const string OrcRigFile = Root + "/Data/Rigs/orc_rig.json";

        public const string MaterialsFolder = Root + "/Materials";
        public const string DataFolder = Root + "/Data";
        public const string PrefabsFolder = Root + "/Prefabs";
        public const string ScenesFolder = Root + "/Scenes";
        public const string RenderingFolder = Root + "/Settings/Rendering";

        public const string UrpAsset = RenderingFolder + "/URP_Battler.asset";
        public const string UrpRenderer = RenderingFolder + "/URP_Battler_Renderer.asset";

        public const string OrcProfile = DataFolder + "/Units/UP_Orc.asset";
        public const string GoblinProfile = DataFolder + "/Units/UP_Goblin.asset";
        public const string ClubDefinition = DataFolder + "/Weapons/WD_ClubBig.asset";
        public const string ClubSwing = DataFolder + "/Attacks/AD_ClubLowSwing.asset";

        public const string OrcPrefab = PrefabsFolder + "/Units/P_Orc.prefab";
        public const string GoblinPrefab = PrefabsFolder + "/Units/P_Goblin.prefab";
        public const string ClubPrefab = PrefabsFolder + "/Weapons/P_Weapon_ClubBig.prefab";
        public const string CratePrefab = PrefabsFolder + "/Props/P_Prop_Crate.prefab";
        public const string BarrelPrefab = PrefabsFolder + "/Props/P_Prop_Barrel.prefab";

        public const string SandboxScene = ScenesFolder + "/Sandbox.unity";

        public const string GroundMaterial = MaterialsFolder + "/M_Ground.mat";
        public const string CrateMaterial = MaterialsFolder + "/M_Prop_Crate.mat";
        public const string BarrelMaterial = MaterialsFolder + "/M_Prop_Barrel.mat";
    }
}
