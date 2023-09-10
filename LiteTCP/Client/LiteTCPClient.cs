using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Client
{
    public class LiteTCPClient
    {
        private TcpClient client;

        public LiteTCPClient() => client = new TcpClient();

        public void Connect(IPAddress IP, int Port)
        {
            IPEndPoint ep = new IPEndPoint(IP, Port);
            Connect(ep);
        }

        public void Connect(IPEndPoint endPoint)
        {
            if(client == null) client = new TcpClient();
            client.Connect(endPoint);

            Task.Run(() =>
            {
                _ = ReceiveDataAsync();
            });
        }

        public void Disconnect()
        {
            if (client == null) return;
            client.Close();
            client = null;
        }


        public void sendData(byte[] data)
        {
            var stream = client.GetStream();
            Utils.writeDataToNetworkStream(data, stream);
        }

        public event EventHandler<Exception> ConnectionErrored;
        public event EventHandler<byte[]> DataReceived;

        private async Task ReceiveDataAsync()
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while(client != null && client.Connected)
                {
                    byte[] data = Utils.readDataFromNetworkStream(stream);

                    if (DataReceived != null) DataReceived.Invoke(this, data);

                }
            }
            catch(Exception ex)
            {
                if(ConnectionErrored != null) ConnectionErrored.Invoke(this, ex);
            }
            finally
            {
                Disconnect();
            }
        }

    }
}
