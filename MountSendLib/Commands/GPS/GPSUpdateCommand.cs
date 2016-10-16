using System.Threading;

namespace MountSend.Commands.GPS
{
    public class GPSUpdateCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public GPSUpdateCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

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

        #endregion
    }
}
