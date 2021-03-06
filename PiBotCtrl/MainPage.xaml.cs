﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TouchPanels.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiBotCtrl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        const string CalibrationFilename = "TSC2046";
        private Tsc2046 tsc2046;
        private TouchPanels.TouchProcessor processor;
        private Point lastPosition = new Point(double.NaN, double.NaN);
        private CmdServer udpServer; // tcp server
        private Speech voice;

        public MainPage()
        {
            this.InitializeComponent();
                       
        }
    
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            voice = new Speech(audio);
            initTouch();
            startServer();
            Status.Text = "Controller Initialised waiting for connection";
            voice.say("Marvin Controller ready so get a move on and connect to me");
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Status.Text = "Checking Processor";
            if (processor != null)
            {
                Status.Text = "Processor ok";
                //Unhooking from all the touch events, will automatically shut down the processor.
                //Remember to do this, or you view could be staying in memory.
                processor.PointerDown -= Processor_PointerDown;
                processor.PointerMoved -= Processor_PointerMoved;
                processor.PointerUp -= Processor_PointerUp;
            }
            base.OnNavigatingFrom(e);
        }

        private async void initTouch()
        {
            tsc2046 = await TouchPanels.Devices.Tsc2046.GetDefaultAsync();
            try
            {
                await tsc2046.LoadCalibrationAsync(CalibrationFilename);
            }
            catch (System.IO.FileNotFoundException)
            {
                await CalibrateTouch(); //Initiate calibration if we don't have a calibration on file
            }
            catch (System.UnauthorizedAccessException)
            {
                //No access to documents folder
                Status.Text ="Make sure the application manifest specifies access to the documents folder and declares the file type association for the calibration file.";
                throw;
            }
            //Load up the touch processor and listen for touch events
            processor = new TouchPanels.TouchProcessor(tsc2046);
            processor.PointerDown += Processor_PointerDown;
            processor.PointerMoved += Processor_PointerMoved;
            processor.PointerUp += Processor_PointerUp;
        }

        IScrollProvider currentScrollItem;

        private void Processor_PointerDown(object sender, TouchPanels.PointerEventArgs e)
        {
            //Status.Text = "Pointer Down";
          //  WriteStatus(e, "Down");
            currentScrollItem = FindElementsToInvoke(e.Position);
            lastPosition = e.Position;
        }
        private void Processor_PointerMoved(object sender, TouchPanels.PointerEventArgs e)
        {
           // WriteStatus(e, "Moved");
            lastPosition = e.Position;
        }

        private void Processor_PointerUp(object sender, TouchPanels.PointerEventArgs e)
        {
           // WriteStatus(e, "Up");
           currentScrollItem = null;
        }
        private void WriteStatus(TouchPanels.PointerEventArgs args, string type)
        {
           // Status.Text = $"{type}\nPosition: {args.Position.X},{args.Position.Y}\nPressure:{args.Pressure}";
        }

        private async void btnCal_Click(object sender, RoutedEventArgs e)
        {
            await CalibrateTouch();
        }


        private bool _isCalibrating = false; //flag used to ignore the touch processor while calibrating
        private async Task CalibrateTouch()
        {
            _isCalibrating = true;
            var calibration = await TouchPanels.UI.LcdCalibrationView.CalibrateScreenAsync(tsc2046);
            _isCalibrating = false;
            tsc2046.SetCalibration(calibration.A, calibration.B, calibration.C, calibration.D, calibration.E, calibration.F);
            try
            {
                await tsc2046.SaveCalibrationAsync(CalibrationFilename);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void btnDance_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Oh No i think i broke it";
            voice.say(Status.Text);


        }
       
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Up Click";
            voice.say(Status.Text);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Down Click";
            voice.say(Status.Text);

        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Left Click";
            voice.say(Status.Text);

        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Right Click";
            voice.say(Status.Text);

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Ok Click";
            voice.say(Status.Text);

        }

        private void btnKick_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Kick Click";
            voice.say("Oh be quiey t women i am having fun which is not easy for a depressed robot");

        }

        private void btnPower_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Power Click";
            voice.say(Status.Text);
        }

        private IScrollProvider FindElementsToInvoke(Point screenPosition)
        {
            if (_isCalibrating) return null;

            var elements = VisualTreeHelper.FindElementsInHostCoordinates(new Windows.Foundation.Point(screenPosition.X, screenPosition.Y), this, false);
            //Search for buttons in the visual tree that we can invoke
            //If we can find an element button that implements the 'Invoke' automation pattern (usually buttons), we'll invoke it
            foreach (var e in elements.OfType<FrameworkElement>())
            {
                var element = e;
                AutomationPeer peer = null;
                object pattern = null;
                while (true)
                {
                    peer = FrameworkElementAutomationPeer.FromElement(element);
                    if (peer != null)
                    {
                        pattern = peer.GetPattern(PatternInterface.Invoke);
                        if (pattern != null)
                        {
                            break;
                        }
                        pattern = peer.GetPattern(PatternInterface.Scroll);
                        if (pattern != null)
                        {
                            break;
                        }
                    }
                    var parent = VisualTreeHelper.GetParent(element);
                    if (parent is FrameworkElement)
                        element = parent as FrameworkElement;
                    else
                        break;
                }
                if (pattern != null)
                {
                    var p = pattern as Windows.UI.Xaml.Automation.Provider.IInvokeProvider;
                    p?.Invoke();
                    return pattern as IScrollProvider;
                }
            }
            return null;
        }

        

        private void startServer()
        {
            udpServer = new CmdServer(9000);
            Status.Text = "udp Server on port 9000";
            udpServer.OnDataReceived += udpServer_OnDataReceived;
            Status.Text = "udp Server event handlers ready";
           // tcpServer.Start();
           
        }

        private void udpServer_OnDataReceived(string data)
        {
            Status.Text = data; // client sends back command + ok

        }

        private void udpServer_OnError(string message)
        {
            Status.Text = message;
            voice.say("oh no somethings broken"+Status.Text);
        }

    }
}
