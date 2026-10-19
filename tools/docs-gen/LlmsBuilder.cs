using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Gera os artefatos de descoberta para agentes: llms.txt, um Markdown por entrada,
/// llms-full.txt e os recursos auxiliares (ícones, serviços e enums).
/// </summary>
internal static class LlmsBuilder
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

    public static void Write(Manifest manifest, string outDir, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl, manifest);
        var llmsDir = Path.Combine(outDir, "llms");
        Directory.CreateDirectory(llmsDir);

        var entries = manifest.Components
            .OrderBy(c => KindOrder(c.Kind))
            .ThenBy(c => GuideOrder(c))
            .ThenBy(c => c.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duplicate = entries
            .GroupBy(e => e.Id, StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Id duplicado no catálogo: '{duplicate.Key}'.");
        }

        foreach (var entry in entries)
        {
            if (!Regex.IsMatch(entry.Id, "^[a-z0-9-]+$", RegexOptions.CultureInvariant))
            {
                throw new InvalidOperationException($"Id inválido para arquivo Markdown: '{entry.Id}'.");
            }

            WriteFile(Path.Combine(llmsDir, entry.Id + ".md"), RenderEntry(manifest, entry, baseUrl));
        }

        WriteFile(Path.Combine(llmsDir, "icons.md"), RenderIcons(manifest));
        WriteFile(Path.Combine(llmsDir, "services.md"), RenderServices(manifest, baseUrl));
        WriteFile(Path.Combine(llmsDir, "enums.md"), RenderEnums(manifest));
        WriteFile(Path.Combine(outDir, "llms.txt"), RenderIndex(manifest, baseUrl, entries));
        WriteFile(Path.Combine(outDir, "llms-full.txt"), RenderFull(manifest, baseUrl, entries));
    }

    /// <summary>Confere se os artefatos estão completos e se os links locais do índice existem.</summary>
    public static IReadOnlyList<string> Verify(string outDir)
    {
        var errors = new List<string>();
        var manifestPath = Path.Combine(outDir, "manifest.json");
        var indexPath = Path.Combine(outDir, "llms.txt");
        var fullPath = Path.Combine(outDir, "llms-full.txt");

        if (!File.Exists(manifestPath))
        {
            return [$"manifesto ausente: {manifestPath}"];
        }

        if (!File.Exists(indexPath))
        {
            errors.Add($"índice ausente: {indexPath}");
        }

        if (!File.Exists(fullPath))
        {
            errors.Add($"arquivo completo ausente: {fullPath}");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        }
        catch (JsonException exception)
        {
            return [$"manifesto JSON inválido: {exception.Message}"];
        }

        using (document)
        {
            var root = document.RootElement;
            var baseUrl = root.GetProperty("library").GetProperty("docsBaseUrl").GetString()?.TrimEnd('/') ?? "";
            var components = root.GetProperty("components").EnumerateArray()
                .Select(c => (
                    Id: c.GetProperty("id").GetString() ?? "",
                    Name: c.GetProperty("name").GetString() ?? ""))
                .Where(c => c.Id.Length > 0 && c.Name.Length > 0)
                .ToList();

            if (components.Count == 0)
            {
                errors.Add("manifesto sem componentes.");
            }

            var index = File.Exists(indexPath) ? File.ReadAllText(indexPath) : "";
            var full = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "";

            foreach (var (id, name) in components)
            {
                var file = Path.Combine(outDir, "llms", id + ".md");
                if (!File.Exists(file))
                {
                    errors.Add($"Markdown ausente: llms/{id}.md");
                }
                else
                {
                    var firstLine = File.ReadLines(file).FirstOrDefault() ?? "";
                    if (!string.Equals(firstLine, $"# {name}", StringComparison.Ordinal))
                    {
                        errors.Add($"título inesperado em llms/{id}.md: '{firstLine}' (esperado '# {name}')");
                    }
                }

                if (!index.Contains($"llms/{id}.md", StringComparison.Ordinal))
                {
                    errors.Add($"llms.txt não referencia llms/{id}.md");
                }

                if (!full.Contains($"# {name}", StringComparison.Ordinal))
                {
                    errors.Add($"llms-full.txt não contém a entrada '{name}'");
                }
            }

            foreach (var resource in new[] { "icons.md", "services.md", "enums.md" })
            {
                if (!File.Exists(Path.Combine(outDir, "llms", resource)))
                {
                    errors.Add($"recurso ausente: llms/{resource}");
                }
            }

            if (baseUrl.Length > 0 && index.Length > 0)
            {
                foreach (Match match in Regex.Matches(index, @"\]\((?<url>https?://[^)\s]+)\)"))
                {
                    var url = match.Groups["url"].Value;
                    if (!url.StartsWith(baseUrl + "/", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var relative = url[(baseUrl.Length + 1)..];
                    var suffix = relative.IndexOfAny(['#', '?']);
                    if (suffix >= 0)
                    {
                        relative = relative[..suffix];
                    }

                    var local = relative == "llms/manifest.json" ? "manifest.json" : relative;
                    if (!File.Exists(Path.Combine(outDir, local)))
                    {
                        errors.Add($"llms.txt aponta para arquivo inexistente: {url}");
                    }
                }
            }
        }

        return errors;
    }

    private static string RenderIndex(
        Manifest manifest,
        string baseUrl,
        IReadOnlyList<ComponentManifest> entries)
    {
        var guides = entries.Where(e => e.Kind == "guide").ToList();
        var services = entries.Where(e => e.Kind == "service").ToList();
        var components = entries.Where(e => e.Kind is not ("guide" or "service")).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# OpBlazorUI");
        sb.AppendLine();
        sb.AppendLine(
            $"> Port para Blazor do Optimus UI (fork MIT do PrimeNG v21): " +
            $"{components.Count} componentes, {guides.Count} guias e {services.Count} serviços, " +
            $"com temas {JoinThemes(manifest.Themes)}.");
        sb.AppendLine();
        sb.AppendLine($"- Versão: `{manifest.Library.Version}`" +
                      (string.IsNullOrWhiteSpace(manifest.Library.Commit) ? "" : $" (commit `{manifest.Library.Commit}`)"));
        sb.AppendLine($"- Target: `{manifest.Library.TargetFramework}`");
        sb.AppendLine($"- Documentação: <{baseUrl}>");
        sb.AppendLine($"- Pacote: `{manifest.GettingStarted.Package.Id}` no GitHub Packages" +
                      (manifest.GettingStarted.Package.RequiresPat ? " (o restore exige PAT com `read:packages`)" : ""));
        sb.AppendLine();
        sb.AppendLine("As páginas HTML do Showcase são renderizadas no cliente (Blazor WebAssembly). " +
                      "Os arquivos Markdown abaixo são a fonte estática para agentes e crawlers.");
        sb.AppendLine();
        sb.AppendLine("## Comece por aqui");
        sb.AppendLine();
        sb.AppendLine($"- [Installation]({baseUrl}/llms/installation.md) — instalação, autenticação no GitHub Packages e primeiro componente.");
        sb.AppendLine($"- [Configuration]({baseUrl}/llms/configuration.md) — temas, modo escuro, RTL e render modes.");
        sb.AppendLine($"- [Manifesto da API]({baseUrl}/llms/manifest.json) — nomes, tipos, valores padrão e descrições em JSON.");
        sb.AppendLine($"- [Documentação completa]({baseUrl}/llms-full.txt) — todos os arquivos em um único texto.");
        sb.AppendLine();

        if (guides.Count > 0)
        {
            sb.AppendLine("## Guias");
            sb.AppendLine();
            foreach (var guide in guides)
            {
                sb.AppendLine(CatalogLine(guide, baseUrl));
            }

            sb.AppendLine();
        }

        if (components.Count > 0)
        {
            sb.AppendLine("## Componentes");
            sb.AppendLine();

            foreach (var group in components.GroupBy(c => c.Category))
            {
                sb.AppendLine($"### {group.Key}");
                sb.AppendLine();
                foreach (var component in group)
                {
                    sb.AppendLine(CatalogLine(component, baseUrl));
                }

                sb.AppendLine();
            }
        }

        if (services.Count > 0)
        {
            sb.AppendLine("## Serviços");
            sb.AppendLine();
            foreach (var service in services)
            {
                sb.AppendLine(CatalogLine(service, baseUrl));
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Recursos");
        sb.AppendLine();
        sb.AppendLine($"- [manifest.json]({baseUrl}/llms/manifest.json) — manifesto da API (componentes, tipos, parâmetros, eventos, templates, serviços, enums e ícones).");
        sb.AppendLine($"- [llms-full.txt]({baseUrl}/llms-full.txt) — documentação completa em um único arquivo.");
        sb.AppendLine($"- [Ícones]({baseUrl}/llms/icons.md) — glyphs disponíveis na fonte nativa (`pi pi-*`).");
        sb.AppendLine($"- [Serviços]({baseUrl}/llms/services.md) — assinaturas de todos os serviços registrados.");
        sb.AppendLine($"- [Enums]({baseUrl}/llms/enums.md) — enums públicos da biblioteca.");
        sb.AppendLine("- [Repositório](https://github.com/victorAFRodrigues/OpBlazorUI) · [Releases](https://github.com/victorAFRodrigues/OpBlazorUI/releases)");
        sb.AppendLine();
        sb.AppendLine("## Notas para agentes");
        sb.AppendLine();
        sb.AppendLine("- Use `manifest.json` como fonte de verdade para nomes, tipos e valores padrão; não invente parâmetros.");
        sb.AppendLine("- Os textos estão em pt-BR, mas os nomes de tipos, propriedades, classes CSS e aliases estão em inglês.");
        sb.AppendLine("- As classes CSS dos componentes são `p-*` (PrimeNG/Optimus UI). Utilitários de layout são da aplicação.");
        sb.AppendLine("- Ícones usam a fonte nativa: `<i class=\"pi pi-*\"></i>`; não crie SVGs.");
        sb.AppendLine("- O pacote é publicado no GitHub Packages e o restore exige um PAT com `read:packages`.");
        sb.AppendLine("- Para uma versão específica, use o `llms-full.txt` e o `manifest.json` anexados à release correspondente.");

        return sb.ToString();
    }

    private static string RenderEntry(
        Manifest manifest,
        ComponentManifest entry,
        string baseUrl,
        bool includeInstall = true)
    {
        var docsUrl = DocsUrl(entry, baseUrl);
        var sb = new StringBuilder();

        sb.AppendLine($"# {entry.Name}");
        sb.AppendLine();
        sb.AppendLine($"> {EscapeInline(SummaryFor(entry))}");
        sb.AppendLine();
        sb.AppendLine($"- **Tipo:** {KindLabel(entry.Kind)}");
        if (entry.Types.Count > 0)
        {
            var label = entry.Types.Count == 1 ? "Tipo principal" : "Tipos públicos";
            sb.AppendLine($"- **{label}:** {string.Join(", ", entry.Types.Select(t => $"`{t.Name}`"))}");
        }

        sb.AppendLine($"- **Categoria:** {entry.Category}");
        sb.AppendLine($"- **Status:** {StatusLabel(entry.Status)}");
        if (docsUrl is not null)
        {
            sb.AppendLine($"- **Documentação:** [{entry.Name}]({docsUrl})");
        }

        sb.AppendLine($"- **Pacote:** `{manifest.GettingStarted.Package.Id}`");
        if (entry.Aliases.Count > 0)
        {
            sb.AppendLine($"- **Aliases:** {string.Join(", ", entry.Aliases)}");
        }

        sb.AppendLine();

        if (includeInstall)
        {
            if (entry.Id == "installation")
            {
                AppendGettingStarted(sb, manifest);
            }
            else if (entry.Id == "configuration")
            {
                AppendConfiguration(sb, manifest);
            }
            else
            {
                sb.AppendLine("## Instalação");
                sb.AppendLine();
                sb.AppendLine(
                    $"O pacote `{manifest.GettingStarted.Package.Id}` é publicado no GitHub Packages e exige um PAT com " +
                    $"`read:packages` no restore. Veja [Installation]({baseUrl}/llms/installation.md) para o `nuget.config`, " +
                    "o registro dos serviços e o setup do tema.");
                sb.AppendLine();
                sb.AppendLine($"Para temas, modo escuro, RTL e render modes, veja [Configuration]({baseUrl}/llms/configuration.md).");
                sb.AppendLine();
            }
        }

        if (entry.Types.Count > 0)
        {
            AppendApi(sb, entry);
        }

        if (entry.Examples.Count > 0)
        {
            sb.AppendLine("## Exemplos");
            sb.AppendLine();
            foreach (var example in entry.Examples)
            {
                if (string.IsNullOrWhiteSpace(example.Code))
                {
                    continue;
                }

                var label = OneLine(example.SectionLabel);
                sb.AppendLine($"### {EscapeInline(label.Length > 0 ? label : "Exemplo")}");
                sb.AppendLine();
                AppendCode(sb, LanguageFor(example), example.Code);
            }
        }

        if (entry.CssClasses.Count > 0)
        {
            sb.AppendLine("## Classes CSS");
            sb.AppendLine();
            AppendTable(
                sb,
                ["Classe", "Descrição"],
                entry.CssClasses
                    .Select(c => new[] { Code(c.Class), Cell(c.Description) })
                    .ToList());
        }

        if (entry.Tokens.Count > 0)
        {
            sb.AppendLine("## Tokens");
            sb.AppendLine();
            AppendTable(
                sb,
                ["Variável", "Valor"],
                entry.Tokens
                    .Select(t => new[] { Code(t.Var), Code(t.Value) })
                    .ToList());
        }

        if (entry.Sections.Count > 0)
        {
            sb.AppendLine("## Seções da documentação");
            sb.AppendLine();
            foreach (var section in entry.Sections)
            {
                var label = OneLine(section.Label);
                if (label.Length == 0)
                {
                    continue;
                }

                if (docsUrl is not null && !string.IsNullOrWhiteSpace(section.Id))
                {
                    sb.AppendLine($"- [{EscapeInline(label)}]({docsUrl}#{section.Id})");
                }
                else
                {
                    sb.AppendLine($"- {EscapeInline(label)}");
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Links");
        sb.AppendLine();
        if (docsUrl is not null)
        {
            sb.AppendLine($"- Documentação: {docsUrl}");
        }

        sb.AppendLine($"- Índice para agentes: {baseUrl}/llms.txt");
        sb.AppendLine($"- Manifesto da API: {baseUrl}/llms/manifest.json");
        sb.AppendLine($"- Versão completa: {baseUrl}/llms-full.txt");

        return sb.ToString();
    }

    private static void AppendApi(StringBuilder sb, ComponentManifest entry)
    {
        sb.AppendLine("## API");
        sb.AppendLine();

        foreach (var type in entry.Types)
        {
            sb.AppendLine($"### `{type.Name}`");
            sb.AppendLine();

            var typeSummary = OneLine(type.Summary);
            if (typeSummary.Length > 0)
            {
                sb.AppendLine(EscapeInline(typeSummary));
                sb.AppendLine();
            }

            var wroteSection = false;

            if (type.Parameters.Count > 0)
            {
                AppendParameterTable(sb, "Parâmetros", type.Parameters);
                wroteSection = true;
            }

            if (type.Events.Count > 0)
            {
                AppendParameterTable(sb, "Eventos", type.Events);
                wroteSection = true;
            }

            if (type.Templates.Count > 0)
            {
                AppendParameterTable(sb, "Templates", type.Templates);
                wroteSection = true;
            }

            if (type.Properties.Count > 0)
            {
                sb.AppendLine("#### Propriedades");
                sb.AppendLine();
                AppendTable(
                    sb,
                    ["Nome", "Tipo", "Descrição"],
                    type.Properties
                        .Select(p => new[]
                        {
                            p.Inherited ? $"{p.Name} (herdado)" : p.Name,
                            Code(p.Type),
                            Cell(p.Description)
                        })
                        .ToList());
                wroteSection = true;
            }

            if (!wroteSection)
            {
                sb.AppendLine("_Sem membros públicos refletidos._");
                sb.AppendLine();
            }

            if (type.Parameters.Any(p => p.Required))
            {
                sb.AppendLine("_`*` indica parâmetro obrigatório (`[EditorRequired]`)._");
                sb.AppendLine();
            }
        }
    }

    private static void AppendParameterTable(
        StringBuilder sb,
        string title,
        IReadOnlyList<ParameterManifest> parameters)
    {
        sb.AppendLine($"#### {title}");
        sb.AppendLine();
        AppendTable(
            sb,
            ["Nome", "Tipo", "Padrão", "Descrição"],
            parameters
                .Select(p => new[]
                {
                    ParameterName(p),
                    Code(p.Type),
                    Code(p.DefaultValue),
                    Cell(p.Description)
                })
                .ToList());
    }

    private static void AppendGettingStarted(StringBuilder sb, Manifest manifest)
    {
        sb.AppendLine("## Instalação");
        sb.AppendLine();
        AppendGettingStartedBody(sb, manifest);
    }

    private static void AppendConfiguration(StringBuilder sb, Manifest manifest)
    {
        var configure = manifest.GettingStarted.Configure;

        sb.AppendLine("## Configuração");
        sb.AppendLine();
        sb.AppendLine("### Registrar os serviços");
        sb.AppendLine();
        AppendCode(sb, "csharp", configure.Program);
        sb.AppendLine("### `_Imports.razor`");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Imports);
        sb.AppendLine("### Tema e render mode");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Head);
        sb.AppendLine("### Setup do layout");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Layout);

        if (manifest.GettingStarted.Notes.Count > 0)
        {
            AppendNotes(sb, manifest);
        }
    }

    private static void AppendGettingStartedBody(StringBuilder sb, Manifest manifest)
    {
        var package = manifest.GettingStarted.Package;
        var configure = manifest.GettingStarted.Configure;

        sb.AppendLine("### 1. Autenticação no GitHub Packages");
        sb.AppendLine();
        sb.AppendLine(
            $"O pacote é publicado em <{package.Feed}> e o restore exige um PAT com escopo `read:packages`, " +
            "mesmo para pacotes públicos. Crie ou atualize o `nuget.config` na raiz da solução:");
        sb.AppendLine();
        AppendCode(sb, "xml", package.NugetConfig);
        sb.AppendLine("### 2. Adicionar o pacote");
        sb.AppendLine();
        AppendCode(sb, "bash", package.AddPackage);
        if (!string.IsNullOrWhiteSpace(package.ChartsId))
        {
            sb.AppendLine($"Para gráficos, adicione também `dotnet add package {package.ChartsId}`.");
            sb.AppendLine();
        }

        sb.AppendLine("### 3. Registrar os serviços");
        sb.AppendLine();
        AppendCode(sb, "csharp", configure.Program);
        sb.AppendLine("### 4. `_Imports.razor`");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Imports);
        sb.AppendLine("### 5. Tema e render mode");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Head);
        sb.AppendLine("### 6. Setup do layout");
        sb.AppendLine();
        AppendCode(sb, "razor", configure.Layout);
        AppendNotes(sb, manifest);
    }

    private static void AppendNotes(StringBuilder sb, Manifest manifest)
    {
        if (manifest.GettingStarted.Notes.Count == 0)
        {
            return;
        }

        sb.AppendLine("### Notas");
        sb.AppendLine();
        foreach (var note in manifest.GettingStarted.Notes)
        {
            sb.AppendLine($"- {EscapeInline(OneLine(note))}");
        }

        sb.AppendLine();
    }

    private static string RenderFull(
        Manifest manifest,
        string baseUrl,
        IReadOnlyList<ComponentManifest> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# OpBlazorUI — documentação completa para agentes");
        sb.AppendLine();
        sb.AppendLine(
            $"> Port para Blazor do Optimus UI (fork MIT do PrimeNG v21). " +
            $"Versão `{manifest.Library.Version}`, target `{manifest.Library.TargetFramework}`, " +
            $"{entries.Count} entradas de documentação.");
        sb.AppendLine();
        sb.AppendLine($"- Documentação: <{baseUrl}>");
        sb.AppendLine($"- Manifesto da API: {baseUrl}/llms/manifest.json");
        sb.AppendLine($"- Índice resumido: {baseUrl}/llms.txt");
        sb.AppendLine();
        sb.AppendLine("## Instalação e configuração");
        sb.AppendLine();
        AppendGettingStartedBody(sb, manifest);

        foreach (var entry in entries)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.Append(RenderEntry(manifest, entry, baseUrl, includeInstall: false));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string RenderIcons(Manifest manifest)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Ícones");
        sb.AppendLine();
        sb.AppendLine(
            "A biblioteca usa a fonte nativa de ícones. Não crie SVGs à mão: use uma classe " +
            "`pi pi-*` dentro de `<i>`.");
        sb.AppendLine();
        sb.AppendLine("```razor");
        sb.AppendLine("<i class=\"pi pi-search\"></i>");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine($"Total: {manifest.Icons.Count} glyphs.");
        sb.AppendLine();

        foreach (var icon in manifest.Icons.OrderBy(i => i, StringComparer.OrdinalIgnoreCase))
        {
            var name = icon.StartsWith("pi-", StringComparison.Ordinal) ? "pi " + icon : icon;
            sb.AppendLine($"- `{name}`");
        }

        return sb.ToString();
    }

    private static string RenderServices(Manifest manifest, string baseUrl)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Serviços");
        sb.AppendLine();
        sb.AppendLine("Serviços públicos registrados por `AddOpBlazorUI()`. As assinaturas vêm da reflexão sobre os assemblies.");
        sb.AppendLine();

        foreach (var service in manifest.Services)
        {
            sb.AppendLine($"## {service.Name}");
            sb.AppendLine();
            sb.AppendLine($"`{service.FullName}`");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(service.DocsUrl))
            {
                sb.AppendLine($"Documentação: {service.DocsUrl}");
                sb.AppendLine();
            }

            if (service.Methods.Count == 0)
            {
                sb.AppendLine("_Sem métodos públicos declarados._");
                sb.AppendLine();
                continue;
            }

            AppendTable(
                sb,
                ["Método", "Assinatura", "Resumo"],
                service.Methods
                    .Select(m => new[] { Code(m.Name), Code(m.Signature), Cell(m.Summary) })
                    .ToList());
        }

        sb.AppendLine("## Links");
        sb.AppendLine();
        sb.AppendLine($"- Índice para agentes: {baseUrl}/llms.txt");
        sb.AppendLine($"- Manifesto da API: {baseUrl}/llms/manifest.json");

        return sb.ToString();
    }

    private static string RenderEnums(Manifest manifest)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Enums");
        sb.AppendLine();
        sb.AppendLine("Enums públicos usados por parâmetros e eventos da biblioteca.");
        sb.AppendLine();

        foreach (var value in manifest.Enums)
        {
            sb.AppendLine($"## {value.Name}");
            sb.AppendLine();
            sb.AppendLine($"`{value.FullName}`");
            sb.AppendLine();
            foreach (var item in value.Values)
            {
                sb.AppendLine($"- `{item}`");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void AppendTable(
        StringBuilder sb,
        IReadOnlyList<string> headers,
        IReadOnlyList<string[]> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        sb.AppendLine("| " + string.Join(" | ", headers) + " |");
        sb.AppendLine("| " + string.Join(" | ", headers.Select(_ => "---")) + " |");
        foreach (var row in rows)
        {
            sb.AppendLine("| " + string.Join(" | ", row) + " |");
        }

        sb.AppendLine();
    }

    private static void AppendCode(StringBuilder sb, string language, string code)
    {
        var normalized = code.Replace("\r\n", "\n").Trim();
        if (normalized.Length == 0)
        {
            return;
        }

        var fence = normalized.Contains("```", StringComparison.Ordinal) ? "````" : "```";
        sb.AppendLine($"{fence}{language}");
        sb.AppendLine(normalized);
        sb.AppendLine(fence);
        sb.AppendLine();
    }

    private static string CatalogLine(ComponentManifest entry, string baseUrl)
    {
        var sb = new StringBuilder();
        sb.Append($"- [{entry.Name}]({baseUrl}/llms/{entry.Id}.md) — {WithPeriod(EscapeInline(ShortSummary(entry)))}");

        if (entry.Types.Count > 0)
        {
            sb.Append(" Tipo: ");
            sb.Append(string.Join(", ", entry.Types.Select(t => $"`{t.Name}`")));
            sb.Append('.');
        }

        if (entry.Status != "ported")
        {
            sb.Append($" Status: {StatusLabel(entry.Status)}.");
        }

        if (entry.Aliases.Count > 0)
        {
            sb.Append($" Aliases: {string.Join(", ", entry.Aliases)}.");
        }

        return sb.ToString();
    }

    private static string ParameterName(ParameterManifest parameter)
    {
        var name = parameter.Name;
        if (parameter.Required)
        {
            name += " *";
        }

        if (parameter.Inherited)
        {
            name += " (herdado)";
        }

        if (parameter.Cascading)
        {
            name += " (cascading)";
        }

        if (parameter.CaptureUnmatchedValues)
        {
            name += " (atributos livres)";
        }

        return Cell(name);
    }

    private static string Code(string? value)
    {
        var text = OneLine(value);
        if (text.Length == 0)
        {
            return "—";
        }

        return "`" + text.Replace("|", "\\|").Replace("`", "\\`") + "`";
    }

    private static string Cell(string? value)
    {
        var text = OneLine(value);
        return text.Length == 0 ? "—" : EscapeInline(text).Replace("|", "\\|");
    }

    private static string OneLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static string EscapeInline(string value) =>
        value.Replace("<", "&lt;").Replace(">", "&gt;");

    private static string SummaryFor(ComponentManifest entry)
    {
        var summary = OneLine(entry.Summary);
        if (summary.Length == 0)
        {
            summary = OneLine(entry.Types.FirstOrDefault()?.Summary);
        }

        if (summary.Length == 0)
        {
            summary = entry.Id switch
            {
                "installation" => "Instalação do pacote, autenticação no GitHub Packages e primeiro componente.",
                "configuration" => "Temas, modo escuro, RTL e configuração dos serviços.",
                "llms-txt" => "Arquivos estáticos (llms.txt, manifest.json e Markdown) para agentes de IA consumirem a documentação.",
                "fluid" => "Ainda não portado para Blazor.",
                _ => $"Documentação de {entry.Name}."
            };
        }

        return summary;
    }

    private static string ShortSummary(ComponentManifest entry)
    {
        var summary = SummaryFor(entry);
        return summary.Length <= 200 ? summary : summary[..197] + "...";
    }

    private static string WithPeriod(string text) =>
        text.EndsWith('.') || text.EndsWith('!') || text.EndsWith('?') ? text : text + ".";

    private static string StatusLabel(string status) => status switch
    {
        "ported" => "Portado",
        "placeholder" => "Placeholder (ainda não portado)",
        _ => status
    };

    private static string KindLabel(string kind) => kind switch
    {
        "guide" => "Guia",
        "service" => "Serviço",
        _ => "Componente"
    };

    private static string? DocsUrl(ComponentManifest entry, string baseUrl)
    {
        if (!string.IsNullOrWhiteSpace(entry.DocsUrl))
        {
            return entry.DocsUrl;
        }

        return string.IsNullOrWhiteSpace(entry.DocsRoute) ? null : baseUrl + entry.DocsRoute;
    }

    private static string LanguageFor(ExampleManifest example)
    {
        var code = example.Code;
        var trimmed = code.TrimStart();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
        {
            return "json";
        }

        if (trimmed.StartsWith("<?xml", StringComparison.Ordinal) ||
            trimmed.StartsWith("<configuration", StringComparison.Ordinal))
        {
            return "xml";
        }

        if (code.Contains('<') || code.Contains('@'))
        {
            return "razor";
        }

        return trimmed.Contains(".txt", StringComparison.Ordinal) ||
               trimmed.Contains(".md", StringComparison.Ordinal)
            ? "text"
            : "csharp";
    }

    private static int KindOrder(string kind) => kind switch
    {
        "guide" => 0,
        "service" => 1,
        _ => 2
    };

    private static int GuideOrder(ComponentManifest entry) => entry.Id switch
    {
        "installation" => 0,
        "configuration" => 1,
        _ => 2
    };

    private static string JoinThemes(IReadOnlyList<string> themes) =>
        themes.Count == 0
            ? "padrão"
            : string.Join(", ", themes.Select(t => char.ToUpperInvariant(t[0]) + t[1..]));

    private static string NormalizeBaseUrl(string? baseUrl, Manifest manifest) =>
        (string.IsNullOrWhiteSpace(baseUrl) ? manifest.Library.DocsBaseUrl : baseUrl).TrimEnd('/');

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!content.EndsWith('\n'))
        {
            content += "\n";
        }

        File.WriteAllText(path, content, Utf8);
    }
}
