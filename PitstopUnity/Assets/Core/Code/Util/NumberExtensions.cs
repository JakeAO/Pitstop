namespace SadPumpkin.Game.Pitstop.Core.Code.Util
{
    public static class FloatExtensions
    {
        public static float Remap(this float value,
            float originalFrom, float originalTo,
            float targetFrom, float targetTo)
            => (value - originalFrom) / (originalTo - originalFrom) * (targetTo - targetFrom) + targetFrom;
    }

    public static class IntExtensions
    {
        public static float Remap(this int value,
            int originalFrom, int originalTo,
            float targetFrom, float targetTo)
            => (value - originalFrom) / (float)(originalTo - originalFrom) * (targetTo - targetFrom) + targetFrom;
    }
}