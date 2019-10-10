namespace AudioFil
{
    public class DownloadMedia : BindableBase
    {
        private string url;
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value, nameof(Url));
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }

        private int progress;
        public int Progress
        {
            get => progress;
            set => SetProperty(ref progress, value, nameof(Progress));
        }

        private string strProgress;
        public string StrProgress
        {
            get => strProgress;
            set => SetProperty(ref strProgress, value, nameof(StrProgress));
        }

    }
}
