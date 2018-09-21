using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tulpep.NotificationWindow;

namespace AudioFil
{
    public sealed class RadioViewModel : PlayerViewModel, IPlayer
    {
        private ObservableCollection<Radio> radios;
        public ObservableCollection<Radio> Radios
        {
            get => radios;
            set => SetProperty(ref radios, value, "Radios");
        }

        private Radio oldRadio;

        private Radio selectedRadio;
        public Radio SelectedRadio
        {
            get => selectedRadio;
            set
            {
                if (selectedRadio != value)
                {
                    oldRadio = selectedRadio;
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

            listener.OnKeyPressed += OnKeyPressed;
            listener.HookKeyboard();
        }

        public void Play()
        {
            if (SelectedRadio == null)
            {
                if (Radios[0] != null)
                    SelectedRadio = Radios[0];
                else
                    return;
            }

            wmp.controls.stop();
            if (oldRadio != null)
                oldRadio.Stop();

            wmp.URL = SelectedRadio.Url;
            wmp.controls.play();

            SelectedRadio.Start();
            SelectedRadio.OnCurrentSongChanged += (ss, ee) =>
            {
                Title = ee.NewSong.Artist + " - " + ee.NewSong.Title;

                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    PopupNotifier popup = new PopupNotifier
                    {
                        TitleText = SelectedRadio.NazwaStacja,
                        ContentText = Title
                    };
                    popup.Popup();
                });
            };
        }

        public void Stop()
        {
            wmp.controls.stop();

            SelectedRadio.Stop();
            Title = "";
        }

        public void Next()
        {
            int index = Radios.IndexOf(SelectedRadio) + 1;

            if (index < Radios.Count)
                SelectedRadio = Radios[index];
        }

        public void Previous()
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
                avm.StationName = SelectedRadio.NazwaStacja;
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
            MessageBoxResult result = MessageBox.Show($"Czy na pewno chcesz usunąć {SelectedRadio.NazwaStacja}?", "Usuń", MessageBoxButton.YesNo);
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
