namespace MonitoringApplication.Model.Entities;

public record EventDto(
    string Description,
    string Place,
    string Initiator,
    DateTime DateTime,
    Dictionary<string, string> Data
);