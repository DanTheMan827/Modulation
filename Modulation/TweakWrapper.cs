using AmpHelper.Attributes;
using AmpHelper.Interfaces;

namespace DanTheMan827.Modulation
{
    public class TweakWrapper
    {
        public InstantiableTweakInfo Info { get; set; }
        public ObservableProperty<bool> Enabled { get; set; } = new(false);
        private ITweak Tweak;
        public string Name => Info.Name;
        public string Description => Info.Description;

        public TweakWrapper(string unpackedPath, InstantiableTweakInfo info)
        {
            Info = info;
            Tweak = info.CreateInstance(unpackedPath);
            Enabled.Value = Tweak.IsEnabled();
        }

        public string ToggleTweak()
        {

            if (Enabled.Value)
            {
                Tweak.DisableTweak();
                Enabled.Value = false;

                return Info.DisabledText;
            }
            else
            {
                Tweak.EnableTweak();
                Enabled.Value = true;

                return Info.EnabledText;
            }
        }
    }
}
