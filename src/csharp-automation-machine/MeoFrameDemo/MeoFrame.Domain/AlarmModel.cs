namespace MeoFrame.Domain;

// Code 10.2f, Chương 10 (bản đầy đủ ở Chương 15 mục 15.1.2)
public enum AlarmSeverity { Critical = 4, Major = 3, Minor = 2, Warning = 1 }

public enum AlarmLifecycle { Unacknowledged, Acknowledged, Cleared }

public sealed record AlarmModel(
    int            AlarmCode,
    string         Station,
    AlarmSeverity  Severity,
    string         Message,
    DateTimeOffset OccurredAt)
{
    public AlarmLifecycle Lifecycle { get; init; } = AlarmLifecycle.Unacknowledged;
}

// Mục 10.2.11, Chương 10
public enum ConnectionState { Connected, Reconnecting, Disconnected }
