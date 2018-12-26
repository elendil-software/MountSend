using System.Globalization;

namespace MountSend.Commands.Set
{
    public class SetRefractionCommand : ICommand<bool>
    {
        private readonly CommandSender _sender;

        public SetRefractionCommand(CommandSender sender)
        {
            _sender = sender;
        }

        public double MinFirmwareVersion { get; } = 0;
        public string Message { get; private set; } = "";

        public bool Execute(string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                Message = "Parameters are mandatory";
                return false;
            }

            var hpa = parameters[0];
            var tmp = parameters[1];

            _sender.SendCommand(":SRTMP" + MountDecimal(decimal.Parse(tmp)) + "#");
            string rep = _sender.GetReply(1000);
            if (rep == "1")
            {
                _sender.SendCommand(":SRPRS" + MountDecimal(decimal.Parse(hpa)) + "#");
                
                if (_sender.GetReply(1000) == "1")
                {
                    return true;
                }
                else
                {
                    Message = "?Refraction pressure invalid";
                    return false;
                }
            }
            else
            {
                Message = "?Refraction temp invalid";
                return false;
            }
        }

        public static string MountDecimal(decimal n)
        {
            var str = string.Format(n.ToString(CultureInfo.InvariantCulture), "0.0");
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            return str.Trim().Replace(decimalSeparator, ".");
        }
    }
}