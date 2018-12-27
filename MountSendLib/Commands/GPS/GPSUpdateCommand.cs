using System.Threading;

namespace MountSend.Commands.GPS
{
    public class GpsUpdateCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public GpsUpdateCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters = null)
        {
            _sender.SendCommand(":gT#");
            Thread.Sleep(100);
            var succeeded = _sender.GetReply(30000) == "1";
            Message = succeeded ? "Updated" : "?Error";
            return succeeded;
        }
    }
}
