namespace MountSend.Commands.Set
{
    public class SetMaxSlewCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public SetMaxSlewCommand(CommandSender sender)
        {
            _sender = sender;
        }

        #region Implementation of ICommand

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters = null)
        {
            if (parameters == null || parameters.Length < 1)
            {
                Message = "Rate parameter is mandatory";
                return false;
            }

            var rate = parameters[0];
            _sender.SendCommand($":Sw{rate.Trim()}#");

            if (_sender.GetReply(1000) == "0")
            {
                Message = $"Rate {rate} is not a valid value";
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion
    }
}