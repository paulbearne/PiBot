using System;
using System.Diagnostics;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ServerSocket
{
    internal class SocketServer
    {
        private readonly int _port;
        public bool connected = false;
        public int Port { get { return _port; } }

        private StreamSocketListener listener;
        private DataWriter _writer;

        public delegate void DataReceived(string data);
        public event DataReceived OnDataReceived;

        public delegate void Error(string message);
        public event Error OnError;

        public delegate void Connected(string message);
        public event Connected OnConnected;

        public SocketServer(int port)
        {
            _port = port;
        }

        public async void Start()
        {
            try
            {
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }

                listener = new StreamSocketListener();

                listener.ConnectionReceived += Listener_ConnectionReceived;
                await listener.BindServiceNameAsync(Port.ToString());
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(e.Message);
            }
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var reader = new DataReader(args.Socket.InputStream);
            _writer = new DataWriter(args.Socket.OutputStream);
            connected = true;
            if (OnConnected != null)
            {
                string data ="Connected to " + args.Socket.Information.RemoteHostName.ToString() + '(' + args.Socket.Information.RemoteAddress.ToString() + ')';
                OnConnected(data);
            }
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                        return;

                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                        return;
                    if (OnDataReceived != null)
                    {
                        string data = reader.ReadString(actualStringLength);
                        OnDataReceived(data);
                    }
                }

            }
            catch (Exception ex)
            {
                // Dispara evento em caso de erro, com a mensagem de erro
                if (OnError != null)
                    OnError(ex.Message);
            }
        }

        public async void Send(string message)
        {
            if (_writer != null)
            {
                _writer.WriteUInt32(_writer.MeasureString(message));
                _writer.WriteString(message);
                try
                {
                    await _writer.StoreAsync();
                    await _writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError(ex.Message);
                }
            }
        }
    }
}
