namespace OpBlazorUI.Base.Globalization;

/// <summary>
/// Textos de UI dos componentes — nomes de mês/dia, rótulos, mensagens e aria — definidos uma vez
/// no setup do app: <c>AddOpBlazorUI(theme =&gt; ..., OpTranslations.PtBr)</c>. Espelha o objeto
/// <c>translation</c> do PrimeNG, mas com strings explícitas (sem depender da cultura/ICU da
/// plataforma), então funciona igual em Server, WebAssembly e SSR estático.
/// Os defaults são o inglês (equivalente a <see cref="OpTranslations.EnUs"/>).
/// </summary>
public sealed class OpTranslation
{
    // Filtro / comparação (usados por filtros de coluna, FilterService etc.).
    public string StartsWith { get; set; } = "Starts with";
    public string Contains { get; set; } = "Contains";
    public string NotContains { get; set; } = "Not contains";
    public string EndsWith { get; set; } = "Ends with";

    /// <summary>Rótulo de operador "igual" (no upstream é a chave <c>equals</c>).</summary>
    public string EqualsText { get; set; } = "Equals";

    public string NotEquals { get; set; } = "Not equals";
    public string NoFilter { get; set; } = "No filter";
    public string Lt { get; set; } = "Less than";
    public string Lte { get; set; } = "Less than or equal to";
    public string Gt { get; set; } = "Greater than";
    public string Gte { get; set; } = "Greater than or equal to";
    public string Is { get; set; } = "Is";
    public string IsNot { get; set; } = "Is not";
    public string Before { get; set; } = "Before";
    public string After { get; set; } = "After";
    public string DateIs { get; set; } = "Date is";
    public string DateIsNot { get; set; } = "Date is not";
    public string DateBefore { get; set; } = "Date is before";
    public string DateAfter { get; set; } = "Date is after";

    // Ações e mensagens genéricas.
    public string Clear { get; set; } = "Clear";
    public string Apply { get; set; } = "Apply";
    public string Cancel { get; set; } = "Cancel";
    public string Accept { get; set; } = "Yes";
    public string Reject { get; set; } = "No";
    public string Choose { get; set; } = "Choose";
    public string Upload { get; set; } = "Upload";
    public string AddRule { get; set; } = "Add Rule";
    public string RemoveRule { get; set; } = "Remove Rule";
    public string MatchAll { get; set; } = "All";
    public string MatchAny { get; set; } = "Any";
    public string Pending { get; set; } = "Pending";
    public string Completed { get; set; } = "Completed";
    public string EmptyMessage { get; set; } = "No results found";
    public string EmptyFilterMessage { get; set; } = "No results found";
    public string SearchMessage { get; set; } = "{0} results are available";
    public string SelectionMessage { get; set; } = "{0} items selected";
    public string EmptySelectionMessage { get; set; } = "No selected item";
    public string EmptySearchMessage { get; set; } = "No results found";
    public string[] FileSizeTypes { get; set; } = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
    public string FileChosenMessage { get; set; } = "{0} files chosen";
    public string NoFileChosenMessage { get; set; } = "No file chosen";

    /// <summary>Mensagem de validação de um valor não conversível (com <c>{0}</c> = valor e <c>{1}</c> = tipo).</summary>
    public string InvalidValue { get; set; } = "The value '{0}' is not valid for {1}.";

    /// <summary>Placeholder/rótulo do campo de filtro embutido nos seletores (extensão nossa).</summary>
    public string FilterPlaceholder { get; set; } = "Filter";

    /// <summary>Nome acessível do filtro de uma coluna, com <c>{0}</c> = cabeçalho (extensão nossa).</summary>
    public string FilterColumn { get; set; } = "Filter {0}";

    // Data / hora (DatePicker).
    public string[] DayNames { get; set; } =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    public string[] DayNamesShort { get; set; } = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    public string[] DayNamesMin { get; set; } = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
    public string[] MonthNames { get; set; } =
        ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    public string[] MonthNamesShort { get; set; } =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    public string DateFormat { get; set; } = "mm/dd/yy";
    public int FirstDayOfWeek { get; set; }
    public string Today { get; set; } = "Today";
    public string WeekHeader { get; set; } = "Wk";
    public string ChooseDate { get; set; } = "Choose Date";
    public string ChooseMonth { get; set; } = "Choose Month";
    public string ChooseYear { get; set; } = "Choose Year";
    public string PrevDecade { get; set; } = "Previous Decade";
    public string NextDecade { get; set; } = "Next Decade";
    public string PrevYear { get; set; } = "Previous Year";
    public string NextYear { get; set; } = "Next Year";
    public string PrevMonth { get; set; } = "Previous Month";
    public string NextMonth { get; set; } = "Next Month";
    public string PrevHour { get; set; } = "Previous Hour";
    public string NextHour { get; set; } = "Next Hour";
    public string PrevMinute { get; set; } = "Previous Minute";
    public string NextMinute { get; set; } = "Next Minute";
    public string PrevSecond { get; set; } = "Previous Second";
    public string NextSecond { get; set; } = "Next Second";
    public string Am { get; set; } = "AM";
    public string Pm { get; set; } = "PM";

    // Senha.
    public string Weak { get; set; } = "Weak";
    public string Medium { get; set; } = "Medium";
    public string Strong { get; set; } = "Strong";
    public string PasswordPrompt { get; set; } = "Enter a password";

    // Números (InputNumber): separadores e símbolo explícitos, no lugar de CultureInfo/ICU.
    public string DecimalSeparator { get; set; } = ".";
    public string ThousandSeparator { get; set; } = ",";
    public string CurrencySymbol { get; set; } = "$";

