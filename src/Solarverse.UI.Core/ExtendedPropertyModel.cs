using System.ComponentModel;

namespace Solarverse.UI.Core
{
    public class ExtendedPropertyModel : INotifyPropertyChanged
    {
        private string _message;

        public ExtendedPropertyModel(string caption, string message)
        {
            Caption = caption;
            _message = message;
        }

        public string Caption { get; }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
