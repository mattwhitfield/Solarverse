namespace Solarverse.UI.Core
{
    using System.ComponentModel;

    public class MemoryLogEntryModel : INotifyPropertyChanged
    {
        private string _message;

        public MemoryLogEntryModel(long index, string message)
        {
            Index = index;
            _message = message;
        }

        public long Index { get; }

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
