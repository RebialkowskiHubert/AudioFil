using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioFil
{
    public class Radio
    {
        bool _running;
        Task runningTask;

        string _metadata;
        public event EventHandler<MetadataEventArgs> OnMetadataChanged;

        SongInfo _currentSong;
        public event EventHandler<CurrentSongEventArgs> OnCurrentSongChanged;

        public event EventHandler<StreamUpdateEventArgs> OnStreamUpdate;

        IntPtr _disposed = IntPtr.Zero;

        static readonly ReadOnlyCollection<string> metadataSongPatterns = new ReadOnlyCollection<string>(new string[]
        {
            "StreamTitle=\'(?<title>[^~]+?) / (?<artist>[^~]+?)\'",
            "StreamTitle=\'(?<title>[^~]+?) - (?<artist>[^~]+?)\'",
            "StreamTitle=\'(?<title>.+?)~(?<artist>.+?)~",
        });

        public int IdStacja { get; set; }

        public string NazwaStacja { get; set; }

        public string Url { get; set; }

        public bool Running
        {
            get =>_running;
            set
            {
                _running = value;
                if (!_running && runningTask != null)
                    runningTask.Wait();
            }
        }

        public string Metadata
        {
            get =>_metadata;
            set
            {
                OnMetadataChanged?.Invoke(this, new MetadataEventArgs(_metadata, value));

                _metadata = value;
            }
        }

        public SongInfo CurrentSong
        {
            get =>_currentSong;
            set
            {
                //if(OnCurrentSongChanged!=null){OnCurrentSongChanged=(this, new...);} to samo co OnCurrentSongChanged?.Invoke
                OnCurrentSongChanged?.Invoke(this, new CurrentSongEventArgs(_currentSong, value));

                _currentSong = value;
            }
        }

        public Radio(int idStacja, string nazwaStacja, string url)
        {
            IdStacja = idStacja;
            NazwaStacja = nazwaStacja;
            Url = url;

            OnMetadataChanged += UpdateCurrentSong;
        }

        public void UpdateCurrentSong(object sender, MetadataEventArgs args)
        {
            foreach (var metadataSongPattern in metadataSongPatterns)
            {
                Match match = Regex.Match(args.NewMetadata, metadataSongPattern);
                if (match.Success)
                {
                    CurrentSong = new SongInfo(match.Groups["artist"].Value.Trim(), match.Groups["title"].Value.Trim());
                    return;
                }
            }
        }

        public void Start(string pluginPath = null)
        {
            runningTask = Task.Run(() => GetHttpStream());
        }

        public void Stop()
        {
            Running = false;
        }

        void GetHttpStream()
        {
            do
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Headers.Add("icy-metadata", "1");
                    request.ReadWriteTimeout = 10 * 1000;
                    request.Timeout = 10 * 1000;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        //get the position of metadata
                        int metaInt = 0;
                        if (!string.IsNullOrEmpty(response.GetResponseHeader("icy-metaint")))
                            metaInt = Convert.ToInt32(response.GetResponseHeader("icy-metaint"));
                        using (Stream socketStream = response.GetResponseStream())
                        {
                            byte[] buffer = new byte[16384];
                            int metadataLength = 0;
                            int streamPosition = 0;
                            int bufferPosition = 0;
                            int readBytes = 0;
                            StringBuilder metadataSb = new StringBuilder();

                            Running = true;

                            while (Running)
                            {
                                if (bufferPosition >= readBytes)
                                {
                                    readBytes = socketStream.Read(buffer, 0, buffer.Length);
                                    bufferPosition = 0;
                                }
                                if (readBytes <= 0)
                                {
                                    break;
                                }

                                if (metadataLength == 0)
                                {
                                    if (metaInt == 0 || streamPosition + readBytes - bufferPosition <= metaInt)
                                    {
                                        streamPosition += readBytes - bufferPosition;
                                        ProcessStreamData(buffer, ref bufferPosition, readBytes - bufferPosition);
                                        continue;
                                    }

                                    ProcessStreamData(buffer, ref bufferPosition, metaInt - streamPosition);
                                    metadataLength = Convert.ToInt32(buffer[bufferPosition++]) * 16;
                                    //check if there's any metadata, otherwise skip to next block
                                    if (metadataLength == 0)
                                    {
                                        streamPosition = Math.Min(readBytes - bufferPosition, metaInt);
                                        ProcessStreamData(buffer, ref bufferPosition, streamPosition);
                                        continue;
                                    }
                                }

                                //get the metadata and reset the position
                                while (bufferPosition < readBytes)
                                {
                                    metadataSb.Append(Convert.ToChar(buffer[bufferPosition++]));
                                    metadataLength--;
                                    if (metadataLength == 0)
                                    {
                                        Metadata = metadataSb.ToString();
                                        metadataSb.Clear();
                                        streamPosition = Math.Min(readBytes - bufferPosition, metaInt);
                                        ProcessStreamData(buffer, ref bufferPosition, streamPosition);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            } while (Running);
        }

        void ProcessStreamData(byte[] buffer, ref int offset, int length)
        {
            if (length < 1)
                return;
            if (OnStreamUpdate != null)
            {
                byte[] data = new byte[length];
                Buffer.BlockCopy(buffer, offset, data, 0, length);
                OnStreamUpdate(this, new StreamUpdateEventArgs(data));
            }
            offset += length;
        }

        public static implicit operator string(Radio v)
        {
            throw new NotImplementedException();
        }
    }

    public class SongInfo
    {
        public string Artist { get; set; }
        public string Title { get; set; }

        public SongInfo(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }
    }

    public class MetadataEventArgs : EventArgs
    {
        public string OldMetadata { get; set; }
        public string NewMetadata { get; set; }

        public MetadataEventArgs(string oldMetadata, string newMetadata)
        {
            OldMetadata = oldMetadata;
            NewMetadata = newMetadata;
        }
    }

    public class CurrentSongEventArgs : EventArgs
    {
        public SongInfo OldSong { get; set; }
        public SongInfo NewSong { get; set; }

        public CurrentSongEventArgs(SongInfo oldSong, SongInfo newSong)
        {
            OldSong = oldSong;
            NewSong = newSong;
        }
    }

    public class StreamUpdateEventArgs : EventArgs
    {
        public byte[] Data { get; set; }

        public StreamUpdateEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
