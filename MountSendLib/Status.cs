using System.Threading;
using MountSend.Commands;
using MountSend.Commands.Information;

namespace MountSend
{
    public static class Status
    {
        public static bool WaitForStatus(CommandSender sender, MountState stat, int timeout)
        {
            //Wait for a certain status to be obtained for a specified number of seconds
            int t = timeout * 4;
            while (t > 0)
            {
                var s = new StatusCommand(sender).Execute();
                if (s != stat)
                {
                    Thread.Sleep(250);
                    t -= 1;
                }
                else
                {
                    break;
                }
            }

            return t > 0;
        }
    }
}