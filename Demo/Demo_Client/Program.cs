using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using LiteTCP.Client;
using LiteTCP.Events;

namespace Demo_Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press {ENTER} to start the client");
            Console.ReadLine();

            LiteTCPClient client = new LiteTCPClient();

            client.DataReceived += Client_DataReceived;

            client.Connect(IPAddress.Parse("127.0.0.1"), 8080);
            Console.WriteLine("Connected to the server");

            while(true)
            {
                string dataToSend = Console.ReadLine();

                await client.SendAsync(dataToSend, Encoding.UTF8);
            }

        }

        private static void Client_DataReceived(object sender, TCPDataReceivedEventArgs e)
        {
            string data = e.GetDataAsString(Encoding.UTF8);
            Console.WriteLine("Received data: " + data);
        }
    }
}
