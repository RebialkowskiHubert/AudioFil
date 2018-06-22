using System.Windows;

namespace AudioFil
{
    public class AddStationViewModel : PropChanged
    {
        private string stationName;

        public string StationName
        {
            get => stationName;
            set
            {
                if(stationName != value)
                {
                    stationName = value;
                    RaisePropertyChanged("StationName");
                }
            }
        }


        private string stationUrl;

        public string StationUrl
        {
            get => stationUrl;
            set
            {
                if(stationUrl != value)
                {
                    stationUrl = value;
                    RaisePropertyChanged("StationUrl");
                }
            }
        }

        public RelayCommand<Window> SaveCommand { get; set; }
        public RelayCommand<Window> CloseCommand { get; set; }


        public AddStationViewModel()
        {
            SaveCommand = new RelayCommand<Window>(Save);
            CloseCommand = new RelayCommand<Window>(Close);
        }

        private void Save(Window w)
        {
            XMLHandling xml = new XMLHandling();
            xml.AddRadio(new Radio(0, StationName, StationUrl));

            MessageBox.Show("Stacja została dodana");
            Close(w);
        }

        private void Close(Window w)
        {
            w.Close();
        }
    }
}
