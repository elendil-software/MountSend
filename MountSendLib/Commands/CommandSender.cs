using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MountSend.Commands
{
    public class CommandSender
    {
        private string ipAddress;
        private TcpClient tcpClient = new TcpClient();
        private NetworkStream stream;

        public CommandSender(string ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        public void OpenConnection()
        {
            if (!tcpClient.Connected)
            {
                tcpClient.Connect(ipAddress, 3492);
            }

            stream = tcpClient.GetStream();
        }

        public void CloseConnection()
        {
            stream.Dispose();
            tcpClient.Close();
        }
        
        public void SendCommand(string s)
        {
            Byte[] data = Encoding.ASCII.GetBytes(s);
            stream.Write(data, 0, data.Length);
        }

        public string GetReply(int timeout)
        {
            //stream.ReadTimeout = timeout
            string s = "";
            for (int i = 1; i <= timeout; i++)
            {
                if (stream.DataAvailable)
                    break; // TODO: might not be correct. Was : Exit For
                Thread.Sleep(1);
            }

            while (stream.DataAvailable)
            {
                s += (char)stream.ReadByte();
            }

            return s;
        }
    }
}