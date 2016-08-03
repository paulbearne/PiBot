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
using Windows.System.Threading;
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
    public sealed partial class ManualControl : Page
    {
        private SerialDevice serialPort = null;
        private byte deviceNumber = 0x00;
        private MediaCapture headCapture;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        bool commsConnected = false;
        ThreadPoolTimer timer;
        byte[] inBuffer = new byte[256];
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        private CancellationTokenSource WriteCancellationTokenSource;
        private UInt32 bytesRead;
        bool calibrating = false;        
       
        public ManualControl()
        {
            
            this.InitializeComponent();
            Variables.voice.say("Pi Bot Control Online");
            controlState(false);
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            timer = ThreadPoolTimer.CreatePeriodicTimer(RefreshTimer_Tick, TimeSpan.FromMilliseconds(2000));
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

        private void RefreshTimer_Tick(ThreadPoolTimer timer)
        {
            if (commsConnected == false)
            {
                // refresh the list tacky way to do it but works
               // listOfDevices.Clear();
               // ListAvailablePorts();
            }
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
            if(dataWriteObject != null)
            {
                dataWriteObject.Dispose();
                dataWriteObject = null;
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

        // change so writebytes justads to the datawrite Object
        private async void writeBytes(byte[] data,byte numofbytes)
        {
            if (serialPort != null)
            {
                               
                // put the bytes int the buffer
                for (int i = 0; i < numofbytes; i++)
                {
                    dataWriteObject.WriteByte(data[i]);
                    await WriteAsync(WriteCancellationTokenSource.Token);
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
            setServoStartPosition();


        }


    private async void startTransmit()
        {
            try
            {
                if (serialPort != null)
                {
                    
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    // keep write the serial input
                    while (true)
                    {
                        // loop writing bytes if we have any
                        await WriteAsync(WriteCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    tbStatus.Text = "writing task was cancelled, closing device and cleaning up";
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
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
       
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    Variables.voice.say("Motor Control System Activate");
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input and writing
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
                inBuffer = dataReaderObject.ReadBuffer((uint)inBuffer.Count()).ToArray();
                tbStatus.Text = "bytes read successfully!";
            }
        }

        private async Task WriteAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> writeAsyncTask;

            uint bytesWritten;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            // dataWriteObject.add
            //dataWriteObject.OutputStreamOptions = InputStreamOptions.Partial;
            
            writeAsyncTask = dataWriteObject.StoreAsync().AsTask(cancellationToken);
            // Create a task object to wait for data on the serialPort.InputStream
            //loadAsyncTask = datawriteObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            bytesWritten = await writeAsyncTask;
            if (bytesWritten > 0)
            {
                //  rcvdText.Text = dataReaderObject.ReadString(bytesRead);
               // tbStatus.Text = "bytes read successfully!";
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

        private void cancelWriteTask()
        {
            if (WriteCancellationTokenSource != null)
            {
                if (!WriteCancellationTokenSource.IsCancellationRequested)
                {
                    WriteCancellationTokenSource.Cancel();
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
                    cancelWriteTask();
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
            }
        }

        public bool setTarget(byte channelNumber, UInt16 target)
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

        }

        private void btnGetChannel_Click(object sender, RoutedEventArgs e)
        {
            Variables.voice.say("Nukes activated");
            goHome();
        }

        private void servoLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(0, (UInt16)servoLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.target = (UInt16)servoLeftHip.Value;
            }
        }

        private void accelerationLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(1, (UInt16)accelerationLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.acceleration = (UInt16)accelerationLeftLeg.Value;
            }
        }

        private void speedLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(0, (UInt16)speedLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.speed = (UInt16)speedLeftHip.Value;
            }
        }


        private void servoLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(1, (UInt16)servoLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.target = (UInt16)servoLeftLeg.Value;
            }
        }

        private void speedLeftLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(1, (UInt16)speedLeftLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftleg.speed = (UInt16)speedLeftLeg.Value;
            }
        }

        private void accelerationLeftHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(0, (UInt16)accelerationLeftHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.lefthip.acceleration = (UInt16)accelerationLeftHip.Value;
            }
        }

        private void speedLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(2, (UInt16)speedLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.speed = (UInt16)speedLeftAnkle.Value;
            }
        }

        private void accelerationLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(2, (UInt16)accelerationLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.acceleration = (UInt16)accelerationLeftAnkle.Value;
            }
        }

        private void servoLeftAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(2, (UInt16)servoLeftAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.leftankle.target = (UInt16)servoLeftAnkle.Value;
            }
        }

        private void speedRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(3, (UInt16)speedRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.speed = (UInt16)speedRightLeg.Value;
            }
        }

        private void accelerationRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(3, (UInt16)accelerationRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.acceleration = (UInt16)accelerationRightLeg.Value;
            }
        }

        private void servoRightHip_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(3, (UInt16)servoRightHip.Value);
            if (calibrating)
            {
                Variables.config.servocal.righthip.target = (UInt16)servoRightHip.Value;
            }
        }

        private void speedRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(4, (UInt16)speedRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.speed = (UInt16)speedRightLeg.Value;
            }
        }

        private void accelerationRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(4, (UInt16)accelerationRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.acceleration = (UInt16)accelerationRightLeg.Value;
            }
        }

        private void servoRightLeg_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(4, (UInt16)servoRightLeg.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightleg.target = (UInt16)servoRightLeg.Value;
            }
        }

        private void speedRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setSpeed(5, (UInt16)speedRightAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightankle.speed = (UInt16)speedRightAnkle.Value;
            }
        }

        private void accelerationRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setAcceleration(5, (UInt16)accelerationRightAnkle.Value);
            if (calibrating)
            {
                Variables.config.servocal.rightankle.acceleration = (UInt16)accelerationRightAnkle.Value;
            }
        }

        private void servoRightAnkle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setTarget(5, (UInt16)servoRightAnkle.Value);
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
