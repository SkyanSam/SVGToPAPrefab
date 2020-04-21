using PA_PrefabBuilder;

namespace SVGToPrefab
{
    public struct Input
    {
        public static string prefabName = "ScalingTestSVG";
        public static PrefabType prefabType = PrefabType.Misc1;
        public static float secondsToLast = 15f;
        public static string svgPath = @"D:/Documents/CSharpProjects/ScalingTest.svg";
        public static string prefabPath = @"C:\Program Files (x86)\Steam\steamapps\common\Project Arrhythmia\beatmaps\prefabs";

        public static float sizeMultiplier = 1f / 10f;

        public struct Colors
        {
            /* Colors corresponding with their ids. this must be auto filled by the user.
             * NOTE: Need to be careful. Not sure if svg also used hex or hsv. May need to convert later */
            public static string[] ids = new string[10]
            {
        "rgb(255,0,0)",
        "rgb(0,255,0)",
        "rgb(0,0,255)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
            };
        }
    }
}
