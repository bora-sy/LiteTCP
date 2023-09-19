using LiteTCP.Server;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Events
{
    /// <summary>
    /// TCPServerDataReceivedEventArgs
    /// </summary>
    public class TCPServerDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Lite TCP Server
        /// </summary>
        public LiteTCPServer LiteServer { get; private set; }

        /// <summary>
        /// TCP Client
        /// </summary>
        public TcpClient tcpClient { get; private set; }


        /// <summary>
        /// Incoming Data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Converts the incoming data to a string using the given encoding
        /// </summary>
        /// <param name="encoding">The encoding that'll be used for converting</param>
        /// <returns>Incoming data in string format</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetDataAsString(Encoding encoding)
        {
            if(encoding == null) throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(Data);
        }


        /// <summary>
        /// Creates a response to the incoming data
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        public async Task<bool> CreateResponseAsync(byte[] data)
        {
            return await LiteServer.SendAsync(tcpClient, data);
        }

        /// <summary>
        /// Creates a response to the incoming data
        /// </summary>
        /// <param name="text">Text data</param>
        /// <param name="encoding">The encoding that's used for the text</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        public async Task<bool> CreateResponseAsync(string text, Encoding encoding)
        {
            return await CreateResponseAsync(encoding.GetBytes(text));
        }


        /// <summary>
        /// TCPServerDataReceivedEventArgs
        /// </summary>
        /// <param name="server">Lite TCP LiteServer</param>
        /// <param name="data">Data in byte[] format</param>
        /// <param name="client">TCP Client</param>
        public TCPServerDataReceivedEventArgs(LiteTCPServer server, TcpClient client, byte[] data)
        {
            this.tcpClient = client;
            this.LiteServer = server;
            this.Data = data;
        }
    }
}
