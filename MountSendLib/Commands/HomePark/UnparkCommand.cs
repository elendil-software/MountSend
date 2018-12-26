namespace MountSend.Commands.HomePark
{
    public class UnparkCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public UnparkCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(":PO#");
            return null;
        }
    }
}
