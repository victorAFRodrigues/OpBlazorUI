using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.DatePicker;

public enum OpDateSelectionMode
{
    Single,
    Multiple,
    Range
}

public enum OpDatePickerView
{
    Date,
    Month,
    Year
}

public sealed class OpDateMeta
{
    public int Day { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public bool OtherMonth { get; init; }
    public bool Today { get; init; }
    public bool Selectable { get; init; } = true;

    public DateTime Date => new(Year, Month + 1, Day);
}

public sealed class OpMonth
{
    public int Month { get; init; }
    public int Year { get; init; }
    public List<OpDateMeta?[]> Weeks { get; init; } = new();
    public List<int> WeekNumbers { get; init; } = new();
}

public sealed class OpDatePickerButtonBarContext
{
    public EventCallback<MouseEventArgs> TodayCallback { get; init; }
    public EventCallback<MouseEventArgs> ClearCallback { get; init; }
}

public sealed class OpDatePickerInputIconContext
{
    public EventCallback<MouseEventArgs> ClickCallback { get; init; }
}

public partial class OpDatePicker : ComponentBase
{
    private string? _inputId;
    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private DateTime _viewDate;
    private OpDatePickerView _currentView = OpDatePickerView.Date;
    private DateTime _today;
    private List<OpMonth> _months = new();

    private int _currentHour = 12;
    private int _currentMinute;
    private int _currentSecond;
    private bool _pm;

    private bool _focus;
    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;

    private string _inputText = "";
    private DateTime? _prevSingle;
    private List<DateTime?>? _prevMultiple;
    private List<DateTime?>? _prevRange;

    // ---------------------------------------------------------------- params
    [Parameter] public DateTime? Value { get; set; }
    [Parameter] public EventCallback<DateTime?> ValueChanged { get; set; }

