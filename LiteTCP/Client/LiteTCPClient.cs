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

        /// <summary>
        /// Lite TCP Client
        /// </summary>
        public LiteTCPClient() => client = new TcpClient();


        #region Connect / Disconnect
        /// <summary>
        /// Connects the client to a server
        /// </summary>
        /// <param name="IP">IP Address</param>
        /// <param name="Port">Port</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Connect(IPAddress IP, int Port)
        {
            if (IP == null) throw new ArgumentNullException(nameof(IP));
            if (Connected) throw new Exception("Client is already Connected");

            IPEndPoint ep = new IPEndPoint(IP, Port);
            Connect(ep);
        }

        /// <summary>
        /// Connects the client to a server
        /// </summary>
        /// <param name="EP">Endpoint</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Connect(IPEndPoint EP)
        {
            if (EP == null) throw new ArgumentNullException(nameof(EP));
            if (Connected) throw new Exception("Client is already Connected");

            client.Connect(EP);
            Task.Run(() => HandleIncomingAsync());
        }

        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Disconnect()
        {
            if (!Connected) throw new Exception("Client is not Connected");
            if (ClientDisconnected != null) ClientDisconnected.Invoke(this, new EventArgs());
            client.Close();
        }
        #endregion

        #region Events
        /// <summary>
        /// The event that'll be triggered when a data is received from the server
        /// </summary>
        public event EventHandler<TCPDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// The event that'll be triggered when the client is disconnected from the server
        /// </summary>
        public event EventHandler<EventArgs> ClientDisconnected;
        #endregion

        #region Send Data

        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="text">Text to send</param>
        /// <param name="encoding">The encoding that's used for the text</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> SendAsync(string text, Encoding encoding)
        {
            if (!Connected) throw new Exception("Client is not Connected");
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await SendAsync(encoding.GetBytes(text));
        }


        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="data">Bytes to send</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
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

                if (Connected && ClientDisconnected != null) ClientDisconnected.Invoke(this, new EventArgs());

                client.Dispose();
            }
        }


    }
}
