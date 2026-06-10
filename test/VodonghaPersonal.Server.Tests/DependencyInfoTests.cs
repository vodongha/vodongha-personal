using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Server.Tests;

[TestFixture]
public class DependencyInfoTests
{
    private static DependencyInfo Make(string current, string? latest) =>
        new("lib", current, latest, DependencyType.NuGet, "https://example.com");

    // ── Status computation ──────────────────────────────────────────────────

    [Test]
    public void Status_WhenLatestIsNull_ReturnsUnknown()
    {
        Make("1.0.0", null).Status.ShouldBe(DependencyStatus.Unknown);
    }

    [Test]
    public void Status_WhenCurrentEqualsLatest_ReturnsUpToDate()
    {
        Make("1.0.0", "1.0.0").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    [Test]
    public void Status_WhenLatestIsNewer_ReturnsOutdated()
    {
        Make("1.0.0", "1.0.1").Status.ShouldBe(DependencyStatus.Outdated);
    }

    [Test]
    public void Status_WhenLatestIsOlder_ReturnsOutdated()
    {
        Make("2.0.0", "1.9.9").Status.ShouldBe(DependencyStatus.Outdated);
    }

    // ── NormalizeVersion (tested via Status equality) ───────────────────────

    [Test]
    public void Status_CaretPrefixStripped_CurrentAndLatestMatch()
    {
        Make("^1.0.0", "1.0.0").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    [Test]
    public void Status_TildePrefixStripped_CurrentAndLatestMatch()
    {
        Make("~2.3.1", "2.3.1").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    [Test]
    public void Status_LowerVPrefixStripped_CurrentAndLatestMatch()
    {
        Make("v3.0.0", "3.0.0").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    [Test]
    public void Status_PrereleaseSuffixStripped_MatchesCoreVersion()
    {
        // "1.0.0-beta" normalizes to "1.0.0", same as current "1.0.0"
        Make("1.0.0", "1.0.0-beta").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    [Test]
    public void Status_BothHavePrerelease_NormalizedEquality()
    {
        Make("1.0.0-rc.1", "1.0.0-rc.2").Status.ShouldBe(DependencyStatus.UpToDate);
    }

    // ── Record properties ───────────────────────────────────────────────────

    [Test]
    public void DependencyInfo_Notes_DefaultsToNull()
    {
        Make("1.0.0", "1.0.0").Notes.ShouldBeNull();
    }

    [Test]
    public void DependencyInfo_Notes_CanBeSet()
    {
        var dep = new DependencyInfo("lib", "1.0.0", "1.0.0", DependencyType.Cdn, "https://example.com", "some note");
        dep.Notes.ShouldBe("some note");
    }

    [Test]
    public void DependencyInfo_RegistryUrl_StoredCorrectly()
    {
        var dep = Make("1.0.0", "1.0.0");
        dep.RegistryUrl.ShouldBe("https://example.com");
    }
}
