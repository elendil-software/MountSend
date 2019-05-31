using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MountSend.Commands.Network
{
    public class WakeOnLanCommand : ICommand<object>
    {
        public double MinFirmwareVersion { get; } = 2.1507;
        public string Message { get; private set; } = "";
        public object Execute(string[] parameters = null)
        {
            if (parameters == null || parameters.Length < 1)
            {
                Message = "Parameter is mandatory, it must contains valid MAC address";
                return false;
            }
            
            string macAddress = Regex.Replace(parameters[0], @"[^0-9A-Fa-f]", "");

            using (var udpClient = new UdpClient())
            {
                udpClient.Connect(IPAddress.Parse("255.255.255.255"), 0x2fff);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);

                byte[] magicPacket = BuildMagicPacket(macAddress);

                udpClient.Send(magicPacket, magicPacket.Length);
            }

            return null;
        }

        private byte[] BuildMagicPacket(string macAddress)
        {
            byte[] magicPacket = new byte[102]; 
            int byteCount = 0;
            
            for (int i = 0; i < 6; i++)
            {
                magicPacket[byteCount++] = 0xFF;
            }

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 12; j+=2)
                {
                    magicPacket[byteCount++] = byte.Parse(macAddress.Substring(j, 2), NumberStyles.HexNumber);
                }
            }

            return magicPacket;
        }
    }
}
