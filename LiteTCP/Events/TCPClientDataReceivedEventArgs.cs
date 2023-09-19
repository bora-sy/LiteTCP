using LiteTCP.Client;
using LiteTCP.Server;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiteTCP.Events
{
    /// <summary>
    /// TCPServerDataReceivedEventArgs
    /// </summary>
    public class TCPClientDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Lite TCP Client
        /// </summary>
        public LiteTCPClient LiteClient { get; private set; }

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
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(Data);
        }

        /// <summary>
        /// Creates a response to the incoming data
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>True if the data sending was successfull; otherwise, false</returns>
        public async Task<bool> CreateResponseAsync(byte[] data)
        {
            return await LiteClient.SendAsync(data);
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
        /// <param name="liteClient">Lite TCP Client</param>
        /// <param name="data">Data in byte[] format</param>
        public TCPClientDataReceivedEventArgs(LiteTCPClient liteClient, byte[] data)
        {
            this.LiteClient = liteClient;
            this.Data = data;
        }
    }
}
