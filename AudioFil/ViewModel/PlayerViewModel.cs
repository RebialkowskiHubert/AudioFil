using WMPLib;

namespace AudioFil
{
    public class PlayerViewModel : PropChanged
    {
        protected WindowsMediaPlayer wmp;

        protected XMLHandling xml;

        protected LowLevelKeyboardListener listener;

        protected bool play = false;

        public PlayerViewModel()
        {
            xml = new XMLHandling();

            wmp = new WindowsMediaPlayer();

            listener = new LowLevelKeyboardListener();

            App.Current.Exit += (ss, ee) => {
                listener.UnHookKeyboard();
            };
        }
    }
}
