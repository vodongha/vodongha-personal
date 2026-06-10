using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Tests;

[TestFixture]
public class AdminLocalizationServiceTests
{
    private AdminLocalizationService _service = null!;

    [SetUp]
    public void SetUp() => _service = new AdminLocalizationService();

    [Test]
    public void Default_LangIsVi()
    {
        _service.Lang.ShouldBe("VI");
    }

    [Test]
    public void T_ViLang_KnownKey_ReturnsViTranslation()
    {
        _service.T("Save").ShouldBe("Lưu");
    }

    [Test]
    public void T_ViLang_UnknownKey_ReturnsFallbackKey()
    {
        _service.T("nonexistent.key").ShouldBe("nonexistent.key");
    }

    [Test]
    public void T_EnLang_KnownKey_ReturnsKeyAsIs()
    {
        _service.SetLang("EN");

        _service.T("Save").ShouldBe("Save");
    }

    [Test]
    public void Toggle_SwitchesFromViToEn()
    {
        _service.Toggle();

        _service.Lang.ShouldBe("EN");
    }

    [Test]
    public void Toggle_SwitchesFromEnToVi()
    {
        _service.SetLang("EN");

        _service.Toggle();

        _service.Lang.ShouldBe("VI");
    }

    [Test]
    public void SetLang_SameValue_DoesNotFireEvent()
    {
        int callCount = 0;
        _service.OnChanged += () => { callCount++; return Task.CompletedTask; };

        _service.SetLang("VI");

        callCount.ShouldBe(0);
    }

    [Test]
    public void SetLang_DifferentValue_FiresOnChangedEvent()
    {
        int callCount = 0;
        _service.OnChanged += () => { callCount++; return Task.CompletedTask; };

        _service.SetLang("EN");

        callCount.ShouldBe(1);
    }
}

[TestFixture]
public class LanguageServiceTests
{
    private LanguageService _service = null!;

    [SetUp]
    public void SetUp() => _service = new LanguageService();

    [Test]
    public void Default_LangIsEn()
    {
        _service.Current.ShouldBe("en");
        _service.IsVi.ShouldBeFalse();
    }

    [Test]
    public void Set_Vi_SetsViLang()
    {
        _service.Set("vi");

        _service.IsVi.ShouldBeTrue();
        _service.Current.ShouldBe("vi");
    }

    [Test]
    public void T_ViLang_KnownKey_ReturnsViTranslation()
    {
        _service.Set("vi");

        _service.T("nav.skills").ShouldBe("Kỹ năng");
    }

    [Test]
    public void T_EnLang_UnknownKey_ReturnsFallbackKey()
    {
        _service.T("nonexistent.key").ShouldBe("nonexistent.key");
    }

    [Test]
    public void Set_FiresOnChangeEvent()
    {
        int callCount = 0;
        _service.OnChange += () => callCount++;

        _service.Set("vi");

        callCount.ShouldBe(1);
    }

    [Test]
    public void T_ViLang_UnknownKey_ReturnsFallbackKey()
    {
        _service.Set("vi");

        _service.T("unknown.key").ShouldBe("unknown.key");
    }
}
