using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Services;

public sealed class OpMessageService
{
    private readonly List<OpToastMessage> _messages = new();
    public event Action? MessagesChanged;

    public IReadOnlyList<OpToastMessage> Messages => _messages;

    public void Add(OpToastMessage message)
    {
        if (message == null) return;
        _messages.Add(message);
        MessagesChanged?.Invoke();
    }

    public void AddAll(IEnumerable<OpToastMessage> messages)
    {
        if (messages == null) return;
        _messages.AddRange(messages);
        MessagesChanged?.Invoke();
    }

    public void Clear(string? key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            _messages.Clear();
        }
        else
        {
            _messages.RemoveAll(m => string.Equals(m.Key, key, StringComparison.Ordinal));
        }
        MessagesChanged?.Invoke();
    }

    public void Remove(OpToastMessage message)
    {
        if (message != null && _messages.Remove(message))
        {
            MessagesChanged?.Invoke();
        }
    }
}