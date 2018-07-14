using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AudioFil
{
    public class DownloaderViewModel : PropChanged
    {
        private string url;
        public string Url
        {
            get => url; 
            set
            {
                if(url != value)
                {
                    url = value;
                    RaisePropertyChanged("Url");
                    DownloadCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private ObservableCollection<Control> downloadedMedia;
        public ObservableCollection<Control> DownloadedMedia
        {
            get => downloadedMedia;
            set
            {
                if(downloadedMedia != value)
                {
                    downloadedMedia = value;
                }
            }
        }


        public RelayCommand DownloadCommand { get; set; }

        public DownloaderViewModel()
        {
            DownloadedMedia = new ObservableCollection<Control>();
            DownloadCommand = new RelayCommand(Download);
        }

        private void Download()
        {
            DownloadMediaViewModel dmvm = new DownloadMediaViewModel
            {
                UrlDown = Url
            };
            DownloadedMedia.Add(new DownloadMediaView
            {
                DataContext = dmvm
            });
            dmvm.RunDownload();
        }
    }
}
