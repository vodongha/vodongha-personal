namespace vodongha.Services;

public class ToastService
{
    public event Action? OnChange;

    private readonly List<ToastItem> _toasts = [];
    public IReadOnlyList<ToastItem> Toasts => _toasts;

    public void Show(string message, bool success = true)
    {
        ToastItem item = new() { Id = Guid.NewGuid(), Message = message, Success = success };
        _toasts.Add(item);
        OnChange?.Invoke();
        _ = RemoveAfterDelayAsync(item.Id);
    }

    public void Remove(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        OnChange?.Invoke();
    }

    private async Task RemoveAfterDelayAsync(Guid id)
    {
        await Task.Delay(3000);
        Remove(id);
    }
}

public class ToastItem
{
    public Guid Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
}
