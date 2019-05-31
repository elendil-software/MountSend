namespace MountSend.Commands.Network
{
    /// <summary>
    /// Get the wake-on-LAN configuration.
    /// <para>Wake-on-LAN is available only on Q-TYPE2012 or Q-TYPE2016 control boxes.</para>
    /// </summary>
    public class GetWakeOnLanCommand : ICommand<WakeOnLanConfiguration>
    {
        private readonly string _command = ":GWOL#";
        private readonly CommandSender _sender;

        public GetWakeOnLanCommand(CommandSender sender)
        {
            _sender = sender;
        }
        
        public double MinFirmwareVersion { get; } = 2.1507;
        public string Message { get; private set; } = "";
        
        public WakeOnLanConfiguration Execute(string[] parameters = null)
        {
            _sender.SendCommand(_command);
            string reply = _sender.GetReply(1000);

            switch (reply)
            {
                case "N#":
                    return WakeOnLanConfiguration.NotAvailable;
                case "0#":
                    return WakeOnLanConfiguration.NotActive;
                case "1#":
                    return WakeOnLanConfiguration.Active;
                
                default:
                    return WakeOnLanConfiguration.NotAvailable;
            }
        }
    }
}