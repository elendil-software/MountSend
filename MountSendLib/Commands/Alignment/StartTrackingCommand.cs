namespace MountSend.Commands.Alignment
{
    public class StartTrackingCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public StartTrackingCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(":AP#");
            return null;
        }
    }
}
