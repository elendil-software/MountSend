namespace MountSend
{
    public enum MountState
    {
        tracking = 0,
        stoppedOrHomed = 1,
        parking = 2,
        unparking = 3,
        slewinghome = 4,
        parked = 5,
        slewing = 6,
        stationary = 7,
        outsideTrackLimits = 9,
        needsOK = 11,
        mountError = 99,
        noreply = -1
    }
}
