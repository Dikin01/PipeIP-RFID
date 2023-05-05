using MonitoringApplication.Model;
using MonitoringApplication.Model.Exceptions;

const int successStopApplicationExitCode = 0;
const int readerNotFoundExitCode = -1;
const int argumentNotFoundExitCode = -2;

const ConsoleKey stopKey = ConsoleKey.Q;

if (args.Length < 2)
    return argumentNotFoundExitCode;

var sendEventEndpoint = new Uri(args[0]);
var monitorName = args[1];
var updateFrequency = TimeSpan.FromMilliseconds(100);
var outWriter = Console.Out;
var keyBuffer = ConsoleKey.NoName;

Console.WriteLine("This program will monitor all SmartCard readers.");
outWriter.WriteLine($"Enter {stopKey} to stop monitoring.");

using var cardMonitor = new CardMonitor(monitorName, outWriter, sendEventEndpoint);

try
{
    cardMonitor.Start();
}
catch (ReaderNotFoundException e)
{
    outWriter.WriteLine(e.Message);
    return readerNotFoundExitCode;
}

while (!WasStopCommand())
    await Task.Delay(updateFrequency);

cardMonitor.Stop();

return successStopApplicationExitCode;


bool WasStopCommand()
{
    var result = !Console.KeyAvailable && keyBuffer is not stopKey;
    keyBuffer = Console.ReadKey(true).Key;

    return result;
}