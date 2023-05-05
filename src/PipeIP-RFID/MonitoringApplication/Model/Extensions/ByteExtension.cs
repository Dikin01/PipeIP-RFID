namespace MonitoringApplication.Model.Extensions;

public static class ByteExtension
{
    public static string ToHexString(this byte[] source)
    {
        return BitConverter.ToString(source);
    }
}