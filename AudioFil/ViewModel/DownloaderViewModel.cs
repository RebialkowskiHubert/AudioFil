using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AudioFil
{
    public class DownloaderViewModel : BindableBase
    {
        private string url;
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value, nameof(Url));
        }

        private ObservableCollection<DownloadMedia> songsList;
        public ObservableCollection<DownloadMedia> SongsList
        {
            get => songsList;
            set => SetProperty(ref songsList, value, nameof(SongsList));
        }

        private DownloadMedia selectedSong;
        public DownloadMedia SelectedSong
        {
            get => selectedSong;
            set => SetProperty(ref selectedSong, value, nameof(SelectedSong));
        }


        public RelayCommand DownloadCommand { get; set; }

        public DownloaderViewModel()
        {
            SongsList = new ObservableCollection<DownloadMedia>();
            DownloadCommand = new RelayCommand(DownloadAsync);
        }

        private async void DownloadAsync()
        {
            SelectedSong = new DownloadMedia()
            {
                Url = this.Url,
                Progress = 0,
                StrProgress = "0%"
            };

            DownloadHandler handler = new DownloadHandler(this)
            {
                Song = SelectedSong
            };

            await handler.StartDownloadAsync();
        }

        public void AddToSongsList(DownloadMedia media)
        {
            SongsList.Add(media);
        }
    }
}
