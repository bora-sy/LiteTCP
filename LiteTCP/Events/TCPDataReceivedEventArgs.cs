using System;
using System.Net.Sockets;
using System.Text;

namespace LiteTCP.Events
{
    /// <summary>
    /// TCPDataReceivedEventArgs
    /// </summary>
    public class TCPDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The TCP Client
        /// </summary>
        public TcpClient Client { get; private set; }

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
        /// TCPDataReceivedEventArgs
        /// </summary>
        /// <param name="client">TCP Client</param>
        /// <param name="data">Data in byte[] format</param>
        public TCPDataReceivedEventArgs(TcpClient client, byte[] data)
        {
            this.Client = client;
            this.Data = data;
        }
    }
}
