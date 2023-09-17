using LiteTCP.Events;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Server
{
    public class LiteTCPServer
    {
        private TcpListener listener;
        public bool Listening { get; private set; } = false;

        public LiteTCPServer(IPAddress IP, int Port) => listener = new TcpListener(IP, Port);
        public LiteTCPServer(IPEndPoint endPoint) => listener = new TcpListener(endPoint);


        #region Start / Stop
        public void StopListening()
        {
            if (!Listening) throw new Exception("Server was not Listening");

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
                while (true)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        _ = HandleIncomingAsync(client);
                    }
                    catch (Exception ex)
                    {
                        if (ServerErrored != null) ServerErrored(this, ex);
                    }
                    finally
                    {
                        StopListening();
                    }
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
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return await SendAsync(client, encoding.GetBytes(data));
        }
        #endregion

        #region Events
        public event EventHandler<TcpClient> ClientConnected;
        public event EventHandler<TcpClient> ClientDisconnected;
        public event EventHandler<TCPDataReceivedEventArgs> DataReceived;
        public event EventHandler<Exception> ServerErrored;
        #endregion


        private async Task HandleIncomingAsync(TcpClient client)
        {
            try
            {
                if (ClientConnected != null) ClientConnected.Invoke(this, client);

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

                client.Close();
                client.Dispose();
            }




        }
    }

}
