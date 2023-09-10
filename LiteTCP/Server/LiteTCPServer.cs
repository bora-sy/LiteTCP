using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteTCP.Server
{
    public class LiteTCPServer
    {
        private TcpListener listener;


        public LiteTCPServer(IPAddress IP, int Port) => listener = new TcpListener(IP, Port);
        public LiteTCPServer(IPEndPoint endPoint) => listener = new TcpListener(endPoint);

        private bool listening = false;

        public void StopListening()
        {
            if (!listening) throw new Exception("Server was not listening");

            listener.Stop();
            listening = false;
        }

        public void StartListening()
        {
            if (listening) throw new Exception("Server is already listening");

            listener.Start();
            listening = true;

            Task.Run(async() =>
            {
                while(true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    _ = HandleIncomingAsync(client);

                }
            });
        }


        public event EventHandler<TcpClient> ClientConnected;
        public event EventHandler<TcpClient> ClientDisconnected;

        public event EventHandler<TCPData> DataReceived;



        private async Task HandleIncomingAsync(TcpClient client)
        {
            try
            {
                if (ClientConnected != null) ClientConnected.Invoke(this, client);

                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    byte[] data = Utils.readDataFromNetworkStream(stream);

                    if (DataReceived != null) DataReceived.Invoke(this, new TCPData(client, data));
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
                if (ClientDisconnected != null) ClientDisconnected.Invoke(this, client);
                
                client.Dispose();
            }




        }




        public class TCPData
        {
            public TCPData(TcpClient Client, byte[] Data)
            {
                this.Client = Client;
                this.Data = Data;
            }
            public TcpClient Client { get; internal set; }
            public byte[] Data { get; internal set; }
        }

    }

}
