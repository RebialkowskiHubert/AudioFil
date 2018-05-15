using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tulpep.NotificationWindow;

namespace AudioFil
{
    public partial class MainWindow : Window
    {
        LogikaRadio lr;

        public MainWindow()
        {
            InitializeComponent();
            lr = new LogikaRadio();
            LadujStacje();
        }

        private async void LadujStacje()
        {
            await lr.PolaczBaza();
            lstItems.ItemsSource = lr.GetRadia().DefaultView;
            gridItems.Columns[1].Width = 0;
        }

        private async void Odswiez()
        {
            for(int i=0; i<15; i++)
            {
                tbStatus.Text = lr.SprawdzStat();
                await Task.Delay(1000);
            }
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            lr.Stop();
            tbStatus.Text = lr.SprawdzStat();
        }

        private void LstItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView dg = (ListView)sender;
            DataRowView wiersz = dg.SelectedItem as DataRowView;
            if (wiersz != null)
            {
                tbRadio.Text = wiersz["NazwaStacji"].ToString();
                string url = wiersz["Url"].ToString();
                lr.Graj(url);
                Odswiez();
                Play(url, tbRadio.Text);
            }
        }

        private void DodajStacje(object sender, RoutedEventArgs e)
        {
            Dodawanie okno = new Dodawanie();
            okno.ShowDialog();
            LadujStacje();
        }

        private void Play(string url, string stacja)
        {
            Radio radio = new Radio(1, "", url);
            radio.Start();
            radio.OnCurrentSongChanged += (ss, ee) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    string message = ee.NewSong.Artist + " - " + ee.NewSong.Title;
                    tbTytul.Text = message;

                    PopupNotifier popup = new PopupNotifier();
                    popup.Image = Properties.Resources.info;
                    popup.TitleText = stacja;
                    popup.ContentText = message;
                    popup.Popup();
                });
            };
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            lr.Zamknij();
        }
    }
}
