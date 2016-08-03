using Windows.UI.Xaml.Controls;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Command;
using System;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPBiped
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool firstRun = true;
        
        public MainPage()
        {
            this.InitializeComponent();
            firstRun = true;
            slVolume.Value = Variables.config.volume;
            Variables.voice = new Speech(audio);
            Variables.voice.OnComplete += Voice_OnComplete;
            
            

        }

        private void Voice_OnComplete()
        {
            
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            audio.Volume = slVolume.Value / 100;
            if (firstRun == false)
            {
                Variables.config.volume = (UInt16)(slVolume.Value);
            }
            firstRun = false;
        }

        private void btnExit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Variables.config.Save();
            App.Current.Exit();
        }
    }
}
