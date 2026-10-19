using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>Uma receita (arquivo Markdown) que acompanha a skill.</summary>
internal sealed record SkillRecipe(
    string FileName,
    string Title,
    string Description,
    string Content);

/// <summary>
/// Carrega a skill de agentes do repositório (<c>.opencode/skills/opblazorui</c>), publica os
/// arquivos em <c>skills/</c> e gera o catálogo HTTP usado por ferramentas compatíveis.
/// </summary>
internal sealed class SkillBundle
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }

    /// <summary>Conteúdo do SKILL.md, incluindo o frontmatter YAML.</summary>
    public required string Markdown { get; init; }

    /// <summary>Conteúdo do SKILL.md sem o frontmatter (para o llms-full.txt).</summary>
    public required string Body { get; init; }

    public required IReadOnlyList<SkillRecipe> Recipes { get; init; }

    public static SkillBundle Load(string skillDir)
    {
        var skillPath = Path.Combine(skillDir, "SKILL.md");
        if (!File.Exists(skillPath))
        {
            throw new InvalidOperationException($"Skill não encontrada: {skillPath}.");
        }

        var markdown = Normalize(File.ReadAllText(skillPath));
        var (frontmatter, body) = SplitFrontmatter(markdown);

        var name = FrontmatterValue(frontmatter, "name") ?? "OpBlazorUI";
        var description = FrontmatterValue(frontmatter, "description")
                          ?? "Instruções para gerar código com a biblioteca OpBlazorUI.";

        var recipesDir = Path.Combine(skillDir, "references");
        var recipes = new List<SkillRecipe>();
        if (Directory.Exists(recipesDir))
        {
            foreach (var path in Directory.EnumerateFiles(recipesDir, "*.md").OrderBy(Path.GetFileName, StringComparer.Ordinal))
            {
                var content = Normalize(File.ReadAllText(path));
                recipes.Add(new SkillRecipe(
                    Path.GetFileName(path),
                    FirstHeading(content) ?? Path.GetFileNameWithoutExtension(path),
                    FirstParagraph(content) ?? "",
                    content));
            }
        }

        if (recipes.Count == 0)
        {
            throw new InvalidOperationException($"A skill não tem receitas em {recipesDir}.");
        }

        return new SkillBundle
        {
            Id = RepoContext.Slug(name),
            Name = name,
            Description = description,
            Markdown = markdown,
            Body = body,
            Recipes = recipes
        };
    }

    /// <summary>Publica a skill em <c>{outDir}/skills/{id}</c> e escreve o catálogo HTTP.</summary>
    public void Stage(string outDir, Manifest manifest)
    {
        var skillDir = Path.Combine(outDir, "skills", Id);
        Directory.CreateDirectory(Path.Combine(skillDir, "references"));

        WriteFile(Path.Combine(skillDir, "SKILL.md"), Markdown);
        // Forma nomeada usada pelo catálogo HTTP: o arquivo vira a skill de id {Id}.
        WriteFile(Path.Combine(skillDir, Id + ".md"), Markdown);

        var files = new List<string> { Id + ".md" };
        foreach (var recipe in Recipes)
        {
            WriteFile(Path.Combine(skillDir, "references", recipe.FileName), recipe.Content);
            files.Add("references/" + recipe.FileName);
        }

        var version = string.IsNullOrWhiteSpace(manifest.Library.Commit)
            ? manifest.Library.Version
            : manifest.Library.Commit!;

        Json.Write(Path.Combine(outDir, "skills", "index.json"), new
        {
            skills = new[]
            {
                new
                {
                    name = Id,
                    version,
                    files
                }
            }
        });
    }

    /// <summary>Confere os arquivos publicados da skill e o catálogo HTTP.</summary>
    public static IReadOnlyList<string> Verify(string outDir, string? fullText = null)
    {
        var errors = new List<string>();
        var indexPath = Path.Combine(outDir, "skills", "index.json");
        if (!File.Exists(indexPath))
        {
            return [$"catálogo de skills ausente: {indexPath}"];
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(indexPath));
        }
        catch (JsonException exception)
        {
            return [$"catálogo de skills inválido: {exception.Message}"];
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("skills", out var skills) ||
                skills.ValueKind != JsonValueKind.Array ||
                skills.GetArrayLength() == 0)
            {
                return ["catálogo de skills sem entradas."];
            }

            foreach (var skill in skills.EnumerateArray())
            {
                var name = skill.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add("catálogo de skills com entrada sem nome.");
                    continue;
                }

                var skillDir = Path.Combine(outDir, "skills", name);
                var skillPath = Path.Combine(skillDir, "SKILL.md");
                if (!File.Exists(skillPath))
                {
                    errors.Add($"SKILL.md ausente: skills/{name}/SKILL.md");
                }
                else if (File.ReadLines(skillPath).FirstOrDefault() != "---")
                {
                    errors.Add($"skills/{name}/SKILL.md sem frontmatter YAML.");
                }

                if (!skill.TryGetProperty("files", out var files) || files.ValueKind != JsonValueKind.Array)
                {
                    errors.Add($"skills/{name}: catálogo sem a lista de arquivos.");
                    continue;
                }

                foreach (var file in files.EnumerateArray())
                {
                    var relative = file.GetString();
                    if (string.IsNullOrWhiteSpace(relative))
                    {
                        continue;
                    }

                    var path = Path.Combine(skillDir, relative);
                    if (!File.Exists(path))
                    {
                        errors.Add($"arquivo da skill ausente: skills/{name}/{relative}");
                        continue;
                    }

                    if (fullText is { Length: > 0 } && relative.StartsWith("references/", StringComparison.Ordinal))
                    {
                        var title = File.ReadLines(path)
                            .FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
                        if (title is not null && !fullText.Contains(title, StringComparison.Ordinal))
                        {
                            errors.Add($"llms-full.txt não contém a receita '{title}'");
                        }
                    }
                }
            }
        }

        return errors;
    }

    private static string Normalize(string content) => content.Replace("\r\n", "\n").TrimEnd() + "\n";

    private static (string Frontmatter, string Body) SplitFrontmatter(string markdown)
    {
        if (!markdown.StartsWith("---\n", StringComparison.Ordinal))
        {
            return ("", markdown);
        }

        var end = markdown.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return ("", markdown);
        }

        return (markdown[4..end], markdown[(end + 5)..].TrimStart('\n'));
    }

    private static string? FrontmatterValue(string frontmatter, string key)
    {
        foreach (var line in frontmatter.Split('\n'))
        {
            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            if (!line[..separator].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return line[(separator + 1)..].Trim();
        }

        return null;
    }

    private static string? FirstHeading(string markdown) =>
        markdown.Split('\n').FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal))?[2..].Trim();

    private static string? FirstParagraph(string markdown)
    {
        var lines = markdown.Split('\n');
        var paragraph = new List<string>();
        var collecting = false;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith("```"))
            {
                if (collecting)
                {
                    break;
                }

                continue;
            }

            collecting = true;
            paragraph.Add(line);
        }

        if (paragraph.Count == 0)
        {
            return null;
        }

        var text = string.Join(' ', paragraph);
        text = Regex.Replace(text, @"\[([^\]]+)\]\([^)]+\)", "$1");
        text = text.TrimStart('-', '*', '>', ' ').Replace("**", "").Replace("`", "");
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > 0 ? text : null;
    }

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content, Utf8);
    }
}
