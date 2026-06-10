using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Tests;

[TestFixture]
public class TimezoneServiceTests
{
    private TimezoneService _service = null!;

    [SetUp]
    public void SetUp() => _service = new TimezoneService();

    [Test]
    public void Default_IsUtcAndNotSet()
    {
        _service.Timezone.ShouldBe(TimeZoneInfo.Utc);
        _service.IsSet.ShouldBeFalse();
    }

    [Test]
    public void Set_ValidUtcId_SetsTimezone()
    {
        _service.Set("UTC");

        _service.IsSet.ShouldBeTrue();
        _service.Timezone.ShouldBe(TimeZoneInfo.Utc);
    }

    [Test]
    public void Set_InvalidId_RemainsUtcAndNotSet()
    {
        _service.Set("Not/ATimezone");

        _service.Timezone.ShouldBe(TimeZoneInfo.Utc);
        _service.IsSet.ShouldBeFalse();
    }

    [Test]
    public void Set_EmptyString_DoesNotChangeState()
    {
        _service.Set("");

        _service.IsSet.ShouldBeFalse();
        _service.Timezone.ShouldBe(TimeZoneInfo.Utc);
    }

    [Test]
    public void Set_WhitespaceString_DoesNotChangeState()
    {
        _service.Set("   ");

        _service.IsSet.ShouldBeFalse();
    }

    [Test]
    public void Set_ValidId_FiresOnTimezoneSetEvent()
    {
        int callCount = 0;
        _service.OnTimezoneSet += () => callCount++;

        _service.Set("UTC");

        callCount.ShouldBe(1);
    }

    [Test]
    public void Set_InvalidId_DoesNotFireEvent()
    {
        int callCount = 0;
        _service.OnTimezoneSet += () => callCount++;

        _service.Set("Invalid/Zone");

        callCount.ShouldBe(0);
    }

    [Test]
    public void ToUserTime_UtcTimezone_ReturnsSameTime()
    {
        DateTime utc = new DateTime(2025, 6, 15, 12, 0, 0);

        DateTime result = _service.ToUserTime(utc);

        result.ShouldBe(utc);
    }
}