    // Seletor de tema (extensão nossa: o PrimeNG não tem ThemeSwitcher).
    public string LightMode { get; set; } = "Light mode";
    public string DarkMode { get; set; } = "Dark mode";

    /// <summary>Nome acessível de uma cor de tema, com <c>{0}</c> = nome do tema.</summary>
    public string Theme { get; set; } = "Theme {0}";

    // Editor (toolbar própria; o miolo do texto fica a cargo do Quill).
    public OpEditorTranslation Editor { get; set; } = new();

    public OpAria Aria { get; set; } = new();
}

/// <summary>
/// Rótulos de acessibilidade (atributos <c>aria-*</c>). Espelha a interface <c>Aria</c> do PrimeNG.
/// </summary>
public sealed class OpAria
{
    public string TrueLabel { get; set; } = "True";
    public string FalseLabel { get; set; } = "False";
    public string NullLabel { get; set; } = "Not selected";
    public string Star { get; set; } = "1 star";
    public string Stars { get; set; } = "{star} stars";
    public string SelectAll { get; set; } = "All items selected";
    public string UnselectAll { get; set; } = "All items unselected";
    public string Close { get; set; } = "Close";
    public string Previous { get; set; } = "Previous";
    public string Next { get; set; } = "Next";
    public string Navigation { get; set; } = "Navigation";
    public string ScrollTop { get; set; } = "Scroll Top";
    public string MoveTop { get; set; } = "Move Top";
    public string MoveUp { get; set; } = "Move Up";
    public string MoveDown { get; set; } = "Move Down";
    public string MoveBottom { get; set; } = "Move Bottom";
    public string MoveToTarget { get; set; } = "Move to Target";
    public string MoveToSource { get; set; } = "Move to Source";
    public string MoveAllToTarget { get; set; } = "Move All to Target";
    public string MoveAllToSource { get; set; } = "Move All to Source";
    public string PageLabel { get; set; } = "Page {page}";
    public string FirstPageLabel { get; set; } = "First Page";
    public string LastPageLabel { get; set; } = "Last Page";
    public string NextPageLabel { get; set; } = "Next Page";
    public string PrevPageLabel { get; set; } = "Previous Page";
    public string PreviousPageLabel { get; set; } = "Previous Page";
    public string RowsPerPageLabel { get; set; } = "Rows per page";
    public string JumpToPageDropdownLabel { get; set; } = "Jump to Page Dropdown";
    public string JumpToPageInputLabel { get; set; } = "Jump to Page Input";
    public string SelectRow { get; set; } = "Row Selected";
    public string UnselectRow { get; set; } = "Row Unselected";
    public string ExpandRow { get; set; } = "Row Expanded";
    public string CollapseRow { get; set; } = "Row Collapsed";
    public string ShowFilterMenu { get; set; } = "Show Filter Menu";
    public string HideFilterMenu { get; set; } = "Hide Filter Menu";
    public string FilterOperator { get; set; } = "Filter Operator";
    public string FilterConstraint { get; set; } = "Filter Constraint";
    public string EditRow { get; set; } = "Row Edit";
    public string SaveEdit { get; set; } = "Save";
    public string CancelEdit { get; set; } = "Cancel";
    public string ListView { get; set; } = "List View";
    public string GridView { get; set; } = "Grid View";
    public string Slide { get; set; } = "Slide";
    public string SlideNumber { get; set; } = "{slideNumber}";
    public string ZoomImage { get; set; } = "Zoom Image";
    public string ZoomIn { get; set; } = "Zoom In";
    public string ZoomOut { get; set; } = "Zoom Out";
    public string RotateRight { get; set; } = "Rotate Right";
    public string RotateLeft { get; set; } = "Rotate Left";
    public string ThumbnailPrev { get; set; } = "Previous thumbnails";
    public string ThumbnailNext { get; set; } = "Next thumbnails";
    public string ListLabel { get; set; } = "Option List";
    public string DropdownTrigger { get; set; } = "Show options";
    public string SelectColor { get; set; } = "Select a color";
    public string RemoveLabel { get; set; } = "Remove";
    public string BrowseFiles { get; set; } = "Browse Files";
    public string MaximizeLabel { get; set; } = "Maximize";
    public string MinimizeLabel { get; set; } = "Minimize";

    /// <summary>Nome acessível de um campo de caractere, com <c>{0}</c> = posição (extensão nossa).</summary>
    public string Character { get; set; } = "Character {0}";
}

/// <summary>
/// Textos da toolbar do <see cref="OpBlazorUI.Base.Components.Editor"/> (rótulos de acessibilidade e
/// do seletor de cabeçalho). O conteúdo em si é do Quill — aqui só entra o que nós renderizamos.
/// </summary>
public sealed class OpEditorTranslation
{
    public string Bold { get; set; } = "Bold";
    public string Italic { get; set; } = "Italic";
    public string Underline { get; set; } = "Underline";
    public string TextColor { get; set; } = "Text color";
    public string BackgroundColor { get; set; } = "Background color";
    public string OrderedList { get; set; } = "Ordered list";
    public string UnorderedList { get; set; } = "Unordered list";
    public string Align { get; set; } = "Align";
    public string InsertLink { get; set; } = "Insert link";
    public string InsertImage { get; set; } = "Insert image";
    public string CodeBlock { get; set; } = "Code block";
    public string ClearFormatting { get; set; } = "Clear formatting";

    // Opções do seletor de cabeçalho (<select class="ql-header">).
    public string Heading1 { get; set; } = "Title";
    public string Heading2 { get; set; } = "Subtitle";
    public string Normal { get; set; } = "Normal";

    /// <summary>Mensagem exibida quando o Quill não carrega.</summary>
    public string LoadError { get; set; } = "Could not load the editor (Quill).";
}
