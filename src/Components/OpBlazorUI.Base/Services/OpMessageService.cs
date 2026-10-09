using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Services;

public sealed class OpMessageService
{
    private readonly object _gate = new();
    private readonly List<OpToastMessage> _messages = new();
    public event Action? MessagesChanged;

    /// <summary>
    /// Cópia da lista. O <see cref="Add"/> pode vir de uma thread de fundo (Server) durante o
    /// render; devolver um snapshot evita "Collection was modified" ao enumerar.
    /// </summary>
    public IReadOnlyList<OpToastMessage> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToList();
            }
        }
    }

    public void Add(OpToastMessage message)
    {
        if (message == null) return;

        lock (_gate)
        {
            _messages.Add(message);
        }

        MessagesChanged?.Invoke();
    }

    public void AddAll(IEnumerable<OpToastMessage> messages)
    {
        if (messages == null) return;

        lock (_gate)
        {
            _messages.AddRange(messages);
        }

        MessagesChanged?.Invoke();
    }

    public void Clear(string? key = null)
    {
        lock (_gate)
        {
            if (string.IsNullOrEmpty(key))
            {
                _messages.Clear();
            }
            else
            {
                _messages.RemoveAll(m => string.Equals(m.Key, key, StringComparison.Ordinal));
            }
        }

        MessagesChanged?.Invoke();
    }

    public void Remove(OpToastMessage message)
    {
        if (message == null) return;

        bool removed;
        lock (_gate)
        {
            removed = _messages.Remove(message);
        }

        if (removed)
        {
            MessagesChanged?.Invoke();
        }
    }
}
