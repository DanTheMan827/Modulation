using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanTheMan827.Modulation
{
    public class ObservableProperty<T>: INotifyPropertyChanged
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged("Value"); }
        }

        public static implicit operator T(ObservableProperty<T> op) => op.Value;
        public static explicit operator ObservableProperty<T>(T val) => new ObservableProperty<T>(val);

        public event PropertyChangedEventHandler? PropertyChanged;

        internal void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableProperty(T value)
        {
            _value = value;
        }
    }
}
