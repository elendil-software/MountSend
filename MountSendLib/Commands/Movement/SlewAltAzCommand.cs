using MountSend.Commands.Alignment;

namespace MountSend.Commands.Movement
{
    public class SlewAltAzCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public SlewAltAzCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters = null)
        {
            if (parameters == null || parameters.Length < 3)
            {
                Message = "Az, Alt and enableTraking parameters are mandatory";
                return false;
            }

            var az = parameters[0];
            var alt = parameters[1];
            var enableTraking = parameters[2] == "1";

            _sender.SendCommand($":Sa{AltString(alt)}*00#");
            string rep = _sender.GetReply(1000);
            
            if (rep == "1")
            {
                _sender.SendCommand($":Sz{az.Trim()}*00#");
                rep = _sender.GetReply(1000);

                if (rep == "1")
                {
                    _sender.SendCommand(":MA#");
                    string q = _sender.GetReply(1000);

                    if (q.Left(1) == "0")
                    {
                        if (enableTraking)
                        {
                            if (!Status.WaitForStatus(_sender, MountState.Stationary, 60))
                            {
                                Message = "?Timeout";
                                return false;
                            }

                            new StartTrackingCommand(_sender).Execute();
                            return true;
                        }

                        return true;
                    }
                    else
                    {
                        Message = q;
                        return false;
                    }
                }
                else
                {
                    Message = "Coordinates may not be reachable";
                    return false;
                }
            }
            else
            {
                Message = "Coordinates may not be reachable";
                return false;
            }
        }

        private static string AltString(string alt)
        {
            return alt.Contains("-") ? alt : $"+{alt}";
        }
    }
}
