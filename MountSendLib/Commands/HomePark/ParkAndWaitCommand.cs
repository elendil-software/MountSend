using System.Threading;

namespace MountSend.Commands.HomePark
{
    public class ParkAndWaitCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public ParkAndWaitCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters = null)
        {
            _sender.SendCommand(":KA#");
            Thread.Sleep(100);
            var succeeded = Status.WaitForStatus(_sender, MountState.Parked, 60);
            Message = succeeded ? "Parked OK" : "Timeout";
            return succeeded;
        }

        #endregion
    }
}
