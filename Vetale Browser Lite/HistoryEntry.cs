using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Vetale_Browser_Lite
{
    public sealed class HistoryEntry : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime VisitedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        private byte[] _favicon;
        public byte[] Favicon
        {
            get => _favicon;
            set { _favicon = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Favicon))); }
        }

        [JsonIgnore]
        public string Display => $"{VisitedAt:dd.MM.yyyy HH:mm}  —  {(string.IsNullOrWhiteSpace(Title) ? Url : Title)}";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
