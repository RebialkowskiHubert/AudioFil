using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMPLib;

namespace AudioFil
{
    public sealed class SongsViewModel : PlayerViewModel
    {
        private IWMPPlaylist playlist;
        private IWMPPlaylistArray array;

        private ObservableCollection<Song> songs;
        public ObservableCollection<Song> Songs
        {
            get => songs;
            set => SetProperty(ref songs, value, "Songs");
        }


        private Song selectedSong;
        public Song SelectedSong
        {
            get => selectedSong;
            set
            {
                if(selectedSong != value)
                {
                    selectedSong = value;
                    OnPropertyChanged("SelectedSong");
                    wmp.URL = SelectedSong.Path;
                    Play();
                }
            }
        }

        private int realTime;
        public int RealTime
        {
            get => realTime;
            set
            {
                if(realTime != value)
                {
                    realTime = value;
                    OnPropertyChanged("RealTime");

                    if(wmp.controls.currentItem != null)
                    {
                        //wmp.controls.currentPosition = (realTime * wmp.controls.currentItem.duration) / 100;
                    }
                }
            }
        }

        private string description = "Gotowy";
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value, "Description");
        }

        private bool pauseBool = false;
        public bool PauseBool
        {
            get => pauseBool;
            set => SetProperty(ref pauseBool, value, "PauseBool");
        }

        private bool playBool = true;
        public bool PlayBool
        {
            get => playBool;
            set => SetProperty(ref playBool, value, "PlayBool");
        }

        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand PreviousCommand { get; set; }

        public SongsViewModel()
        {
            //LoadSongs();

            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);

            wmp.StatusChange += CheckStatus;
        }


        private void LoadSongs()
        {
            Songs = new ObservableCollection<Song>();

            array = wmp.playlistCollection.getByName("MUZA");
            playlist = array.Item(0);

            int i;
            for(i = 0; i < playlist.count; i++)
            {
                IWMPMedia song = playlist.Item[i];
                Songs.Add(new Song()
                {
                    Title = song.getItemInfo("Title"),
                    Artist = song.getItemInfo("Artist"),
                    Path = song.sourceURL,
                    Time = DateTime.Parse(song.durationString)
                });
            }
            
            wmp.currentPlaylist = playlist;
            wmp.controls.stop();
        }

        private void PlayPause()
        {
            PauseBool = (Description != "Gotowy" && Description != "Zatrzymanie" && Description != "Wstrzymanie") ? true : false;
            PlayBool = !PauseBool;
        }

        public void Play()
        {
            if (string.IsNullOrEmpty(wmp.URL))
                return;

            wmp.controls.play();

            UpdateTime();

            PlayPause();
        }

        public void Pause()
        {
            wmp.controls.pause();
            PlayPause();
        }

        public void Stop()
        {
            wmp.controls.stop();
            PlayPause();
        }

        public void Next()
        {
            int index = Songs.IndexOf(SelectedSong) + 1;

            if (index < Songs.Count)
                SelectedSong = Songs[index];
        }

        public void Previous()
        {
            int index = Songs.IndexOf(SelectedSong) - 1;

            if (index >= 0)
                SelectedSong = Songs[index];
            else
                SelectedSong = Songs[Songs.Count - 1];
        }

        private void CheckStatus()
        {
            Description = wmp.status;
        }

        private void UpdateTime()
        {
            int i;
            Task.Run(async () =>
            {
                for(i = 0; i < wmp.controls.currentItem.duration; i++)
                {
                    RealTime = (int) ((wmp.controls.currentPosition * 100) / wmp.controls.currentItem.duration);
                    await Task.Delay(1000);
                }
            });
        }
    }
}