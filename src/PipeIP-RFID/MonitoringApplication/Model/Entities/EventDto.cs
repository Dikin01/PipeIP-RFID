namespace MonitoringApplication.Model.Entities;

public record EventDto(
    string Description,
    string Place,
    string Monitor,
    DateTime DateTime,
    Dictionary<string, string> Data
);