using System.Globalization;
using System.Text.RegularExpressions;

namespace OpBlazorUI.Base.Components.ColorPicker;

/// <summary>Conversões HSB/RGB/HEX e (de)serialização do valor textual do ColorPicker.</summary>
internal static class OpColorMath
{
    // H em [0,360], S/B em [0,100]. Mantém frações para round-trip fiel (ex.: #336699).
    public static (int R, int G, int B) HsbToRgb(double h, double s, double b)
    {
        var s255 = s * 255.0 / 100.0;
        var v255 = b * 255.0 / 100.0;
        double r, g, bl;

        if (s255 == 0)
        {
            r = g = bl = v255;
        }
        else
        {
            if (h >= 360)
            {
                h -= 360;
            }

            var t1 = v255;
            var t2 = (255 - s255) * v255 / 255.0;
            var t3 = (t1 - t2) * (h % 60) / 60.0;

            if (h < 60) { r = t1; bl = t2; g = t2 + t3; }
            else if (h < 120) { g = t1; bl = t2; r = t1 - t3; }
            else if (h < 180) { g = t1; r = t2; bl = t2 + t3; }
            else if (h < 240) { bl = t1; r = t2; g = t1 - t3; }
            else if (h < 300) { bl = t1; g = t2; r = t2 + t3; }
            else { r = t1; g = t2; bl = t1 - t3; }
        }

        return ((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(bl));
    }

    public static (double H, double S, double B) RgbToHsb(int r, int g, int b)
    {
        var min = Math.Min(r, Math.Min(g, b));
        var max = Math.Max(r, Math.Max(g, b));
        var delta = max - min;

        double h;
        var s = max != 0 ? 255.0 * delta / max : 0;
        var brightness = (double)max;

        if (s != 0)
        {
            if (r == max) h = (g - b) / (double)delta;
            else if (g == max) h = 2 + (b - r) / (double)delta;
            else h = 4 + (r - g) / (double)delta;
        }
        else
        {
            h = -1;
        }

        h *= 60;
        if (h < 0) h += 360;

        s *= 100.0 / 255.0;
        brightness = brightness * 100.0 / 255.0;

        return (h, s, brightness);
    }

    public static string RgbToHex(int r, int g, int b)
        => $"{r:x2}{g:x2}{b:x2}";

    public static string HsbToHex(double h, double s, double b)
    {
        var (r, g, bl) = HsbToRgb(h, s, b);
        return "#" + RgbToHex(r, g, bl);
    }

    public static (int R, int G, int B) HexToRgb(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new FormatException("Hex vazio.");
        }

        var value = hex.TrimStart('#');

        // Aceita #RGB, #RGBA, #RRGGBB e #RRGGBBAA (ignora o alfa).
        if (value.Length == 3 || value.Length == 4)
        {
            var expanded = new string(value.SelectMany(c => new[] { c, c }).ToArray());
            value = expanded;
        }

        if (value.Length == 8)
        {
            value = value[..6];
        }

        if (value.Length != 6 || !value.All(Uri.IsHexDigit))
        {
            throw new FormatException($"Hex inválido: {hex}");
        }

        var number = int.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return ((number >> 16) & 0xff, (number >> 8) & 0xff, number & 0xff);
    }

    public static (double H, double S, double B) Parse(string? value, string format, string defaultColor)
    {
        var text = string.IsNullOrWhiteSpace(value) ? defaultColor : value!;

        try
        {
            switch (format)
            {
                case "rgb":
                    var rgb = Extract(text);
                    if (rgb.Count >= 3)
                    {
                        return RgbToHsb(rgb[0], rgb[1], rgb[2]);
                    }

                    break;

                case "hsb":
                    var hsb = Extract(text);
                    if (hsb.Count >= 3)
                    {
                        return ClampHsb(hsb[0], hsb[1], hsb[2]);
                    }

                    break;

                default:
                    return HexToHsb(text);
            }
        }
        catch
        {
            // valor inválido: usa o padrão
        }

        return HexToHsb(defaultColor);
    }

    public static string Serialize(double h, double s, double b, string format) => format switch
    {
        "rgb" => RgbString(h, s, b),
        "hsb" => $"hsb({(int)Math.Round(h)}, {(int)Math.Round(s)}%, {(int)Math.Round(b)}%)",
        _ => HsbToHex(h, s, b)
    };

    private static (double H, double S, double B) HexToHsb(string hex)
    {
        var (r, g, b) = HexToRgb(hex);
        return RgbToHsb(r, g, b);
    }

    private static string RgbString(double h, double s, double b)
    {
        var (r, g, bl) = HsbToRgb(h, s, b);
        return $"rgb({r}, {g}, {bl})";
    }

    private static (double H, double S, double B) ClampHsb(double h, double s, double b)
        => (Math.Clamp(h, 0, 360), Math.Clamp(s, 0, 100), Math.Clamp(b, 0, 100));

    private static List<int> Extract(string text)
    {
        var result = new List<int>();
        foreach (Match match in Regex.Matches(text, @"-?\d+"))
        {
            result.Add(int.Parse(match.Value, CultureInfo.InvariantCulture));
        }

        return result;
    }
}
