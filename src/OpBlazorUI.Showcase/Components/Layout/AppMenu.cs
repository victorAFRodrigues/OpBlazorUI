using System.Collections.Generic;

namespace OpBlazorUI.Showcase.Components.Layout;

public sealed class AppMenuItem
{
    public string Label { get; init; } = "";
    public string Route { get; init; } = "/coming-soon";
    public bool ComingSoon { get; init; } = true;
    public string? Icon { get; init; }
}

public sealed class AppMenuGroup
{
    public string Label { get; init; } = "";
    public List<AppMenuItem> Items { get; init; } = [];
}

public sealed class AppMenuEntry
{
    public string Label { get; init; } = "";
    public string? Icon { get; init; }
    public string? Route { get; init; }
    public bool ComingSoon { get; init; }
    public List<AppMenuItem> Children { get; init; } = [];
    public List<AppMenuGroup> Groups { get; init; } = [];

    public bool HasChildren => Children.Count > 0 || Groups.Count > 0;
}

public static class AppMenu
{
    private static AppMenuItem Item(string label, string? route = null) => new()
    {
        Label = label,
        Route = route ?? $"/coming-soon/{Slug(label)}",
        ComingSoon = route is null
    };

    private static AppMenuItem GroupItem(string label, string? route = null) => Item(label, route);

    private static string Slug(string s) => s.ToLowerInvariant()
        .Replace(" ", "-").Replace(".", "").Replace("/", "-");

