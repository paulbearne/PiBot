using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPBiped
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ControlPage : Page
    {
        private bool connected = false;
        public ControlPage()
        {
            this.InitializeComponent();
        }

        private async void btnPower_Click(object sender, RoutedEventArgs e)
        {
            if (connected == false)
            {
                // connect to maestro command port if more than one command port connect to first
                if (Variables.maestroPorts.Count <= 0)
                {
                    Variables.maestroPorts = await Variables.maestro.locateDevices();
                }
                for (int i = 0; i < Variables.maestroPorts.Count; i++)
                {
                    // look for first command port
                    if (Variables.maestroPorts[i].portName.Contains("Command") == true)
                    {
                        Variables.maestro.closeActivePort();
                        Variables.maestro.connect(Variables.maestroPorts[i].portName);
                        connected = true;
                    }
                }
            }
            else
            {
                Variables.maestro.closeActivePort();
                Variables.maestro.cleanup();
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDance_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnKick_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
