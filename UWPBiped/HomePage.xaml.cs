using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class HomePage : Page
    {
        private SerialDevice serialPort = null;
        private byte deviceNumber = 0x00;
        private MediaCapture headCapture;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        private UInt32 bytesRead;
        private Speech voice;
       


        public HomePage()
        {
            
            this.InitializeComponent();
            AudioElement.Volume = 0.6;
            voice = new Speech(AudioElement);
            voice.OnComplete += Voice_OnComplete;
            voice.say("Pi Bot Control Online");
            controlState(false);
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            
            // start camera for obstacle avoidance

        

        }


        private void Voice_OnComplete()
        {
           
        }

        public void updateStatus(string msg)
        {
            tbStatus.Text = msg;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            initHead(); // restart head
            base.OnNavigatedTo(e);
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
            //headCapture.Failed += new MediaCaptureFailedEventHandler(headCapture_Failed);
            //headCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(headCapture_RecordLimitExceeded);
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

        //todo add calibration and start servos at there calibrated start position


        private async void writeBytes(byte[] data,byte numofbytes)
        {
            Task<UInt32> storeAsyncTask;
            
            try
            {
                UInt32 bytesWritten;
                if (serialPort != null)
                {
                   
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                    if (dataWriteObject != null)
                    {/*
                        for (int i = 0; i < numofbytes; i++)
                        {
                            // Load the text from the sendText input text box to the dataWriter object
                            dataWriteObject.WriteByte(data[i]);
                            // Launch an async task to complete the write operation
                            storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                            bytesWritten = await storeAsyncTask;
                        }*/
                        try
                        {
                            dataWriteObject.WriteBytes(data);
                            // Launch an async task to complete the write operation
                            storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                            bytesWritten = await storeAsyncTask;
                            if (bytesWritten > 0)
                            {
                                

                            }
                        }
                        catch (Exception e)
                        {
                            tbStatus.Text = "Error Sending Command : "+e;
                        }
                        

                    }
                }
            }
            catch (Exception ex)
            {
                tbStatus.Text = "Write Bytes Error: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }


        }

        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                tbStatus.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;

              //  connectedDevices.SelectedIndex = 0;
                //comPortInput_Click(this, null);

            }
            catch (Exception ex)
            {
                tbStatus.Text = ex.Message;
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
        }

       
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    voice.say("Motor Control System Activate");
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                   tbStatus.Text = "Reading task was cancelled, closing device and cleaning up";
                   closeDevice();
                }
                else
                {
                    tbStatus.Text = ex.Message;
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        private void closeComPort()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            //btnConnect.IsEnabled = true;
            controlState(false);
            listOfDevices.Clear();
            //cleanup();
        }

        private void closeDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            //btnConnect.IsEnabled = true;
            controlState(false);
            listOfDevices.Clear();
            cleanup();
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
              //  rcvdText.Text = dataReaderObject.ReadString(bytesRead);
                tbStatus.Text = "bytes read successfully!";
            }
        }

        private void cancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

       
        private async void btnMaestro_Toggled(object sender, RoutedEventArgs e)
        {
           // var selection = servoController.SelectedItem;
            if (btnMaestro.IsOn == false)
            {
                controlState(false);
                try
                {
                    tbStatus.Text = "";
                    cancelReadTask();
                    closeComPort();
                    ListAvailablePorts();
                    
                }
                catch (Exception ex)
                {
                    tbStatus.Text = ex.Message;
                }
                return;
            }

            controlState(true);

            if (servoController.SelectedIndex < 0)
            {
                tbStatus.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)servoController.SelectedItem;

            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);

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

                // Enable 'WRITE' button to allow sending data
                //sendTextButton.IsEnabled = true;

                Listen();
            }
            catch (Exception ex)
            {
                tbStatus.Text = ex.Message;
                //comPortInput.IsEnabled = true;
                //sendTextButton.IsEnabled = false;
            }
        }

        public bool setTarget(byte channelNumber, UInt16 target)
        {
            byte[] command = new byte[6];
            byte[] targetBytes = BitConverter.GetBytes((target));
            //Array.Reverse(targetBytes);
               
               

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x04;
            command[3] = channelNumber;
            command[4] = (byte)(targetBytes[0] / 0x02);   // div 2 as c# doesnt do Masking so can't mask with 7F
            command[5] = (byte)(targetBytes[1] / 0x02);
            writeBytes(command, 6);

            return true;

        }

        public bool setSpeed(byte channelNumber, UInt16 speed)
        {
            byte[] command = new byte[6];
            byte[] targetBytes = BitConverter.GetBytes(speed);
            //Array.Reverse(targetBytes);


            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x07;
            command[3] = channelNumber;
            command[4] = (byte)(targetBytes[0] / 0x02);   // should be 7 bit data 
            command[5] = (byte)(targetBytes[1] / 0x02);
            writeBytes(command, 6);

            return true;
        }

        public bool setAcceleration(byte channelNumber, UInt16 acceleration)
        {
            byte[] command = new byte[6];
            byte[] targetBytes = BitConverter.GetBytes(acceleration);
            //Array.Reverse(targetBytes);

            
            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x09;
            command[3] = channelNumber;
            command[4] = (byte)(targetBytes[0] / 0x02);   // should be 7 bit data 
            command[5] = (byte)(targetBytes[1] / 0x02);
            writeBytes(command,6);

            return true;

        }

        public bool goHome()
        {
            byte[] command = new byte[3];
            
            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x22;
            writeBytes(command,6);

            return true;

        }


        public async void getPosition(byte channel)
        {
            byte[] command = new byte[4];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x10;
            command[3] = channel;
            writeBytes(command,4);

            await ReadAsync(ReadCancellationTokenSource.Token);
            
            if (bytesRead >= 2)
            {
                tbStatus.Text = "Channel " + channel.ToString() + " set to "+dataReaderObject.ReadInt16().ToString();
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

            if (bytesRead >= 2)
            {
                tbStatus.Text = "Error State: " + dataReaderObject.ToString();
            }

        }

        private void btnGetChannel_Click(object sender, RoutedEventArgs e)
        {
            voice.say("Nukes activated");
            goHome();
        }

        private void servoLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(0, (UInt16)servoLeftHip.Value);
        }

        private void accelerationLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(1, (UInt16)accelerationLeftLeg.Value);
        }

        private void speedLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(0, (UInt16)speedLeftHip.Value);
        }


        private void servoLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(1, (UInt16)servoLeftLeg.Value);
        }

        private void speedLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(1, (UInt16)speedLeftLeg.Value);
        }

        private void accelerationLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(0, (UInt16)accelerationLeftHip.Value);
        }

        private void speedLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(2, (UInt16)speedLeftAnkle.Value);
        }

        private void accelerationLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(2, (UInt16)accelerationLeftAnkle.Value);
        }

        private void servoLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(2, (UInt16)servoLeftAnkle.Value);
        }

        private void speedRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(3, (UInt16)speedRightHip.Value);
        }

        private void accelerationRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(3, (UInt16)accelerationRightHip.Value);
        }

        private void servoRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(3, (UInt16)servoRightHip.Value);
        }

        private void speedRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(4, (UInt16)speedRightLeg.Value);
        }

        private void accelerationRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(4, (UInt16)accelerationRightLeg.Value);
        }

        private void servoRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(4, (UInt16)servoRightLeg.Value);
        }

        private void speedRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(5, (UInt16)speedRightAnkle.Value);
        }

        private void accelerationRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(5, (UInt16)accelerationRightAnkle.Value);
        }

        private void servoRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(5, (UInt16)servoRightAnkle.Value);
        }
    }
}
