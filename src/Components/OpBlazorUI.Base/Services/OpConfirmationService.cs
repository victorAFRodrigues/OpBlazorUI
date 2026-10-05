using OpBlazorUI.Base.Components.ConfirmDialog.Models;

namespace OpBlazorUI.Base.Services;

public sealed class OpConfirmationService
{
    private ConfirmOptions? _currentOptions;

    public event Action? OnConfirmRequested;
    public event Action? OnConfirmClosed;

    public ConfirmOptions? CurrentOptions => _currentOptions;
    public bool HasActiveConfirmation => _currentOptions != null;

    public void Confirm(ConfirmOptions options)
    {
        if (options == null) return;
        _currentOptions = options;
        OnConfirmRequested?.Invoke();
    }

    public void ConfirmDeclarative(ConfirmOptions? options = null, string? key = null)
    {
        if (options != null)
        {
            _currentOptions = options;
            if (key != null) _currentOptions.Key = key;
        }
        else if (_currentOptions != null)
        {
            _currentOptions.Key = key;
        }
        else
        {
            _currentOptions = new ConfirmOptions();
        }
        OnConfirmRequested?.Invoke();
    }

    public void Accept()
    {
        _currentOptions?.Accept?.Invoke();
        _currentOptions?.OnClose?.Invoke(true);
        Close();
    }

    public void Reject()
    {
        _currentOptions?.Reject?.Invoke();
        _currentOptions?.OnClose?.Invoke(false);
        Close();
    }

    public void Close(bool? result = null)
    {
        if (result.HasValue)
        {
            _currentOptions?.OnClose?.Invoke(result.Value);
        }
        _currentOptions = null;
        OnConfirmClosed?.Invoke();
    }

    internal void Subscribe(Action callback)
    {
        OnConfirmRequested += callback;
    }

    internal void Unsubscribe(Action callback)
    {
        OnConfirmRequested -= callback;
    }
}