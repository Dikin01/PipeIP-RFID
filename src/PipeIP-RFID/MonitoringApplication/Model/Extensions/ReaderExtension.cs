using MonitoringApplication.Model.Exceptions;
using PCSC;

namespace MonitoringApplication.Model.Extensions;

public static class ReaderExtension
{
    private const int ResponseLength = 1024;
    private const int StatusCodeLength = 2;

    private static readonly byte[] GetCardUidCommand = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };

    public static byte[] GetCardUID(this SCardReader reader)
    {
        var response = new byte[ResponseLength];
        var result = reader.Transmit(GetCardUidCommand, ref response);

        if (result is not SCardError.Success)
            throw new TransmitException($"Transmit card return SCardError {result}.");

        return response.Take(response.Length - StatusCodeLength).ToArray();
    }
}