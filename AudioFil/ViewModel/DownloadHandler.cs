using System;
using System.Configuration;
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
        private string path;

        private DownloadMedia song;
        public DownloadMedia Song
        {
            get => song;
            set => SetProperty(ref song, value, nameof(Song));
        }


        public DownloadHandler()
        {
            path = ConfigurationManager.AppSettings["MusicPath"];
            client = new YoutubeClient();
        }

        public async Task StartDownloadAsync()
        {
            try
            {
                SetProgress(0);

                if (string.IsNullOrEmpty(path))
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
            Video info = await client.GetVideoAsync(videoId);

            path += info.Title + ".mp3";

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
