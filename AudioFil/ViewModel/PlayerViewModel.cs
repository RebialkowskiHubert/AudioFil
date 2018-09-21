using System;
using System.Windows;
using WMPLib;

namespace AudioFil
{
    public class PlayerViewModel : BindableBase
    {
        protected WindowsMediaPlayer wmp;

        protected XMLHandling xml;

        protected LowLevelKeyboardListener listener;

        protected bool play = false;

        public PlayerViewModel()
        {
            try
            {
                xml = new XMLHandling();

                wmp = new WindowsMediaPlayer();

                listener = new LowLevelKeyboardListener();

                App.Current.Exit += (ss, ee) => {
                    listener.UnHookKeyboard();
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
