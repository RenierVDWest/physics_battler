namespace PhysicsBattler.Editor.Setup
{
    internal static class BattlerPaths
    {
        public const string Root = "Assets/_Battler";

        public const string BodiesArtFolder = Root + "/Art/Bodies";
        public const string RigsFolder = Root + "/Data/Rigs";

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
