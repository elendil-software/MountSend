namespace MountSend.Commands.HomePark
{
    public class ParkCommand : ICommand<object>
    {
        private readonly CommandSender _sender;

        public ParkCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; } = "";

        public object Execute(string[] parameters = null)
        {
            _sender.SendCommand(":KA#");
            return null;
        }
    }
}
