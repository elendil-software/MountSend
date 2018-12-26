using System;
using System.Diagnostics;

namespace MountSend.Commands.Set
{
    public class SetTimeCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public SetTimeCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters = null)
        {
            var sw = new Stopwatch();
            var currentTime = DateTime.Now;
            sw.Start();
            _sender.SendCommand($":SL{currentTime:HH:mm:ss.ff}#");
            string rep = _sender.GetReply(1000);
            sw.Stop();

            Message = $"Set to {currentTime:HH:mm:ss.ff}\nTook {sw.ElapsedMilliseconds:0} ms\n";

            if (rep.Left(1) == "1")
            {
                _sender.SendCommand($":SC{currentTime:MM\\/dd\\/yyyy}#");
                rep = _sender.GetReply(1000);
                bool succeded = rep.Left(1) != "0";
                Message += succeded ? "Updated" : "?Error";
                return succeded;
            }
            else
            {
                Message += "?Error";
                return false;
            }
       }
    }
}