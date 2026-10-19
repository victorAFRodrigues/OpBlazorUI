using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Lê o XML de documentação gerado junto das DLLs (GenerateDocumentationFile) e
/// expõe resumos de tipos, propriedades e métodos.
/// </summary>
internal sealed class XmlDocs
{
    private readonly Dictionary<string, string> _members = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _methods = new(StringComparer.Ordinal);
    private readonly List<string> _loadedFrom = [];

    public IReadOnlyList<string> LoadedFrom => _loadedFrom;

    public static XmlDocs Load(RepoContext repo, params Assembly[] assemblies)
    {
        var docs = new XmlDocs();

        foreach (var assembly in assemblies)
        {
            var path = Locate(assembly, repo);
            if (path is null)
            {
                Console.Error.WriteLine($"[docs-gen] aviso: XML de documentação não encontrado para {assembly.GetName().Name}.");
                continue;
            }

            docs._loadedFrom.Add(path);
            var document = XDocument.Load(path);
            foreach (var member in document.Descendants("member"))
            {
                var name = member.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var summary = member.Element("summary");
                if (summary is null)
                {
                    continue;
                }

                var text = Normalize(summary.Value);
                if (text.Length == 0)
                {
                    continue;
                }

                docs._members[name] = text;

                if (name.StartsWith("M:", StringComparison.Ordinal))
                {
                    var key = name[2..];
                    var paren = key.IndexOf('(');
                    docs._methods[paren > 0 ? key[..paren] : key] = text;
                }
            }
        }

        return docs;
    }

    public string? ForType(Type type) => Get("T:" + MemberTypeName(type));

    public string? ForProperty(PropertyInfo property)
    {
        var declaring = property.DeclaringType is null ? "" : MemberTypeName(property.DeclaringType) + ".";
        return Get("P:" + declaring + property.Name);
    }

    public string? ForMethod(MethodInfo method)
    {
        var declaring = method.DeclaringType is null ? "" : MemberTypeName(method.DeclaringType) + ".";
        return _methods.GetValueOrDefault(declaring + method.Name);
    }

    private string? Get(string key) => _members.TryGetValue(key, out var value) ? value : null;

    private static string MemberTypeName(Type type) =>
        (type.FullName ?? type.Namespace + "." + type.Name).Replace('+', '.');

    private static string Normalize(string value) =>
        Regex.Replace(value, "\\s+", " ").Trim();

    private static string? Locate(Assembly assembly, RepoContext repo)
    {
        var nextToAssembly = Path.ChangeExtension(assembly.Location, ".xml");
        if (File.Exists(nextToAssembly))
        {
            return nextToAssembly;
        }

        var name = assembly.GetName().Name;
        if (name is null)
        {
            return null;
        }

        var binDir = Path.Combine(repo.Root, "src", "Components", name, "bin");
        if (!Directory.Exists(binDir))
        {
            return null;
        }

        return Directory.EnumerateFiles(binDir, name + ".xml", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
