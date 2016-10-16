namespace MountSend.Commands.Alignment
{
    public class StopTrackingCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public StopTrackingCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(":AL#");
            return null;
        }

        #endregion
    }
}
