using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Extensions;
using DanTheMan827.Modulation.Interfaces;
using DtxCS;
using DtxCS.DataTypes;
using System.IO;
using System.Linq;

namespace DanTheMan827.Modulation.Tweaks
{
    internal class FpsUnlimiter : ITweak
    {
        private UnpackedInfo Info { get; }
        private string ConfigPath { get; }
        public FpsUnlimiter(UnpackedInfo info)
        {
            this.Info = info;
            this.ConfigPath = Path.Join(this.Info.UnpackedPath, this.Info.ConsoleLabel, "system", "data", "config", $"default.dta_dta_{this.Info.ConsoleLabel}");
        }
        public bool GetState()
        {
            return Helpers.DoWithDtbFile(this.ConfigPath, dtx =>
            {
                var rndNode = dtx.FindByName("rnd")?.OfType<DataArray>()?.FirstOrDefault();

                if (rndNode != null)
                {
                    var vsyncModeNode = rndNode.FindByName("vsync_mode")?.OfType<DataArray>().FirstOrDefault();
                    var vsyncEnabledNode = rndNode.FindByName("vsync_enabled")?.OfType<DataArray>().FirstOrDefault();

                    if (vsyncModeNode != null && vsyncEnabledNode != null && vsyncModeNode.Children.Count == 2 && vsyncEnabledNode.Children.Count == 2)
                    {
                        return vsyncModeNode.Children[1].ToString(1) == "1" && vsyncEnabledNode.Children[1].ToString(1) == "FALSE";
                    }
                }

                return false;
            }, false);
        }

        public bool SetState(bool enabled)
        {
            return Helpers.DoWithDtbFile(this.ConfigPath, dtx =>
            {
                var rndNode = dtx.FindByName("rnd")?.OfType<DataArray>()?.FirstOrDefault();

                if (rndNode != null)
                {
                    var vsyncModeNode = rndNode.FindByName("vsync_mode")?.OfType<DataArray>().FirstOrDefault();
                    var vsyncEnabledNode = rndNode.FindByName("vsync_enabled")?.OfType<DataArray>().FirstOrDefault();

                    if (vsyncModeNode != null && vsyncEnabledNode != null && vsyncModeNode.Children.Count == 2 && vsyncEnabledNode.Children.Count == 2)
                    {
                        vsyncModeNode.Children[1] = DTX.FromDtaString(enabled ? "1" : "2").Children[0];
                        vsyncEnabledNode.Children[1] = DTX.FromDtaString(enabled ? "FALSE" : "TRUE").Children[0];
                    }
                }

                return true;
            });
        }
    }
}
