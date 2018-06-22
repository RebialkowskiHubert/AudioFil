using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Tulpep.NotificationWindow;
using WMPLib;

namespace AudioFil
{
    public class RadioViewModel : PropChanged
    {
        private WindowsMediaPlayer wmp;

        private ObservableCollection<Radio> radios;

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


        public RelayCommand PlayCommand { get; set; }
        public RelayCommand StopCommand { get; set; }

        public RadioViewModel()
        {
            PlayCommand = new RelayCommand(Play);
            StopCommand = new RelayCommand(Stop);

            XMLHandling xml = new XMLHandling();
            radios = xml.LoadRadios("Playlista.xml", radios);

            wmp = new WindowsMediaPlayer();
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

                PopupNotifier popup = new PopupNotifier();
                popup.Image = Properties.Resources.info;
                popup.TitleText = SelectedRadio.NazwaStacja;
                popup.ContentText = Title;
                popup.Popup();
            };

            CheckStatus();
        }

        private void CheckStatus()
        {
            for (int i = 0; i < 15; i++)
            {
                Description = wmp.status;
                Task.Delay(1000);
            }
        }

        private void Stop()
        {
            wmp.controls.stop();
        }
    }
}
