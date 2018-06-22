using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace AudioFil
{
    public class XMLHandling
    {
        private XmlDocument doc;
        private XmlNode stacja;

        public ObservableCollection<Radio> LoadRadios(string path, ObservableCollection<Radio> radios)
        {
            doc = new XmlDocument();
            try
            {
                doc.Load(path);
                int stacje = doc.GetElementsByTagName("Stacja").Count;
                radios = new ObservableCollection<Radio>();

                int i;
                for (i = 0; i < stacje; i++)
                {
                    stacja = doc.GetElementsByTagName("Stacja").Item(i);
                    var id = Int32.Parse(stacja.FirstChild.InnerText);
                    var nazwa = stacja.LastChild.PreviousSibling.InnerText;
                    var url = stacja.LastChild.InnerText;
                    radios.Add(new Radio(id, nazwa, url));
                }

                return radios;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
