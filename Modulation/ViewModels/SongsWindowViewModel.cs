using DanTheMan827.ModulateDotNet;
using System.Collections.ObjectModel;
using System.Windows;

namespace DanTheMan827.Modulation.ViewModels
{
    public class SongsWindowViewModel
    {
        public UnpackedInfo? OpenedInfo { get; set; }
        public ObservableProperty<Visibility> SaveVisibility { get; set; } = new ObservableProperty<Visibility>(Visibility.Collapsed);
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public ObservableProperty<bool> ShowSongs { get; set; } = new ObservableProperty<bool>(false);

        public SongsWindowViewModel()
        {
            if (App.IsDesign)
            {
                this.Songs.Add(new Song()
                {
                    ID = "MYSONG",
                    Name = "My Song",
                    Path = "../Songs/mysong/mysong.moggsong",
                    Arena = "World1",
                    Type = "kExtraSong",
                    UnlockType = "play_num",
                    UnlockValue = "0"
                });
            }
        }
    }
}