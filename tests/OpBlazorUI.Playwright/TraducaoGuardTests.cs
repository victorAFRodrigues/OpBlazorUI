using System.Text.RegularExpressions;
using Xunit;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Guarda da globalização: falha se sobrar texto de UI literal nos componentes da biblioteca.
/// Todo rótulo visível/aria deve sair do <c>OpTranslation</c> (via <c>OpTranslationService</c>),
/// para que funcione igual em inglês e português.
/// </summary>
/// <remarks>
/// Só texto humano entra na checagem: valores de ARIA/HTML que são tokens (ex.: <c>aria-live="polite"</c>,
/// <c>aria-haspopup="listbox"</c>, <c>aria-modal="true"</c>) não são traduzíveis e ficam de fora.
/// </remarks>
public class TraducaoGuardTests
{
    private static readonly string[] TextAttributes = ["aria-label", "title", "placeholder"];

    [Fact]
    public void Componentes_nao_tem_aria_title_ou_placeholder_literais()
    {
        var componentsDir = Path.Combine(FindRepositoryRoot(), "src", "Components", "OpBlazorUI.Base", "Components");
        Assert.True(Directory.Exists(componentsDir), $"Pasta de componentes não encontrada: {componentsDir}");

        var offenders = new List<string>();
        var scanned = 0;
        foreach (var file in Directory.EnumerateFiles(componentsDir, "*.razor", SearchOption.AllDirectories))
        {
            scanned++;
            var relative = Path.GetRelativePath(componentsDir, file);
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var attribute in TextAttributes)
                {
                    var literal = LiteralAttributeValue(lines[i], attribute);
                    if (literal is not null)
                    {
                        offenders.Add($"{relative}:{i + 1} {attribute}=\"{literal}\"");
                    }
                }
            }
        }

        foreach (var file in Directory.EnumerateFiles(componentsDir, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(componentsDir, file);
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var attribute in TextAttributes)
                {
                    // RenderFragment escrito em C#: builder.AddAttribute(seq, "aria-label", "Texto").
                    var match = Regex.Match(lines[i], $"\"{Regex.Escape(attribute)}\"\\s*,\\s*\"([^\"]+)\"");
                    if (match.Success && ContainsLetter(match.Groups[1].Value))
                    {
                        offenders.Add($"{relative}:{i + 1} {attribute}=\"{match.Groups[1].Value}\"");
                    }
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Texto de UI literal em componente — mova para o OpTranslation (Tr):\n" + string.Join("\n", offenders));

        // Evita falso-negativo: se nada foi varrido, a guarda não está fazendo nada.
        Assert.True(scanned > 50, $"Esperava varrer os .razor da biblioteca, mas encontrei {scanned} arquivo(s) em {componentsDir}.");
    }

    /// <summary>Valor literal de um atributo: ignora expressões Razor (<c>@...</c>) e tokens sem letras.</summary>
    private static string? LiteralAttributeValue(string line, string attribute)
    {
        var match = Regex.Match(line, $"{Regex.Escape(attribute)}=\"([^\"]*)\"");
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups[1].Value;
        if (value.Length == 0 || value.StartsWith('@'))
        {
            return null;
        }

        return ContainsLetter(value) ? value : null;
    }

    private static bool ContainsLetter(string value) => value.Any(char.IsLetter);

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "Components", "OpBlazorUI.Base", "Components")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Raiz do repositório não encontrada a partir de {AppContext.BaseDirectory}.");
    }
}