    [Parameter] public IReadOnlyList<DateTime?>? RangeValue { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<DateTime?>?> RangeValueChanged { get; set; }

    [Parameter] public IReadOnlyList<DateTime?>? MultipleValue { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<DateTime?>?> MultipleValueChanged { get; set; }

    [Parameter] public OpDateSelectionMode SelectionMode { get; set; } = OpDateSelectionMode.Single;
    [Parameter] public OpDatePickerView View { get; set; } = OpDatePickerView.Date;
    [Parameter] public string DateFormat { get; set; } = "mm/dd/yy";
    [Parameter] public bool Inline { get; set; }
    [Parameter] public bool ShowIcon { get; set; }
    [Parameter] public string IconDisplay { get; set; } = "button";
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool ReadonlyInput { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public int? InputSize { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? IconAriaLabel { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public bool ShowOnFocus { get; set; } = true;
    [Parameter] public bool ShowWeek { get; set; }
    [Parameter] public bool StartWeekFromFirstDayOfYear { get; set; }
    [Parameter] public int FirstDayOfWeek { get; set; }
    [Parameter] public int NumberOfMonths { get; set; } = 1;
    [Parameter] public DateTime? MinDate { get; set; }
    [Parameter] public DateTime? MaxDate { get; set; }
    [Parameter] public IReadOnlyCollection<DateTime>? DisabledDates { get; set; }
    [Parameter] public IReadOnlyCollection<int>? DisabledDays { get; set; }
    [Parameter] public bool SelectOtherMonths { get; set; }
    [Parameter] public bool ShowOtherMonths { get; set; } = true;
    [Parameter] public bool ShowButtonBar { get; set; }
    [Parameter] public string? TodayButtonStyleClass { get; set; }
    [Parameter] public string? ClearButtonStyleClass { get; set; }
    [Parameter] public bool ShowTime { get; set; }
    [Parameter] public bool TimeOnly { get; set; }
    [Parameter] public int HourFormat { get; set; } = 24;
    [Parameter] public bool ShowSeconds { get; set; }
    [Parameter] public int StepHour { get; set; } = 1;
    [Parameter] public int StepMinute { get; set; } = 1;
    [Parameter] public int StepSecond { get; set; } = 1;
    [Parameter] public string TimeSeparator { get; set; } = ":";
    [Parameter] public string MultipleSeparator { get; set; } = ", ";
    [Parameter] public string RangeSeparator { get; set; } = " - ";
    [Parameter] public string? PanelStyleClass { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? InputStyleClass { get; set; }
    [Parameter] public bool HideOnDateTimeSelect { get; set; }
    [Parameter] public string? TodayLabel { get; set; } = "Today";
    [Parameter] public string? ClearLabel { get; set; } = "Clear";

    // events
    [Parameter] public EventCallback<DateTime?> OnSelect { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
    [Parameter] public EventCallback<string> OnInput { get; set; }
    [Parameter] public EventCallback<(int Month, int Year)> OnMonthChange { get; set; }
    [Parameter] public EventCallback<(int Month, int Year)> OnYearChange { get; set; }
    [Parameter] public EventCallback<DateTime> OnTodayClick { get; set; }
    [Parameter] public EventCallback OnClearClick { get; set; }
    [Parameter] public EventCallback OnClickOutside { get; set; }

    // templates
    [Parameter] public RenderFragment<OpDateMeta>? DateTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? PreviousIconTemplate { get; set; }
    [Parameter] public RenderFragment? NextIconTemplate { get; set; }
    [Parameter] public RenderFragment? TriggerIconTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }
    [Parameter] public RenderFragment<OpDatePickerInputIconContext>? InputIconTemplate { get; set; }
    [Parameter] public RenderFragment? IncrementIconTemplate { get; set; }
    [Parameter] public RenderFragment? DecrementIconTemplate { get; set; }
    [Parameter] public RenderFragment<IReadOnlyList<int>>? DecadeTemplate { get; set; }
    [Parameter] public RenderFragment<OpDatePickerButtonBarContext>? ButtonBarTemplate { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _today = DateTime.Today;
        _currentView = View;

        var reference = Value ?? RangeValue?.FirstOrDefault() ?? MultipleValue?.FirstOrDefault() ?? _today;
        _viewDate = new DateTime(reference.Year, reference.Month, 1);

        if (ShowTime || TimeOnly)
        {
            var t = Value ?? _today;
            _currentHour = t.Hour;
            _currentMinute = t.Minute;
            _currentSecond = t.Second;
            if (HourFormat == 12)
            {
                _pm = t.Hour >= 12;
                _currentHour = t.Hour % 12 == 0 ? 12 : t.Hour % 12;
            }
        }

        _inputId = InputId ?? $"op-dp-{Guid.NewGuid():N}";
        _inputText = InputValue;
        _prevSingle = Value;
        _prevMultiple = MultipleValue?.ToList();
        _prevRange = RangeValue?.ToList();
        RebuildMonths();
    }

    protected override void OnParametersSet()
    {
        if (SelectionMode == OpDateSelectionMode.Single)
        {
            if (!EqualityComparer<DateTime?>.Default.Equals(Value, _prevSingle))
            {
                _prevSingle = Value;
                _inputText = InputValue;
            }
        }
        else if (SelectionMode == OpDateSelectionMode.Multiple)
        {
            if (!SequenceEquals(MultipleValue, _prevMultiple))
            {
                _prevMultiple = MultipleValue?.ToList();
                _inputText = InputValue;
            }
        }
        else
        {
            if (!SequenceEquals(RangeValue, _prevRange))
            {
                _prevRange = RangeValue?.ToList();
                _inputText = InputValue;
            }
        }
    }

    private static bool SequenceEquals(IReadOnlyList<DateTime?>? a, List<DateTime?>? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.SequenceEqual(b);
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-datepicker p-component p-inputwrapper",
        Invalid ? "p-invalid" : null,
        Fluid ? "p-datepicker-fluid" : null,
        HasValue ? "p-inputwrapper-filled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus p-focus" : null,
        StyleClass);

    private string InputClass => BuildClass(
        "p-inputtext p-datepicker-input",
        Variant == "filled" ? "p-variant-filled" : null,
        Invalid ? "p-invalid" : null,
        InputStyleClass,
        SizeCss);

    private string PanelClass => BuildClass(
        "p-datepicker-panel p-component",
        Inline ? "p-datepicker-panel-inline" : null,
        Disabled ? "p-disabled" : null,
        TimeOnly ? "p-datepicker-timeonly" : null,
        PanelStyleClass,
        Inline ? null : _panelAnimationClass);

    private string? SizeCss => Size switch
    {
        "small" => "p-inputtext-sm",
        "large" => "p-inputtext-lg",
        _ => null
    };

    private bool HasValue => SelectionMode switch
    {
        OpDateSelectionMode.Single => Value.HasValue,
        OpDateSelectionMode.Multiple => MultipleValue is { Count: > 0 },
        OpDateSelectionMode.Range => RangeValue is { Count: > 0 },
        _ => false
    };

    private string InputValue
    {
        get
        {
            return SelectionMode switch
            {
                OpDateSelectionMode.Single => Value is { } v ? FormatDate(v, DateFormat) : string.Empty,
                OpDateSelectionMode.Multiple => MultipleValue is { } m
                    ? string.Join(MultipleSeparator,
                        m.Where(d => d.HasValue).Select(d => FormatDate(d!.Value, DateFormat)))
                    : string.Empty,
                OpDateSelectionMode.Range => RangeValue is { } r
                    ? string.Join(RangeSeparator,
                        r.Where(d => d.HasValue).Select(d => FormatDate(d!.Value, DateFormat)))
                    : string.Empty,
                _ => string.Empty
            };
        }
    }

    private (DateTime? Start, DateTime? End) RangeStartEnd
    {
        get
        {
            var list = RangeValue;
            if (list is null) return (null, null);
            var arr = list.Where(d => d.HasValue).Select(d => d!.Value).ToList();
            if (arr.Count == 0) return (null, null);
            var start = arr.Min();
            var end = arr.Count > 1 ? arr.Max() : (DateTime?)null;
            return (start, end);
        }
    }

    private bool IsRangeSelection => SelectionMode == OpDateSelectionMode.Range;

    // ------------------------------------------------------------ month grid
    private void RebuildMonths()
    {
        _months = new List<OpMonth>();
        for (var i = 0; i < Math.Max(1, NumberOfMonths); i++)
        {
            var monthStart = _viewDate.AddMonths(i);
            _months.Add(BuildMonth(monthStart.Year, monthStart.Month - 1));
        }
    }

    private OpMonth BuildMonth(int year, int month)
    {
        var first = new DateTime(year, month + 1, 1);
        var offset = ((int)first.DayOfWeek - FirstDayOfWeek + 7) % 7;
        var daysInMonth = DateTime.DaysInMonth(year, month + 1);

        var result = new OpMonth { Month = month, Year = year };
        var cell = first.AddDays(-offset);

        for (var w = 0; w < 6; w++)
        {
            var week = new OpDateMeta?[7];
            for (var d = 0; d < 7; d++)
            {
                var date = cell;
                week[d] = new OpDateMeta
                {
                    Day = date.Day,
                    Month = date.Month - 1,
                    Year = date.Year,
                    OtherMonth = date.Month - 1 != month,
                    Today = date.Date == _today,
                    Selectable = IsSelectable(date)
                };
                cell = cell.AddDays(1);
            }

            result.Weeks.Add(week);
            result.WeekNumbers.Add(WeekNumber(cell.AddDays(-1)));
        }

        _ = daysInMonth;
        return result;
    }

    private static int WeekNumber(DateTime date)
    {
        return System.Globalization.ISOWeek.GetWeekOfYear(date);
    }

    private bool IsSelectable(DateTime date)
    {
        if (MinDate is { } min && date.Date < min.Date) return false;
        if (MaxDate is { } max && date.Date > max.Date) return false;
        if (DisabledDates is { } dd && dd.Any(x => x.Date == date.Date)) return false;
        if (DisabledDays is { } days && days.Contains((int)date.DayOfWeek)) return false;
        return true;
    }

    private bool IsDateSelectable(OpDateMeta d)
    {
        if (d.OtherMonth && !SelectOtherMonths) return false;
        return d.Selectable;
    }

    // ------------------------------------------------------------ selection
    private bool IsSelected(OpDateMeta d)
    {
        var date = d.Date;
        return SelectionMode switch
        {
            OpDateSelectionMode.Single => Value?.Date == date.Date,
            OpDateSelectionMode.Multiple => MultipleValue?.Any(x => x?.Date == date.Date) ?? false,
            OpDateSelectionMode.Range => IsInRange(date),
            _ => false
        };
    }

    private bool IsInRange(DateTime date)
    {
        var (start, end) = RangeStartEnd;
        if (start is null) return false;
        if (end is null) return date.Date == start.Value.Date;
        return date.Date >= start.Value.Date && date.Date <= end.Value.Date;
    }

    private string DayClass(OpDateMeta d)
    {
        var classes = new List<string> { "p-datepicker-day" };

        if (IsRangeSelection && IsSelected(d) && d.Selectable)
        {
            var (start, end) = RangeStartEnd;
            var isStart = start is { } s && d.Date.Date == s.Date;
            var isEnd = end is { } e && d.Date.Date == e.Date;
            classes.Add(isStart || isEnd ? "p-datepicker-day-selected" : "p-datepicker-day-selected-range");
        }
        else if (!IsRangeSelection && IsSelected(d) && d.Selectable)
        {
            classes.Add("p-datepicker-day-selected");
        }

        if (Disabled || !IsDateSelectable(d))
        {
            classes.Add("p-disabled");
        }

        return string.Join(' ', classes);
    }

    private string DayCellClass(OpDateMeta d)
    {
        var classes = new List<string> { "p-datepicker-day-cell" };
        if (d.OtherMonth) classes.Add("p-datepicker-other-month");
        if (d.Today) classes.Add("p-datepicker-today");
        return string.Join(' ', classes);
    }

    private async Task OnDateSelect(MouseEventArgs _, OpDateMeta meta)
    {
        if (Disabled || !IsDateSelectable(meta)) return;

        var selected = new DateTime(meta.Year, meta.Month + 1, meta.Day);

        if (ShowTime || TimeOnly)
        {
            selected = WithCurrentTime(selected);
        }

        switch (SelectionMode)
        {
            case OpDateSelectionMode.Single:
                Value = selected;
                await ValueChanged.InvokeAsync(selected);
                await OnSelect.InvokeAsync(selected);
                break;
            case OpDateSelectionMode.Multiple:
                await ToggleMultiple(selected);
                break;
            case OpDateSelectionMode.Range:
                await SelectRange(selected);
                break;
        }

        _inputText = InputValue;

        if (ShouldHideAfterSelect())
        {
            await CloseAsync();
        }
    }

    /// <summary>
    /// Indica se o overlay deve fechar após uma seleção. Com seletor de hora o painel
    /// permanece aberto para o usuário ajustar o horário e só fecha ao clicar fora
    /// (ou quando <see cref="HideOnDateTimeSelect"/> é habilitado explicitamente).
    /// No modo Range só fecha depois que as duas pontas foram escolhidas.
    /// </summary>
    private bool ShouldHideAfterSelect()
    {
        if (Inline || Disabled) return false;

        var selectionComplete = SelectionMode switch
        {
            OpDateSelectionMode.Single => true,
            OpDateSelectionMode.Range => RangeValue is { Count: >= 2 },
            _ => false
        };

        if (!selectionComplete) return false;

        return ShowTime || TimeOnly ? HideOnDateTimeSelect : true;
    }

    private DateTime WithCurrentTime(DateTime date)
    {
        var hour = HourFormat == 12
            ? (_pm ? _currentHour % 12 + 12 : _currentHour % 12)
            : _currentHour;

        return new DateTime(date.Year, date.Month, date.Day, hour, _currentMinute, _currentSecond);
    }

    private async Task ToggleMultiple(DateTime date)
    {
        var list = (MultipleValue ?? Array.Empty<DateTime?>()).ToList();
        var idx = list.FindIndex(d => d?.Date == date.Date);
        if (idx >= 0) list.RemoveAt(idx);
        else list.Add(date);
        MultipleValue = list;
        await MultipleValueChanged.InvokeAsync(list);
        await OnSelect.InvokeAsync(date);
    }

    private async Task SelectRange(DateTime date)
    {
        var list = (RangeValue ?? Array.Empty<DateTime?>()).ToList();
        if (list.Count == 2)
        {
            list = new List<DateTime?> { date };
        }
        else if (list.Count == 1)
        {
            var existing = list[0]!.Value;
            list = existing.Date <= date.Date
                ? new List<DateTime?> { existing, date }
                : new List<DateTime?> { date, existing };
        }
        else
        {
            list = new List<DateTime?> { date };
        }

        RangeValue = list;
        await RangeValueChanged.InvokeAsync(list);
        await OnSelect.InvokeAsync(date);
    }

    private bool IsMonthSelected(int monthIndex)
    {
        if (SelectionMode != OpDateSelectionMode.Single || !Value.HasValue) return false;
        return Value.Value.Year == _viewDate.Year && Value.Value.Month - 1 == monthIndex;
    }

    private bool IsYearSelected(int year)
    {
        if (SelectionMode != OpDateSelectionMode.Single || !Value.HasValue) return false;
        return Value.Value.Year == year;
    }

    private bool IsMonthDisabled(int monthIndex)
    {
        if (MinDate is { } min &&
            new DateTime(_viewDate.Year, monthIndex + 1, 1) < new DateTime(min.Year, min.Month, 1)) return true;
        if (MaxDate is { } max &&
            new DateTime(_viewDate.Year, monthIndex + 1, 1) > new DateTime(max.Year, max.Month, 1)) return true;
        return false;
    }

    private bool IsYearDisabled(int year)
    {
        if (MinDate is { } min && year < min.Year) return true;
        if (MaxDate is { } max && year > max.Year) return true;
        return false;
    }

    // ------------------------------------------------------------ navigation
    private IReadOnlyList<int> YearPickerValues()
    {
        var start = _viewDate.Year - _viewDate.Year % 10;
        return Enumerable.Range(start, 10).ToList();
    }

    private async Task PrevMonth()
    {
        _viewDate = _viewDate.AddMonths(-1);
        await Navigated(monthChange: true);
    }

    private async Task NextMonth()
    {
        _viewDate = _viewDate.AddMonths(1);
        await Navigated(monthChange: true);
    }

    private async Task PrevYear()
    {
        _viewDate = _viewDate.AddYears(-1);
        await Navigated(yearChange: true);
    }

    private async Task NextYear()
    {
        _viewDate = _viewDate.AddYears(1);
        await Navigated(yearChange: true);
    }

    private async Task PrevDecade()
    {
        _viewDate = _viewDate.AddYears(-10);
        await Navigated(yearChange: true);
    }

    private async Task NextDecade()
    {
        _viewDate = _viewDate.AddYears(10);
        await Navigated(yearChange: true);
    }

    private async Task Navigated(bool monthChange = false, bool yearChange = false)
    {
        RebuildMonths();
        if (monthChange) await OnMonthChange.InvokeAsync((_viewDate.Month - 1, _viewDate.Year));
        if (yearChange) await OnYearChange.InvokeAsync((_viewDate.Month - 1, _viewDate.Year));
    }

    private void SwitchToMonthView()
    {
        _currentView = OpDatePickerView.Month;
    }

    private void SwitchToYearView()
    {
        _currentView = OpDatePickerView.Year;
    }

    private async Task OnMonthSelect(MouseEventArgs e, int monthIndex)
    {
        if (IsMonthDisabled(monthIndex)) return;

        if (View == OpDatePickerView.Month)
        {
            var meta = new OpDateMeta { Year = _viewDate.Year, Month = monthIndex, Day = 1, Selectable = true };
            await OnDateSelect(e, meta);
        }
        else
        {
            _viewDate = new DateTime(_viewDate.Year, monthIndex + 1, 1);
            _currentView = OpDatePickerView.Date;
            await Navigated(monthChange: true);
        }
    }

    private async Task OnYearSelect(MouseEventArgs e, int year)
    {
        if (IsYearDisabled(year)) return;

        if (View == OpDatePickerView.Year)
        {
            var meta = new OpDateMeta { Year = year, Month = 0, Day = 1, Selectable = true };
            await OnDateSelect(e, meta);
        }
        else
        {
            _viewDate = new DateTime(year, _viewDate.Month, 1);
            _currentView = OpDatePickerView.Date;
            await Navigated(yearChange: true);
        }
    }

    // ------------------------------------------------------------ overlay
    private async Task OnInputClick()
    {
        if (!ShowOnFocus && !_overlayVisible && !Inline) return;
        await OpenAsync();
    }

    private async Task OnInputFocus()
    {
        if (ShowOnFocus) await OpenAsync();
    }

    private async Task OpenAsync()
    {
        if (_overlayVisible || Inline || Disabled) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!_overlayVisible) return;
        _overlayVisible = false;
        _panelClosing = true;
        _panelAnimationClass = "p-anchored-overlay-leave-active";
        StateHasChanged();
        await OnClose.InvokeAsync();
        _ = ForceRemovePanelAsync();
    }

    private async Task ForceRemovePanelAsync()
    {
        await Task.Delay(400);
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
            _panelAnimationClass = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPanelAnimationEnd()
    {
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
        }

        _panelAnimationClass = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnRootFocusIn(FocusEventArgs e)
    {
        _focusInside = true;
    }

    /// <summary>
    /// Marca a interação com o painel. Os dias do calendário não são focáveis, então
    /// clicar neles dispara <c>focusout</c> no input e fecharia o overlay antes de o
    /// usuário terminar a seleção (range e horário).
    /// </summary>
    private void OnPanelMouseDown()
    {
        _lastPanelPointerDown = DateTime.UtcNow;
        _focusInside = true;
    }

    private async Task OnRootFocusOut(FocusEventArgs e)
    {
        if (!_overlayVisible || Inline || _panelClosing) return;

        _focusInside = false;

        await Task.Delay(10);

        var interactedWithPanel = (DateTime.UtcNow - _lastPanelPointerDown).TotalMilliseconds < 250;

        if (!_focusInside && !interactedWithPanel)
        {
            await OnClickOutside.InvokeAsync();
            await CloseAsync();
        }
    }

    private async Task OnInputFocusEvent(FocusEventArgs e)
    {
        _focus = true;
        _focusInside = true;

        await OnFocus.InvokeAsync(e);
        await OnInputFocus();
        StateHasChanged();
    }

    private async Task OnUserInput(ChangeEventArgs e)
    {
        _inputText = e.Value?.ToString() ?? string.Empty;
        await OnInput.InvokeAsync(_inputText);

        if (SelectionMode == OpDateSelectionMode.Single && !ReadonlyInput)
        {
            if (DateTime.TryParse(_inputText, out var parsed))
            {
                Value = parsed;
                await ValueChanged.InvokeAsync(parsed);
            }
        }
    }

    private async Task OnInputBlur(FocusEventArgs e)
    {
        _focus = false;
        await OnBlur.InvokeAsync(e);
        StateHasChanged();
    }

    private async Task OnButtonClick(MouseEventArgs e)
    {
        if (_overlayVisible) await CloseAsync();
        else await OpenAsync();
    }

    private async Task OnInputKeydown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Enter":
                await OpenAsync();
                break;
            case "ArrowDown":
                await OpenAsync();
                break;
        }
    }

    private async Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _overlayVisible)
        {
            await CloseAsync();
        }
    }

    private async Task OnDateCellKeydown(KeyboardEventArgs e, OpDateMeta date)
    {
        if (e.Key is "Enter" or " " or "Spacebar")
        {
            await OnDateSelect(new MouseEventArgs(), date);
        }
        else if (e.Key == "Escape")
        {
            await CloseAsync();
        }
    }

    private IReadOnlyList<string> WeekDayNames()
    {
        var names = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        var result = new List<string>();
        for (var i = 0; i < 7; i++)
        {
            result.Add(names[(FirstDayOfWeek + i) % 7]);
        }

        return result;
    }

    private async Task Clear(MouseEventArgs? _ = null)
    {
        Value = null;
        RangeValue = null;
        MultipleValue = null;
        _inputText = "";
        await ValueChanged.InvokeAsync(null);
        await OnClear.InvokeAsync();
    }

    // ------------------------------------------------------------ button bar
    private async Task OnTodayButtonClick(MouseEventArgs e)
    {
        var today = (ShowTime || TimeOnly) ? WithCurrentTime(_today) : _today;

        switch (SelectionMode)
        {
            case OpDateSelectionMode.Single:
                Value = today;
                await ValueChanged.InvokeAsync(today);
                await OnSelect.InvokeAsync(today);
                break;

            case OpDateSelectionMode.Range:
                var range = new List<DateTime?> { today };
                RangeValue = range;
                await RangeValueChanged.InvokeAsync(range);
                break;

            case OpDateSelectionMode.Multiple:
                var multiple = (MultipleValue ?? Array.Empty<DateTime?>()).ToList();
                if (!multiple.Any(d => d?.Date == today.Date))
                {
                    multiple.Add(today);
                    MultipleValue = multiple;
                    await MultipleValueChanged.InvokeAsync(multiple);
                }

                break;
        }

        _viewDate = new DateTime(_today.Year, _today.Month, 1);
        RebuildMonths();
        _inputText = InputValue;
        await OnTodayClick.InvokeAsync(today);

        if (ShouldHideAfterSelect()) await CloseAsync();
    }

    private async Task OnClearButtonClick(MouseEventArgs e)
    {
        await Clear();
        await OnClearClick.InvokeAsync();

        if (!Inline) await CloseAsync();
    }

    // ------------------------------------------------------------ time
    private Task IncrementHour() => ChangeHour(StepHour);
    private Task DecrementHour() => ChangeHour(-StepHour);
    private Task IncrementMinute() => ChangeMinute(StepMinute);
    private Task DecrementMinute() => ChangeMinute(-StepMinute);
    private Task IncrementSecond() => ChangeSecond(StepSecond);
    private Task DecrementSecond() => ChangeSecond(-StepSecond);

    private async Task ChangeHour(int step)
    {
        _currentHour += step;
        if (HourFormat == 12)
        {
            if (_currentHour > 12) _currentHour = 1;
            if (_currentHour < 1) _currentHour = 12;
        }
        else
        {
            if (_currentHour > 23) _currentHour = 0;
            if (_currentHour < 0) _currentHour = 23;
        }

        await CommitTimeAsync();
    }

    private async Task ChangeMinute(int step)
    {
        _currentMinute += step;
        if (_currentMinute > 59) _currentMinute = 0;
        if (_currentMinute < 0) _currentMinute = 59;

        await CommitTimeAsync();
    }

    private async Task ChangeSecond(int step)
    {
        _currentSecond += step;
        if (_currentSecond > 59) _currentSecond = 0;
        if (_currentSecond < 0) _currentSecond = 59;

        await CommitTimeAsync();
    }

    private async Task ToggleAmPm()
    {
        _pm = !_pm;
        await CommitTimeAsync();
    }

    /// <summary>
    /// Aplica o horário atual do seletor ao valor já selecionado e atualiza o input.
    /// </summary>
    private async Task CommitTimeAsync()
    {
        if (Disabled) return;

        switch (SelectionMode)
        {
            case OpDateSelectionMode.Single:
                if (Value is { } single)
                {
                    var updated = WithCurrentTime(single);
                    Value = updated;
                    _inputText = InputValue;
                    await ValueChanged.InvokeAsync(updated);
                }
                else if (TimeOnly)
                {
                    var updated = WithCurrentTime(_today);
                    Value = updated;
                    _inputText = InputValue;
                    await ValueChanged.InvokeAsync(updated);
                }

                break;

            case OpDateSelectionMode.Range:
                if (RangeValue is { Count: > 0 } range)
                {
                    var list = range.Select(d => d.HasValue ? WithCurrentTime(d.Value) : d).ToList();
                    RangeValue = list;
                    _inputText = InputValue;
                    await RangeValueChanged.InvokeAsync(list);
                }

                break;

            case OpDateSelectionMode.Multiple:
                if (MultipleValue is { Count: > 0 } multiple)
                {
                    var list = multiple.Select(d => d.HasValue ? WithCurrentTime(d.Value) : d).ToList();
                    MultipleValue = list;
                    _inputText = InputValue;
                    await MultipleValueChanged.InvokeAsync(list);
                }

                break;
        }

        StateHasChanged();
    }

    private string HourText => _currentHour < 10 ? $"0{_currentHour}" : _currentHour.ToString();
    private string MinuteText => _currentMinute < 10 ? $"0{_currentMinute}" : _currentMinute.ToString();
    private string SecondText => _currentSecond < 10 ? $"0{_currentSecond}" : _currentSecond.ToString();

    // ------------------------------------------------------------ helpers
    private string GetMonthName(int month) =>
        System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);

    private string GetYear(OpMonth month) => month.Year.ToString();

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));

