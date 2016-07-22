using PiBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using TouchPanels.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace pibot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    
    public sealed partial class MainPage : Page
    {

        private CmdServer udpServer; // tcp server
        private Speech voice;
        string[] welcome = new string[] { "Hello i am Marvin", "I see you baby shaking that ass shaking that ass", "Hey Good looking what you got cooking","ok bored now","whats up doc","dum dum ","Just One Core net oh Give it to me deliciuos ice scream you scream we all scream together" };
        int welcomeCount=0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            audio.Volume = 0.6;
            voice = new Speech(audio);
            voice.say(welcome[welcomeCount++]);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20);
            timer.Tick += Timer_Tick;
            timer.Start();
            
            base.OnNavigatedTo(e);
        }

        private void Timer_Tick(object sender, object e)
        {
            voice.say(welcome[welcomeCount++]);
            if (welcomeCount > 6)
            {
                welcomeCount = 0;
            }
            
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
           
        }

       
       
    }
}
