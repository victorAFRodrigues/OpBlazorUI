namespace OpBlazorUI.Showcase.Components.Pages;

public static class ConfirmDialogCodeSamples
{
    public static readonly string ImportCode = 
        "@using OpBlazorUI.Base.Components.ConfirmDialog\n" +
        "@using OpBlazorUI.Base.Components.ConfirmDialog.Models\n" +
        "@using OpBlazorUI.Base.Services\n\n" +
        "// Program.cs - registra OpConfirmationService e OpMessageService\n" +
        "builder.Services.AddOpBlazorUI(theme => theme.Preset = \"lara\");";

    public static readonly string BasicCode = 
        "<OpToast />\n" +
        "<OpConfirmDialog Key=\"basic\" />\n" +
        "<OpButton Label=\"Salvar\" Outlined=\"true\" @onclick=\"ConfirmSave\" />\n" +
        "<OpButton Label=\"Excluir\" Severity=\"danger\" Outlined=\"true\" @onclick=\"ConfirmDelete\" />\n\n" +
        "@code {\n" +
        "    [Inject] OpConfirmationService ConfirmationService { get; set; } = default!;\n" +
        "    [Inject] OpMessageService MessageService { get; set; } = default!;\n\n" +
        "    // Key do componente e Key das opções precisam ser iguais\n" +
        "    void ConfirmSave() => ConfirmationService.Confirm(new ConfirmOptions\n" +
        "    {\n" +
        "        Key = \"basic\",\n" +
        "        Header = \"Salvar\", Message = \"Deseja salvar?\", Icon = \"pi pi-save\",\n" +
        "        Accept = () => MessageService.Add(new OpToastMessage { Severity = \"success\", Summary = \"OK\", Detail = \"Salvo\" })\n" +
        "    });\n" +
        "}";

    public static readonly string CustomCode = 
        "<OpConfirmDialog Key=\"custom\" />\n" +
        "<OpButton Label=\"Excluir\" Severity=\"danger\" Outlined=\"true\" @onclick=\"ConfirmCustom\" />\n\n" +
        "@code {\n" +
        "    void ConfirmCustom() => ConfirmationService.Confirm(new ConfirmOptions\n" +
        "    {\n" +
        "        Key = \"custom\",\n" +
        "        Header = \"Excluir\", Message = \"Esta ação não pode ser desfeita.\", Icon = \"pi pi-exclamation-triangle\",\n" +
        "        AcceptLabel = \"Excluir\", RejectLabel = \"Cancelar\",\n" +
        "        AcceptButtonProps = new Dictionary<string, object> { [\"severity\"] = \"danger\" },\n" +
        "        RejectButtonProps = new Dictionary<string, object> { [\"severity\"] = \"secondary\", [\"outlined\"] = true }\n" +
        "    });\n" +
        "}";

    public static readonly string PositionCode = 
        "<OpConfirmDialog Key=\"position\" Position=\"@_position\" />\n" +
        "<OpButton Label=\"Centro\" @onclick=\"() => ConfirmPosition(\\\"center\\\")\" />\n" +
        "<OpButton Label=\"Topo\" @onclick=\"() => ConfirmPosition(\\\"top\\\")\" />\n" +
        "<OpButton Label=\"Base\" @onclick=\"() => ConfirmPosition(\\\"bottom\\\")\" />\n\n" +
        "@code {\n" +
        "    private string _position = \"center\";\n\n" +
        "    void ConfirmPosition(string position)\n" +
        "    {\n" +
        "        _position = position;\n" +
        "        ConfirmationService.Confirm(new ConfirmOptions\n" +
        "        {\n" +
        "            Key = \"position\",\n" +
        "            Header = $\"Posição: {position}\",\n" +
        "            Message = \"O dialog abre na posição escolhida.\",\n" +
        "            Position = position\n" +
        "        });\n" +
        "    }\n" +
        "}";

