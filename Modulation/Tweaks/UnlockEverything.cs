using DanTheMan827.ModulateDotNet;
using DanTheMan827.Modulation.Extensions;
using DanTheMan827.Modulation.Interfaces;
using DtxCS;
using DtxCS.DataTypes;
using System;
using System.IO;
using System.Linq;

namespace DanTheMan827.Modulation.Tweaks
{
    internal class UnlockEverything : ITweak
    {
        private UnpackedInfo Info { get; }
        private string ConfigPath { get; }
        public UnlockEverything(UnpackedInfo info)
        {
            this.Info = info;
            this.ConfigPath = Path.Join(this.Info.UnpackedPath, this.Info.ConsoleLabel, "config", $"amp_config.dta_dta_{this.Info.ConsoleLabel}");
        }

        private static readonly string[] DefaultUnlockTypes = new string[]
        {
            "ALLTHETIME",
            "ASSAULT_ON",
            "ASTROSIGHT",
            "BREAKFORME",
            "CONCEPT",
            "CRAZY_RIDE",
            "CRYSTAL",
            "DALATECHT",
            "DECODE_ME",
            "DIGITALPARALYSIS",
            "DONOTRETREAT",
            "DREAMER",
            "ENERGIZE",
            "ENTOMOPHOBIA",
            "FORCEQUIT",
            "HUMANLOVE",
            "IMPOSSIBLE",
            "ISEEYOU",
            "LIGHTS",
            "MAGPIE",
            "MUZE",
            "NECRODANCER",
            "PERFECTBRAIN",
            "PHANTOMS",
            "RECESSION",
            "REDGIANT",
            "SUPRASPATIAL",
            "SYNTHESIZED2014",
            "UNFINISHEDBUSINESS",
            "WAYFARER",
            "WETWARE",
            "World1",
            "World2",
            "World3",
            "autocatcher",
            "bumper",
            "crippler",
            "freestyle",
            "freq_mode",
            "multiplier",
            "skill_nightmare",
            "slowdown"
        };
        public bool GetState()
        {
            return Helpers.DoWithDtbFile(this.ConfigPath, dtx =>
            {
                var unlocks = dtx.FindByName("db")?.OfType<DataArray>()?.FirstOrDefault()?.FindByName("campaign")?.OfType<DataArray>()?.FirstOrDefault();

                if (unlocks != null)
                {
                    string?[]? originalNodes = (DTX.FromDtaString(AppResources.default_unlocks).Children[0] as DataArray)?.Children.Select(node => node.ToString()).ToArray();

                    if (originalNodes != null)
                    {
                        string?[] currentNodes = unlocks.Children.Take(originalNodes.Length).Select(node => node.ToString()).ToArray();

                        return string.Join("\n", originalNodes) != string.Join("\n", currentNodes);
                    }
                }

                return false;
            });
        }

        public bool SetState(bool enabled)
        {
            return Helpers.DoWithDtbFile(this.ConfigPath, dtx =>
            {
                var unlocks = dtx.FindByName("db")?.OfType<DataArray>()?.FirstOrDefault()?.FindByName("campaign")?.OfType<DataArray>()?.FirstOrDefault();

                if (unlocks != null)
                {
                    foreach (var unlock in unlocks.Children.OfType<DataArray>())
                    {
                        unlock.Children[0] = DTX.FromDtaString("beat_num").Children[0];
                        unlock.Children[1] = DTX.FromDtaString("0").Children[0];
                        unlock.Children[2] = DTX.FromDtaString("kUnlockArena").Children[0];
                    }

                    if (enabled == false)
                    {
                        var newNodes = unlocks.Children.OfType<DataArray>().Where(node => !DefaultUnlockTypes.Contains(node.Children[3].ToString())).ToArray();
                        var originalNodes = (DTX.FromDtaString(AppResources.default_unlocks).Children[0] as DataArray)?.Children;

                        if (originalNodes != null)
                        {
                            unlocks.Children.Clear();
                            unlocks.Children.AddRange(originalNodes);
                            unlocks.Children.AddRange(newNodes);
                        }
                    }

                    return true;
                }

                return false;
            });
        }


    }
}
