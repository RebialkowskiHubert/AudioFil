using System;
using System.Threading.Tasks;

namespace AudioFil
{
    public class Radio : BindableBase
    {
        private int id;
        public int Id
        {
            get => id;
            set => id = value;
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }

        private Uri url;
        public Uri Url
        {
            get => url;
            set => url = value;
        }

        private bool running;
        public bool Running
        {
            get => running;
            set
            {
                running = value;
                if (!running && runningTask != null)
                    runningTask.Wait();
            }
        }

        private string metadata;
        public string Metadata
        {
            get => metadata;
            set
            {
                OnMetadataChanged?.Invoke(this, new MetadataEventArgs(metadata, value));
                metadata = value;
            }
        }

        private SongInfo currentSong;
        public SongInfo CurrentSong
        {
            get => currentSong;
            set
            {
                OnCurrentSongChanged?.Invoke(this, new CurrentSongEventArgs(currentSong, value));
                currentSong = value;
            }
        }

        public Task runningTask;

        public event EventHandler<CurrentSongEventArgs> OnCurrentSongChanged;
        public event EventHandler<MetadataEventArgs> OnMetadataChanged;

        public Radio(int id, string name, Uri url)
        {
            Id = id;
            Name = name;
            Url = url;
        }

    }
}
