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
            set => SetProperty(ref url, value, "Url");
        }

        private ObservableCollection<Control> downloadedMedia;
        public ObservableCollection<Control> DownloadedMedia
        {
            get => downloadedMedia;
            set => SetProperty(ref downloadedMedia, value, "DownloadedMedia");
        }


        public RelayCommand DownloadCommand { get; set; }

        public DownloaderViewModel()
        {
            DownloadedMedia = new ObservableCollection<Control>();
            DownloadCommand = new RelayCommand(DownloadAsync);
        }

        private async void DownloadAsync()
        {
            DownloadMediaViewModel dmvm = new DownloadMediaViewModel
            {
                UrlDown = Url
            };
            DownloadedMedia.Add(new DownloadMediaView
            {
                DataContext = dmvm
            });
            await dmvm.StartDownloadAsync();
        }
    }
}
