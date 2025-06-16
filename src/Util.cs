using Discord;

namespace VoiceOfReason
{
    public static class Util
    {
        public static int DivCeil(this int a, int b)
        {
            return 1 + ((a - 1) / b);
        }

    }
}
