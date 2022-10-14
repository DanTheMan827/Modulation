using System.Collections.ObjectModel;
using WPFCustomMessageBox;

namespace DanTheMan827.Modulation.ViewModels
{
    public class CustomMessageBoxWindowViewModel
    {
        public object Result { get; set; }
        public ObservableProperty<string> Caption { get; set; } = new("A caption");
        public ObservableProperty<string> Message { get; set; } = new("Some message being shown");
        public ObservableCollection<CustomMessageBoxWindow.Button> Buttons { get; set; } = new();
        public CustomMessageBoxWindowViewModel()
        {
            if (App.IsDesign)
            {
                this.Buttons.Add(new CustomMessageBoxWindow.Button() { Label = "OK", Result = 0 });
            }
        }
    }
}
