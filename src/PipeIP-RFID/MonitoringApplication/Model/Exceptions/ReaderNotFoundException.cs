namespace MonitoringApplication.Model.Exceptions;

public class ReaderNotFoundException : Exception
{
    public ReaderNotFoundException(string message) : base(message)
    {
    }
}