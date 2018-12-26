namespace MountSend.Commands.Information
{
    public class FirmwareCommand : ICommand<decimal>
    {
        private readonly CommandSender _sender;

        public FirmwareCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 2.0808;
        public string Message { get; } = "";

        public decimal Execute(string[] parameters = null)
        {
            //Read mount firmware version.
            //Firmware version is stored as a decimal.
            //2.9.9 yields 2.0909, 2.9.19 yields 2.0919 and 2.11.3 yields 2.1103

            _sender.SendCommand(":GVN#");
            string s = _sender.GetReply(1000);
            string[] ss = s.Replace("#", "").Split('.');
            decimal firmWareVersion = decimal.Parse(ss[0]);
            if (ss.GetUpperBound(0) > 0)
                firmWareVersion += decimal.Parse(ss[1]) / 100;
            if (ss.GetUpperBound(0) > 1)
                firmWareVersion += decimal.Parse(ss[2]) / 10000;

            return firmWareVersion;
        }
    }
}
