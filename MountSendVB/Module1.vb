Imports System.Net.Sockets
Imports System.Net
Imports System

Module Module1
    Enum mountState As Integer
        tracking = 0
        stoppedOrHomed = 1
        parking = 2
        unparking = 3
        slewinghome = 4
        parked = 5
        slewing = 6
        stationary = 7
        outsideTrackLimits = 9
        needsOK = 11
        mountError = 99
        noreply = -1
    End Enum

    Const version As String = "v2.10"
    Const appname As String = "10Micron Mount Fixer by Per Frejvall"

    Dim stream As NetworkStream
    Dim decimalseparator As String


    Sub Main()

        Dim needreply As Boolean = False
        decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
        Dim IPaddress As String = ""
        Dim Auto As Integer = 0

        Try
            Dim tcp As New TcpClient()
            If My.Application.CommandLineArgs.Count < 1 Then
                help()
                End
            End If

            'See if we are to find the IP address in environment
            If My.Application.CommandLineArgs(My.Application.CommandLineArgs.Count - 1) = "/a" Then
                IPaddress = System.Environment.GetEnvironmentVariable("MOUNT", EnvironmentVariableTarget.User)
                If IPaddress Is Nothing Then
                    Console.WriteLine("?Cannot retrieve IP address of mount. Try saving it with the save command first.")
                    Exit Sub
                Else
                    Auto = 1
                End If
            Else
                IPaddress = My.Application.CommandLineArgs(0)
            End If

            tcp.Connect(IPaddress, 3492)
            stream = tcp.GetStream()
            Dim command As String = LCase(Trim(My.Application.CommandLineArgs(1 - Auto)))
            Select Case command

                Case "park", "p"
                    sendCommand(":KA#")

                Case "parkw"
                    sendCommand(":KA#")
                    Threading.Thread.Sleep(100)
                    If waitForStatus(mountState.parked, 60) Then
                        Console.WriteLine("Parked OK")
                    Else
                        Console.WriteLine("Timeout")
                    End If

                Case "unpark", "up"
                    sendCommand(":PO#")

                Case "command", "c"
                    sendCommand(My.Application.CommandLineArgs(2 - Auto))

                Case "waitstationary"
                    waitForStatus(mountState.stationary, 30)

                Case "stat"
                    Console.WriteLine(getStatus().ToString)

                Case "stop"
                    sendCommand(":AL#")

                Case "start"
                    sendCommand(":AP#")

                Case "maxslew"
                    Dim rate As Integer = Integer.Parse(My.Application.CommandLineArgs(2 - Auto))
                    sendCommand(":Sw" & Trim(rate.ToString) & "#")
                    If getReply(1000) = "0" Then
                        Console.WriteLine("?Invalid rate")
                    End If

                Case "gpsupdate"
                    sendCommand(":gT#")
                    If getReply(30000) = "1" Then Console.WriteLine("Updated") Else Console.WriteLine("?Error")

                Case "move", "movew"
                    'Start by getting tracking state
                    Dim tracking As mountState = getStatus()

                    Dim rep As String = ""
                    Dim az As Integer = Integer.Parse(My.Application.CommandLineArgs(2 - Auto))
                    Dim alt As Integer = Integer.Parse(My.Application.CommandLineArgs(3 - Auto))
                    sendCommand(":Sa" & altString(alt) & "*00#")
                    rep = getReply(1000)
                    Debug.WriteLine("Sa: " & rep)
                    If rep = "1" Then
                        sendCommand(":Sz" & Trim(az.ToString) & "*00#")
                        rep = getReply(1000)
                        Debug.WriteLine("Sz: " & rep)
                        If rep = "1" Then
                            sendCommand(":MA#")
                            Dim q As String = getReply(1000)
                            Debug.WriteLine("MA: " & q)
                            If Left(q, 1) <> "0" Then
                                Console.WriteLine("? " & q)
                                End
                            End If
                        Else
                            Console.WriteLine("?Coordinates may not be reachable")
                            End
                        End If
                    Else
                        Console.WriteLine("?Coordinates may not be reachable")
                        End
                    End If
                    If command = "movew" Then
                        If Not waitForStatus(mountState.stationary, 60) Then
                            Console.WriteLine("?Timeout")
                        End If
                        If tracking = mountState.tracking Then
                            sendCommand(":AP#") 're-instate tracking if it was on
                        End If
                    End If

                Case "refr", "refraction", "r"
                    Dim hpa As Decimal = Decimal.Parse(My.Application.CommandLineArgs(2 - Auto))
                    Dim tmp As Decimal = Decimal.Parse(My.Application.CommandLineArgs(3 - Auto))
                    Dim rep As String = ""
                    sendCommand(":SRTMP" & mountDecimal(tmp) & "#")
                    rep = getReply(1000)
                    If rep = "1" Then
                        sendCommand(":SRPRS" & mountDecimal(hpa) & "#")
                        Dim q As String = getReply(1000)
                        If q <> "1" Then
                            Console.WriteLine("?Refraction pressure invalid")
                        End If
                    Else
                        Console.WriteLine("?Refraction temp invalid")
                    End If

                Case "autorefr", "ar"
                    'Try to find file
                    If FileIO.FileSystem.FileExists(My.Application.CommandLineArgs(2 - Auto)) Then
                        Dim f As New System.IO.StreamReader(My.Application.CommandLineArgs(2 - Auto))
                        Dim data As String() = Split(f.ReadLine, " ")
                        Dim hpa As Decimal = parseDecimal(data(10))
                        Dim tmp As Decimal = parseDecimal(data(2))
                        Dim rep As String = ""
                        sendCommand(":SRTMP" & mountDecimal(tmp) & "#")
                        rep = getReply(1000)
                        If rep = "1" Then
                            sendCommand(":SRPRS" & mountDecimal(hpa) & "#")
                            Dim q As String = getReply(1000)
                            If q <> "1" Then
                                Console.WriteLine("?Refraction pressure invalid")
                            End If
                        Else
                            Console.WriteLine("?Refraction temp invalid")
                        End If
                    Else
                        Console.WriteLine("?Refraction file not found.")
                    End If

                Case "time"
                    'New behavior: do it directly with high precision
                    Dim sw As System.Diagnostics.Stopwatch = New Stopwatch

                    Dim currenttime As DateTime = Now
                    sw.Start()
                    sendCommand((":SL" & Format(currenttime, "HH:mm:ss.ff") & "#"))
                    Dim rep As String = getReply(1000)
                    sw.Stop()

                    Console.WriteLine("  Set to  " & Format(currenttime, "HH:mm:ss.ff"))
                    Console.WriteLine("  Took " & sw.ElapsedMilliseconds.ToString("0") & " ms")
                    If Microsoft.VisualBasic.Left(rep, 1) = "1" Then
                        sendCommand(":SC" & Format(currenttime, "MM\/dd\/yyyy") & "#")
                        rep = getReply(1000)
                        If Microsoft.VisualBasic.Left(rep, 1) <> "0" Then
                            Console.WriteLine("Updated")
                        Else
                            Console.WriteLine("?Error")
                        End If
                    Else
                        Console.WriteLine("?Error")
                    End If

                Case "save"
                    'Save the IP address for the future
                    Try
                        Environment.SetEnvironmentVariable("MOUNT", My.Application.CommandLineArgs(0), EnvironmentVariableTarget.User)
                        Console.WriteLine("Saved " & My.Application.CommandLineArgs(0) & " to 'MOUNT'.")
                    Catch ex As Exception
                        Console.WriteLine("?Error saving user variable (" & ex.ToString)
                        ex = Nothing
                    End Try


                Case "fw"
                    Try
                        Console.WriteLine("Firmware: " & readFirmware().ToString("0.0000"))
                    Catch ex As Exception
                        Console.WriteLine("?Error reading firmware version from mount: " & ex.ToString)
                        ex = Nothing
                    End Try

                Case Else
                    help()
                    End
            End Select

            tcp.Close()

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Function readFirmware() As Decimal
        'Read mount firmware version.
        'Firmware version is stored as a decimal.
        '2.9.9 yields 2.0909, 2.9.19 yields 2.0919 and 2.11.3 yields 2.1103

        sendCommand(":GVN#")
        Dim s As String = getReply(1000)
        Dim ss() As String = Split(s.Replace("#", ""), ".")
        Dim firmWareVersion As Decimal = Decimal.Parse(ss(0))
        If ss.GetUpperBound(0) > 0 Then firmWareVersion += Decimal.Parse(ss(1)) / 100
        If ss.GetUpperBound(0) > 1 Then firmWareVersion += Decimal.Parse(ss(2)) / 10000
        Return firmWareVersion
    End Function
    Sub sendCommand(s As String)
        Dim data As [Byte]() = System.Text.Encoding.ASCII.GetBytes(s)
        stream.Write(data, 0, data.Length)
    End Sub

    Function waitForStatus(stat As mountState, timeout As Integer) As Boolean
        'Wait for a certain status to be obtained for a specified number of seconds
        Dim t As Integer = timeout * 4
        Dim s As mountState
        While t > 0
            s = getStatus()
            If s <> stat Then
                Threading.Thread.Sleep(250)
                t -= 1
            Else
                Exit While
            End If
        End While
        If t > 0 Then Return True Else Return False
    End Function

    Function getStatus() As mountState
        sendCommand(":Gstat#")
        Dim rep As String = getReply(1000)
        Select Case rep
            Case "0#"
                Return mountState.tracking
            Case "1#"
                Return mountState.stoppedOrHomed
            Case "2#"
                Return mountState.parking
            Case "3#"
                Return mountState.unparking
            Case "4#"
                Return mountState.slewinghome
            Case "5#"
                Return mountState.parked
            Case "6#"
                Return mountState.slewing
            Case "7#"
                Return mountState.stationary
            Case "9#"
                Return mountState.outsideTrackLimits
            Case "11#"
                Return mountState.needsOK
            Case "99#"
                Return mountState.mountError
            Case Else
                Return mountState.noreply
        End Select
    End Function

    Function getReply(timeout As Integer) As String
        'stream.ReadTimeout = timeout
        Dim s As String = ""
        For i As Integer = 1 To timeout
            If stream.DataAvailable Then Exit For
            Threading.Thread.Sleep(1)
        Next

        While stream.DataAvailable
            s &= Chr(stream.ReadByte())
        End While
        Return s
    End Function

    Sub help()
        Console.WriteLine(appname & " " & version)
        Console.WriteLine("Usage:")
        Console.WriteLine(" mountsend <ip-address> <command> [<options>].")
        Console.WriteLine("     unpark            Unparks mount. No reply.")
        Console.WriteLine("     park              Parks mount. No reply.")
        Console.WriteLine("     parkw             Parks mount and waits for completion in 1 min. Replies success or timeout")
        Console.WriteLine("     stop              Stops tracking. No reply.")
        Console.WriteLine("     start             Starts tracking. No reply.")
        Console.WriteLine("     gpsupdate         Updates site info from GPS. Reports success or error.")
        Console.WriteLine("     waitstationary    Waits until all movement has stopped. No reply.")
        Console.WriteLine("     move <az> <alt>   Moves to AZ and ALT, leaves tracking off.")
        Console.WriteLine("     movew <az> <alt>  Moves and waits for completion, restores tracking if it was on.")
        Console.WriteLine("     maxslew <deg>     Set max slew rate. Reports error if invalid slew rate.")
        Console.WriteLine("     refr <hPa> <deg>  Sends refraction parameters to mount.")
        Console.WriteLine("     autorefr <file>   Reads to get refraction parameters, then sends them to mount.")
        Console.WriteLine("     time              Updates mount time from PC clock (both date and time).")
        Console.WriteLine("     save              Saves the mount IP in a user environment variable (MOUNT).")
        Console.WriteLine("     fw                Get the mount firmware version.")
        Console.WriteLine()
        Console.WriteLine("     NOTE! you can leave out the IP address and add '/a' as a last argument")
        Console.WriteLine("           to have mountsend use the environment variable 'MOUNT' for getting")
        Console.WriteLine("           the mount address.")


        Console.WriteLine()
    End Sub

    Function altString(s As Integer) As String
        Return IIf(s >= 0, "+", "") & Trim(s.ToString)
    End Function

    Function mountDecimal(n As Decimal) As String
        Return Replace(Trim(Format(n, "0.0")), decimalseparator, ".")
    End Function

    Function parseDecimal(s As String) As Decimal
        Dim k As String = Replace(s, ",", decimalseparator)
        k = Replace(k, ".", decimalseparator)
        Return Decimal.Parse(Trim(k))
    End Function
End Module