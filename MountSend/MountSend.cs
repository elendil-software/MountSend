using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using MountSend.Commands;
using MountSend.Commands.Alignment;
using MountSend.Commands.Custom;
using MountSend.Commands.GPS;
using MountSend.Commands.HomePark;
using MountSend.Commands.Information;
using MountSend.Commands.Set;

namespace MountSend
{
    public static class MountSend
    {
        private static CommandSender _commandSender;

        public static void Main(string[] args)
        {
            int auto = 0;

            try
            {
                string ipAddress;

                if (args.Length < 1)
                {
                    Help();
                    Environment.Exit(0);
                }

                if (args[args.Length - 1] == "/a")
                {
                    ipAddress = Environment.GetEnvironmentVariable("MOUNT", EnvironmentVariableTarget.User);
                    if (ipAddress == null)
                    {
                        Console.WriteLine(
                            "?Cannot retrieve IP address of mount. Try saving it with the save command first.");
                        return;
                    }
                    auto = 1;
                }
                else
                {
                    ipAddress = args[0];
                }

                _commandSender = new CommandSender(ipAddress);
                _commandSender.OpenConnection();

                string commandString = args[1 - auto].Trim().ToLower();

                var result = ExecuteCommand(args, commandString, auto);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ExecuteCommand(string[] args, string commandString, int auto)
        {
            object command;
            object commandResult = null;
            string message = null;

            switch (commandString)
            {
                case "park":
                case "p":
                    new ParkCommand(_commandSender).Execute();
                    break;

                case "parkw":
                    command = new ParkAndWaitCommand(_commandSender);
                    commandResult = ((ParkAndWaitCommand) command).Execute();
                    message = ((ParkAndWaitCommand) command).Message;
                    break;

                case "unpark":
                case "up":
                    new UnparkCommand(_commandSender).Execute();
                    break;

                case "command":
                case "c":
                    new CustomCommand(_commandSender).Execute(new[] {args[2 - auto]});
                    break;

                case "waitstationary":
                    Status.WaitForStatus(_commandSender, MountState.Stationary, 30);
                    break;

                case "stat":
                    command = new StatusCommand(_commandSender);
                    commandResult = ((StatusCommand) command).Execute();
                    break;

                case "stop":
                    new StopTrackingCommand(_commandSender).Execute();
                    break;

                case "start":
                    new StartTrackingCommand(_commandSender).Execute();
                    break;

                case "maxslew":
                    command = new SetMaxSlewCommand(_commandSender);
                    commandResult = ((SetMaxSlewCommand) command).Execute(new[] {args[2 - auto]});
                    message = ((SetMaxSlewCommand) command).Message;
                    break;

                case "gpsupdate":
                    command = new GPSUpdateCommand(_commandSender);
                    commandResult = ((GPSUpdateCommand) command).Execute();
                    message = ((GPSUpdateCommand) command).Message;
                    break;

                case "move":
                case "movew":
                    MountState tracking = new StatusCommand(_commandSender).Execute();
                    string az = args[2 - auto];
                    string alt = args[3 - auto];
                    string enableTraking = tracking == MountState.Tracking && commandString == "movew" ? "1" : "0";
                    command = new SlewAltAzCommand(_commandSender);
                    commandResult = ((SlewAltAzCommand) command).Execute(new[] {az, alt, enableTraking});
                    message = ((SlewAltAzCommand) command).Message;
                    break;

                case "refr":
                case "refraction":
                case "r":
                    string pressure = args[2 - auto];
                    string temperature = args[3 - auto];
                    command = new SetRefractionCommand(_commandSender);
                    commandResult = ((SetRefractionCommand) command).Execute(new[] {pressure, temperature});
                    message = ((SetRefractionCommand) command).Message;
                    break;

                case "autorefr":
                case "ar":
                    if (File.Exists(args[2 - auto]))
                    {
                        StreamReader streamReader = new StreamReader(args[2 - auto]);
                        string[] data = streamReader.ReadLine()?.Split(' ');
                        string autoPressure = ParseDecimal(data?[10]);
                        string autoTemperature = ParseDecimal(data?[2]);
                        command = new SetRefractionCommand(_commandSender);
                        commandResult = ((SetRefractionCommand) command).Execute(new[] {autoPressure, autoTemperature});
                        message = ((SetRefractionCommand) command).Message;
                    }
                    else
                    {
                        message = "?Refraction file not found.";
                    }
                    break;

                case "time":
                    command = new SetTimeCommand(_commandSender);
                    commandResult = ((SetTimeCommand) command).Execute();
                    message = ((SetTimeCommand) command).Message;
                    break;

                case "save":
                    SaveIpAddress(args);
                    break;

                case "fw":
                    command = new FirmwareCommand(_commandSender);
                    commandResult = ((FirmwareCommand) command).Execute();
                    break;
                default:
                    Help();
                    Environment.Exit(0);
                    break;
            }

            var result = "";
            if (commandResult != null)
            {
                result += $"Result : {commandResult}\n";
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                result += $"{message}\n";
            }

            _commandSender.CloseConnection();
            return result;
        }

        private static void SaveIpAddress(string[] args)
        {
            try
            {
                Environment.SetEnvironmentVariable("MOUNT", args[0], EnvironmentVariableTarget.User);
                Console.WriteLine("Saved " + args[0] + " to 'MOUNT'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("?Error saving user variable (" + ex.Message);
            }
        }

        public static void Help()
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var description = "";

            var type = typeof(AssemblyDescriptionAttribute);

            if (Attribute.IsDefined(execAssembly, type))
            {
                var assemblyDescriptionAttribute =
                    (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(execAssembly, type);
                description = assemblyDescriptionAttribute.Description;
            }

            Console.WriteLine($@"{appName} v{version.Major}.{version.Minor}.{version.Build}");
            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(" mountsend <ip-address> <command> [<options>].");
            Console.WriteLine("     unpark            Unparks mount. No reply.");
            Console.WriteLine("     park              Parks mount. No reply.");
            Console.WriteLine("     parkw             Parks mount and waits for completion in 1 min. Replies success or timeout");
            Console.WriteLine("     stop              Stops tracking. No reply.");
            Console.WriteLine("     start             Starts tracking. No reply.");
            Console.WriteLine("     gpsupdate         Updates site info from GPS. Reports success or error.");
            Console.WriteLine("     waitstationary    Waits until all movement has stopped. No reply.");
            Console.WriteLine("     move <az> <alt>   Moves to AZ and ALT, leaves tracking off.");
            Console.WriteLine("     movew <az> <alt>  Moves and waits for completion, restores tracking if it was on.");
            Console.WriteLine("     maxslew <deg>     Set max slew rate. Reports error if invalid slew rate.");
            Console.WriteLine("     refr <hPa> <deg>  Sends refraction parameters to mount.");
            Console.WriteLine("     autorefr <file>   Reads to get refraction parameters, then sends them to mount.");
            Console.WriteLine("     time              Updates mount time from PC clock (both date and time).");
            Console.WriteLine("     save              Saves the mount IP in a user environment variable (MOUNT).");
            Console.WriteLine("     fw                Get the mount firmware version.");
            Console.WriteLine();
            Console.WriteLine("     NOTE! you can leave out the IP address and add '/a' as a last argument");
            Console.WriteLine("           to have mountsend use the environment variable 'MOUNT' for getting");
            Console.WriteLine("           the mount address.");


            Console.WriteLine();
        }


        public static string ParseDecimal(string s)
        {
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string k = s.Replace(",", decimalSeparator);
            k = k.Replace(".", decimalSeparator);
            return k.Trim();
        }
    }
}
