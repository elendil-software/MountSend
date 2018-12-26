namespace MountSend.Commands.Information
{
    public class StatusCommand : ICommand<MountState>
    {
        private readonly CommandSender _sender;

        public StatusCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 2.0808;
        public string Message { get; } = "";

        public MountState Execute(string[] parameters = null)
        {
            _sender.SendCommand(":Gstat#");
            string reply = _sender.GetReply(1000);

            switch (reply)
            {
                case "0#":
                    return MountState.Tracking;
                case "1#":
                    return MountState.StoppedOrHomed;
                case "2#":
                    return MountState.Parking;
                case "3#":
                    return MountState.Unparking;
                case "4#":
                    return MountState.SlewingHome;
                case "5#":
                    return MountState.Parked;
                case "6#":
                    return MountState.Slewing;
                case "7#":
                    return MountState.Stationary;
                case "9#":
                    return MountState.OutsideTrackLimits;
                case "11#":
                    return MountState.NeedsOk;
                case "99#":
                    return MountState.MountError;
                default:
                    return MountState.NoReply;
            }
        }
    }
}
