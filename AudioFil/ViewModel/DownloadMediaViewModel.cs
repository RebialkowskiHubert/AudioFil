using System.ComponentModel;
using System.IO;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;

namespace AudioFil
{
    public class DownloadMediaViewModel : PropChanged
    {
        private XMLHandling xMLHandling;

        private string urlDown;
        public string UrlDown
        {
            get => urlDown;
            set
            {
                if(urlDown != value)
                {
                    urlDown = value;
                    RaisePropertyChanged("UrlDown");
                }
            }
        }


        private string nameDown;
        public string NameDown
        {
            get => nameDown;
            set
            {
                if(nameDown != value)
                {
                    nameDown = value;
                    RaisePropertyChanged("NameDown");
                }
            }
        }

        private int progress;
        public int Progress
        {
            get => progress;
            set
            {
                if(progress != value)
                {
                    progress = value;
                    RaisePropertyChanged("Progress");
                }
            }
        }

        private string strProgress;
        public string StrProgress
        {
            get => strProgress;
            set
            {
                if (strProgress != value)
                {
                    strProgress = value;
                    RaisePropertyChanged("StrProgress");
                }
            }
        }

        private bool progressBusy;
        public bool ProgressBusy
        {
            get => progressBusy;
            set
            {
                if(progressBusy != value)
                {
                    progressBusy = value;
                    RaisePropertyChanged("ProgressBusy");
                }
            }
        }


        public void RunDownload()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += OnComplete;
            worker.WorkerReportsProgress = true;
            worker.DoWork += StartDownload;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerAsync();
            StrProgress = "0%";
        }

        private void StartDownload(object o, DoWorkEventArgs e)
        {
            YouTube yt = YouTube.Default;
            YouTubeVideo video = yt.GetVideo(UrlDown);

            string audioPath = @"D:\Muza\Muza\" + video.FullName;

            File.WriteAllBytes(audioPath, video.GetBytes());

            MediaFile input = new MediaFile { Filename = audioPath };
            MediaFile output = new MediaFile { Filename = $"{audioPath}.mp3" };

            using(Engine engine = new Engine())
            {
                engine.GetMetadata(input);
                engine.Convert(input, output);
            }

            File.Delete(input.Filename);

            xMLHandling = new XMLHandling();

            xMLHandling.AddSong(output.Filename);
        }

        private void ProgressChanged(object o, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            StrProgress = (string)e.UserState;
        }

        private void OnComplete(object o, RunWorkerCompletedEventArgs e)
        {
            Progress = 100;
            StrProgress = "Gotowy";
        }
    }
}
