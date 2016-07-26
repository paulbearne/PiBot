using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace pibot
{
	class Maestro
	{

		private const UInt16 mMinimumChannelValue = 3968;
		private const UInt16 mMaximumChannelValue = 8000;
		private SerialDevice cmdPort = null;
		private SerialDevice ttlPort = null;
		private bool maestroPresent = false;
		private string maestroError = "";
		private CancellationTokenSource ReadCancellationTokenSource;
		DataWriter dataWriteObject = null;
		DataReader dataReaderObject = null;
		byte[] buffer = new byte[1024];

		public delegate void connected();
		public event connected OnConnected;

		public Maestro()
		{
			findPorts();
			
		}

		public bool present()
		{
			return maestroPresent;
		}

		/// <summary>
		/// find maestro cmnd and ttl ports
		/// </summary>
		private async void findPorts()

		{


			try
			{

				String aqs = SerialDevice.GetDeviceSelector();
				var dis = await DeviceInformation.FindAllAsync(aqs);
				DeviceInformation comportinfo;
				string portId;

				for (int i = 0; i < dis.Count; i++)
				{
					comportinfo = dis[i];
					portId = comportinfo.Id;
					Debug.WriteLine(portId);
					// find our command and data port vendor id VID_1FFB MI_02 TTL MI_00
					if ((comportinfo.Id.IndexOf("VID_1FFB") > -1) && (comportinfo.Id.IndexOf("MI_02") > -1))
					{
						cmdPort = await SerialDevice.FromIdAsync(comportinfo.Id);
						// Create the DataWriter object and attach to OutputStream
						//dataWriteObject = new DataWriter(cmdPort.OutputStream);
											
						maestroPresent = true;
						

					}

					if ((comportinfo.Id.IndexOf("VID_1FFB") > -1) && (comportinfo.Id.IndexOf("MI_00") > -1))
					{
						ttlPort = await SerialDevice.FromIdAsync(comportinfo.Id);
						//maestroPresent = true;

					}
				}

			}
			catch (Exception ex)
			{
				maestroError = ex.Message;
			}
		}

		/// <param name="e"></param>
		public void connect()
		{
		   
				try
				{
					// Configure serial settings
					

					/*ttlPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
					ttlPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
					ttlPort.BaudRate = 9600;
					ttlPort.Parity = SerialParity.None;
					ttlPort.StopBits = SerialStopBitCount.One;
					ttlPort.DataBits = 8;
					ttlPort.Handshake = SerialHandshake.None;*/

					// Create cancellation token object to close I/O operations when closing the device
					ReadCancellationTokenSource = new CancellationTokenSource();
					if (cmdPort != null)
					{
						cmdPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
						cmdPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
						cmdPort.BaudRate = 9600;
						cmdPort.Parity = SerialParity.None;
						cmdPort.StopBits = SerialStopBitCount.One;
						cmdPort.DataBits = 8;
						cmdPort.Handshake = SerialHandshake.None;
						dataWriteObject = new DataWriter(cmdPort.OutputStream);
						if (OnConnected != null)
						{
						  OnConnected();
						}
						Listen();
					   
					}
				}
				catch (Exception ex)
				{
					maestroError = ex.Message;
				}
			
		}

		public bool commandPortReady()
		{
			return cmdPort != null;
		}

		public void clearErrorMessage()
		{
			maestroError = "";
		}

		public void setErrorMessage(string message)
		{
			maestroError = message;
		}

		public UInt16 getMinChannelValue()
		{
			return mMinimumChannelValue;
		}

		public UInt16 getMaxChannelValue()
		{
			return mMaximumChannelValue;
		}

		private async Task<bool> writeBytes( byte[] data,uint numBytesToWrite )
		{
			Task<UInt32> storeAsyncTask;
			// UInt32 bytesTransferred = 0;
			bool datawritten = false;

			if (dataWriteObject != null)
			{
				// Load the text from the sendText input text box to the dataWriter object
				dataWriteObject.WriteBytes(data);
				// Launch an async task to complete the write operation
				storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

				UInt32 bytesWritten = await storeAsyncTask;
				if (bytesWritten > 0)
				{
					datawritten = true;
				}
				
			}
			return datawritten;
			
		}


		private async void Listen()
		{
			try
			{
				if (cmdPort != null)
				{
					dataReaderObject = new DataReader(cmdPort.InputStream);

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
				   
					CloseDevice();
				}
				else
				{
					maestroError = ex.Message;
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

		/// <summary>
		/// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
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
				dataReaderObject.ReadBytes(buffer);
			}
		}

		private void CloseDevice()
		{
			if (cmdPort != null)
			{
				cmdPort.Dispose();
			}
			cmdPort = null;

			
		}

		/// <summary>
		/// CancelReadTask:
		/// - Uses the ReadCancellationTokenSource to cancel read operations
		/// </summary>
		private void CancelReadTask()
		{
			if (ReadCancellationTokenSource != null)
			{
				if (!ReadCancellationTokenSource.IsCancellationRequested)
				{
					ReadCancellationTokenSource.Cancel();
				}
			}
		}

		public bool setTarget(byte channelNumber ,UInt16 target)
		{
			byte[] command = new byte[4];
			byte[] targetBytes = BitConverter.GetBytes(target);
			Array.Reverse(targetBytes);
			clearErrorMessage();
			if( (target < getMinChannelValue()) || (target > getMaxChannelValue()))
					return false;

			command[0] = 0x84;
			command[1] = channelNumber;
			command[2] = targetBytes[0];
			command[3] = targetBytes[1];
			writeBytes(command, 4).RunSynchronously();

			return true;
		   
		}

		public bool setTarget(byte deviceNumber ,byte channelNumber, UInt16 target)
		{
			byte[] command = new byte[6];
			byte[] targetBytes = BitConverter.GetBytes(target);
			Array.Reverse(targetBytes);
			clearErrorMessage();
			if ((target < getMinChannelValue()) || (target > getMaxChannelValue()))
				return false;

			command[0] = 0xAA;
			command[1] = deviceNumber;
			command[2] = 0x84 & 0x7F;
			command[3] = channelNumber;
			command[4] = targetBytes[0];
			command[5] = targetBytes[1];
			writeBytes(command, 6).RunSynchronously();
			return true;

		}

		public bool setAcceleration(byte channelNumber, UInt16 acceleration)
		{
			byte[] command = new byte[4];
			byte[] targetBytes = BitConverter.GetBytes(acceleration);
			Array.Reverse(targetBytes);
			clearErrorMessage();
			command[0] = 0x89;
			command[1] = channelNumber;
			command[2] = targetBytes[0];
			command[3] = targetBytes[1];
			writeBytes(command, 4).RunSynchronously();
			return true;

		}

		public bool setAcceleration(byte deviceNumber, byte channelNumber, UInt16 acceleration)
		{
			byte[] command = new byte[6];
			byte[] targetBytes = BitConverter.GetBytes(acceleration);
			Array.Reverse(targetBytes);
			clearErrorMessage();

			command[0] = 0xAA;
			command[1] = deviceNumber;
			command[2] = 0x89 & 0x7F;
			command[3] = channelNumber;
			command[4] = targetBytes[0];
			command[5] = targetBytes[1];
			writeBytes(command, 6).RunSynchronously();
			return true;

		}


		public bool setSpeed(byte channelNumber, UInt16 speed)
		{
			byte[] command = new byte[4];
			byte[] targetBytes = BitConverter.GetBytes(speed);
			Array.Reverse(targetBytes);
			clearErrorMessage();
			
			command[0] = 0x87;
			command[1] = channelNumber;
			command[2] = targetBytes[0];
			command[3] = targetBytes[1];
			writeBytes(command, 4).RunSynchronously();
			return true;

		}

		public bool setSpeed(byte deviceNumber, byte channelNumber, UInt16 speed)
		{
			byte[] command = new byte[6];
			byte[] targetBytes = BitConverter.GetBytes(speed);
			Array.Reverse(targetBytes);
			clearErrorMessage();

			command[0] = 0xAA;
			command[1] = deviceNumber;
			command[2] = 0x87 & 0x7F;
			command[3] = channelNumber;
			command[4] = targetBytes[0];
			command[5] = targetBytes[1];
			writeBytes(command, 6).RunSynchronously();
			return true;

		}

		public bool getPosition(ref int position ,byte channelNumber)
		{
			clearErrorMessage();
			byte[] command = { 0x90, channelNumber };
			writeBytes(command, 2).RunSynchronously();
			position = buffer[0] + 256 * buffer[1];
			return true;
		}

		public bool getPosition(byte deviceNumber,ref int position, byte channelNumber)
		{
			clearErrorMessage();
			byte[] command = { 0xAA, deviceNumber, 0x90 & 0x7F, channelNumber };
			writeBytes(command, 4).RunSynchronously();
			position = buffer[0] + 256 * buffer[1];
			return true;
		}

		public bool getErrors(ref UInt16 errors)
		{
			clearErrorMessage();
			byte[] command = { 0xA1};
			writeBytes(command, 1).RunSynchronously();
			errors =(UInt16)(( buffer[0] & 0x7f) + 256 * (buffer[1] & 0x7f));
			return true;
		}

		public bool getErrors(byte deviceNumber, ref int errors)
		{
			clearErrorMessage();
			byte[] command = { 0xAA, deviceNumber, 0xA1 & 0x7F};
			writeBytes(command, 3).RunSynchronously();
			errors = (UInt16)((buffer[0] & 0x7f) + 256 * (buffer[1] & 0x7f));
			return true;
		}

		public bool getMovingState()
		{
			clearErrorMessage();
			byte[] command = { 0x93};
			writeBytes(command, 1).RunSynchronously();
			return buffer[0] == 0x01;
		}

		public bool getMovingState(byte deviceNumber)
		{
			clearErrorMessage();
			byte[] command = { 0xAA, deviceNumber, 0x93 & 0x7F };
			writeBytes(command, 3).RunSynchronously();
			return buffer[0] == 0x01;
		}



		public bool goHome()
		{
			byte[] command = new byte[2];
			clearErrorMessage();

			command[0] = 0xA2;
		   
			writeBytes(command, 1).RunSynchronously();
			return true;

		}

		public bool goHome(byte deviceNumber)
		{
			byte[] command = { 0xAA, deviceNumber, 0xA2 & 0x7F };
			clearErrorMessage();

			writeBytes(command, 3).RunSynchronously();
			return true;

		}


	}
}
