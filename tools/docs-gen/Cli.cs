using System.Reflection;

namespace OpBlazorUI.DocsGen;

internal static class Cli
{
    private const string DefaultBaseUrl = "https://victorafrodrigues.github.io/OpBlazorUI";

    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
        {
            PrintUsage();
            return args.Length == 0 ? 2 : 0;
        }

        var command = args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());
        var repo = RepoContext.Locate(options.GetValueOrDefault("repo"));
        var overrides = Overrides.Load(repo.OverridesPath);
        var catalog = CatalogBuilder.Build(repo, overrides);

        return command switch
        {
            "generate" => Generate(repo, catalog, options),
            "audit" => Audit(repo, catalog, overrides, options),
            _ => Unknown(command)
        };
    }

    private static int Generate(RepoContext repo, IReadOnlyList<CatalogEntry> catalog, Dictionary<string, string?> options)
    {
        var outDir = options.GetValueOrDefault("out") ?? Path.Combine(repo.Root, "artifacts", "docs");
        var manifestOptions = new ManifestOptions
        {
            Version = options.GetValueOrDefault("version") ?? "0.0.0-dev",
            Commit = options.GetValueOrDefault("commit") ?? Git.Head(repo.Root),
            BaseUrl = options.GetValueOrDefault("base-url") ?? DefaultBaseUrl
        };

        var xml = XmlDocs.Load(
            repo,
            Assembly.Load("OpBlazorUI"),
            Assembly.Load("OpBlazorUI.Charts"));

        var manifest = ManifestBuilder.Build(repo, catalog, xml, manifestOptions);
        var path = Path.Combine(outDir, "manifest.json");
        Json.Write(path, manifest);

        var types = manifest.Components.Sum(c => c.Types.Count);
        var parameters = manifest.Components.Sum(c => c.Types.Sum(t => t.Parameters.Count));
        var eventsCount = manifest.Components.Sum(c => c.Types.Sum(t => t.Events.Count));
        var templates = manifest.Components.Sum(c => c.Types.Sum(t => t.Templates.Count));

        Console.WriteLine($"[docs-gen] manifesto: {path}");
        Console.WriteLine($"  componentes: {manifest.Components.Count} " +
                          $"({manifest.Components.Count(c => c.Kind == "component")} componentes, " +
                          $"{manifest.Components.Count(c => c.Kind == "service")} serviços, " +
                          $"{manifest.Components.Count(c => c.Kind == "guide")} guias)");
        Console.WriteLine($"  tipos: {types} | parâmetros: {parameters} props, {eventsCount} eventos, {templates} templates");
        Console.WriteLine($"  serviços: {manifest.Services.Count} | enums: {manifest.Enums.Count} | ícones: {manifest.Icons.Count}");
        Console.WriteLine($"  versão: {manifest.Library.Version} | commit: {manifest.Library.Commit ?? "—"}");
        if (xml.LoadedFrom.Count > 0)
        {
            Console.WriteLine($"  XML docs: {string.Join(", ", xml.LoadedFrom)}");
        }

        return 0;
    }

    private static int Audit(
        RepoContext repo,
        IReadOnlyList<CatalogEntry> catalog,
        Overrides overrides,
        Dictionary<string, string?> options)
    {
        var mode = (options.GetValueOrDefault("mode") ?? "warn").ToLowerInvariant();
        if (mode is not ("warn" or "fail"))
        {
            throw new ArgumentException("--mode deve ser 'warn' ou 'fail'.");
        }

        var version = options.GetValueOrDefault("version") ?? "0.0.0-dev";
        var report = AuditRunner.Run(catalog, overrides, mode, version);
        var reportPath = options.GetValueOrDefault("report")
                         ?? Path.Combine(repo.Root, "artifacts", "docs", "docs-audit.json");

        Json.Write(reportPath, report);
        AuditRunner.Print(report);
        Console.WriteLine($"[docs-gen] relatório: {reportPath}");

        return mode == "fail" && report.Summary.Hard > 0 ? 1 : 0;
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"[docs-gen] comando desconhecido: {command}");
        PrintUsage();
        return 2;
    }

    private static Dictionary<string, string?> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Argumento inesperado: {arg}");
            }

            var key = arg[2..];
            string? value = null;

            var equals = key.IndexOf('=');
            if (equals >= 0)
            {
                value = key[(equals + 1)..];
                key = key[..equals];
            }
            else if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[++i];
            }
            else
            {
                value = "true";
            }

            if (key.Length == 0)
            {
                throw new ArgumentException($"Opção inválida: {arg}");
            }

            options[key] = value;
        }

        return options;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            """
            uso: docs-gen <comando> [opções]

            Comandos:
              generate   Gera o manifest.json a partir da reflexão + páginas do Showcase
              audit      Compara as tabelas de API das páginas com o código
              help       Mostra esta ajuda

            Opções de generate:
              --out <dir>        Diretório de saída (padrão: artifacts/docs)
              --version <semver> Versão da lib carimbada no manifesto
              --commit <sha>     Commit carimbado no manifesto (padrão: git rev-parse --short HEAD)
              --base-url <url>   Base das URLs de documentação

            Opções de audit:
              --mode <warn|fail> warn = só reporta (padrão); fail = sai com código 1 se houver hard drift
              --report <arquivo> Caminho do relatório JSON (padrão: artifacts/docs/docs-audit.json)
              --version <semver> Versão carimbada no relatório

            Opções globais:
              --repo <caminho>   Raiz do repositório (padrão: procura OpBlazorUI.slnx subindo do cwd)
            """);
    }
}
