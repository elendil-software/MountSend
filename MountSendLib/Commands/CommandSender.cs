using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MountSend.Commands
{
    public class CommandSender
    {
        private readonly string _ipAddress;
        private readonly TcpClient _tcpClient = new TcpClient();
        private NetworkStream _stream;

        public CommandSender(string ipAddress)
        {
            this._ipAddress = ipAddress;
        }

        public void OpenConnection()
        {
            if (!_tcpClient.Connected)
            {
                _tcpClient.Connect(_ipAddress, 3492);
            }

            _stream = _tcpClient.GetStream();
        }

        public void CloseConnection()
        {
            _stream.Dispose();
            _tcpClient.Close();
        }
        
        public void SendCommand(string s)
        {
            Byte[] data = Encoding.ASCII.GetBytes(s);
            _stream.Write(data, 0, data.Length);
        }

        public string GetReply(int timeout)
        {
            string s = "";
            for (int i = 1; i <= timeout; i++)
            {
                if (_stream.DataAvailable)
                    break;
                Thread.Sleep(1);
            }

            while (_stream.DataAvailable)
            {
                s += (char)_stream.ReadByte();
            }

            return s;
        }
    }
}