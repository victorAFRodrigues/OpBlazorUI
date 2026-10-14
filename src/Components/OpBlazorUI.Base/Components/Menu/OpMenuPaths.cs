namespace OpBlazorUI.Base.Components.Menu;

/// <summary>
/// Utilitário de caminhos de submenu (<c>0_1_2</c>) compartilhado pela família de menus para
/// manter apenas um submenu aberto por nível e fechar os descendentes ao recolher.
/// </summary>
internal static class OpMenuPaths
{
    public static string Parent(string path)
    {
        var i = path.LastIndexOf('_');
        return i < 0 ? string.Empty : path[..i];
    }

    public static bool IsDescendantOf(string candidate, string path) =>
        candidate.Length > path.Length &&
        candidate.StartsWith(path, StringComparison.Ordinal) &&
        candidate[path.Length] == '_';

    public static void RemoveWithDescendants(HashSet<string> open, string path)
    {
        open.Remove(path);
        foreach (var p in open.Where(p => IsDescendantOf(p, path)).ToList())
        {
            open.Remove(p);
        }
    }

    /// <summary>
    /// Alterna <paramref name="path"/>. Ao abrir, fecha os irmãos do mesmo nível (e seus
    /// descendentes), como no PrimeNG.
    /// </summary>
    public static void Toggle(HashSet<string> open, string path)
    {
        if (open.Contains(path))
        {
            RemoveWithDescendants(open, path);
            return;
        }

        var parent = Parent(path);
        foreach (var p in open.ToList())
        {
            if (Parent(p) == parent)
            {
                RemoveWithDescendants(open, p);
            }
        }

        open.Add(path);
    }
}
