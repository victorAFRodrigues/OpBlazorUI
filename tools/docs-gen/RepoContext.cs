using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

internal sealed class RepoContext
{
    public required string Root { get; init; }

    public string ShowcaseDir => Path.Combine(Root, "src", "OpBlazorUI.Showcase");
    public string PagesDir => Path.Combine(ShowcaseDir, "Components", "Pages");
    public string AppMenuPath => Path.Combine(ShowcaseDir, "Components", "Layout", "AppMenu.cs");
    public string DocThemingPath => Path.Combine(ShowcaseDir, "Components", "Doc", "DocTheming.cs");
    public string IconsCssPath => Path.Combine(Root, "src", "Components", "OpBlazorUI", "wwwroot", "openng-icons.css");
    public string OverridesPath => Path.Combine(Root, "tools", "docs-gen", "overrides.json");

    public static RepoContext Locate(string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
        {
            return Create(explicitRoot);
        }

        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "OpBlazorUI.slnx")))
                {
                    return Create(dir.FullName);
                }

                dir = dir.Parent;
            }
        }

        throw new InvalidOperationException(
            "Não encontrei a raiz do repositório (OpBlazorUI.slnx). Use --repo <caminho>.");
    }

    private static RepoContext Create(string root)
    {
        var full = Path.GetFullPath(root);
        var ctx = new RepoContext { Root = full };

        foreach (var path in new[] { ctx.AppMenuPath, ctx.DocThemingPath, ctx.IconsCssPath, ctx.PagesDir })
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new InvalidOperationException($"Arquivo/diretório esperado não existe: {path}");
            }
        }

        return ctx;
    }

    /// <summary>Slug estável para ids, nomes de arquivo e chaves de theming.</summary>
    public static string Slug(string value)
    {
        var chars = value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
        return chars.Length > 0 ? new string(chars) : "component";
    }

    /// <summary>Remove tags HTML e normaliza espaços; usado em descrições vindas das páginas.</summary>
    public static string StripHtml(string value)
    {
        // Tags primeiro, entidades depois: se decodificar antes, "EventCallback&lt;T&gt;"
        // vira "EventCallback<T>" e o regex de tags come "<T>" como se fosse HTML.
        // Tags de bloco viram espaço; tags inline (code/strong) somem sem separar palavras.
        var text = Regex.Replace(value, @"<br\s*/?>|</p>|</li>|</div>", " ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<[^>]+>", "");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = text.Replace("@@", "@");
        text = Regex.Replace(text, "\\s+", " ");
        text = Regex.Replace(text, @"\s+([,.;:!?])", "$1");
        return text.Trim();
    }

    /// <summary>Nome curto de um tipo, sem namespace e sem aridade genérica.</summary>
    public static string ShortName(Type type)
    {
        var name = type.Name;
        var tick = name.IndexOf('`');
        return tick >= 0 ? name[..tick] : name;
    }

    private static readonly Dictionary<Type, string> TypeAliases = new()
    {
        [typeof(string)] = "string",
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(decimal)] = "decimal",
        [typeof(char)] = "char",
        [typeof(object)] = "object",
        [typeof(void)] = "void"
    };

    public static string FriendlyType(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable is not null)
        {
            return FriendlyType(nullable) + "?";
        }

        if (TypeAliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsArray)
        {
            return FriendlyType(type.GetElementType()!) + "[]";
        }

        var name = type.Name;
        var tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name[..tick];
        }

        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments().Select(FriendlyType);
            name += "<" + string.Join(", ", args) + ">";
        }

        return name;
    }

    public static string DeclarationName(Type type)
    {
        var name = ShortName(type);
        if (type.IsGenericTypeDefinition || (type.IsGenericType && type.GetGenericArguments().All(a => a.IsGenericParameter)))
        {
            var args = type.GetGenericArguments().Select(a => a.Name);
            name += "<" + string.Join(", ", args) + ">";
        }

        return name;
    }

    public static string FullDeclarationName(Type type)
    {
        var ns = type.Namespace is null ? "" : type.Namespace + ".";
        return ns + DeclarationName(type);
    }

    public static bool IsEventCallback(Type type) =>
        type == typeof(Microsoft.AspNetCore.Components.EventCallback) ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.Components.EventCallback<>));

    public static bool IsRenderFragment(Type type) =>
        type == typeof(Microsoft.AspNetCore.Components.RenderFragment) ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.Components.RenderFragment<>));

    public static bool IsComponent(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current == typeof(Microsoft.AspNetCore.Components.ComponentBase) ||
                current.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
            {
                return true;
            }
        }

        return typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(type);
    }
}
