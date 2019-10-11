using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace AudioFil
{
    public class DownloadHandler : BindableBase
    {
        private readonly YoutubeClient client;
        private string basePath;
        private readonly DownloaderViewModel downloaderViewModel;

        private DownloadMedia song;
        public DownloadMedia Song
        {
            get => song;
            set => SetProperty(ref song, value, nameof(Song));
        }


        public DownloadHandler(DownloaderViewModel downloaderViewModelInstance)
        {
            basePath = ConfigurationManager.AppSettings["MusicPath"];
            client = new YoutubeClient();
            downloaderViewModel = downloaderViewModelInstance;
        }

        public async Task StartDownloadAsync()
        {
            try
            {
                SetProgress(0);

                if (string.IsNullOrEmpty(basePath))
                {
                    MessageBox.Show("Wpisz ścieżkę folderu z muzyką", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (YoutubeClient.TryParsePlaylistId(Song.Url, out string playlistId))
                {
                    Playlist playlist = await client.GetPlaylistAsync(playlistId);

                    foreach(Video song in playlist.Videos)
                    {
                        await DownloadSongAsync(song.Id);
                    }
                }
                else if (YoutubeClient.TryParseVideoId(Song.Url, out string videoId))
                {
                    await DownloadSongAsync(videoId);
                }
                else
                {
                    MessageBox.Show("Podany link jest nieprawidłowy", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task DownloadSongAsync(string videoId)
        {
            string url = Song.Url ?? "";

            Song = new DownloadMedia()
            {
                Url = url
            };

            downloaderViewModel.AddToSongsList(Song);

            Video info = await client.GetVideoAsync(videoId);

            string path = info.Title;

            foreach(char invalid in Path.GetInvalidFileNameChars())
            {
                path = path.Replace(invalid, '_');
            }

            path = basePath + path + ".mp3";

            if(path.Length > 255)
            {
                path = path.Substring(0, 255 - 4) + ".mp3";
            }

            Song.Name = info.Title;

            SetProgress(50);

            MediaStreamInfoSet video = await client.GetVideoMediaStreamInfosAsync(videoId);

            AudioStreamInfo streamInfo = video.Audio.WithHighestBitrate();

            await client.DownloadMediaStreamAsync(streamInfo, path);

            SetProgress(90);

            XMLHandling xml = new XMLHandling();
            xml.AddSong(path);

            SetProgress(100);
        }

        private void SetProgress(int progress)
        {
            Song.Progress = progress;
            Song.StrProgress = progress.ToString() + "%";

            if(progress == 100)
            {
                Song.StrProgress = "Gotowy";
            }
        }
    }
}
