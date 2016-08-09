using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace UWPBiped
{

    public class PololuMaestroPort
    {
        private string PortName;
        private SerialDevice SerialPort;
       
        public PololuMaestroPort(string name,string id)
        {
            PortName = name;
            setComport(id);

        }

        public async void setComport(string id)
        {
            SerialPort = await SerialDevice.FromIdAsync(id);
           
        }

        public SerialDevice getComport()
        {
            return SerialPort;
        }

        public string portName
        {
            get
            {
                return PortName;
            }
            set
            {
                PortName = value;
            }
        }

    }

      
    public class PololuMaestro
    {
       
        
        private ObservableCollection<PololuMaestroPort> maestroPorts;
        private PololuMaestroPort CommandPort;
        private PololuMaestroPort TTLPort;
        private byte deviceNumber = 0;
        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;
        byte[] inBuffer = new byte[256];
        private CancellationTokenSource ReadCancellationTokenSource;
        private CancellationTokenSource WriteCancellationTokenSource;
        private UInt32 bytesRead;
        private SerialDevice serialPort;

        public PololuMaestro()
        {
            
        }

        public void cancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        public void cleanup() {
            if (dataWriteObject != null)
            {
                dataWriteObject.Dispose();
                dataWriteObject = null;
            }
        }

        public void cancelWriteTask()
        {
            if (WriteCancellationTokenSource != null)
            {
                if (!WriteCancellationTokenSource.IsCancellationRequested)
                {
                    WriteCancellationTokenSource.Cancel();
                }
            }
        }

        private async void writeBytes(byte[] data, byte numofbytes)
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

        private async Task WriteAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> writeAsyncTask;

            uint bytesWritten;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            writeAsyncTask = dataWriteObject.StoreAsync().AsTask(cancellationToken);
            // Launch the task and wait
            bytesWritten = await writeAsyncTask;

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
                
            }
        }

        public byte DeviceNumber
        {
            get
            {
                return deviceNumber;
            }
            set
            {
                deviceNumber = value;
            }
        }
        
        // note PID 0089 6 channel 008A 12 channel 008C 24 channel 
        public async Task<ObservableCollection<PololuMaestroPort>> locateDevices()
        {
            
            // here we search for pololu command port and set the name of the port depending on the id MI_00
            try
            {
                // we try to find a pololu device
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                maestroPorts = new ObservableCollection<PololuMaestroPort>();

                if (dis.Count > 0)
                {
                    
                    // we have found a 6 channel device
                    for (int i = 0;i < dis.Count; i++)
                    {
                        if (dis[i].Id.Contains("PID_0089"))
                        {
                            if (dis[i].Id.Contains("MI_00"))
                            {
                                CommandPort = new PololuMaestroPort("Maestro 6-Channel(Command Port)", dis[i].Id);
                                maestroPorts.Add(CommandPort);
                                
                            }
                            if (dis[i].Id.Contains("MI_02"))
                            {
                                TTLPort = new PololuMaestroPort("Maestro 6-Channel(TTL Port)", dis[i].Id);
                                maestroPorts.Add(TTLPort);
                            }
                            
                        }
                        if (dis[i].Id.Contains("PID_008A"))
                        {
                            if (dis[i].Id.Contains("MI_00"))
                            {
                                CommandPort = new PololuMaestroPort("Maestro 12-Channel(Command Port)", dis[i].Id);
                                maestroPorts.Add(CommandPort);
                            }
                            if (dis[i].Id.Contains("MI_02"))
                            {
                                TTLPort = new PololuMaestroPort("Maestro 12-Channel(TTL Port)", dis[i].Id);
                                maestroPorts.Add(TTLPort);
                            }
                        }
                        if (dis[i].Id.Contains("PID_008C"))
                        {
                            if (dis[i].Id.Contains("MI_00"))
                            {
                                CommandPort = new PololuMaestroPort("Maestro 24-Channel(Command Port)", dis[i].Id);
                                maestroPorts.Add(CommandPort);
                            }
                            if (dis[i].Id.Contains("MI_02"))
                            {
                                TTLPort = new PololuMaestroPort("Maestro 24-Channel(TTL Port)", dis[i].Id);
                                maestroPorts.Add(TTLPort);
                            }
                        }

                    }
                }

                return maestroPorts;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public void connect(string name)
        {
            CommandPort = findPort(name);
            if (CommandPort != null)
            {
                try
                {
                    serialPort = CommandPort.getComport();
                    ReadCancellationTokenSource = new CancellationTokenSource();
                    WriteCancellationTokenSource = new CancellationTokenSource();
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.BaudRate = 9600;
                    serialPort.Parity = SerialParity.None;
                    serialPort.StopBits = SerialStopBitCount.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = SerialHandshake.None;
                } catch(Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }


        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
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
                    Debug.WriteLine("Reading task was cancelled, closing device and cleaning up");
                    closeActivePort();
                }
                else
                {
                    Debug.WriteLine(ex.Message);
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

        public void closeActivePort()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }


        public PololuMaestroPort findPort(string name)
        {
            for (int i=0; i< maestroPorts.Count; i++)
            {
                if (maestroPorts[i].portName == name)
                {
                    return maestroPorts[i];
                }
            }
            
            return null;
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
            command[5] = (byte)((target >> 7) & 0x7f);
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
            for (int i = 0; i < targets.Count(); i++)
            {
                targets[i] *= 4;
                command[commandCount++] = (byte)(targets[i] & 0x7F);   // div 2 as c# doesnt do Masking so can't mask with 7F
                command[commandCount++] = (byte)((targets[i] >> 7) & 0x7f);
            }
            writeBytes(command, (byte)(commandCount - 1));
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

        public bool goCalibratedHome()
        {
            
            return true;
        }

        public bool goHome()
        {
            byte[] command = new byte[3];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x22;
            writeBytes(command, 3);

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


        private UInt16 map(UInt16 x, UInt16 in_min, UInt16 in_max, UInt16 out_min, UInt16 out_max)
        {
            return  (UInt16)((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
        }

        public void setAngle(byte channel,UInt16 target)
        {
            UInt16 actualTarget = map(target, 0, 180, 500, 2500);
            setTarget(channel,actualTarget);
        }

        public void setAngles(byte firstChannel, UInt16[] targets)
        {
            UInt16[] actualTargets = new UInt16[targets.Count()];
            for(int i =0; i < targets.Count(); i++)
            {
                actualTargets[i] = map(targets[i], 0, 180, 500, 2500);
            }
            setTargets(firstChannel, actualTargets);
        }


        public async Task<string> getPosition(byte channel)
        {
            byte[] command = new byte[4];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x10;
            command[3] = (byte)(channel & 0x7f);
            writeBytes(command, 4);

            await ReadAsync(ReadCancellationTokenSource.Token);

            if (bytesRead > 0)
            {
                return "Channel " + channel.ToString() + " set to " + dataReaderObject.ReadInt16().ToString();
            }

            return "";

        }



        public async Task<string> getErrors()
        {
            byte[] command = new byte[3];

            command[0] = 0xAA;
            command[1] = deviceNumber;
            command[2] = 0x21;

            writeBytes(command, 3);

            await ReadAsync(ReadCancellationTokenSource.Token);

            if (bytesRead > 0)
            {
                return "Error State: " + dataReaderObject.ToString();
            }
            return "";
        }

    }
}