    public static readonly List<AppMenuEntry> Entries =
    [
        new AppMenuEntry
        {
            Label = "Getting Started", Icon = "pi pi-home",
            Children =
            [
                Item("Installation", "/installation"), Item("Configuration", "/configuration")
            ]
        },
        new AppMenuEntry
        {
            Label = "Templates", Icon = "pi pi-clone",
            Children = [Item("Sparked")]
        },
        new AppMenuEntry
        {
            Label = "Theming", Icon = "pi pi-palette",
            Children = [Item("Styled Mode"), Item("Unstyled Mode"), Item("Tailwind CSS")]
        },
        new AppMenuEntry
        {
            Label = "AI Tools", Icon = "pi pi-sparkles",
            Children = [Item("LLMs.txt")]
        },
        new AppMenuEntry
        {
            Label = "Components", Icon = "pi pi-compass",
            Groups =
            [
                new AppMenuGroup
                {
                    Label = "Form",
                    Items =
                    [
                        GroupItem("AutoComplete"), GroupItem("CascadeSelect", "/cascadeselect"),
                        GroupItem("Checkbox", "/checkbox"), GroupItem("ColorPicker"),
                        GroupItem("DatePicker", "/datepicker"), GroupItem("Editor", "/editor"),
                        GroupItem("FloatLabel", "/floatlabel"), GroupItem("IconField", "/iconfield"),
                        GroupItem("IftaLabel", "/iftalabel"), GroupItem("InputGroup", "/inputgroup"), GroupItem("InputMask", "/inputmask"),
                        GroupItem("InputNumber", "/inputnumber"), GroupItem("InputOtp", "/inputotp"),
                        GroupItem("InputText", "/inputtext"), GroupItem("KeyFilter", "/keyfilter"),
                        GroupItem("Listbox", "/listbox"), GroupItem("MultiSelect", "/multiselect"),
                        GroupItem("Password", "/password"), GroupItem("RadioButton", "/radiobutton"), GroupItem("Rating", "/rating"),
                        GroupItem("Select", "/select"), GroupItem("SelectButton", "/selectbutton"),
                        GroupItem("Textarea"),
                        GroupItem("ToggleButton", "/togglebutton"), GroupItem("ToggleSwitch", "/toggleswitch"),
                        GroupItem("TreeSelect", "/treeselect")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "Button",
                    Items = [GroupItem("Button", "/button"), GroupItem("SpeedDial", "/speeddial"), GroupItem("SplitButton", "/splitbutton")]
                },
                new AppMenuGroup
                {
                    Label = "Data",
                    Items =
                    [
                        GroupItem("DataView"), GroupItem("OrderList"),
                        GroupItem("Paginator"), GroupItem("PickList"), GroupItem("Table", "/table"),
                        GroupItem("Timeline"), GroupItem("Tree"), GroupItem("TreeTable"), GroupItem("VirtualScroller")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "Panel",
                    Items =
                    [
                        GroupItem("Accordion"), GroupItem("Card"), GroupItem("Divider"),
                        GroupItem("Fieldset"), GroupItem("Panel"), GroupItem("ScrollPanel"),
                        GroupItem("Splitter"), GroupItem("Stepper"), GroupItem("Tabs"), GroupItem("Toolbar")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "Overlay",
                    Items =
                    [
                        GroupItem("ConfirmDialog"), GroupItem("ConfirmPopup"), GroupItem("Dialog"),
                        GroupItem("Drawer"), GroupItem("DynamicDialog"), GroupItem("Popover"),
                        GroupItem("Tooltip", "/tooltip")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "File",
                    Items = [GroupItem("Upload")]
                },
                new AppMenuGroup
                {
                    Label = "Menu",
                    Items =
                    [
                        GroupItem("Breadcrumb"), GroupItem("ContextMenu"),
                        GroupItem("Menu", "/menu"), GroupItem("Menubar"), GroupItem("MegaMenu"),
                        GroupItem("PanelMenu"), GroupItem("TieredMenu")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "Chart",
                    Items = [GroupItem("Chart.js")]
                },
                new AppMenuGroup
                {
                    Label = "Messages",
                    Items = [GroupItem("Message", "/message"), GroupItem("Toast", "/toast")]
                },
                new AppMenuGroup
                {
                    Label = "Media",
                    Items = [GroupItem("Carousel"), GroupItem("Galleria"), GroupItem("Image"), GroupItem("ImageCompare")]
                },
                new AppMenuGroup
                {
                    Label = "Misc",
                    Items =
                    [
                        GroupItem("AnimateOnScroll"), GroupItem("AutoFocus"), GroupItem("Avatar", "/avatar"),
                        GroupItem("Badge", "/badge"), GroupItem("Bind"), GroupItem("BlockUI"), GroupItem("Chip", "/chip"),
                        GroupItem("ClassNames"), GroupItem("DragDrop"), GroupItem("Fluid"),
                        GroupItem("FocusTrap"), GroupItem("Inplace"), GroupItem("MeterGroup"),
                        GroupItem("ProgressBar", "/progressbar"), GroupItem("ProgressSpinner", "/progressspinner"),
                        GroupItem("ScrollTop"), GroupItem("Skeleton", "/skeleton"), GroupItem("StyleClass"),
                        GroupItem("Tag", "/tag")
                    ]
                },
                new AppMenuGroup
                {
                    Label = "Utilities",
                    Items = [GroupItem("FilterService")]
                }
            ]
        },
        new AppMenuEntry { Label = "Pass Through", Icon = "pi pi-directions", ComingSoon = true },
        new AppMenuEntry
        {
            Label = "Guides", Icon = "pi pi-book",
            Children = [Item("Accessibility"), Item("Animations"), Item("PrimeFlex"), Item("RTL")]
        },
        new AppMenuEntry
        {
            Label = "Icons", Icon = "pi pi-eye",
            Children = [Item("OpenNG Icons"), Item("Custom Icons")]
        },
        new AppMenuEntry
        {
            Label = "Migration", Icon = "pi pi-arrow-up",
            Children = [Item("From PrimeNG"), Item("Update Optimus UI")]
        },
        new AppMenuEntry
        {
            Label = "Support", Icon = "pi pi-question",
            Children = [Item("FAQ"), Item("Discord"), Item("GitHub Discussions")]
        },
        new AppMenuEntry
        {
            Label = "Discover", Icon = "pi pi-search",
            Children = [Item("Roadmap"), Item("Source Code"), Item("Changelog"), Item("Contribution")]
        }
    ];
}
