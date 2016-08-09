using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UWPBiped
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigurationPage : Page
    {
        
        
        
        private MediaCapture headCapture;
       // bool commsConnected = false;
        //private UInt32 bytesRead;
        bool calibrating = false;        
       
        public ConfigurationPage()
        {
            
            this.InitializeComponent();
            controlState(false);
            // should already have a list of ports 
            if (Variables.maestroPorts.Count <= 0)
            {
                listMaestroPorts();
            }
            else
            {
                ComportListSource.Source = Variables.maestroPorts;
            }
            btnCal.Unchecked += BtnCal_Unchecked;
            btnSaveCalData.Click += BtnSaveCalData_Click;

            


        }


        private void setServoStartPosition()
        {
            // set sliders to calibration settings at start up
            accelerationLeftAnkle.Value = Variables.config.servocal.leftankle.acceleration;
            accelerationLeftLeg.Value = Variables.config.servocal.leftleg.acceleration;
            accelerationLeftHip.Value = Variables.config.servocal.lefthip.acceleration;
            accelerationRightAnkle.Value = Variables.config.servocal.rightankle.acceleration;
            accelerationRightLeg.Value = Variables.config.servocal.rightleg.acceleration;
            accelerationRightHip.Value = Variables.config.servocal.righthip.acceleration;
            speedLeftAnkle.Value = Variables.config.servocal.leftankle.speed;
            speedLeftLeg.Value = Variables.config.servocal.leftleg.speed;
            speedLeftHip.Value = Variables.config.servocal.lefthip.speed;
            speedRightAnkle.Value = Variables.config.servocal.rightankle.speed;
            speedRightLeg.Value = Variables.config.servocal.rightleg.speed;
            speedRightHip.Value = Variables.config.servocal.righthip.speed;
            servoLeftAnkle.Value = Variables.config.servocal.leftankle.target;
            servoLeftLeg.Value = Variables.config.servocal.leftleg.target;
            servoLeftHip.Value = Variables.config.servocal.lefthip.target;
            servoRightAnkle.Value = Variables.config.servocal.rightankle.target;
            servoRightLeg.Value = Variables.config.servocal.rightleg.target;
            servoRightHip.Value = Variables.config.servocal.righthip.target;
        }

        private void BtnSaveCalData_Click(object sender, RoutedEventArgs e)
        {
            Variables.config.Save();
        }
        

        public void updateStatus(string msg)
        {
            tbStatus.Text = msg;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            initHead(); // restart head
            base.OnNavigatedTo(e);
            Variables.voice.say("Manual Control System Active");
        }

        private async void initHead()
        {
            try
            {
                headCapture = new MediaCapture();
                if (headCapture != null)
                {
                    await headCapture.InitializeAsync();
                    // Set callbacks for failure and recording limit exceeded
                    headCapture.Failed += new MediaCaptureFailedEventHandler(headCapture_Failed);
                    headCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(headCapture_RecordLimitExceeded);
                }
            }
            catch (Exception e)
            {
                tbStatus.Text = "init Head Failed " +e;
                headCapture.Dispose();
                headCapture = null;
            }
            // Set callbacks for failure and recording limit exceeded
            if (headCapture != null)
            {
                // Start Preview  
                try
                {
                    headElement.Source = headCapture;
                    await headCapture.StartPreviewAsync();
                }
                catch (Exception e)
                {
                    tbStatus.Text = "Preview Start Failed " + e;
                    headCapture.Dispose();
                    headCapture = null;
                }
            }
        }

        private void headCapture_RecordLimitExceeded(MediaCapture sender)
        {
            throw new NotImplementedException();
        }

        private async void cleanup()
        {
            if (headCapture != null)
            {
                await headCapture.StopPreviewAsync();
                headCapture.Dispose();
                headCapture = null;
            }
           
        }

        private async void headCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            try
            {
                tbStatus.Text = "MediaCaptureFailed: ";
                await headCapture.StopRecordAsync();
                
            }
            catch (Exception)
            {
            }
            finally
            {
                tbStatus.Text += "\nCheck if camera is diconnected. Try re-launching the app";
            }
        }

        private async void listMaestroPorts()
        {
            if (Variables.maestro == null)
                Variables.maestro = new PololuMaestro();
            try
            {
                Variables.maestroPorts = await Variables.maestro.locateDevices();
                ComportListSource.Source = Variables.maestroPorts;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void controlState(bool enable)
        {
           
            servoLeftAnkle.IsEnabled = enable;
            servoLeftLeg.IsEnabled = enable;
            servoLeftHip.IsEnabled = enable;
            servoRightAnkle.IsEnabled = enable;
            servoRightLeg.IsEnabled = enable;
            servoRightHip.IsEnabled = enable;
            speedLeftAnkle.IsEnabled = enable;
            speedLeftLeg.IsEnabled = enable;
            speedLeftHip.IsEnabled = enable;
            speedRightAnkle.IsEnabled = enable;
            speedRightLeg.IsEnabled = enable;
            speedRightHip.IsEnabled = enable;
            accelerationLeftAnkle.IsEnabled = enable;
            accelerationLeftLeg.IsEnabled = enable;
            accelerationLeftHip.IsEnabled = enable;
            accelerationRightAnkle.IsEnabled = enable;
            accelerationRightLeg.IsEnabled = enable;
            accelerationRightHip.IsEnabled = enable;
            setServoStartPosition();


        }


        private void closeDevice()
        {
            Variables.maestro.closeActivePort();

            //btnConnect.IsEnabled = true;
            controlState(false);
            //listOfDevices.Clear();
            //Variables.maestroPorts.Clear();
            cleanup();
            Variables.maestro.cleanup();
        }


     
        private void btnMaestro_Toggled(object sender, RoutedEventArgs e)
        {
           // var selection = servoController.SelectedItem;
            if (btnMaestro.IsOn == false)
            {
                controlState(false);
                try
                {
                    tbStatus.Text = "";
                    Variables.maestro.cancelReadTask();
                    Variables.maestro.cancelWriteTask();
                    Variables.maestro.closeActivePort();
                    //closeComPort();
                    if (Variables.maestroPorts.Count <= 0)
                    {
                        listMaestroPorts();
                    }
                    else
                    {
                        ComportListSource.Source = Variables.maestroPorts;
                    }



                }
                catch (Exception ex)
                {
                    tbStatus.Text = ex.Message;
                }
                return;
            }

            controlState(true);

            if (Comports.SelectedIndex < 0)
            {
                tbStatus.Text = "Select a device and connect";
                return;
            }

           // DeviceInformation entry = (DeviceInformation)servoController.SelectedItem;
            PololuMaestroPort maestroPort = (PololuMaestroPort)Comports.SelectedItem;
            Variables.maestro.connect(maestroPort.portName);
            
/*
            try
            {
                serialPort = maestroPort.getComport();
                Debug.WriteLine(serialPort.PortName);
                
                // Disable the 'Connect' button 
                //btnConnect.IsEnabled = false;
                controlState(true);

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                // Display configured settings
                tbStatus.Text = "Serial port configured successfully: ";
                tbStatus.Text += serialPort.BaudRate + "-";
                tbStatus.Text += serialPort.DataBits + "-";
                tbStatus.Text += serialPort.Parity.ToString() + "-";
                tbStatus.Text += serialPort.StopBits;

                // Set the RcvdText field to invoke the TextChanged callback
                // The callback launches an async Read task to wait for data
                //rcvdText.Text = "Waiting for data...";

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                WriteCancellationTokenSource = new CancellationTokenSource();
                dataWriteObject = new DataWriter(serialPort.OutputStream);
                // Enable 'WRITE' button to allow sending data
                // could probably combine these

                Listen();
              //  startTransmit();
            }
            catch (Exception ex)
            {
                tbStatus.Text = ex.Message;
                //comPortInput.IsEnabled = true;
                //sendTextButton.IsEnabled = false;
            }*/
        }

       /* public bool setTarget(byte channelNumber, UInt16 target)
        {
            byte[] command = new byte[6];
            target *= 4;

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x04;
            command[3] = (byte)(channelNumber & 0x7f);
            command[4] = (byte)(target & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[5] = (byte)((target >> 7)  & 0x7f);
            writeBytes(command, 6);

            return true;

        }

        public bool setToNuetral(byte channelNumber)
        {
            byte[] command = new byte[3];

            command[0] = 0xFF;
            command[1] = (byte)(channelNumber & 0x7f); ;
            command[2] = 0x7F;
            writeBytes(command, 3);
            return true;
        }

        public bool setTargets(byte firstChannel, UInt16[] targets)
        {
            byte[] command = new byte[(targets.Count() * 2) + 4];
            byte commandCount = 4;

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x1F;
            command[3] = (byte)(firstChannel & 0x7f);
            for (int i=0; i < targets.Count(); i++)
            {
                targets[i] *= 4;
                command[commandCount++] = (byte)(targets[i] & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
                command[commandCount++] = (byte)((targets[i] >> 7) & 0x7f);
            }
            writeBytes(command,(byte)( commandCount - 1));
            return true;
        }

        public bool setSpeed(byte channelNumber, UInt16 speed)
        {
            byte[] command = new byte[6];

            
            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x07;
            command[3] = (byte)(channelNumber & 0x7f);
            command[4] = (byte)(speed & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[5] = (byte)((speed >> 7) & 0x7f);
            writeBytes(command, 6);
            return true;
        }

        public bool setAcceleration(byte channelNumber, UInt16 acceleration)
        {
            byte[] command = new byte[6];

           
            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x09;
            command[3] = (byte)(channelNumber & 0x7f);
            command[4] = (byte)(acceleration & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[5] = (byte)((acceleration >> 7) & 0x7f);
            writeBytes(command, 6);
            return true;

        }

        public bool goHome()
        {
            byte[] command = new byte[3];
            
            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x22;
            writeBytes(command,3);

            return true;

        }

        public void SetPWM(UInt16 onTime, UInt16 Period)
        {
            byte[] command = new byte[7];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x0A;
            command[3] = (byte)(onTime & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[4] = (byte)((onTime >> 7) & 0x7f);
            command[5] = (byte)(Period & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[6] = (byte)((Period >> 7) & 0x7f);
            writeBytes(command, 7);
        }


        public async void getPosition(byte channel)
        {
            byte[] command = new byte[4];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x10;
            command[3] = (byte)(channel & 0x7f);
            writeBytes(command,4);

            await ReadAsync(ReadCancellationTokenSource.Token);
            
            if (bytesRead > 0)
            {
                tbStatus.Text = "Channel " + channel.ToString() + " set to "+ dataReaderObject.ReadInt16().ToString();
            }

        }

        public async void getErrors()
        {
            byte[] command = new byte[3];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x21;

            writeBytes(command,3);

            await ReadAsync(ReadCancellationTokenSource.Token);

            if (bytesRead > 0)
            {
                tbStatus.Text = "Error State: " + dataReaderObject.ToString();
            }

        }*/

        private void btnGetChannel_Click(object sender, RoutedEventArgs e)
        {
            Variables.voice.say("Nukes activated");
            Variables.maestro.goHome();
        }

        private void servoLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(0, (UInt16)servoLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.target = (UInt16)servoLeftHip.Value;
            }
        }

        private void accelerationLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(1, (UInt16)accelerationLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.acceleration = (UInt16)accelerationLeftLeg.Value;
            }
        }

        private void speedLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(0, (UInt16)speedLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.speed = (UInt16)speedLeftHip.Value;
            }
        }


        private void servoLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(1, (UInt16)servoLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.target = (UInt16)servoLeftLeg.Value;
            }
        }

        private void speedLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(1, (UInt16)speedLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.speed = (UInt16)speedLeftLeg.Value;
            }
        }

        private void accelerationLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(0, (UInt16)accelerationLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.acceleration = (UInt16)accelerationLeftHip.Value;
            }
        }

        private void speedLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(2, (UInt16)speedLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.speed = (UInt16)speedLeftAnkle.Value;
            }
        }

        private void accelerationLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(2, (UInt16)accelerationLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.acceleration = (UInt16)accelerationLeftAnkle.Value;
            }
        }

        private void servoLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(2, (UInt16)servoLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.target = (UInt16)servoLeftAnkle.Value;
            }
        }

        private void speedRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(3, (UInt16)speedRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.speed = (UInt16)speedRightLeg.Value;
            }
        }

        private void accelerationRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(3, (UInt16)accelerationRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.acceleration = (UInt16)accelerationRightLeg.Value;
            }
        }

        private void servoRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(3, (UInt16)servoRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.righthip.target = (UInt16)servoRightHip.Value;
            }
        }

        private void speedRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(4, (UInt16)speedRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.speed = (UInt16)speedRightLeg.Value;
            }
        }

        private void accelerationRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(4, (UInt16)accelerationRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.acceleration = (UInt16)accelerationRightLeg.Value;
            }
        }

        private void servoRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(4, (UInt16)servoRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.target = (UInt16)servoRightLeg.Value;
            }
        }

        private void speedRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setSpeed(5, (UInt16)speedRightAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightankle.speed = (UInt16)speedRightAnkle.Value;
            }
        }

        private void accelerationRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setAcceleration(5, (UInt16)accelerationRightAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightankle.acceleration = (UInt16)accelerationRightAnkle.Value;
            }
        }

        private void servoRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Variables.maestro.setTarget(5, (UInt16)servoRightAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightankle.target = (UInt16)servoRightAnkle.Value;
            }
        }

        private void btnCal_Checked(object sender, RoutedEventArgs e)
        {
            calibrating = true;
            btnSaveCalData.IsEnabled = true;
        }

        private void BtnCal_Unchecked(object sender, RoutedEventArgs e)
        {
            calibrating = false;
            btnSaveCalData.IsEnabled = false;
        }
    }
}
