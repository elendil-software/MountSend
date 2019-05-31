namespace MountSend.Commands.Network
{
    /// <summary>
    /// Represents a Wake-on-LAN configuration that can me returned by <see cref="GetWakeOnLanCommand"/>
    /// </summary>
    public enum WakeOnLanConfiguration
    {
        NotAvailable,
        NotActive,
        Active
    }
}