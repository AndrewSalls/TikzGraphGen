namespace TikzGraphGen.GraphData
{
    public static class UnitConverter
    {
        public static readonly float ST_PX = 1;
        public static readonly float ST_IN = 96;
        public static readonly float ST_MM = 2438.4f;

        public static float PxToIn(float px) => px / ST_IN;
        public static float PxToMm(float px) => px / ST_MM;
        public static float InToPx(float inc) => inc * ST_IN;
        public static float InToMm(float inc) => inc * (ST_MM / ST_IN);
        public static float MmToPx(float mm) => mm / ST_MM;
        public static float MmToIn(float mm) => mm / (ST_MM / ST_IN);
    }
}
