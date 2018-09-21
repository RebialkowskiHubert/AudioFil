using System.ComponentModel;
using System.IO;
using MediaToolkit.Model;
using MediaToolkit;
using System;
using System.Windows;
using YoutubeExtractor;
using System.Collections.Generic;
using System.Linq;

namespace AudioFil
{
    public class DownloadMediaViewModel : BindableBase
    {
        private XMLHandling xMLHandling;
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
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerAsync();
            StrProgress = "0%";
        }

        private void StartDownload(object o, DoWorkEventArgs e)
        {
            try
            {
                IEnumerable<VideoInfo> videoInfo = DownloadUrlResolver.GetDownloadUrls(UrlDown);

                VideoInfo video = videoInfo.First(i => i.VideoType == VideoType.Mp4 && i.Resolution == 360);

                worker.ReportProgress(25);

                if (video.RequiresDecryption)
                    DownloadUrlResolver.DecryptDownloadUrl(video);

                char[] illegal = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

                string audioPath = video.Title;

                foreach(char character in illegal)
                {
                    if (audioPath.Contains(character))
                        audioPath = audioPath.Replace(character, ' ');
                }

                if (Properties.Settings.Default.MusicPath != "")
                    audioPath = Properties.Settings.Default.MusicPath + "\\" + audioPath;
                else
                    audioPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\" + audioPath;

                VideoDownloader downloader = new VideoDownloader(video, audioPath);
                downloader.Execute();

                worker.ReportProgress(50);

                MediaFile input = new MediaFile { Filename = audioPath };
                MediaFile output = new MediaFile { Filename = $"{audioPath}.mp3" };

                using (Engine engine = new Engine())
                {
                    engine.GetMetadata(input);
                    engine.Convert(input, output);
                    worker.ReportProgress(90);
                }

                File.Delete(input.Filename);

                worker.ReportProgress(99);

                xMLHandling = new XMLHandling();

                xMLHandling.AddSong(output.Filename);

                worker.ReportProgress(100);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProgressChanged(object o, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            StrProgress = Progress.ToString() + "%";
        }

        private void OnComplete(object o, RunWorkerCompletedEventArgs e)
        {
            Progress = 100;
            StrProgress = "Gotowy";
        }
    }
}
