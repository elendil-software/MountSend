namespace MountSend.Commands
{
    public interface ICommand<T>
    {
        double MinFirmwareVersion { get; }
        string Message { get; }

        T Execute(string[] parameters = null);
    }
}