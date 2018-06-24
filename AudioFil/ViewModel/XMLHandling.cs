using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace AudioFil
{
    public class XMLHandling
    {
        private XmlDocument doc;
        private readonly string pathRadio = "Playlista.xml";
        private readonly string pathSongs = @"C:\Users\huber\Music\Listy odtwarzania\MUZA.wpl";

        public ObservableCollection<Radio> LoadRadios(ObservableCollection<Radio> radios)
        {
            doc = new XmlDocument();
            try
            {
                doc.Load(pathRadio);
                int stacje = doc.GetElementsByTagName("Stacja").Count;
                radios = new ObservableCollection<Radio>();

                int i;
                for (i = 0; i < stacje; i++)
                {
                    XmlNode stacja = doc.GetElementsByTagName("Stacja").Item(i);
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
                nazwa.InnerText = r.NazwaStacja;
                XmlNode url = doc.CreateNode(XmlNodeType.Element, "Url", null);
                url.InnerText = r.Url;

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
                    if (node.FirstChild.InnerText.Equals(old.IdStacja.ToString()))
                    {
                        node.FirstChild.NextSibling.InnerText = newr.NazwaStacja;
                        node.LastChild.InnerText = newr.Url;
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
                xDoc.Root.Elements("Stacja").Elements("Id").Where(stat => stat.Value == r.IdStacja.ToString()).Select(stat => stat.Parent).Remove();
                xDoc.Save(pathRadio);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<string> LoadSongs(List<string> songs)
        {
            try
            {
                XDocument xDoc = XDocument.Load(pathSongs);

                songs = new List<string>();

                List<XAttribute> srcsX = xDoc.Descendants().SelectMany(s => s.Attributes()).Where(a => a.Name.LocalName == "src").ToList();
                List<string> srcs = srcsX.ConvertAll(delegate (XAttribute attr)
                {
                    return attr.ToString();
                });
                foreach(string src in srcs)
                {
                    string s = src.Replace("src=", "");
                    s = s.Replace("\"", "");
                    s = s.Replace("\\", "/");

                    songs.Add(s);
                }                

                return songs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
