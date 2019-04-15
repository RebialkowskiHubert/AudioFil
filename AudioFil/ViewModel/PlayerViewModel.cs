using System;
using System.Windows;
using WMPLib;

namespace AudioFil
{
    public class PlayerViewModel : BindableBase
    {
        protected WindowsMediaPlayer wmp;

        protected XMLHandling xml;

        protected LowLevelKeyboardListener keyListener;

        protected bool play = false;

        public PlayerViewModel()
        {
            try
            {
                xml = new XMLHandling();

                wmp = new WindowsMediaPlayer();

                keyListener = new LowLevelKeyboardListener();

                App.Current.Exit += (ss, ee) => {
                    keyListener.UnHookKeyboard();
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
