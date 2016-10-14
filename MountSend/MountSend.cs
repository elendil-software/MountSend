using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MountSend
{
    public static class MountSend
    {
        public const string Version = "v2.10";
        private const string AppName = "10Micron Mount Fixer by Per Frejvall";
        private static NetworkStream _stream;
        private static string _decimalSeparator;


        public static void Main(string[] args)
        {
            //args =
            //{
            //};

            _decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            int auto = 0;


            try {
                string ipAddress;
                decimal hpa;
                decimal tmp;
                string rep;

                TcpClient tcp = new TcpClient();
                if (args.Length < 1) {
                    Help();
                    Environment.Exit(0);
                }

                //See if we are to find the IP address in environment
	        

                if (args[args.Length - 1] == "/a") {
                    ipAddress = Environment.GetEnvironmentVariable("MOUNT", EnvironmentVariableTarget.User);
                    if (ipAddress == null) {
                        Console.WriteLine("?Cannot retrieve IP address of mount. Try saving it with the save command first.");
                        return;
                    }
                    auto = 1;
                } else {
                    ipAddress = args[0];
                }

                tcp.Connect(ipAddress, 3492);
                _stream = tcp.GetStream();

                string command = args[1 - auto].Trim().ToLower();
            

                switch (command) {

                    case "park":
                    case "p":
                        SendCommand(":KA#");

                        break;
                    case "parkw":
                        SendCommand(":KA#");
                        Thread.Sleep(100);
                        Console.WriteLine(WaitForStatus(MountState.parked, 60) ? "Parked OK" : "Timeout");

                        break;
                    case "unpark":
                    case "up":
                        SendCommand(":PO#");

                        break;
                    case "command":
                    case "c":
                        SendCommand(args[2 - auto]);

                        break;
                    case "waitstationary":
                        WaitForStatus(MountState.stationary, 30);

                        break;
                    case "stat":
                        Console.WriteLine(GetStatus().ToString());

                        break;
                    case "stop":
                        SendCommand(":AL#");

                        break;
                    case "start":
                        SendCommand(":AP#");

                        break;
                    case "maxslew":
                        int rate = int.Parse(args[2 - auto]);
                        SendCommand(":Sw" + rate.ToString().Trim() + "#");
                        if (GetReply(1000) == "0") {
                            Console.WriteLine("?Invalid rate");
                        }

                        break;
                    case "gpsupdate":
                        SendCommand(":gT#");
                        Console.WriteLine(GetReply(30000) == "1" ? "Updated" : "?Error");

                        break;
                    case "move":
                    case "movew":
                        //Start by getting tracking state
                        MountState tracking = GetStatus();

                        int az = int.Parse(args[2 - auto]);
                        int alt = int.Parse(args[3 - auto]);
                        SendCommand(":Sa" + AltString(alt) + "*00#");
                        rep = GetReply(1000);
                        Debug.WriteLine("Sa: " + rep);
                        if (rep == "1") {
                            SendCommand(":Sz" + az.ToString().Trim() + "*00#");
                            rep = GetReply(1000);
                            Debug.WriteLine("Sz: " + rep);
                            if (rep == "1") {
                                SendCommand(":MA#");
                                string q = GetReply(1000);
                                Debug.WriteLine("MA: " + q);
                                if (q.Left(1) != "0") {
                                    Console.WriteLine("? " + q);
                                    Environment.Exit(0);
                                }
                            } else {
                                Console.WriteLine("?Coordinates may not be reachable");
                                Environment.Exit(0);
                            }
                        } else {
                            Console.WriteLine("?Coordinates may not be reachable");
                            Environment.Exit(0);
                        }
                        if (command == "movew") {
                            if (!WaitForStatus(MountState.stationary, 60)) {
                                Console.WriteLine("?Timeout");
                            }
                            if (tracking == MountState.tracking) {
                                SendCommand(":AP#");
                                //re-instate tracking if it was on
                            }
                        }

                        break;
                    case "refr":
                    case "refraction":
                    case "r":
                        hpa = decimal.Parse(args[2 - auto]);
                        tmp = decimal.Parse(args[3 - auto]);
                        SendCommand(":SRTMP" + MountDecimal(tmp) + "#");
                        rep = GetReply(1000);
                        if (rep == "1") {
                            SendCommand(":SRPRS" + MountDecimal(hpa) + "#");
                            string q = GetReply(1000);
                            if (q != "1") {
                                Console.WriteLine("?Refraction pressure invalid");
                            }
                        } else {
                            Console.WriteLine("?Refraction temp invalid");
                        }

                        break;
                    case "autorefr":
                    case "ar":
                        //Try to find file
                        if (File.Exists(args[2 - auto])) {
                            StreamReader streamReader = new StreamReader(args[2 - auto]);
                            string[] data = streamReader.ReadLine().Split(' ');
                            hpa = ParseDecimal(data[10]);
                            tmp = ParseDecimal(data[2]);
                            SendCommand(":SRTMP" + MountDecimal(tmp) + "#");
                            rep = GetReply(1000);
                            if (rep == "1") {
                                SendCommand(":SRPRS" + MountDecimal(hpa) + "#");
                                string q = GetReply(1000);
                                if (q != "1") {
                                    Console.WriteLine("?Refraction pressure invalid");
                                }
                            } else {
                                Console.WriteLine("?Refraction temp invalid");
                            }
                        } else {
                            Console.WriteLine("?Refraction file not found.");
                        }

                        break;
                    case "time":
                        //New behavior: do it directly with high precision
                        Stopwatch sw = new Stopwatch();

                        DateTime currenttime = DateTime.Now;
                        sw.Start();
                        SendCommand((":SL" + $"{currenttime:HH:mm:ss.ff}" + "#"));
                        rep = GetReply(1000);
                        sw.Stop();

                        Console.WriteLine("  Set to  " + $"{currenttime:HH:mm:ss.ff}");
                        Console.WriteLine("  Took " + sw.ElapsedMilliseconds.ToString("0") + " ms");
                        if (rep.Left(1) == "1") {
                            SendCommand(":SC" + $"{currenttime:MM\\/dd\\/yyyy}" + "#");
                            rep = GetReply(1000);
                            if (rep.Left(1) != "0") {
                                Console.WriteLine("Updated");
                            } else {
                                Console.WriteLine("?Error");
                            }
                        } else {
                            Console.WriteLine("?Error");
                        }

                        break;
                    case "save":
                        //Save the IP address for the future
                        try {
                            Environment.SetEnvironmentVariable("MOUNT", args[0], EnvironmentVariableTarget.User);
                            Console.WriteLine("Saved " + args[0] + " to 'MOUNT'.");
                        } catch (Exception ex) {
                            Console.WriteLine("?Error saving user variable (" + ex.Message);
                        }

                        break;

                    case "fw":
                        try {
                            Console.WriteLine("Firmware: " + ReadFirmware().ToString("0.0000"));
                        } catch (Exception ex) {
                            Console.WriteLine("?Error reading firmware version from mount: " + ex);
                        }

                        break;
                    default:
                        Help();
                        Environment.Exit(0);
                        break;
                }

                tcp.Close();

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public static decimal ReadFirmware()
        {
            //Read mount firmware version.
            //Firmware version is stored as a decimal.
            //2.9.9 yields 2.0909, 2.9.19 yields 2.0919 and 2.11.3 yields 2.1103

            SendCommand(":GVN#");
            string s = GetReply(1000);
            string[] ss = s.Replace("#", "").Split('.');
            decimal firmWareVersion = decimal.Parse(ss[0]);
            if (ss.GetUpperBound(0) > 0)
                firmWareVersion += decimal.Parse(ss[1]) / 100;
            if (ss.GetUpperBound(0) > 1)
                firmWareVersion += decimal.Parse(ss[2]) / 10000;
            return firmWareVersion;
        }
        public static void SendCommand(string s)
        {
            Byte[] data = Encoding.ASCII.GetBytes(s);
            _stream.Write(data, 0, data.Length);
        }

        public static bool WaitForStatus(MountState stat, int timeout)
        {
            //Wait for a certain status to be obtained for a specified number of seconds
            int t = timeout * 4;
            while (t > 0)
            {
                var s = GetStatus();
                if (s != stat) {
                    Thread.Sleep(250);
                    t -= 1;
                } else {
                    break; // TODO: might not be correct. Was : Exit While
                }
            }
            if (t > 0)
                return true;
            return false;
        }

        public static MountState GetStatus()
        {
            SendCommand(":Gstat#");
            string rep = GetReply(1000);
            switch (rep) {
                case "0#":
                    return MountState.tracking;
                case "1#":
                    return MountState.stoppedOrHomed;
                case "2#":
                    return MountState.parking;
                case "3#":
                    return MountState.unparking;
                case "4#":
                    return MountState.slewinghome;
                case "5#":
                    return MountState.parked;
                case "6#":
                    return MountState.slewing;
                case "7#":
                    return MountState.stationary;
                case "9#":
                    return MountState.outsideTrackLimits;
                case "11#":
                    return MountState.needsOK;
                case "99#":
                    return MountState.mountError;
                default:
                    return MountState.noreply;
            }
        }

        public static string GetReply(int timeout)
        {
            //stream.ReadTimeout = timeout
            string s = "";
            for (int i = 1; i <= timeout; i++) {
                if (_stream.DataAvailable)
                    break; // TODO: might not be correct. Was : Exit For
                Thread.Sleep(1);
            }

            while (_stream.DataAvailable) {
                s += (char)_stream.ReadByte();
            }

            return s;
        }

        public static void Help()
        {
            Console.WriteLine(AppName + " " + Version);
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

        public static string AltString(int s)
        {
            return (s >= 0 ? "+" : "") + s.ToString().Trim();
        }

        public static string MountDecimal(decimal n)
        {
            var str = string.Format(n.ToString(CultureInfo.InvariantCulture), "0.0");

            return str.Trim().Replace(_decimalSeparator, ".");
        }

        public static decimal ParseDecimal(string s)
        {
            string k = s.Replace(",", _decimalSeparator);
            k = k.Replace(".", _decimalSeparator);
            return decimal.Parse(k.Trim());
        }
    }
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
