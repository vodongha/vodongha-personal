using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Tests;

[TestFixture]
public class ToastServiceTests
{
    private ToastService _service = null!;

    [SetUp]
    public void SetUp() => _service = new ToastService();

    [Test]
    public void Show_AddsToastToList()
    {
        _service.Show("Test message");

        _service.Toasts.Count.ShouldBe(1);
        _service.Toasts[0].Message.ShouldBe("Test message");
    }

    [Test]
    public void Show_DefaultSuccessTrue()
    {
        _service.Show("ok");

        _service.Toasts[0].Success.ShouldBeTrue();
    }

    [Test]
    public void Show_SuccessFalse_SetsErrorToast()
    {
        _service.Show("error", success: false);

        _service.Toasts[0].Success.ShouldBeFalse();
    }

    [Test]
    public void Show_FiresOnChangeEvent()
    {
        int callCount = 0;
        _service.OnChange += () => callCount++;

        _service.Show("msg");

        callCount.ShouldBe(1);
    }

    [Test]
    public void Remove_RemovesToastById()
    {
        _service.Show("to remove");
        Guid id = _service.Toasts[0].Id;

        _service.Remove(id);

        _service.Toasts.ShouldBeEmpty();
    }

    [Test]
    public void Remove_FiresOnChangeEvent()
    {
        _service.Show("msg");
        Guid id = _service.Toasts[0].Id;
        int callCount = 0;
        _service.OnChange += () => callCount++;

        _service.Remove(id);

        callCount.ShouldBe(1);
    }

    [Test]
    public void Remove_UnknownId_DoesNotThrow()
    {
        Should.NotThrow(() => _service.Remove(Guid.NewGuid()));
    }

    [Test]
    public void Show_MultipleToasts_AllPresent()
    {
        _service.Show("a");
        _service.Show("b");
        _service.Show("c");

        _service.Toasts.Count.ShouldBe(3);
    }
}
