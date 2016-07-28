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
        private MediaCapture headCapture;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
       

        public HomePage()
        {
            this.InitializeComponent();
            controlState(false);
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            servoLeftHip.PointerReleased += ServoLeftHip_PointerReleased;
            servoLeftHip.Tapped += ServoLeftHip_Tapped;
            servoLeftLeg.PointerReleased += ServoLeftLeg_PointerReleased;
            servoLeftLeg.Tapped += ServoLeftLeg_Tapped;
            servoLeftAnkle.PointerReleased += ServoLeftAnkle_PointerReleased;
            servoLeftAnkle.Tapped += ServoLeftAnkle_Tapped;
            servoRightHip.PointerReleased += ServoRightHip_PointerReleased;
            servoRightHip.Tapped += ServoRightHip_Tapped;
            servoRightLeg.PointerReleased += ServoRightLeg_PointerReleased;
            servoRightLeg.Tapped += ServoRightLeg_Tapped;
            servoRightAnkle.PointerReleased += ServoRightAnkle_PointerReleased;
            servoRightAnkle.Tapped += ServoRightAnkle_Tapped;
            // start camera for obstacle avoidance
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            initHead(); // restart head
            base.OnNavigatedTo(e);
        }
        private async void initHead()
        {
            headCapture = new MediaCapture();
            await headCapture.InitializeAsync();

            // Set callbacks for failure and recording limit exceeded
            headCapture.Failed += new MediaCaptureFailedEventHandler(headCapture_Failed);
            headCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(headCapture_RecordLimitExceeded);

            // Start Preview                
            headElement.Source = headCapture;
            await headCapture.StartPreviewAsync();
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

        private void ServoRightLeg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(4, (UInt16)servoRightLeg.Value);
        }

        private void ServoRightHip_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(3, (UInt16)servoRightHip.Value);
        }

        private void ServoRightAnkle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(5, (UInt16)servoRightAnkle.Value);
        }

        private void ServoLeftAnkle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(2, (UInt16)servoLeftLeg.Value);
        }

        private void ServoLeftLeg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(1, (UInt16)servoLeftLeg.Value);
        }

        private void ServoLeftHip_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setTarget(0, (UInt16)servoLeftHip.Value);
        }

        private void ServoRightAnkle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(5, (UInt16)servoRightAnkle.Value);
        }

        private void ServoRightLeg_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(4, (UInt16)servoRightLeg.Value);
        }

        private void ServoRightHip_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(3, (UInt16)servoRightHip.Value);
        }

        private void ServoLeftAnkle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(2, (UInt16)servoLeftAnkle.Value);
        }

        private void ServoLeftLeg_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(1, (UInt16)servoLeftLeg.Value);
        }

        private void ServoLeftHip_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            setTarget(0, (UInt16)servoLeftHip.Value);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            cleanup(); // release camera
            base.OnNavigatedFrom(e);
        }


        private async void writeBytes(byte[] data)
        {
            Task<UInt32> storeAsyncTask;
           
            if (dataWriteObject != null)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriteObject.WriteBytes(data);
                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    tbStatus.Text = "Command Sent";
                    
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
            servoSpeed.IsEnabled = enable;
            servoAcceleration.IsEnabled = enable;
        }

       
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
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
            UInt32 bytesRead = await loadAsyncTask;
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
            byte[] command = new byte[4];
            byte[] targetBytes = BitConverter.GetBytes(target);
            Array.Reverse(targetBytes);
                  

            command[0] = 0x84;
            command[1] = channelNumber;
            command[2] = targetBytes[0];
            command[3] = targetBytes[1];
            writeBytes(command);

            return true;

        }



    }
}
