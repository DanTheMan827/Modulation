using System.ComponentModel;

namespace DanTheMan827.Modulation
{
    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T _value;
        public T Value
        {
            get => this._value;
            set { this._value = value; this.NotifyPropertyChanged("Value"); }
        }

        public static implicit operator T(ObservableProperty<T> op)
        {
            return op.Value;
        }

        public static explicit operator ObservableProperty<T>(T val)
        {
            return new ObservableProperty<T>(val);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableProperty(T value)
        {
            this._value = value;
        }
    }
}
