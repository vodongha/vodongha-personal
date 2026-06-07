namespace vodongha.Services;

/// <summary>
/// Scoped per Blazor circuit. Populated once on first render by TimezoneDetector
/// using the browser's Intl API. Falls back to UTC until the client reports its timezone.
/// </summary>
public class TimezoneService
{
    public TimeZoneInfo Timezone { get; private set; } = TimeZoneInfo.Utc;
    public bool IsSet { get; private set; }

    /// <summary>Fired on the circuit thread after the timezone is resolved from the browser.</summary>
    public event Action? OnTimezoneSet;

    public void Set(string ianaOrWindowsId)
    {
        if (string.IsNullOrWhiteSpace(ianaOrWindowsId))
        {
            return;
        }

        try
        {
            Timezone = TimeZoneInfo.FindSystemTimeZoneById(ianaOrWindowsId);
            IsSet = true;
            OnTimezoneSet?.Invoke();
        }
        catch
        {
            // Unknown or unsupported timezone — keep UTC
        }
    }

    /// <summary>Converts a UTC DateTime to the user's local time.</summary>
    public DateTime ToUserTime(DateTime utc)
    {
        DateTime utcKind = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcKind, Timezone);
    }
}
