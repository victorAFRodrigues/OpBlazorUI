# Graph Report - OpBlazorUI  (2026-09-16)

## Corpus Check
- 122 files · ~56,918 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 50 file(s) not represented in the graph (top: .css 45, (none) 1, .eot 1)

## Summary
- 1551 nodes · 1943 edges · 110 communities (68 shown, 42 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 6 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- DatePicker Component
- MultiSelect Component
- Select Component
- Menu Component
- AppMenu Navigation
- InputSwitch & ToggleSwitch
- ToggleButton & SelectButton
- Button Component
- Tooltip Component
- IconField Component
- SelectButton Component
- DataTable Component
- Toast Showcase Page
- Checkbox Component
- InputText Component
- DatePicker Events
- ThemeSwitcher Component
- DatePicker Showcase Page
- Showcase App Bootstrap
- Checkbox & DataTable Integration
- Toast Component Internals
- FloatLabel Component
- Select Demo
- DatePicker Calendar Model
- RadioButton Component
- MultiSelect Methods
- SelectButton Demo
- Menu Showcase Page
- Select Methods
- Launch Settings
- Message Showcase Page
- Table Showcase Page
- Button Showcase Page
- MultiSelect Showcase Page
- ToastMessage Model
- InputText Showcase Page
- RadioButton Showcase Page
- Component Razor Markup
- Select Keyboard Nav
- Menu Rendering
- Project Dependencies
- DatePicker Helpers
- Toggle Demo
- Toast Demo
- Button Parts
- Message Demo
- Message Service
- Client Imports
- Sidebar Layout
- ButtonIcon Part
- DataTableColumn Model
- MultiSelect Option Helpers
- Menu Demo
- Button Label & Loading
- Doc Theming
- Topbar Layout
- Toast Auto-Close
- MultiSelect Demo
- App Shell
- Message Component
- Routing
- Library Setup
- Error Page
- OpenCode Graphify Plugin
- DataTable Interactions
- Toast Headless Context
- OpButton Parts
- Main Layout
- DatePicker Icon
- DatePicker View
- MultiSelect Focus
- DataTable Demo
- ToastCloseEvent Model
- OpenCode Plugin Schema
- ButtonLoadingIcon Part
- Checkbox Icon
- RadioButton Change
- Counter Page
- ComingSoon Page
- Select Filter
- Base Imports
- Doc Component
- Doc Section Nav
- Doc Tokens
- Home Page

## God Nodes (most connected - your core abstractions)
1. `OpDatePicker` - 177 edges
2. `OpMultiSelect` - 108 edges
3. `OpSelect` - 107 edges
4. `OpDataTable` - 47 edges
5. `OpMenu` - 43 edges
6. `OpButton` - 39 edges
7. `OpToast` - 38 edges
8. `OpSelectButton` - 35 edges
9. `OpCheckbox` - 33 edges
10. `OpInputText` - 32 edges

## Surprising Connections (you probably didn't know these)
- `OpDataTable` --references--> `Field`  [EXTRACTED]
  src/Components/OpBlazorUI.Base/Components/DataTable/OpDataTable.razor.cs → src/Components/OpBlazorUI.Base/Models/OpDataTableColumn.cs
- `OpDataTable` --references--> `OpDataTableColumn`  [EXTRACTED]
  src/Components/OpBlazorUI.Base/Components/DataTable/OpDataTable.razor.cs → src/Components/OpBlazorUI.Base/Models/OpDataTableColumn.cs
- `OpToastHeadlessContext` --references--> `OpToastMessage`  [EXTRACTED]
  src/Components/OpBlazorUI.Base/Components/Toast/OpToast.razor.cs → src/Components/OpBlazorUI.Base/Models/OpToastMessage.cs
- `OpToast` --references--> `OpToastCloseEvent`  [EXTRACTED]
  src/Components/OpBlazorUI.Base/Components/Toast/OpToast.razor.cs → src/Components/OpBlazorUI.Base/Models/OpToastCloseEvent.cs
- `OpToast` --references--> `OpToastMessage`  [EXTRACTED]
  src/Components/OpBlazorUI.Base/Components/Toast/OpToast.razor.cs → src/Components/OpBlazorUI.Base/Models/OpToastMessage.cs

## Import Cycles
- None detected.

## Communities (110 total, 42 thin omitted)

### Community 0 - "DatePicker Component"
Cohesion: 0.02
Nodes (99): End, IReadOnlyCollection, RenderFragment, OpDatePicker, AriaLabel, AriaLabelledBy, ButtonBarTemplate, ClearButtonStyleClass (+91 more)

### Community 1 - "MultiSelect Component"
Cohesion: 0.03
Nodes (69): Dictionary, ElementReference, EventCallback, IEnumerable, IReadOnlyList, RenderFragment, OpMultiSelect, AdditionalAttributes (+61 more)

### Community 2 - "Select Component"
Cohesion: 0.03
Nodes (68): Dictionary, EventCallback, IEnumerable, IReadOnlyList, RenderFragment, OpSelect, AdditionalAttributes, AllOptions (+60 more)

### Community 3 - "Menu Component"
Cohesion: 0.05
Nodes (39): Dictionary, ElementReference, EventCallback, FocusEventArgs, IReadOnlyList, KeyboardEventArgs, List, RenderFragment (+31 more)

### Community 4 - "AppMenu Navigation"
Cohesion: 0.06
Nodes (32): OpBlazorUI.Showcase.Components.Layout, List, AppMenu, AppMenuEntry, Children, ComingSoon, Groups, HasChildren (+24 more)

### Community 5 - "InputSwitch & ToggleSwitch"
Cohesion: 0.04
Nodes (41): OpBlazorUI.Base.Components.InputSwitch, route:/toggleswitch, ChangeEventArgs, Dictionary, EventCallback, FocusEventArgs, RenderFragment, Task (+33 more)

### Community 6 - "ToggleButton & SelectButton"
Cohesion: 0.04
Nodes (40): OpBlazorUI.Base.Components.ToggleButton, route:/togglebutton, OpToggleButton, EventCallback, RenderFragment, Task, OpToggleButton, AriaLabel (+32 more)

### Community 7 - "Button Component"
Cohesion: 0.05
Nodes (37): Dictionary, EventCallback, FocusEventArgs, MouseEventArgs, RenderFragment, OpButton, AdditionalAttributes, AriaLabel (+29 more)

### Community 8 - "Tooltip Component"
Cohesion: 0.06
Nodes (30): OpBlazorUI.Base.Components.Tooltip, route:/tooltip, Dictionary, RenderFragment, Task, OpTooltip, AdditionalAttributes, ChildContent (+22 more)

### Community 9 - "IconField Component"
Cohesion: 0.06
Nodes (31): OpBlazorUI.Base.Components.IconField, route:/iconfield, Dictionary, RenderFragment, OpIconField, AdditionalAttributes, ChildContent, IconPosition (+23 more)

### Community 10 - "SelectButton Component"
Cohesion: 0.07
Nodes (27): Index, Option, EventCallback, IEnumerable, IReadOnlyList, RenderFragment, Task, OpSelectButton (+19 more)

### Community 11 - "DataTable Component"
Cohesion: 0.06
Nodes (33): Order, Dictionary, EventCallback, IReadOnlyList, List, OpDataTable, AdditionalAttributes, AllPageSelected (+25 more)

### Community 12 - "Toast Showcase Page"
Cohesion: 0.06
Nodes (33): route:/toast, Add, AddMultiple, Clear, ClearSticky, Api, ApiTable, ChildContent (+25 more)

### Community 13 - "Checkbox Component"
Cohesion: 0.06
Nodes (32): ChangeEventArgs, Dictionary, EventCallback, FocusEventArgs, RenderFragment, Task, OpCheckbox, AdditionalAttributes (+24 more)

### Community 14 - "InputText Component"
Cohesion: 0.06
Nodes (30): ChangeEventArgs, Dictionary, EventCallback, FocusEventArgs, KeyboardEventArgs, Task, OpInputText, AdditionalAttributes (+22 more)

### Community 15 - "DatePicker Events"
Cohesion: 0.12
Nodes (5): ChangeEventArgs, FocusEventArgs, KeyboardEventArgs, MouseEventArgs, Task

### Community 16 - "ThemeSwitcher Component"
Cohesion: 0.08
Nodes (23): OpBlazorUI.Base.Components.ThemeSwitcher, IAsyncDisposable, Lazy, microsoft_jsinterop, EventCallback, IJSRuntime, IReadOnlyList, Task (+15 more)

### Community 17 - "DatePicker Showcase Page"
Cohesion: 0.07
Nodes (26): OpBlazorUI.Base.Components.DatePicker, route:/datepicker, EventCallback, OpDatePickerButtonBarContext, ClearCallback, TodayCallback, OpDatePickerInputIconContext, ClickCallback (+18 more)

### Community 18 - "Showcase App Bootstrap"
Cohesion: 0.07
Nodes (20): OpBlazorUI.Base, microsoft_aspnetcore_builder, microsoft_extensions_dependencyinjection, microsoft_extensions_hosting, OpBlazorUI.Showcase, opblazorui_showcase_client_pages, OpBlazorUI.Showcase.Components, OpCss (+12 more)

### Community 19 - "Checkbox & DataTable Integration"
Cohesion: 0.07
Nodes (24): OpBlazorUI.Base.Components.Checkbox, route:/checkbox, OpBlazorUI.Base.Components.Icon, OpCheckbox, OpIcon, OpBlazorUI.Base.Components.Icon, OpBlazorUI.Base.Components.InputIcon, OpCheckbox (+16 more)

### Community 20 - "Toast Component Internals"
Cohesion: 0.07
Nodes (24): CancellationTokenSource, HashSet, IDisposable, Dictionary, EventCallback, IReadOnlyList, RenderFragment, OpToast (+16 more)

### Community 21 - "FloatLabel Component"
Cohesion: 0.07
Nodes (24): OpBlazorUI.Base.Components.FloatLabel, route:/floatlabel, Dictionary, RenderFragment, OpFloatLabel, AdditionalAttributes, ChildContent, RootClass (+16 more)

### Community 22 - "Select Demo"
Cohesion: 0.08
Nodes (22): OpBlazorUI.Base.Components.Select, Description, route:/select, City, CountryGroup, ItemTemplate, OpSelect, Api (+14 more)

### Community 23 - "DatePicker Calendar Model"
Cohesion: 0.11
Nodes (9): DateTime, OpDateMeta, Date, Day, Month, OtherMonth, Selectable, Today (+1 more)

### Community 24 - "RadioButton Component"
Cohesion: 0.08
Nodes (24): Dictionary, EventCallback, FocusEventArgs, OpRadioButton, AdditionalAttributes, AriaLabel, AriaLabelledBy, Checked (+16 more)

### Community 25 - "MultiSelect Methods"
Cohesion: 0.17
Nodes (4): ChangeEventArgs, KeyboardEventArgs, MouseEventArgs, Task

### Community 26 - "SelectButton Demo"
Cohesion: 0.10
Nodes (19): OpBlazorUI.Base.Components.SelectButton, OpOption, route:/selectbutton, ItemTemplate, OpSelectButton, Api, ApiTable, ChildContent (+11 more)

### Community 27 - "Menu Showcase Page"
Cohesion: 0.10
Nodes (19): route:/menu, OnItemClick, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent (+11 more)

### Community 28 - "Select Methods"
Cohesion: 0.20
Nodes (3): FocusEventArgs, MouseEventArgs, Task

### Community 29 - "Launch Settings"
Cohesion: 0.12
Nodes (17): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, inspectUri, launchBrowser, applicationUrl (+9 more)

### Community 30 - "Message Showcase Page"
Cohesion: 0.12
Nodes (16): route:/message, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent, DocSection (+8 more)

### Community 31 - "Table Showcase Page"
Cohesion: 0.12
Nodes (16): route:/table, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent, DocSection (+8 more)

### Community 32 - "Button Showcase Page"
Cohesion: 0.12
Nodes (15): route:/button, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent, DocSection (+7 more)

### Community 33 - "MultiSelect Showcase Page"
Cohesion: 0.12
Nodes (15): route:/multiselect, Api, ApiTable, ChildContent, Code, CodeBlock, CountryGroup, DocComponent (+7 more)

### Community 34 - "ToastMessage Model"
Cohesion: 0.14
Nodes (13): IEqualityComparer, OpToastMessageComparer, OpToastMessage, Closable, Data, Detail, Icon, Key (+5 more)

### Community 35 - "InputText Showcase Page"
Cohesion: 0.13
Nodes (14): route:/inputtext, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent, DocSection (+6 more)

### Community 36 - "RadioButton Showcase Page"
Cohesion: 0.13
Nodes (14): route:/radiobutton, Api, ApiTable, ChildContent, Code, CodeBlock, DocComponent, DocSection (+6 more)

### Community 37 - "Component Razor Markup"
Cohesion: 0.16
Nodes (9): OpBlazorUI.Base.Components.Menu, OpBlazorUI.Base.Components.InputText, OpBlazorUI.Base.Components.Button, microsoft_aspnetcore_components_web, OpButton, OpInputText, OpButton, OpInputText (+1 more)

### Community 39 - "Menu Rendering"
Cohesion: 0.19
Nodes (7): OpBlazorUI.Base.Services, OpBlazorUI.Base.Models, OpBlazorUI.Base.Components.Toast, microsoft_aspnetcore_components_webassembly_hosting, RenderItem, system_collections_generic, system_runtime_compilerservices

### Community 40 - "Project Dependencies"
Cohesion: 0.19
Nodes (9): Microsoft.AspNetCore.Components.Web (10.0.9), Microsoft.AspNetCore.Components.WebAssembly (10.0.9), Microsoft.AspNetCore.Components.WebAssembly.Server (10.0.9), Microsoft.NET.Sdk.BlazorWebAssembly, Microsoft.NET.Sdk.Razor, Microsoft.NET.Sdk.Web, net10.0, net10.0 (+1 more)

### Community 41 - "DatePicker Helpers"
Cohesion: 0.17
Nodes (7): IReadOnlyList, List, OpMonth, Month, WeekNumbers, Weeks, Year

### Community 42 - "Toggle Demo"
Cohesion: 0.18
Nodes (9): OpBlazorUI.Base.Components.RadioButton, OpBlazorUI.Base.Components.InputIcon, OpCheckbox, OpFloatLabel, OpIconField, OpInputIcon, OpInputSwitch, OpInputText (+1 more)

### Community 43 - "Toast Demo"
Cohesion: 0.20
Nodes (9): HeadlessTemplate, ClearAll, Dispose, MessageTemplate, OpButton, OpToast, ShowMultiple, ShowToast (+1 more)

### Community 44 - "Button Parts"
Cohesion: 0.31
Nodes (4): OpBlazorUI.Base.Components.DataTable, OpBlazorUI.Base.Components.Button.Parts, microsoft_aspnetcore_components, system_reflection

### Community 45 - "Message Demo"
Cohesion: 0.22
Nodes (8): DynamicMsg, AddDynamicMessages, ClearDynamicMessages, OpBlazorUI.Base.Components.Message, OpButton, OpInputText, OpMessage, ShowLifeMessage

### Community 46 - "Message Service"
Cohesion: 0.22
Nodes (5): IEnumerable, IReadOnlyList, List, OpMessageService, Messages

### Community 47 - "Client Imports"
Cohesion: 0.22
Nodes (8): Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.RenderMode, Microsoft.AspNetCore.Components.Web.Virtualization, Microsoft.JSInterop, OpBlazorUI.Showcase.Client, System.Net.Http.Json

### Community 48 - "Sidebar Layout"
Cohesion: 0.22
Nodes (8): Dispose, IsActive, IsExpanded, OnDarkChanged, OnInitialized, OnSidebarToggled, Configurator, Toggle

### Community 49 - "ButtonIcon Part"
Cohesion: 0.25
Nodes (7): RenderFragment, OpButtonIcon, HasLabel, Icon, IconClass, IconPos, Template

### Community 50 - "DataTableColumn Model"
Cohesion: 0.25
Nodes (6): OpDataTableColumn, Field, Header, Sortable, StyleClass, Width

### Community 52 - "Menu Demo"
Cohesion: 0.25
Nodes (7): OnInlineClick, OnPopupItemClick, OpButton, OpMenu, OpMenuGroup, OpMenuItem, TogglePopup

### Community 53 - "Button Label & Loading"
Cohesion: 0.29
Nodes (7): ComponentBase, OpButtonLabel, Label, RenderFragment, OpButtonLoadingIcon, Icon, Template

### Community 54 - "Doc Theming"
Cohesion: 0.29
Nodes (5): OpBlazorUI.Showcase.Components.Doc, Dictionary, DocTheming, Value, Var

### Community 55 - "Topbar Layout"
Cohesion: 0.29
Nodes (6): Dispose, OnDarkChanged, OnInitialized, Configurator, ToggleConfig, ToggleDarkMode

### Community 57 - "MultiSelect Demo"
Cohesion: 0.33
Nodes (4): OpBlazorUI.Base.Components.MultiSelect, City, CountryGroup, OpMultiSelect

### Community 58 - "App Shell"
Cohesion: 0.33
Nodes (5): HeadOutlet, ImportMap, OpBlazorUiSetup, ResourcePreloader, Routes

### Community 59 - "Message Component"
Cohesion: 0.33
Nodes (5): AutoCloseAsync, CloseAsync, OnCloseClick, OnInitialized, MouseEventArgs

### Community 61 - "Routing"
Cohesion: 0.40
Nodes (4): FocusOnNavigate, Found, Router, RouteView

### Community 62 - "Library Setup"
Cohesion: 0.40
Nodes (4): HeadContent, Dispose, OnInitialized, OnThemeChanged

### Community 63 - "Error Page"
Cohesion: 0.40
Nodes (4): Microsoft.AspNetCore.Http, OnInitialized, PageTitle, System.Diagnostics

### Community 64 - "OpenCode Graphify Plugin"
Cohesion: 0.40
Nodes (3): IMPORTANT: keep the reminder string free of backticks and $(...) constructs., ref_fs, ref_path

### Community 68 - "Toast Headless Context"
Cohesion: 0.50
Nodes (4): Action, OpToastHeadlessContext, Close, Message

### Community 69 - "OpButton Parts"
Cohesion: 0.50
Nodes (3): OpButtonIcon, OpButtonLabel, OpButtonLoadingIcon

### Community 70 - "Main Layout"
Cohesion: 0.50
Nodes (3): Sidebar, ThemeSetup, Topbar

### Community 71 - "DatePicker Icon"
Cohesion: 0.50
Nodes (3): MouseEventArgs, OpBlazorUI.Base.Components.Icon, OpIcon

### Community 72 - "DatePicker View"
Cohesion: 0.50
Nodes (4): OpDatePickerView, Date, Month, Year

### Community 75 - "DataTable Demo"
Cohesion: 0.50
Nodes (3): OpDataTable, OpDataTableColumn, Product

### Community 76 - "ToastCloseEvent Model"
Cohesion: 0.50
Nodes (3): OpToastCloseEvent, Index, Message

## Knowledge Gaps
- **980 isolated node(s):** `$schema`, `plugin`, `OpButtonLoadingIcon`, `OpButtonIcon`, `OpButtonLabel` (+975 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1153 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **42 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `OpDatePicker` connect `DatePicker Component` to `DatePicker View`, `DatePicker Helpers`, `DatePicker Events`, `DatePicker Hour Controls`, `DatePicker Showcase Page`, `DatePicker Minute Controls`, `DatePicker Second Controls`, `Button Label & Loading`, `DatePicker Calendar Model`, `DatePicker Month Names`?**
  _High betweenness centrality (0.210) - this node is a cross-community bridge._
- **Why does `OpSelect` connect `Select Component` to `Select Option Methods`, `Select Keyboard Nav`, `Button Label & Loading`, `Select Demo`, `Select Filter`, `Select Methods`?**
  _High betweenness centrality (0.162) - this node is a cross-community bridge._
- **Why does `OpBlazorUI.Showcase.Components.Doc` connect `Doc Theming` to `Button Showcase Page`, `MultiSelect Showcase Page`, `InputText Showcase Page`, `RadioButton Showcase Page`, `InputSwitch & ToggleSwitch`, `ToggleButton & SelectButton`, `Tooltip Component`, `IconField Component`, `Toast Showcase Page`, `DatePicker Showcase Page`, `Checkbox & DataTable Integration`, `FloatLabel Component`, `Select Demo`, `SelectButton Demo`, `Menu Showcase Page`, `Message Showcase Page`, `Table Showcase Page`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **What connects `$schema`, `plugin`, `OpButtonLoadingIcon` to the rest of the system?**
  _980 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `DatePicker Component` be split into smaller, more focused modules?**
  _Cohesion score 0.01904761904761905 - nodes in this community are weakly interconnected._
- **Should `MultiSelect Component` be split into smaller, more focused modules?**
  _Cohesion score 0.028169014084507043 - nodes in this community are weakly interconnected._
- **Should `Select Component` be split into smaller, more focused modules?**
  _Cohesion score 0.02857142857142857 - nodes in this community are weakly interconnected._