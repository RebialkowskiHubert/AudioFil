using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace AudioFil
{
    public class DownloadMediaViewModel : BindableBase
    {
        private BackgroundWorker worker;

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
            set => SetProperty(ref progress, value, "Progress");
        }

        private string strProgress;
        public string StrProgress
        {
            get => strProgress;
            set => SetProperty(ref strProgress, value, "StrProgress");
        }

        public void RunDownload()
        {
            worker = new BackgroundWorker();
            worker.RunWorkerCompleted += OnComplete;
            worker.WorkerReportsProgress = true;
            worker.DoWork += StartDownload;
            worker.RunWorkerAsync();
            StrProgress = "0%";
        }

        private async void StartDownload(object o, DoWorkEventArgs e)
        {
            try
            {
                string path = ConfigurationManager.AppSettings["MusicPath"];

                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Wpisz ścieżkę folderu z muzyką", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var client = new YoutubeClient();

                var info = await client.GetVideoAsync(YoutubeClient.ParseVideoId(UrlDown));

                path += info.Title + " - " + info.Author + ".mp3";

                var video = await client.GetVideoMediaStreamInfosAsync(YoutubeClient.ParseVideoId(UrlDown));

                var streamInfo = video.Audio.WithHighestBitrate();
               
                await client.DownloadMediaStreamAsync(streamInfo, path);

                XMLHandling xml = new XMLHandling();
                xml.AddSong(path);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnComplete(object o, RunWorkerCompletedEventArgs e)
        {
            Progress = 100;
            StrProgress = "Gotowy";
        }
    }
}
