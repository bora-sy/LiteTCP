using LiteTCP.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Server
{
    /// <summary>
    /// Lite TCP Server
    /// </summary>
    public class LiteTCPServer
    {
        private List<TcpClient> Clients = new List<TcpClient>();

        private TcpListener listener;

        /// <summary>
        /// Whether the server is currently listening or not
        /// </summary>
        public bool Listening { get; private set; } = false;

        /// <summary>
        /// Lite TCP Server
        /// </summary>
        /// <param name="IP">IP Address</param>
        /// <param name="Port">Port</param>
        public LiteTCPServer(IPAddress IP, int Port) => listener = new TcpListener(IP, Port);

        /// <summary>
        /// Lite TCP Server
        /// </summary>
        /// <param name="endPoint">Endpoint</param>
        public LiteTCPServer(IPEndPoint endPoint) => listener = new TcpListener(endPoint);

        #region Start / Stop

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void StopListening()
        {
            if (!Listening) throw new Exception("Server is not Listening");

            Clients.Clear();

            listener.Stop();
            Listening = false;
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void StartListening()
        {
            if (Listening) throw new Exception("Server is already Listening");

            listener.Start();
            Listening = true;

            var A = Task.Run(() =>
            {
                try
                {
                    while (Listening)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        _ = HandleIncomingAsync(client);
                    }
                }
                catch (Exception ex)
                {
                    if(Listening && ServerErrored != null) ServerErrored(this, ex);
                }
                finally
                {
                    StopListening();
                }
            });

        }

        #endregion

        #region Send Data

        /// <summary>
        /// Sends data to the given client
        /// </summary>
        /// <param name="client">Receiver Client</param>
        /// <param name="data">Data to send</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> SendAsync(TcpClient client, byte[] data)
        {
            if (!Listening) throw new Exception("Server is not listening");
            if (!client.Connected) throw new Exception("Client is not Connected");

            if (data == null) throw new ArgumentNullException(nameof(data));

            try
            {
                NetworkStream stream = client.GetStream();
                bool res = await NetworkStreamUtils.writeDataToNetworkStreamAsync(stream, data);
                return res;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends data to the given client
        /// </summary>
        /// <param name="client">Receiver Client</param>
        /// <param name="data">Text data</param>
        /// <param name="encoding">The encoding that's used for the text</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> SendAsync(TcpClient client, string data, Encoding encoding)
        {
            if (!Listening) throw new Exception("Server is not listening");
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await SendAsync(client, encoding.GetBytes(data));
        }

        #endregion

        #region Broadcast

        /// <summary>
        /// Broadcasts the given data to the all connected clients
        /// </summary>
        /// <param name="data">Text to broadcast</param>
        /// <param name="encoding">The encoding that's used for the text</param>
        /// <returns>A dictionary where the keys are the clients and the values indicate whether the data sending was successful</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Dictionary<TcpClient, bool>> BroadcastAsync(string data, Encoding encoding)
        {
            if (!Listening) throw new Exception("Server is not listening");

            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await BroadcastAsync(encoding.GetBytes(data));
        }

        /// <summary>
        /// Broadcasts the given data to the all connected clients
        /// </summary>
        /// <param name="bytes">Data to broadcast</param>
        /// <returns>A dictionary where the keys are the clients and the values indicate whether the data sending was successful</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Dictionary<TcpClient, bool>> BroadcastAsync(byte[] bytes)
        {
            if (!Listening) throw new Exception("Server is not listening");

            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var clients = GetConnectedClients();


            Task<bool>[] tasks = new Task<bool>[clients.Count];

            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];

                tasks[i] = SendAsync(client, bytes);
            }

            bool[] successValues = await Task.WhenAll<bool>(tasks);

            Dictionary<TcpClient, bool> dict = new Dictionary<TcpClient, bool>();

            for(int i = 0; i < clients.Count; i++)
            {
                dict.Add(clients[i], successValues[i]);
            }

            return dict;
        }

        #endregion

        #region Events

        /// <summary>
        /// The event that'll be triggered when a client connects to the server
        /// </summary>
        public event EventHandler<TcpClient> ClientConnected;

        /// <summary>
        /// The event that'll be triggered when a client disconnects from the server
        /// </summary>
        public event EventHandler<TcpClient> ClientDisconnected;

        /// <summary>
        /// The event that'll be triggered when a data is received from a client
        /// </summary>
        public event EventHandler<TCPDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// The event that'll be triggered when a critical exception is thrown. When triggered, the server stops listening automatically
        /// </summary>
        public event EventHandler<Exception> ServerErrored;
        #endregion

        /// <summary>
        /// Gets all the currently connected clients
        /// </summary>
        /// <returns>Currently connected clients inside a list</returns>
        /// <exception cref="Exception"></exception>
        public IReadOnlyList<TcpClient> GetConnectedClients()
        {
            if (!Listening) throw new Exception("Server is not listening");

            return Clients.AsReadOnly();
        }

        private async Task HandleIncomingAsync(TcpClient client)
        {
            try
            {
                if (ClientConnected != null) ClientConnected.Invoke(this, client);
                Clients.Add(client);
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
                if (ClientDisconnected != null) ClientDisconnected.Invoke(this, client);
                Clients.Remove(client);
                client.Close();
                client.Dispose();
            }




        }
    }

}
