using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using LiteTCP.Server;
using LiteTCP.Events;

namespace Demo_Server
{
    internal class Program
    {
        private static LiteTCPServer server;
        static async Task Main(string[] args)
        {
            server = new LiteTCPServer(IPAddress.Parse("127.0.0.1"), 8080);

            Console.WriteLine("Started Listening");

            server.DataReceived += Server_DataReceived;

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            server.StartListening();

            await Task.Delay(-1); // To prevent the console from closing
        }

        private static async void Server_DataReceived(object sender, TCPDataReceivedEventArgs e)
        {
            string data = e.GetDataAsString(Encoding.UTF8);
            Console.WriteLine($"Received data: " + data);

            string response = "Echo " + data;

            await server.SendAsync(e.Client, response, Encoding.UTF8);

        }

        private static async void Server_ClientConnected(object sender, TcpClient e)
        {
            Console.WriteLine($"New client connected");

            await server.SendAsync(e, "Welcome!", Encoding.UTF8); // Sending a welcome message to the client
        }

        private static void Server_ClientDisconnected(object sender, TcpClient e)
        {
            Console.WriteLine($"A client disconnected");
        }
    }
}
