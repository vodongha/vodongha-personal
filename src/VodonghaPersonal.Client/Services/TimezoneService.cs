namespace VodonghaPersonal.Client.Services;

public class TimezoneService
{
    public TimeZoneInfo Timezone { get; private set; } = TimeZoneInfo.Utc;
    public bool IsSet { get; private set; }

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

    public DateTime ToUserTime(DateTime utc)
    {
        DateTime utcKind = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcKind, Timezone);
    }
}
