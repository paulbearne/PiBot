
using System;
using System.Collections.Generic;
using System.Net;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace udpserver
{
    public sealed partial class CmdServer
    {
        DatagramSocket listenerSocket = null;
        string port = "9000";

        public delegate void DataReceived(string data);
        public event DataReceived OnDataReceived;

        public CmdServer(int portnum)
        {
            //this.InitializeComponent();
            Listen();
            port = portnum.ToString();
            //Send();
        }


        private async void Listen()
        {
            listenerSocket = new DatagramSocket();
            //listenerSocket.MessageReceived += (x, y) =>
            //{
            //    var a = "2";
            //};
            listenerSocket.MessageReceived += MessageReceived;
            await listenerSocket.BindServiceNameAsync(port);
        }

        public void WriteString(string data,int portnum)
        {
            Send(data,portnum.ToString());
        }

        private async void Send(string data,string remoteport)
        {
            IOutputStream outputStream;
            string localIPString = GetLocalIp();
            IPAddress localIP = IPAddress.Parse(localIPString);
            string subnetMaskString = "255.0.0.0";
            IPAddress subnetIP = IPAddress.Parse(subnetMaskString);
            HostName remoteHostname = new HostName(localIP.ToString());
            outputStream = await listenerSocket.GetOutputStreamAsync(remoteHostname, remoteport);

            using (DataWriter writer = new DataWriter(outputStream))
            {
                writer.WriteString(data);
                await writer.StoreAsync();
            }


        }

        //private object GetBroadcastAddress(IPAddress localIP, IPAddress subnetIP)
        //{
        //    throw new NotImplementedException();
        //}

        private void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            uint len = reader.UnconsumedBufferLength;
            string msg = reader.ReadString(len);

            string remoteHost = args.RemoteAddress.DisplayName;
            reader.Dispose();

            if (OnDataReceived != null) {
                OnDataReceived(msg);
            }
         }

        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null
                  && icp.NetworkAdapter != null
                  && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                var name = icp.ProfileName;

                var hostnames = NetworkInformation.GetHostNames();

                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null
                        && hn.IPInformation.NetworkAdapter != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                                                   != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                    == icp.NetworkAdapter.NetworkAdapterId
                        && hn.Type == HostNameType.Ipv4)
                    {
                        return hn.CanonicalName;
                    }
                }
            }

            return "---";
        }

    }
}