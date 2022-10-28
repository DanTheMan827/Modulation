using AmpHelper.Enums;
using AmpHelper.Types;
using DanTheMan827.ModulateDotNet;
using System.Collections.ObjectModel;
using System.Windows;

namespace DanTheMan827.Modulation.ViewModels
{
    public class SongsWindowViewModel
    {
        public ObservableProperty<Visibility> SaveVisibility { get; set; } = new(Visibility.Collapsed);
        public ObservableCollection<MoggSong> Songs { get; set; } = new();
        public ObservableCollection<TweakWrapper> TweakWrappers { get; set; } = new();
        public ObservableProperty<bool> ShowSongs { get; set; } = new(false);
        public ObservableProperty<bool> FpsUnlimited { get; set; } = new(false);
        public ObservableProperty<bool> EverythingUnlocked { get; set; } = new(false);
        public UnpackedInfo OpenedInfo { get; internal set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SongsWindowViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (App.IsDesign)
            {
                this.Songs.Add(new MoggSong()
                {
                    Title = "My Song",
                    MoggPath = "../Songs/mysong/mysong.moggsong",
                    ArenaPath = "World1",
                    UnlockRequirement = UnlockRequirement.PlayCount
                });
            }
        }
    }
}