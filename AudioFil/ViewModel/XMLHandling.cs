using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows;

namespace AudioFil
{
    public class XMLHandling
    {
        private XmlDocument doc;
        private readonly string pathRadio = "Playlista.xml";
        private readonly string pathSongs = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\" + "Listy odtwarzania\\MUZA.wpl";

        private void CreateRadioPlaylistFile()
        {
            using(StreamWriter sw = new StreamWriter(pathRadio))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<AudioFil>");
                sw.WriteLine("</AudioFil>");
            }
        }

        private void CreateSongsPlaylistFile()
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\" + "Listy odtwarzania");
            using (StreamWriter sw = new StreamWriter(pathSongs))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<?wpl version=\"1.0\"?>\n<smil>\n<head>\n<meta name=\"Generator\" content=\"Microsoft Windows Media Player -- 12.0.17134.48\"/>\n<meta name=\"ItemCount\" content=\"146\"/>\n<meta name=\"IsFavorite\"/>\n<meta name=\"ContentPartnerListID\"/>\n<meta name=\"ContentPartnerNameType\"/>\n<meta name=\"ContentPartnerName\"/>\n<meta name=\"Subtitle\"/>\n<author/>\n<title>MUZA</title>\n</head>\n<body>\n<seq>\n</seq>\n</body>\n</smil>");
            }
        }

        public ObservableCollection<Radio> LoadRadios(ObservableCollection<Radio> radios)
        {
            doc = new XmlDocument();
            try
            {
                if (!File.Exists(pathRadio))
                    CreateRadioPlaylistFile();

                if (!File.Exists(pathSongs))
                    CreateSongsPlaylistFile();

                doc.Load(pathRadio);
                int stacje = doc.GetElementsByTagName("Stacja").Count;
                radios = new ObservableCollection<Radio>();

                int i;
                for (i = 0; i < stacje; i++)
                {
                    XmlNode stacja = doc.GetElementsByTagName("Stacja").Item(i);
                    var id = Int32.Parse(stacja.FirstChild.InnerText);
                    var nazwa = stacja.LastChild.PreviousSibling.InnerText;
                    var url = new Uri(stacja.LastChild.InnerText);
                    radios.Add(new Radio(id, nazwa, url));
                }

                return radios;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void AddRadio(Radio r)
        {
            doc = new XmlDocument();
            try
            {
                doc.Load(pathRadio);
                int count = (doc.GetElementsByTagName("Stacja").Count) + 1;

                XmlNode root = doc.SelectSingleNode("AudioFil");
                XmlNode stacja = doc.CreateNode(XmlNodeType.Element, "Stacja", null);
                XmlNode id = doc.CreateNode(XmlNodeType.Element, "Id", null);
                id.InnerText = count.ToString();
                XmlNode nazwa = doc.CreateNode(XmlNodeType.Element, "Nazwa", null);
                nazwa.InnerText = r.Name;
                XmlNode url = doc.CreateNode(XmlNodeType.Element, "Url", null);
                url.InnerText = r.Url.ToString();

                stacja.AppendChild(id);
                stacja.AppendChild(nazwa);
                stacja.AppendChild(url);
                root.AppendChild(stacja);

                doc.Save(pathRadio);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateRadio(Radio old, Radio newr)
        {
            doc = new XmlDocument();

            try
            {
                doc.Load(pathRadio);

                foreach(XmlNode node in doc.GetElementsByTagName("Stacja"))
                {
                    if (node.FirstChild.InnerText.Equals(old.Id.ToString()))
                    {
                        node.FirstChild.NextSibling.InnerText = newr.Name;
                        node.LastChild.InnerText = newr.Url.ToString();
                        break;
                    }
                }

                doc.Save(pathRadio);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteRadio(Radio r)
        {
            try
            {
                XDocument xDoc = XDocument.Load(pathRadio);
                xDoc.Root.Elements("Stacja").Elements("Id").Where(stat => stat.Value == r.Id.ToString()).Select(stat => stat.Parent).Remove();
                xDoc.Save(pathRadio);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AddSong(string songPath)
        {
            XDocument xdoc = XDocument.Load(pathSongs);

            xdoc.Root.Element("body").Element("seq").Add
                (
                    new XElement("media", new XAttribute("src", songPath))
                );

            xdoc.Save(pathSongs);
        }
    }
}
