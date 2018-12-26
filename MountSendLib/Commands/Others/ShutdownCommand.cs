namespace MountSend.Commands.Others
{
    public class ShutdownCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public ShutdownCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 2.0902;
        public string Message { get; } = "";

        public bool Execute(string[] parameters = null)
        {
            _sender.SendCommand(":shutdown#");
            return _sender.GetReply(1000) != "0";
        }
    }
}
