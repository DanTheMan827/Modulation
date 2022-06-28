namespace DanTheMan827.Modulation.ViewModels
{
    public class ProgressWindowViewModel
    {
        public ObservableProperty<string> Title { get; set; } = new("Progress Window");
        public ObservableProperty<string> Message { get; set; } = new("Doing something...");
        public ObservableProperty<bool> IsIndeterminate { get; set; } = new(true);
        public ObservableProperty<double> Maximum { get; set; } = new(1);
        public ObservableProperty<double> Minimum { get; set; } = new(0);
        public ObservableProperty<double> Value { get; set; } = new(0);
    }
}
