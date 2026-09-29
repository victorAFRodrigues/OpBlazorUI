using System.Net;
using System.Text;

namespace OpBlazorUI.Showcase.Components.Doc;

/// <summary>
/// Realce de sintaxe dos exemplos da doc, feito no servidor/WASM (sem JS): devolve HTML com o texto
/// escapado e os tokens em <c>&lt;span class="tk-*"&gt;</c>. As cores ficam no doc-layout.css (paleta Rider Dark).
/// Heurístico por design: o que não for reconhecido sai como texto simples, nunca quebra a renderização.
/// </summary>
public static class SyntaxHighlighter
{
    public static string Highlight(string code, string? language)
    {
        if (string.IsNullOrEmpty(code))
            return "";

        try
        {
            var w = new Writer(code);
            switch (language?.ToLowerInvariant())
            {
                case "razor" or "cshtml":
                    w.Razor = true;
                    Markup(w, code.Length);
                    break;
                case "html" or "xml" or "markup":
                    Markup(w, code.Length);
                    break;
                case "csharp" or "cs" or "c#":
                    CSharp(w, code.Length);
                    break;
                case "css":
                    Css(w, code.Length);
                    break;
                case "bash" or "sh" or "shell" or "powershell":
                    Bash(w, code.Length);
                    break;
                default:
                    w.Text(code.Length);
                    break;
            }
            return w.ToString();
        }
        catch
        {
            return WebUtility.HtmlEncode(code);
        }
    }

    // ---------------------------------------------------------------- escrita

    private sealed class Writer(string s)
    {
        private readonly StringBuilder _sb = new(s.Length * 2);
        public readonly string S = s;
        public int I;
        public bool Razor; // distingue componentes Blazor e interpreta as transições @

        public char At(int i) => i < S.Length ? S[i] : '\0';
        public char Cur => At(I);
        public bool StartsWith(string v) => string.CompareOrdinal(S, I, v, 0, v.Length) == 0;

        /// <summary>Emite de <see cref="I"/> até <paramref name="end"/> com a classe informada.</summary>
        public void Emit(string? cls, int end)
        {
            end = Math.Min(end, S.Length);
            if (end <= I) return;
            var text = WebUtility.HtmlEncode(S[I..end]);
            if (cls == null) _sb.Append(text);
            else _sb.Append("<span class=\"tk-").Append(cls).Append("\">").Append(text).Append("</span>");
            I = end;
        }

        public void Text(int end) => Emit(null, end);
        public override string ToString() => _sb.ToString();
    }

