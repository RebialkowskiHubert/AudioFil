using System;

namespace AudioFil
{
    public class SongInfo : BindableBase
    {
        private string artist;
        public string Artist
        {
            get => artist;
            set => SetProperty(ref artist, value, nameof(Artist));
        }

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value, nameof(Title));
        }

        public SongInfo(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }
    }

    public class MetadataEventArgs : EventArgs
    {
        public string OldMetadata { get; set; }
        public string NewMetadata { get; set; }

        public MetadataEventArgs(string oldMetadata, string newMetadata)
        {
            OldMetadata = oldMetadata;
            NewMetadata = newMetadata;
        }
    }

    public class CurrentSongEventArgs : EventArgs
    {
        public SongInfo OldSong { get; set; }
        public SongInfo NewSong { get; set; }

        public CurrentSongEventArgs(SongInfo oldSong, SongInfo newSong)
        {
            OldSong = oldSong;
            NewSong = newSong;
        }
    }

    public class StreamUpdateEventArgs : EventArgs
    {
        public byte[] Data { get; set; }

        public StreamUpdateEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
