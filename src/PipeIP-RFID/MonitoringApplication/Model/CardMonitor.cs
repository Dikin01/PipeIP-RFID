using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MonitoringApplication.Model.Entities;
using MonitoringApplication.Model.Exceptions;
using MonitoringApplication.Model.Extensions;
using PCSC;
using PCSC.Monitoring;

namespace MonitoringApplication.Model;

public class CardMonitor : IDisposable
{
    private readonly string _name;
    private readonly TextWriter _textWriter;
    private readonly ISCardMonitor _sCardMonitor;
    private readonly Uri _sendEventEndpoint;

    public CardMonitor(string name, TextWriter textWriter, Uri sendEventEndpoint)
    {
        _textWriter = textWriter;
        _sendEventEndpoint = sendEventEndpoint;
        _name = name;
        _sCardMonitor = MonitorFactory.Instance.Create(SCardScope.System);
    }

    public void Start()
    {
        var readerNames = GetReaderNames();

        if (readerNames.Length < 1)
            throw new ReaderNotFoundException("There are currently no readers installed.");

        _sCardMonitor.CardInserted += HandleCardInserted;
        _sCardMonitor.Start(readerNames);
    }

    public void Stop()
    {
        _sCardMonitor.Cancel();
        _sCardMonitor.CardInserted -= HandleCardInserted;
    }

    private static string[] GetReaderNames()
    {
        using var context = ContextFactory.Instance.Establish(SCardScope.System);
        return context.GetReaders();
    }

    private void HandleCardInserted(object sender, CardEventArgs eventArgs)
    {
        using var cardContext = ContextFactory.Instance.Establish(SCardScope.System);
        using var reader = new SCardReader(cardContext);

        var readerName = eventArgs.ReaderName;

        try
        {
            var result = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

            if (result is not SCardError.Success)
            {
                _textWriter.WriteLine("Failed to connect to reader: " + readerName);
                return;
            }

            _textWriter.WriteLine($"Success to connect to reader: {reader.ReaderName}");

            try
            {
                var uid = reader.GetCardUID().ToHexString();
                _textWriter.WriteLine("Card UID: " + uid);
                SendEventByUri(uid);
            }
            catch (TransmitException e)
            {
                _textWriter.WriteLine(e.Message);
            }

            reader.Disconnect(SCardReaderDisposition.Leave);
        }
        catch (Exception e)
        {
            _textWriter.WriteLine(e);
        }
    }

    private void SendEventByUri(string cardUID)
    {
        var eventDto = new EventDto(
            "Read smart card.",
            "Zelenograd",
            _name,
            DateTime.Now,
            new Dictionary<string, string>
            {
                { "UID", cardUID }
            }
        );

        Task.Run(() => SendEventByUriAsync(eventDto));
    }

    private async Task SendEventByUriAsync(EventDto dto)
    {
        using var httpClient = new HttpClient();

        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _sendEventEndpoint);
            var json = JsonSerializer.Serialize(dto);
            requestMessage.Content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }
        catch (AggregateException e)
        {
            var exceptions = e.Flatten().InnerExceptions;

            foreach (var exception in exceptions)
                await _textWriter.WriteLineAsync(exception.Message).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await _textWriter.WriteLineAsync(e.Message).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _sCardMonitor.Cancel();
        _sCardMonitor.Dispose();
    }
}