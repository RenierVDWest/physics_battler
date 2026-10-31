namespace PhysicsBattler.Editor.Setup
{
    internal static class BattlerPaths
    {
        public const string Root = "Assets/_Battler";

        public const string BodiesArtFolder = Root + "/Art/Bodies";
        public const string WeaponsArtFolder = Root + "/Art/Weapons";
        public const string RigsFolder = Root + "/Data/Rigs";
        public const string WeaponsFile = RigsFolder + "/weapons.json";

        public const string MaterialsFolder = Root + "/Materials";
        public const string DataFolder = Root + "/Data";
        public const string PrefabsFolder = Root + "/Prefabs";
        public const string ScenesFolder = Root + "/Scenes";
        public const string RenderingFolder = Root + "/Settings/Rendering";

        public const string UrpAsset = RenderingFolder + "/URP_Battler.asset";
        public const string UrpRenderer = RenderingFolder + "/URP_Battler_Renderer.asset";

        public static readonly string[] Bodies = { "Small", "Medium", "Large" };

        public static string RigFile(string body) => $"{RigsFolder}/{body.ToLowerInvariant()}_rig.json";
        public static string Profile(string body) => $"{DataFolder}/Units/UP_{body}.asset";
        public static string Style(string body) => $"{DataFolder}/Styles/CS_{body}.asset";
        public static string UnitPrefab(string body) => $"{PrefabsFolder}/Units/P_Unit_{body}.prefab";

        public static readonly string[] Weapons = { "Sword", "Club", "Shield" };

        public static string WeaponModel(string weapon) => $"{WeaponsArtFolder}/SM_{weapon}.fbx";
        public static string Weapon(string weapon) => $"{DataFolder}/Weapons/WD_{weapon}.asset";
        public static string WeaponPrefab(string weapon) => $"{PrefabsFolder}/Weapons/P_Weapon_{weapon}.prefab";
        public static string Move(string style) => $"{DataFolder}/Moves/MD_{style}.asset";
        public static string Rhythm(string style) => $"{DataFolder}/Rhythms/RD_{style}.asset";
        public static string Posture(string name) => $"{DataFolder}/Rhythms/Postures/PD_{name}.asset";

        public const string BattleScene = ScenesFolder + "/Battle.unity";
        public const string OldSandboxScene = ScenesFolder + "/Sandbox.unity";

        public const string GroundMaterial = MaterialsFolder + "/M_Ground.mat";
        public const string BodyMainMaterial = MaterialsFolder + "/M_Body_Main.mat";
        public const string BodyJointMaterial = MaterialsFolder + "/M_Body_Joint.mat";
        public const string TeamAMaterial = MaterialsFolder + "/M_Team_A.mat";
        public const string TeamBMaterial = MaterialsFolder + "/M_Team_B.mat";
        public const string BoardLightMaterial = MaterialsFolder + "/M_Board_Light.mat";
        public const string BoardDarkMaterial = MaterialsFolder + "/M_Board_Dark.mat";
        public const string BoardBorderMaterial = MaterialsFolder + "/M_Board_Border.mat";
    }
}
