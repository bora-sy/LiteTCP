using LiteTCP.Events;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Client
{
    public class LiteTCPClient
    {
        private TcpClient client;
        public bool Connected { get { return client.Connected; } }

        public LiteTCPClient() => client = new TcpClient();


        #region Connect / Disconnect
        public void Connect(IPAddress IP, int Port)
        {
            if (IP == null) throw new ArgumentNullException(nameof(IP));
            if (Connected) throw new Exception("Client is already Connected");

            IPEndPoint ep = new IPEndPoint(IP, Port);
            Connect(ep);
        }

        public void Connect(IPEndPoint EP)
        {
            if (EP == null) throw new ArgumentNullException(nameof(EP));
            if (Connected) throw new Exception("Client is already Connected");

            client.Connect(EP);
            Task.Run(() => HandleIncomingAsync());
        }

        public void Disconnect()
        {
            if (!Connected) throw new Exception("Client is not Connected");
            client.Close();
        }
        #endregion

        #region Events
        public event EventHandler<TCPDataReceivedEventArgs> DataReceived;

        public event EventHandler<EventArgs> ServerDisconnected;
        #endregion

        #region Send Data
        public async Task<bool> SendAsync(string data, Encoding encoding)
        {
            if (!Connected) throw new Exception("Client is not Connected");
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await SendAsync(encoding.GetBytes(data));
        }

        public async Task<bool> SendAsync(byte[] data)
        {
            if (!Connected) throw new Exception("Client is not Connected");
            if(data == null) throw new ArgumentNullException(nameof(data));

            try
            {
                NetworkStream stream = client.GetStream();
                bool res = await NetworkStreamUtils.writeDataToNetworkStreamAsync(stream, data);
                return res;
            }
            catch(Exception)
            {
                return false;
            }
        }
        #endregion

        private async Task HandleIncomingAsync()
        {
            try
            {

                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    byte[] incomingData = await NetworkStreamUtils.readDataFromNetworkStreamAsync(stream);
                    if (incomingData == null) throw new IOException();

                    if (DataReceived != null) DataReceived.Invoke(this, new TCPDataReceivedEventArgs(client, incomingData));
                }

            }
            catch (IOException)
            {

            }
            catch (Exception)
            {

            }
            finally
            {
                client.Close();

                if (Connected && ServerDisconnected != null) ServerDisconnected.Invoke(this, new EventArgs());

                client.Dispose();
            }
        }


    }
}
