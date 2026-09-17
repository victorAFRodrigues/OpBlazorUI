using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Models;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Base.Components.Toast;

public sealed class OpToastHeadlessContext
{
    public OpToastMessage Message { get; }
    public Action Close { get; }

    public OpToastHeadlessContext(OpToastMessage message, Action close)
    {
        Message = message;
        Close = close;
    }
}

public partial class OpToast : ComponentBase, IDisposable
{
    [Parameter] public string? Key { get; set; }
    [Parameter] public string Position { get; set; } = "top-right";
    [Parameter] public int Life { get; set; } = 3000;
    [Parameter] public bool AutoZIndex { get; set; } = true;
    [Parameter] public int BaseZIndex { get; set; } = 1000;
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public Dictionary<string, string>? Breakpoints { get; set; }
    [Parameter] public bool PreventDuplicates { get; set; }
    [Parameter] public bool PreventOpenDuplicates { get; set; }
    [Parameter] public RenderFragment<OpToastMessage>? MessageTemplate { get; set; }
    [Parameter] public RenderFragment<OpToastHeadlessContext>? HeadlessTemplate { get; set; }
    [Parameter] public EventCallback<OpToastCloseEvent> OnClose { get; set; }

    [Inject] private OpMessageService MessageService { get; set; } = default!;

    private readonly Dictionary<OpToastMessage, CancellationTokenSource> _ctsMap = new();
    private readonly HashSet<OpToastMessage> _leaving = new();
    private IReadOnlyList<OpToastMessage> _previousMessages = Array.Empty<OpToastMessage>();

    private const int LeaveAnimationMs = 360;

    protected override void OnInitialized()
    {
        MessageService.MessagesChanged += OnMessagesChanged;
    }

    private void OnMessagesChanged()
    {
        CheckAndScheduleAutoClose();
        InvokeAsync(StateHasChanged);
    }

    private IReadOnlyList<OpToastMessage> VisibleMessages
    {
        get
        {
            var all = string.IsNullOrEmpty(Key)
                ? MessageService.Messages
                : MessageService.Messages.Where(m => string.Equals(m.Key, Key, StringComparison.Ordinal)).ToList();

            if (PreventDuplicates)
            {
                var seen = new HashSet<string>();
                return all.Where(m =>
                {
                    var key = $"{m.Severity}|{m.Summary}|{m.Detail}";
                    return seen.Add(key);
                }).ToList();
            }

            if (PreventOpenDuplicates)
            {
                var seen = new HashSet<string>();
                return all.Where(m =>
                {
                    var key = $"{m.Severity}|{m.Summary}|{m.Detail}";
                    return seen.Add(key);
                }).ToList();
            }

            return all;
        }
    }

    private void CheckAndScheduleAutoClose()
    {
        var currentMessages = VisibleMessages;
        var newMessages = currentMessages.Except(_previousMessages, OpToastMessageComparer.Instance).ToList();

        foreach (var msg in newMessages)
        {
            if (!msg.Sticky && !_ctsMap.ContainsKey(msg))
            {
                var life = msg.Life ?? Life;
                if (life > 0)
                {
                    var cts = new CancellationTokenSource();
                    _ctsMap[msg] = cts;
                    _ = AutoCloseAsync(msg, life, cts.Token);
                }
            }
        }

        _previousMessages = currentMessages;
    }

    private async Task AutoCloseAsync(OpToastMessage msg, int life, CancellationToken token)
    {
        try
        {
            await Task.Delay(life, token);
            await Close(msg);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private string RootClass => OpCss.BuildClass(
        "p-toast p-component",
        $"p-toast-{Position}",
        StyleClass);

    private string RootStyle
    {
        get
        {
            if (!AutoZIndex) return string.Empty;
            return $"z-index: {BaseZIndex + 100};";
        }
    }

    private string GetMessageClass(OpToastMessage msg)
    {
        var stateClass = _leaving.Contains(msg)
            ? "p-toast-message-leave-active p-toast-message-leave-to"
            : "p-toast-message-enter-active";

        return OpCss.BuildClass(
            "p-toast-message",
            $"p-toast-message-{msg.Severity}",
            stateClass,
            msg.StyleClass);
    }

    private async Task Close(OpToastMessage msg)
    {
        if (!_leaving.Add(msg)) return;

        if (_ctsMap.TryGetValue(msg, out var cts))
        {
            cts.Cancel();
            _ctsMap.Remove(msg);
        }

        var visibleList = VisibleMessages.ToList();
        var index = visibleList.IndexOf(msg);

        await InvokeAsync(StateHasChanged);
        await Task.Delay(LeaveAnimationMs);

        _leaving.Remove(msg);
        MessageService.Remove(msg);
        await OnClose.InvokeAsync(new OpToastCloseEvent(msg, index));
    }

    private string GetBreakpointStyles()
    {
        if (Breakpoints == null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var kvp in Breakpoints)
        {
            sb.AppendLine($"@media (max-width: {kvp.Key}) {{ .p-toast-{Position} {{ {kvp.Value} }} }}");
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        MessageService.MessagesChanged -= OnMessagesChanged;
        foreach (var cts in _ctsMap.Values)
            cts.Dispose();
    }

    private sealed class OpToastMessageComparer : IEqualityComparer<OpToastMessage>
    {
        public static readonly OpToastMessageComparer Instance = new();
        public bool Equals(OpToastMessage? x, OpToastMessage? y) => ReferenceEquals(x, y);
        public int GetHashCode(OpToastMessage obj) => RuntimeHelpers.GetHashCode(obj);
    }
}