    private static string FormatDate(DateTime date, string format)
    {
        var names = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
        var sb = new System.Text.StringBuilder();

        for (var i = 0; i < format.Length; i++)
        {
            var c = format[i];

            if (c == '\'')
            {
                var end = format.IndexOf('\'', i + 1);
                if (end < 0)
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append(format, i + 1, end - i - 1);
                i = end;
                continue;
            }

            switch (c)
            {
                case 'd' when i + 1 < format.Length && format[i + 1] == 'd':
                    sb.Append(date.Day.ToString("00"));
                    i++;
                    break;
                case 'd':
                    sb.Append(date.Day);
                    break;
                case 'm' when i + 1 < format.Length && format[i + 1] == 'm':
                    sb.Append(date.Month.ToString("00"));
                    i++;
                    break;
                case 'm':
                    sb.Append(date.Month);
                    break;
                case 'M' when i + 1 < format.Length && format[i + 1] == 'M':
                    sb.Append(names.GetMonthName(date.Month));
                    i++;
                    break;
                case 'M':
                    sb.Append(names.GetAbbreviatedMonthName(date.Month));
                    break;
                case 'y' when i + 1 < format.Length && format[i + 1] == 'y' && i + 2 < format.Length &&
                              format[i + 2] == 'y' && i + 3 < format.Length && format[i + 3] == 'y':
                    sb.Append(date.Year.ToString("0000"));
                    i += 3;
                    break;
                case 'y' when i + 1 < format.Length && format[i + 1] == 'y':
                    sb.Append((date.Year % 100).ToString("00"));
                    i++;
                    break;
                case 'D' when i + 1 < format.Length && format[i + 1] == 'D':
                    sb.Append(names.GetDayName(date.DayOfWeek));
                    i++;
                    break;
                case 'D':
                    sb.Append(names.GetAbbreviatedDayName(date.DayOfWeek));
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}