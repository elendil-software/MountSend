namespace MountSend.Commands.Network
{
    /// <summary>
    /// Set the wake-on-LAN configuration to active (N=1) or inactive (N=0).
    /// <para>Available from version 2.15.7</para>
    /// </summary>
    public class SetWakeOnLanCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public SetWakeOnLanCommand(CommandSender sender)
        {
            _sender = sender;
        }
        
        public double MinFirmwareVersion { get; } = 2.1507;
        public string Message { get; private set; } = "";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>0 invalid (also in case wake-on-LAN is not available), 1 valild</returns>
        public bool Execute(string[] parameters = null)
        {
            if (parameters == null || parameters.Length < 1)
            {
                Message = "Parameter is mandatory (active : 1 or inactive : 0)";
                return false;
            }

            string enabled = "0";

            if (parameters[0] == "1")
            {
                enabled = "1";
            }
            
            _sender.SendCommand($":SWOL{enabled}#");
            string reply = _sender.GetReply(1000);
            return reply == "1";
        }
    }
}