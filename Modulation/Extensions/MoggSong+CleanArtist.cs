using AmpHelper.Types;
using DanTheMan827.Modulation.Helpers;

namespace DanTheMan827.Modulation.Extensions
{
    internal static partial class Extensions
    {
        public static string CleanArtist(this MoggSong moggsong)
        {
            return HelperMethods.CleanString(moggsong.Artist ?? moggsong.ShortArtist);
        }
    }
}
