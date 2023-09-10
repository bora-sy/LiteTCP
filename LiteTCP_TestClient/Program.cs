using LiteTCP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP_TestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press {ENTER} to connect");
            Console.ReadLine();

            LiteTCPClient client = new LiteTCPClient();

            client.DataReceived += Client_DataReceived;


            client.Connect(IPAddress.Parse("127.0.0.1"), 8080);

            Console.WriteLine("Connected");

            while(true)
            {
                string inp = Console.ReadLine();

                byte[] data = Encoding.UTF8.GetBytes(inp);

                client.sendData(data);
                Console.WriteLine("Sent");
            }
        }

        private static void Client_DataReceived(object sender, byte[] e)
        {
            throw new NotImplementedException();
        }
    }
}
