using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioFil
{
    public class RadioListener
    {
        public event EventHandler<StreamUpdateEventArgs> OnStreamUpdate;

        private Radio radio;

        private static readonly IReadOnlyCollection<string> metadataSongPatterns = new ReadOnlyCollection<string>(new string[]
        {
            "StreamTitle=\'(?<title>[^~]+?) / (?<artist>[^~]+?)\'",
            "StreamTitle=\'(?<title>[^~]+?) - (?<artist>[^~]+?)\'",
            "StreamTitle=\'(?<title>.+?)~(?<artist>.+?)~",
        });

        public RadioListener(Radio radio)
        {
            this.radio = radio;
            radio.OnMetadataChanged += UpdateCurrentSong;
        }

        public void UpdateCurrentSong(object sender, MetadataEventArgs args)
        {
            foreach (var metadataSongPattern in metadataSongPatterns)
            {
                Match match = Regex.Match(args.NewMetadata, metadataSongPattern);
                if (match.Success)
                {
                    radio.CurrentSong = new SongInfo(match.Groups["artist"].Value.Trim(), match.Groups["title"].Value.Trim());
                    return;
                }
            }
        }

        public void Start()
        {
            radio.RunningTask = Task.Run(() => GetHttpStream());
        }

        public void Stop()
        {
            radio.Running = false;
        }

        private void GetHttpStream()
        {
            do
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(radio.Url);
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

                            radio.Running = true;

                            while (radio.Running)
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
                                        radio.Metadata = metadataSb.ToString();
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
            }
            while (radio.Running);
        }

        private void ProcessStreamData(byte[] buffer, ref int offset, int length)
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
    }
}