    public static readonly string HeadlessCode = 
        "<OpConfirmDialog Key=\"headless\">\n" +
        "    <HeadlessTemplate Context=\"ctx\">\n" +
        "        <div class=\"flex flex-col items-center p-8 bg-surface-0 dark:bg-surface-900 rounded-xl\">\n" +
        "            <div class=\"rounded-full bg-primary text-primary-contrast inline-flex justify-center items-center h-24 w-24 -mt-20\">\n" +
        "                <i class=\"pi pi-question !text-5xl\"></i>\n" +
        "            </div>\n" +
        "            <span class=\"font-bold text-2xl block mb-2 mt-6\">@ctx.Options.Header</span>\n" +
        "            <p class=\"mb-0\">@ctx.Options.Message</p>\n" +
        "            <div class=\"flex items-center gap-2 mt-6\">\n" +
        "                <OpButton Label=\"Confirmar\" @onclick=\"ctx.OnAccept\" StyleClass=\"w-32\" Severity=\"danger\" />\n" +
        "                <OpButton Label=\"Cancelar\" @onclick=\"ctx.OnReject\" StyleClass=\"w-32\" Outlined=\"true\" Severity=\"secondary\" />\n" +
        "            </div>\n" +
        "        </div>\n" +
        "    </HeadlessTemplate>\n" +
        "</OpConfirmDialog>\n" +
        "<OpButton Label=\"Mostrar\" @onclick=\"ConfirmHeadless\" />\n\n" +
        "@code {\n" +
        "    void ConfirmHeadless() => ConfirmationService.Confirm(new ConfirmOptions\n" +
        "    {\n" +
        "        Key = \"headless\", Header = \"Headless Mode\", Message = \"UI totalmente customizada.\"\n" +
        "    });\n" +
        "}";

    public static readonly string TemplateCode = 
        "<OpConfirmDialog Key=\"template\">\n" +
        "    <MessageTemplate Context=\"msg\">\n" +
        "        <div class=\"flex flex-col items-center w-full gap-4 border-b border-surface-200 dark:border-surface-700\">\n" +
        "            <i class=\"@msg.Options.Icon !text-6xl text-primary-500\"></i>\n" +
        "            <p>@msg.Options.Message</p>\n" +
        "        </div>\n" +
        "    </MessageTemplate>\n" +
        "</OpConfirmDialog>\n" +
        "<OpButton Label=\"Confirmar\" @onclick=\"ConfirmTemplate\" />\n\n" +
        "@code {\n" +
        "    // o template do componente é usado; Key/Header/Message vêm das opções\n" +
        "    void ConfirmTemplate() => ConfirmationService.Confirm(new ConfirmOptions\n" +
        "    {\n" +
        "        Key = \"template\", Header = \"Com Template\",\n" +
        "        Message = \"Mensagem renderizada pelo MessageTemplate.\", Icon = \"pi pi-check-circle\"\n" +
        "    });\n" +
        "}";

    public static readonly string DeclarativeCode = 
        "<OpConfirmDialog @ref=\"_confirmDialog\" Key=\"declarative\"\n" +
        "                 Header=\"Confirmação\" Message=\"Tem certeza?\" Icon=\"pi pi-exclamation-triangle\"\n" +
        "                 AcceptLabel=\"Sim\" RejectLabel=\"Não\"\n" +
        "                 AcceptButtonProps=\"@_acceptProps\" RejectButtonProps=\"@_rejectProps\" />\n" +
        "<OpButton Label=\"Ação\" @onclick=\"ConfirmDeclarative\" />\n\n" +
        "@code {\n" +
        "    private OpConfirmDialog? _confirmDialog;\n" +
        "    private readonly Dictionary<string, object> _acceptProps = new() { [\"severity\"] = \"success\" };\n" +
        "    private readonly Dictionary<string, object> _rejectProps = new() { [\"severity\"] = \"secondary\", [\"outlined\"] = true };\n\n" +
        "    // ShowDeclarative monta as opções a partir dos parâmetros do próprio componente\n" +
        "    void ConfirmDeclarative() => _confirmDialog?.ShowDeclarative();\n" +
        "}";

    public static readonly string BadgeCode = 
        "<OpConfirmDialog Key=\"badge\" />\n" +
        "<OpButton Label=\"Mostrar Badge\" @onclick=\"ConfirmBadge\" />\n\n" +
        "@code {\n" +
        "    // Appearance=\"Badge\" é extensão do OpBlazorUI: badge circular no topo,\n" +
        "    // título e botões centralizados, largura de 410px e cores via tokens do tema\n" +
        "    void ConfirmBadge() => ConfirmationService.Confirm(new ConfirmOptions\n" +
        "    {\n" +
        "        Key = \"badge\", Appearance = \"Badge\",\n" +
        "        Header = \"Tem certeza?\", Message = \"Confirme para prosseguir.\",\n" +
        "        Icon = \"pi pi-question\",\n" +
        "        AcceptLabel = \"Salvar\", RejectLabel = \"Cancelar\"\n" +
        "    });\n" +
        "}";
}