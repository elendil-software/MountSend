namespace MountSend
{
    public enum MountState
    {
        Tracking = 0,
        StoppedOrHomed = 1,
        Parking = 2,
        Unparking = 3,
        SlewingHome = 4,
        Parked = 5,
        Slewing = 6,
        Stationary = 7,
        OutsideTrackLimits = 9,
        NeedsOk = 11,
        MountError = 99,
        NoReply = -1
    }
}
