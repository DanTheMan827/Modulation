namespace DanTheMan827.Modulation.ViewModels
{
    public class ProgressWindowViewModel
    {
        public ObservableProperty<string> Title { get; set; } = new ObservableProperty<string>("Progress Window");
        public ObservableProperty<string> Message { get; set; } = new ObservableProperty<string>("Doing something...");
    }
}
