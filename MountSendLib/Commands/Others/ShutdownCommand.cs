namespace MountSend.Commands.Others
{
    public class ShutdownCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public ShutdownCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(":shutdown#");
            return null;
        }

        #endregion
    }
}