    private static char Ch(string s, int i) => i < s.Length ? s[i] : '\0';
    private static bool IsIdStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsId(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static int SkipId(string s, int i)
    {
        while (i < s.Length && IsId(s[i])) i++;
        return i;
    }

    // ---------------------------------------------------------------- markup / razor

    private static readonly HashSet<string> RazorLineDirectives =
        ["using", "page", "namespace", "inject", "inherits", "implements", "attribute", "layout", "rendermode", "typeparam"];

    private static readonly HashSet<string> RazorBlockKeywords =
        ["if", "else", "foreach", "for", "while", "switch", "do", "try", "catch", "finally", "lock"];

    private static void Markup(Writer w, int end)
    {
        while (w.I < end)
        {
            if (w.StartsWith("<!--"))
            {
                var close = w.S.IndexOf("-->", w.I + 4, StringComparison.Ordinal);
                w.Emit("comment", close < 0 ? end : close + 3);
            }
            else if (w.Cur == '<' && (IsIdStart(w.At(w.I + 1)) || (w.At(w.I + 1) == '/' && IsIdStart(w.At(w.I + 2)))))
            {
                Tag(w, end);
            }
            else if (w.Cur == '@' && w.Razor)
            {
                RazorTransition(w, end);
            }
            else
            {
                // texto até o próximo ponto de interesse
                var j = w.I + 1;
                while (j < end && w.S[j] != '<' && !(w.S[j] == '@' && w.Razor)) j++;
                w.Text(j);
            }
        }
    }

    private static void Tag(Writer w, int end)
    {
        w.Emit("punct", w.I + (w.At(w.I + 1) == '/' ? 2 : 1));

        var nameEnd = w.I;
        while (nameEnd < end && (IsId(w.S[nameEnd]) || w.S[nameEnd] is '.' or ':' or '-')) nameEnd++;
        var name = w.S[w.I..nameEnd];
        var component = w.Razor && (char.IsUpper(name[0]) || name.Contains('.'));
        w.Emit(component ? "component" : "tag", nameEnd);

        while (w.I < end)
        {
            var c = w.Cur;
            if (c == '>')
            {
                w.Emit("punct", w.I + 1);
                return;
            }
            if (c == '/' && w.At(w.I + 1) == '>')
            {
                w.Emit("punct", w.I + 2);
                return;
            }
            if (char.IsWhiteSpace(c))
            {
                var j = w.I;
                while (j < end && char.IsWhiteSpace(w.S[j])) j++;
                w.Text(j);
                continue;
            }
            if (c == '@' || IsIdStart(c))
            {
                var j = w.I + 1;
                while (j < end && (IsId(w.S[j]) || w.S[j] is '-' or ':' or '.' or '@')) j++;
                var razorAttr = c == '@' && w.Razor;
                // Handler de evento (OnClick="Load", @onclick="Show"): o valor é um método
                var handler = (component && w.StartsWith("On") && char.IsUpper(w.At(w.I + 2)))
                    || (razorAttr && w.StartsWith("@on"));
                w.Emit(razorAttr ? "razor" : component ? "param" : "attr", j);
                if (w.Cur == '=')
                {
                    w.Emit("punct", w.I + 1);
                    if (w.Cur == '"') AttrValue(w, end, forceCs: razorAttr, maybeCs: component, handler: handler);
                }
                continue;
            }
            // qualquer outro caractere dentro da tag
            w.Text(w.I + 1);
        }
    }

    /// <summary>Valor entre aspas de um atributo; expressões Razor/C# são coloridas como C#.</summary>
    private static void AttrValue(Writer w, int end, bool forceCs, bool maybeCs, bool handler)
    {
        var start = w.I; // aspas de abertura
        if (w.Razor && w.At(start + 1) == '@' && w.At(start + 2) == '(')
        {
            // "@( ... )" — pode conter aspas internas, então fecha pelos parênteses
            var close = MatchBracket(w.S, start + 2, end);
            w.Emit("string", start + 1);
            w.Emit("razor", w.I + 1);
            CSharp(w, close + 1);
            if (w.Cur == '"') w.Emit("string", w.I + 1);
            return;
        }

        var q = w.S.IndexOf('"', start + 1);
        if (q < 0 || q > end) q = end - 1;
        var inner = w.S[(start + 1)..q];

        if (w.Razor && inner.StartsWith('@') && inner.Length > 1 && inner[1] != '@')
        {
            w.Emit("string", start + 1);
            w.Emit("razor", w.I + 1);
            CSharp(w, q);
            w.Emit("string", q + 1);
        }
        else if (handler && inner.Length > 0 && IsIdStart(inner[0]) && SkipId(inner, 0) == inner.Length)
        {
            w.Emit("string", start + 1);
            w.Emit("method", q);
            w.Emit("string", q + 1);
        }
        else if (forceCs || (maybeCs && LooksLikeCSharp(inner)))
        {
            w.Emit("string", start + 1);
            CSharp(w, q);
            w.Emit("string", q + 1);
        }
        else
        {
            w.Emit("string", q + 1);
        }
    }

    /// <summary>Parâmetro de componente cujo valor é expressão (lambda, campo, método), não string literal.</summary>
    private static bool LooksLikeCSharp(string v) =>
        v.Contains("=>") || (v.Length > 1 && v[0] == '_' && IsIdStart(v[1]));

    private static void RazorTransition(Writer w, int end)
    {
        var next = w.At(w.I + 1);

        if (next == '@') // @@ escapa um @ literal
        {
            w.Text(w.I + 2);
            return;
        }
        if (next == '*')
        {
            var close = w.S.IndexOf("*@", w.I + 2, StringComparison.Ordinal);
            w.Emit("comment", close < 0 ? end : close + 2);
            return;
        }
        if (next == '(')
        {
            var close = MatchBracket(w.S, w.I + 1, end);
            w.Emit("razor", w.I + 1);
            CSharp(w, close + 1);
            return;
        }
        if (next == '{')
        {
            var close = MatchBracket(w.S, w.I + 1, end);
            w.Emit("razor", w.I + 1);
            w.Emit("punct", w.I + 1);
            CSharp(w, close);
            w.Emit("punct", close + 1);
            return;
        }
        if (!IsIdStart(next))
        {
            w.Text(w.I + 1);
            return;
        }

        var idEnd = SkipId(w.S, w.I + 1);
        var word = w.S[(w.I + 1)..idEnd];

        if (word is "code" or "functions")
        {
            w.Emit("razor", idEnd);
            var brace = w.S.IndexOf('{', w.I);
            if (brace < 0 || brace >= end) return;
            w.Text(brace);
            var close = MatchBracket(w.S, brace, end);
            w.Emit("punct", brace + 1);
            CSharp(w, close);
            w.Emit("punct", close + 1);
        }
        else if (RazorLineDirectives.Contains(word))
        {
            w.Emit("razor", idEnd);
            var eol = w.S.IndexOf('\n', w.I);
            CSharp(w, eol < 0 || eol > end ? end : eol);
        }
        else if (RazorBlockKeywords.Contains(word))
        {
            // @if (...) / @foreach (...): condição em C#, corpo continua como markup
            w.Emit("razor", w.I + 1);
            w.Emit("keyword", idEnd);
            var j = w.I;
            while (j < end && w.S[j] is ' ' or '\t') j++;
            if (w.At(j) == '(')
            {
                w.Text(j);
                CSharp(w, MatchBracket(w.S, j, end) + 1);
            }
        }
        else
        {
            // @expr implícita: @_count, @s.Label, @Foo(x)
            var j = idEnd;
            while (w.At(j) == '.' && IsIdStart(w.At(j + 1))) j = SkipId(w.S, j + 1);
            if (w.At(j) == '(') j = MatchBracket(w.S, j, end) + 1;
            w.Emit("razor", w.I + 1);
            CSharp(w, j);
        }
    }

    /// <summary>Posição do fechamento de ( [ { em <paramref name="open"/>, ignorando strings e chars.</summary>
    private static int MatchBracket(string s, int open, int end)
    {
        var openCh = s[open];
        var closeCh = openCh switch { '(' => ')', '[' => ']', _ => '}' };
        var depth = 0;
        for (var i = open; i < end; i++)
        {
            var c = s[i];
            if (c == '"' || c == '\'')
            {
                i = SkipQuoted(s, i, end) - 1;
                continue;
            }
            if (c == '/' && i + 1 < end && s[i + 1] == '/')
            {
                var eol = s.IndexOf('\n', i);
                i = eol < 0 ? end : eol;
                continue;
            }
            if (c == openCh) depth++;
            else if (c == closeCh && --depth == 0) return i;
        }
        return end - 1;
    }

    /// <summary>Fim (exclusivo) de uma string/char C# iniciada em <paramref name="i"/>, incluindo raw strings.</summary>
    private static int SkipQuoted(string s, int i, int end)
    {
        var q = s[i];
        if (q == '"')
        {
            var n = 0;
            while (i + n < end && s[i + n] == '"') n++;
            if (n >= 3)
            {
                var close = s.IndexOf(new string('"', n), i + n, StringComparison.Ordinal);
                return close < 0 ? end : close + n;
            }
            if (n == 2) return i + 2; // ""
        }
        var verbatim = i > 0 && (s[i - 1] == '@' || (s[i - 1] == '$' && i > 1 && s[i - 2] == '@'));
        for (var j = i + 1; j < end; j++)
        {
            if (s[j] == '\\' && !verbatim) { j++; continue; }
            if (s[j] == q)
            {
                if (verbatim && j + 1 < end && s[j + 1] == q) { j++; continue; }
                return j + 1;
            }
            if (s[j] == '\n' && !verbatim) return j;
        }
        return end;
    }

    // ---------------------------------------------------------------- C#

    private static readonly HashSet<string> CsKeywords =
    [
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
        "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "get", "goto", "if",
        "implicit", "in", "init", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
        "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly",
        "record", "ref", "required", "return", "sbyte", "sealed", "set", "short", "sizeof", "static", "string",
        "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
        "ushort", "using", "var", "virtual", "void", "volatile", "when", "where", "while", "with", "yield",
        "nameof", "not", "and", "or", "value", "global"
    ];

    private static void CSharp(Writer w, int end)
    {
        var s = w.S;
        var prevWord = "";
        while (w.I < end)
        {
            var c = w.Cur;

            if (c == '/' && w.At(w.I + 1) == '/')
            {
                var eol = s.IndexOf('\n', w.I);
                w.Emit("comment", eol < 0 || eol > end ? end : eol);
                continue;
            }
            if (c == '/' && w.At(w.I + 1) == '*')
            {
                var close = s.IndexOf("*/", w.I + 2, StringComparison.Ordinal);
                w.Emit("comment", close < 0 || close > end ? end : close + 2);
                continue;
            }
            if (c == '"' || c == '\'')
            {
                w.Emit("string", Math.Min(SkipQuoted(s, w.I, end), end));
                prevWord = "";
                continue;
            }
            if ((c == '$' || c == '@') && (w.At(w.I + 1) == '"' || (w.At(w.I + 1) is '$' or '@' && w.At(w.I + 2) == '"')))
            {
                var q = w.At(w.I + 1) == '"' ? w.I + 1 : w.I + 2;
                w.Emit("string", Math.Min(SkipQuoted(s, q, end), end));
                prevWord = "";
                continue;
            }
            if (char.IsDigit(c))
            {
                var j = w.I + 1;
                while (j < end && (char.IsLetterOrDigit(s[j]) || s[j] == '.' && char.IsDigit(w.At(j + 1)) || s[j] == '_')) j++;
                w.Emit("number", j);
                continue;
            }
            if (IsIdStart(c))
            {
                var j = SkipId(s, w.I);
                var word = s[w.I..j];
                w.Emit(ClassifyCs(s, j, word, prevWord), j);
                prevWord = word;
                continue;
            }
            if (c == '[' && IsIdStart(w.At(w.I + 1)) && LineStartsAt(s, w.I))
            {
                // atributo: [Parameter], [Inject]
                w.Text(w.I + 1);
                var j = SkipId(s, w.I);
                w.Emit("type", j);
                continue;
            }

            if (!char.IsWhiteSpace(c)) prevWord = c == '.' ? "." : "";
            w.Text(w.I + 1);
        }
    }

    private static string? ClassifyCs(string s, int end, string word, string prevWord)
    {
        if (CsKeywords.Contains(word) && prevWord != ".")
            return "keyword";

        var next = end;
        while (next < s.Length && s[next] is ' ' or '\t') next++;
        var nextCh = next < s.Length ? s[next] : '\0';

        if (nextCh == '(' && prevWord != "new")
            return "method";
        if (nextCh == '<' && prevWord != "." && char.IsUpper(word[0]) && IsGenericArgs(s, next))
            return "type";
        if (word[0] == '_')
            return "field";
        if (prevWord == ".")
            return char.IsUpper(word[0]) ? "field" : null;
        if (char.IsUpper(word[0]))
        {
            // Tipo: após new, seguido de acesso estático (Task.Delay), ou em declaração (Tipo nome / Tipo[] / Tipo?)
            if (prevWord == "new" || nextCh == '.' || nextCh == '[' && Ch(s, next + 1) == ']')
                return "type";
            if (nextCh == '?' && Ch(s, next + 1) is ' ' or '>' or ',')
                return "type";
            if (IsIdStart(nextCh) && next > end)
                return "type";
        }
        return null;
    }

    private static bool IsGenericArgs(string s, int lt)
    {
        var depth = 0;
        for (var i = lt; i < s.Length && i < lt + 80; i++)
        {
            var c = s[i];
            if (c == '<') depth++;
            else if (c == '>' && --depth == 0) return true;
            else if (!(IsId(c) || c is ',' or ' ' or '.' or '?' or '[' or ']')) return false;
        }
        return false;
    }

    private static bool LineStartsAt(string s, int i)
    {
        for (var j = i - 1; j >= 0 && s[j] != '\n'; j--)
            if (!char.IsWhiteSpace(s[j])) return false;
        return true;
    }

    // ---------------------------------------------------------------- CSS

    private static void Css(Writer w, int end)
    {
        var s = w.S;
        var inBlock = false;
        var inValue = false;
        while (w.I < end)
        {
            var c = w.Cur;
            if (c == '/' && w.At(w.I + 1) == '*')
            {
                var close = s.IndexOf("*/", w.I + 2, StringComparison.Ordinal);
                w.Emit("comment", close < 0 ? end : close + 2);
            }
            else if (c == '{') { inBlock = true; inValue = false; w.Emit("punct", w.I + 1); }
            else if (c == '}') { inBlock = false; inValue = false; w.Emit("punct", w.I + 1); }
            else if (c == ';') { inValue = false; w.Emit("punct", w.I + 1); }
            else if (c == ':' && inBlock && !inValue) { inValue = true; w.Emit("punct", w.I + 1); }
            else if (c is '"' or '\'') w.Emit("string", SkipQuoted(s, w.I, end));
            else if (char.IsWhiteSpace(c)) w.Text(w.I + 1);
            else if (!inBlock)
            {
                var j = w.I;
                while (j < end && s[j] != '{' && s[j] != ',' && s[j] != '\n') j++;
                while (j > w.I + 1 && char.IsWhiteSpace(s[j - 1])) j--;
                w.Emit("tag", j == w.I ? w.I + 1 : j);
            }
            else if (!inValue)
            {
                var j = w.I;
                while (j < end && s[j] != ':' && s[j] != ';' && s[j] != '}' && !char.IsWhiteSpace(s[j])) j++;
                w.Emit("param", j == w.I ? w.I + 1 : j);
            }
            else if (char.IsDigit(c) || c == '#' || (c is '.' or '-' && char.IsDigit(w.At(w.I + 1))))
            {
                var j = w.I + 1;
                while (j < end && (char.IsLetterOrDigit(s[j]) || s[j] is '.' or '%')) j++;
                w.Emit("number", j);
            }
            else if (c == '-' && w.At(w.I + 1) == '-')
            {
                var j = w.I + 2;
                while (j < end && (IsId(s[j]) || s[j] == '-')) j++;
                w.Emit("field", j);
            }
            else if (IsIdStart(c))
            {
                var j = w.I;
                while (j < end && (IsId(s[j]) || s[j] == '-')) j++;
                w.Emit(w.At(j) == '(' ? "method" : "keyword", j);
            }
            else w.Text(w.I + 1);
        }
    }

    // ---------------------------------------------------------------- bash

    private static void Bash(Writer w, int end)
    {
        var s = w.S;
        var lineStart = true;
        while (w.I < end)
        {
            var c = w.Cur;
            if (c == '\n') { lineStart = true; w.Text(w.I + 1); }
            else if (char.IsWhiteSpace(c)) w.Text(w.I + 1);
            else if (c == '#')
            {
                var eol = s.IndexOf('\n', w.I);
                w.Emit("comment", eol < 0 ? end : eol);
            }
            else if (c is '"' or '\'') { w.Emit("string", SkipQuoted(s, w.I, end)); lineStart = false; }
            else
            {
                var j = w.I;
                while (j < end && !char.IsWhiteSpace(s[j])) j++;
                w.Emit(lineStart ? "method" : c == '-' ? "param" : null, j);
                lineStart = false;
            }
        }
    }
}
