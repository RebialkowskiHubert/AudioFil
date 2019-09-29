using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace AudioFil
{
    public class DownloadMediaViewModel : BindableBase
    { 
        private string urlDown;
        public string UrlDown
        {
            get => urlDown;
            set => SetProperty(ref urlDown, value, "UrlDown");
        }


        private string nameDown;
        public string NameDown
        {
            get => nameDown;
            set => SetProperty(ref nameDown, value, nameof(NameDown));
        }

        private int progress;
        public int Progress
        {
            get => progress;
            private set => SetProperty(ref progress, value, "Progress");
        }

        private string strProgress;
        public string StrProgress
        {
            get => strProgress;
            private set => SetProperty(ref strProgress, value, "StrProgress");
        }

        public async Task StartDownloadAsync()
        {
            try
            {
                SetProgress(0);

                string path = ConfigurationManager.AppSettings["MusicPath"];

                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Wpisz ścieżkę folderu z muzyką", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var client = new YoutubeClient();

                Video info = await client.GetVideoAsync(YoutubeClient.ParseVideoId(UrlDown));

                path += info.Title + " - " + info.Author + ".mp3";

                SetProgress(30);

                MediaStreamInfoSet video = await client.GetVideoMediaStreamInfosAsync(YoutubeClient.ParseVideoId(UrlDown));

                AudioStreamInfo streamInfo = video.Audio.WithHighestBitrate();
               
                await client.DownloadMediaStreamAsync(streamInfo, path);

                SetProgress(90);

                XMLHandling xml = new XMLHandling();
                xml.AddSong(path);

                SetProgress(100);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetProgress(int progress)
        {
            Progress = progress;
            StrProgress = progress.ToString() + "%";

            if(progress == 100)
            {
                StrProgress = "Gotowy";
            }
        }
    }
}
