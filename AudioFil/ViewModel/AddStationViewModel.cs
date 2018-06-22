using System.Windows;

namespace AudioFil
{
    public class AddStationViewModel : PropChanged
    {
        private string stationName;
        private bool edit;
        public bool Edit
        {
            get => edit;
            set
            {
                edit = value;
                RaisePropertyChanged("Edit");
            }
        }


        private bool add = true;
        public bool Add
        {
            get => add;
            set
            {
                add = value;
                RaisePropertyChanged("Add");
            }
        }

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
        public RelayCommand<Window> UpdateCommand { get; set; }
        public RelayCommand<Window> CloseCommand { get; set; }

        private Radio oldRadio;

        public void SetMode(bool mode, Radio old)
        {
            Edit = mode;
            Add = !mode;
            oldRadio = old;
        }

        public AddStationViewModel()
        {
            SaveCommand = new RelayCommand<Window>(Save);
            UpdateCommand = new RelayCommand<Window>(Update);
            CloseCommand = new RelayCommand<Window>(Close);
        }

        private void Save(Window w)
        {
            XMLHandling xml = new XMLHandling();
            xml.AddRadio(new Radio(0, StationName, StationUrl));

            MessageBox.Show("Stacja została dodana");
            Close(w);
        }

        private void Update(Window w)
        {
            XMLHandling xml = new XMLHandling();
            xml.UpdateRadio(oldRadio, new Radio(0, StationName, StationUrl));

            MessageBox.Show("Stacja została zmodyfikowana");
            Close(w);
        }

        private void Close(Window w)
        {
            w.Close();
        }
    }
}
