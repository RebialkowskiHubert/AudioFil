using System.Collections.ObjectModel;
using System.Linq;
using Tulpep.NotificationWindow;
using WMPLib;

namespace AudioFil
{
    public class RadioViewModel : PropChanged
    {
        private WindowsMediaPlayer wmp;

        private ObservableCollection<Radio> radios;

        private XMLHandling xml;

        public ObservableCollection<Radio> Radios
        {
            get => radios;
            set
            {   
                if(radios != value)
                {
                    radios = value;
                    RaisePropertyChanged("Radios");
                }
            }
        }

        private Radio oldRadio;

        private Radio selectedRadio;

        public Radio SelectedRadio
        {
            get => selectedRadio;
            set
            {
                if(selectedRadio != value)
                {
                    oldRadio = selectedRadio;
                    selectedRadio = value;
                    RaisePropertyChanged("SelectedRadio");
                    Play();
                }
            }
        }

        private string title = "Brak tytułu";

        public string Title
        {
            get => title;
            set
            {
                if(title != value)
                {
                    title = value;
                    if (string.IsNullOrEmpty(title))
                        title = "Brak tytułu";

                    RaisePropertyChanged("Title");
                }
            }
        }

        private string description = "Gotowy";

        public string Description
        {
            get => description;
            set
            {
                if(description != value)
                {
                    description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }


        public RelayCommand AddCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand PreviousCommand { get; set; }

        public RadioViewModel()
        {
            AddCommand = new RelayCommand(Add);
            PlayCommand = new RelayCommand(Play);
            StopCommand = new RelayCommand(Stop);
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);

            xml = new XMLHandling();
            Radios = xml.LoadRadios(Radios);

            wmp = new WindowsMediaPlayer();

            wmp.StatusChange += CheckStatus;
        }

        private void CheckStatus()
        {
            Description = wmp.status;
        }

        private void Add()
        {
            AddStationView av = new AddStationView();
            av.Show();

            av.Closed += (ss, ee) =>
            {
                Radios = xml.LoadRadios(Radios);
            };
        }

        private void Play()
        {
            wmp.controls.stop();
            if(oldRadio != null)
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
                        Image = Properties.Resources.info,
                        TitleText = SelectedRadio.NazwaStacja,
                        ContentText = Title
                    };
                    popup.Popup();
                });
            };
        }

        private void Stop()
        {
            wmp.controls.stop();

            SelectedRadio.Stop();
            Title = "";
        }

        private void Next()
        {
            int index = Radios.IndexOf(SelectedRadio) + 1;

            if(index < Radios.Count)
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
    }
}
