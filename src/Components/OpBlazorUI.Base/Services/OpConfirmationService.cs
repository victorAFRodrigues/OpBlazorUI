using OpBlazorUI.Base.Components.ConfirmDialog.Models;

namespace OpBlazorUI.Base.Services;

public sealed class OpConfirmationService
{
    private ConfirmOptions? _currentOptions;

    public event Action? OnConfirmRequested;
    public event Action<ConfirmOptions?>? OnConfirmClosed;

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
        var options = _currentOptions;
        if (options == null) return;

        options.Accept?.Invoke();
        options.OnClose?.Invoke(true);
        Complete(options);
    }

    public void Reject()
    {
        var options = _currentOptions;
        if (options == null) return;

        options.Reject?.Invoke();
        options.OnClose?.Invoke(false);
        Complete(options);
    }

    public void Close(bool? result = null)
    {
        var options = _currentOptions;
        if (result.HasValue)
        {
            options?.OnClose?.Invoke(result.Value);
        }

        Complete(options);
    }

    // Não limpa a confirmação se o handler abriu uma nova (encadeada) dentro do Accept/Reject.
    private void Complete(ConfirmOptions? options)
    {
        if (options is not null && !ReferenceEquals(_currentOptions, options))
        {
            return;
        }

        _currentOptions = null;
        OnConfirmClosed?.Invoke(options);
    }

    internal void Subscribe(Action onRequested, Action<ConfirmOptions?> onClosed)
    {
        OnConfirmRequested += onRequested;
        OnConfirmClosed += onClosed;
    }

    internal void Unsubscribe(Action onRequested, Action<ConfirmOptions?> onClosed)
    {
        OnConfirmRequested -= onRequested;
        OnConfirmClosed -= onClosed;
    }
}
