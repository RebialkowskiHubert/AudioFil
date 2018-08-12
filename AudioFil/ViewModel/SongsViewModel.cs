using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMPLib;

namespace AudioFil
{
    public sealed class SongsViewModel : PlayerViewModel, IPlayer
    {
        private ObservableCollection<Song> songs;
        public ObservableCollection<Song> Songs
        {
            get => songs;
            set
            {
                if(songs != value)
                {
                    songs = value;
                    RaisePropertyChanged("Songs");
                }
            }
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
                    RaisePropertyChanged("SelectedSong");
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
                    RaisePropertyChanged("RealTime");

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
            set
            {
                if (description != value)
                {
                    description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        private bool pauseBool = false;
        public bool PauseBool
        {
            get => pauseBool;
            set
            {
                if(pauseBool != value)
                {
                    pauseBool = value;
                    RaisePropertyChanged("PauseBool");
                }
            }
        }

        private bool playBool = true;
        public bool PlayBool
        {
            get => playBool;
            set
            {
                if (playBool != value)
                {
                    playBool = value;
                    RaisePropertyChanged("PlayBool");
                }
            }
        }

        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand PreviousCommand { get; set; }

        private List<string> SongsPathList;

        public SongsViewModel()
        {
            LoadSongs();

            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);

            wmp.StatusChange += CheckStatus;
        }

        private void LoadSongs()
        {
            SongsPathList = xml.LoadSongs(SongsPathList);
            Songs = new ObservableCollection<Song>();

            foreach(string path in SongsPathList)
            {
                IWMPMedia song = wmp.newMedia(path);
                Songs.Add(new Song()
                {
                    Title = song.getItemInfo("Title"),
                    Artist = song.getItemInfo("Artist"),
                    Path = path,
                    Time = DateTime.Parse(song.durationString)
                });
            }
        }

        private void PlayPause(bool b)
        {
            PauseBool = b;
            PlayBool = !b;
        }

        public void Play()
        {
            if (string.IsNullOrEmpty(wmp.URL))
                return;

            wmp.controls.play();

            UpdateTime();

            PlayPause(true);
        }

        public void Pause()
        {
            wmp.controls.pause();
            PlayPause(false);
        }

        public void Stop()
        {
            wmp.controls.stop();
            PlayPause(false);
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
                for(i = 0; i <= wmp.controls.currentItem.duration; i++)
                {
                    RealTime = (int) ((wmp.controls.currentPosition * 100) / wmp.controls.currentItem.duration);
                    await Task.Delay(1000);
                }
            });
        }
    }
}