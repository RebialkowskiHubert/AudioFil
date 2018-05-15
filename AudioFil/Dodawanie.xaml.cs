using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AudioFil
{
    /// <summary>
    /// Logika interakcji dla klasy Dodawanie.xaml
    /// </summary>
    public partial class Dodawanie : Window
    {
        public Dodawanie()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LogikaRadio lr = new LogikaRadio();
            lr.DodajRadio(textBox1.Text, textBox2.Text);

            MessageBox.Show("Stacja została dodana");
            this.Close();
        }
    }
}
