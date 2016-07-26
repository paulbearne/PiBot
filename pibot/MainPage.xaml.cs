using BratPiBot;
using PiBot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using TouchPanels.Devices;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
   enum controlState
    {
        POWERON = 0,
        POWERON_WAIT,
        MAESTRO_CONNECT,
        MAESTRO_CONNECT_WAIT,
        CONFIG_SERVOS,
        SERVOS_CONNECT,
        CONFIG_SERVOS_WAIT,
        CONNECT_REMOTE,
        CONNECT_REMOTE_WAIT,
        CONTROL_SYSTEM_READY,
        RUNNING

    }

    public sealed partial class MainPage : Page
    {

       // private CmdServer udpServer; // tcp server
        private Speech voice;
        private Maestro servoCtrl;
        private int[] servoPositions = new int[24];
        private controlState systemState = controlState.POWERON;
        bool moveNextStep = false;
        bool audioComplete = false;
        string[] welcome = new string[] { "Powering Up", "Control sytem ready", "Hey Good looking what you got cooking","ok bored now","whats up doc","dum dum ","Just One Core net oh Give it to me deliciuos ice scream you scream we all scream together" };
        int welcomeCount=0;
        
      
        public MainPage()
        {
            this.InitializeComponent();
            


        }


        // main application control loop
        private void controlLoop()
        {
             switch (systemState)
               {
                    case controlState.POWERON:
                        audio.Volume = 0.6;
                        voice = new Speech(audio);
                        voice.OnComplete += Voice_OnComplete;
                        voice.say("Powering Up");
                        systemState = controlState.POWERON_WAIT;
                        break;
                    case controlState.POWERON_WAIT:
                        if (moveNextStep && audioComplete)
                            systemState = controlState.CONFIG_SERVOS;
                        break;
                    case controlState.CONFIG_SERVOS:
                        voice.say("Starting Motor Control System");
                        servoCtrl = new Maestro();
                        servoCtrl.OnConnected += ServoCtrl_OnConnected;
                        moveNextStep = false;
                        audioComplete = false;
                        systemState = controlState.SERVOS_CONNECT;
                        break;
                    case controlState.SERVOS_CONNECT:
                        if (servoCtrl.commandPortReady())
                        {
                           servoCtrl.connect();
                           systemState = controlState.CONFIG_SERVOS_WAIT;
                        }
                        break;
                    case controlState.CONFIG_SERVOS_WAIT:
                        
                        if (moveNextStep && audioComplete)
                            systemState = controlState.CONNECT_REMOTE;
                        break;
                    case controlState.CONNECT_REMOTE:
                        voice.say("Searching for Remote Computer");
                        moveNextStep = true;  // we don't have this implemented yet so just wait for the audio
                        audioComplete = false;
                        systemState = controlState.CONNECT_REMOTE_WAIT;
                        break;
                    case controlState.CONNECT_REMOTE_WAIT:
                        if (moveNextStep && audioComplete)
                            systemState = controlState.CONTROL_SYSTEM_READY;
                        break;
                    case controlState.CONTROL_SYSTEM_READY:
                        voice.say("System Check Completed");
                        moveNextStep = false;
                        audioComplete = false;
                        systemState = controlState.RUNNING;
                        break;
                    case controlState.RUNNING:
                        break;

                }

            
        }

        private void Voice_OnComplete()
        {
            audioComplete = true;
        }

      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            moveNextStep = true;       
                    
           DispatcherTimer timer = new DispatcherTimer();
            //audio.IsMuted = true;
           timer.Interval = TimeSpan.FromMilliseconds(100);
           timer.Tick += Timer_Tick;
           timer.Start();
            
            base.OnNavigatedTo(e);

        }

        private void ServoCtrl_OnConnected()
        {
            moveNextStep = true;
            // servo controller connected and ready for data
            // home the servos and then enter our control loop
            
         }

        private void Timer_Tick(object sender, object e)
        {
            controlLoop();
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

        }

        private void btnComms_Click(object sender, RoutedEventArgs e)
        {
            
            
        }
    }
}
