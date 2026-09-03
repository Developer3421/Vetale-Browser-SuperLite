using System;
using System.ComponentModel;
using LiteDB;

namespace Vetale_Browser_Lite
{
    public sealed class BookmarkEntry : INotifyPropertyChanged
    {
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        private byte[] _favicon;
        public byte[] Favicon
        {
            get => _favicon;
            set { _favicon = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Favicon))); }
        }

        public string Display => string.IsNullOrWhiteSpace(Title) ? Url : $"{Title}  —  {Url}";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
