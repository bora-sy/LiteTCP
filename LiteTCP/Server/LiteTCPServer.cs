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
    public class LiteTCPServer
    {
        private List<TcpClient> Clients = new List<TcpClient>();

        private TcpListener listener;
        public bool Listening { get; private set; } = false;

        public LiteTCPServer(IPAddress IP, int Port) => listener = new TcpListener(IP, Port);
        public LiteTCPServer(IPEndPoint endPoint) => listener = new TcpListener(endPoint);

        #region Start / Stop
        public void StopListening()
        {
            if (!Listening) throw new Exception("Server is not Listening");

            Clients.Clear();

            listener.Stop();
            Listening = false;
        }

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
        public async Task<bool> SendAsync(TcpClient client, string data, Encoding encoding)
        {
            if (!Listening) throw new Exception("Server is not listening");
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await SendAsync(client, encoding.GetBytes(data));
        }
        #endregion

        #region Broadcast
        public async Task<Dictionary<TcpClient, bool>> BroadcastAsync(string data, Encoding encoding)
        {
            if (!Listening) throw new Exception("Server is not listening");

            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await BroadcastAsync(encoding.GetBytes(data));
        }

        public async Task<Dictionary<TcpClient, bool>> BroadcastAsync(byte[] bytes)
        {
            if (!Listening) throw new Exception("Server is not listening");

            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var clients = GetListeningClients();


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
        public event EventHandler<TcpClient> ClientConnected;
        public event EventHandler<TcpClient> ClientDisconnected;
        public event EventHandler<TCPDataReceivedEventArgs> DataReceived;
        public event EventHandler<Exception> ServerErrored;
        #endregion

        public IReadOnlyList<TcpClient> GetListeningClients()
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
