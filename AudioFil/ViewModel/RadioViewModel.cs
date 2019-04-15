using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tulpep.NotificationWindow;

namespace AudioFil
{
    public sealed class RadioViewModel : PlayerViewModel
    {
        private ObservableCollection<Radio> radios;
        public ObservableCollection<Radio> Radios
        {
            get => radios;
            set => SetProperty(ref radios, value, "Radios");
        }

        private Radio selectedRadio;
        public Radio SelectedRadio
        {
            get => selectedRadio;
            set
            {
                if (selectedRadio != value)
                {
                    selectedRadio = value;
                    OnPropertyChanged("SelectedRadio");
                    Play();
                    DeleteCommand.RaiseCanExecuteChanged();
                    UpdateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string title = "Brak tytułu";
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    if (string.IsNullOrEmpty(title))
                        title = "Brak tytułu";

                    OnPropertyChanged("Title");
                }
            }
        }

        private string description = "Gotowy";
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value, "Description");
        }

        public RelayCommand PlayCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand PreviousCommand { get; set; }
        public RelayCommand<Radio> AddCommand { get; set; }
        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }

        public RadioViewModel()
        {
            Radios = xml.LoadRadios(Radios);

            PlayCommand = new RelayCommand(Play);
            StopCommand = new RelayCommand(Stop);
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);
            AddCommand = new RelayCommand<Radio>(Add);
            UpdateCommand = new RelayCommand(Update, IsSelected);
            DeleteCommand = new RelayCommand(Delete, IsSelected);

            wmp.StatusChange += CheckStatus;

            keyListener.OnKeyPressed += OnKeyPressed;
            keyListener.HookKeyboard();

            InitRadioListener();
        }

        private void InitRadioListener()
        {
            foreach(Radio radio in Radios)
            {
                RadioListener listener = new RadioListener(radio);
                radio.OnMetadataChanged += listener.UpdateCurrentSong;
                listener.Start();

                radio.OnCurrentSongChanged += (ss, ee) =>
                {
                    App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        radio.CurrentSong.Artist = ee.NewSong.Artist;
                        radio.CurrentSong.Title = ee.NewSong.Title;
                    });
                };
            }
        }

        private void Play()
        {
            if (SelectedRadio == null)
            {
                if (Radios[0] != null)
                    SelectedRadio = Radios[0];
                else
                    return;
            }

            wmp.controls.stop();

            wmp.URL = SelectedRadio.Url.ToString();
            wmp.controls.play();

            SelectedRadio.OnCurrentSongChanged += (ss, ee) =>
            {
                Title = ee.NewSong.Artist + " - " + ee.NewSong.Title;

                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    PopupNotifier popup = new PopupNotifier
                    {
                        TitleText = SelectedRadio.Name,
                        ContentText = Title
                    };
                    popup.Popup();
                });
            };
        }

        private void Stop()
        {
            wmp.controls.stop();

            Title = "";

            SelectedRadio.Running = false;
        }

        private void Next()
        {
            int index = Radios.IndexOf(SelectedRadio) + 1;

            if (index < Radios.Count)
                SelectedRadio = Radios[index];
        }

        private void Previous()
        {
            int index = Radios.IndexOf(SelectedRadio) - 1;

            if (index >= 0)
                SelectedRadio = Radios[index];
            else
                SelectedRadio = Radios[Radios.Count - 1];
        }

        private bool IsSelected()
        {
            return SelectedRadio != null;
        }

        private void Add(Radio r = null)
        {
            AddStationView av = new AddStationView();

            if (r != null)
            {
                AddStationViewModel avm = new AddStationViewModel();
                avm.SetMode(true, SelectedRadio);
                avm.StationName = SelectedRadio.Name;
                avm.StationUrl = SelectedRadio.Url;
                av.DataContext = avm;
            }

            av.ShowDialog();

            av.Closed += (ss, ee) =>
            {
                Radios = xml.LoadRadios(Radios);
            };
        }

        private void Update()
        {
            Add(SelectedRadio);
        }

        private void Delete()
        {
            MessageBoxResult result = MessageBox.Show($"Czy na pewno chcesz usunąć {SelectedRadio.Name}?", "Usuń", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                xml.DeleteRadio(SelectedRadio);
                Radios.Remove(Radios.Where(r => r == SelectedRadio).FirstOrDefault());
            }
        }

        private void OnKeyPressed(object sender, KeyPressedArgs e)
        {
            switch (e.KeyPressed)
            {
                case Key.MediaPlayPause:
                    if (play)
                        Stop();
                    else
                        Play();

                    play = !play;
                    break;

                case Key.MediaNextTrack:
                    Next();
                    break;

                case Key.MediaPreviousTrack:
                    Previous();
                    break;
            }
        }

        private void CheckStatus()
        {
            Description = wmp.status;
        }
    }
}
