using System.Diagnostics;

namespace OpBlazorUI.DocsGen;

internal static class Git
{
    public static string? Head(string root)
    {
        try
        {
            var startInfo = new ProcessStartInfo("git", "rev-parse --short HEAD")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(5000);
            return process.ExitCode == 0 && output.Length > 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }
}
