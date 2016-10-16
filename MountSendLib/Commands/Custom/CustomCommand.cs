namespace MountSend.Commands.Custom
{
    public class CustomCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public CustomCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

        public double MinFirmwareVersion { get; } = 0;
        public string Command { get; set; } = "";
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(Command);
            return null;
        }

        #endregion
    }
}
