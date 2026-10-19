namespace OpBlazorUI.DocsGen;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            return Cli.Run(args);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"[docs-gen] erro: {exception.Message}");
            return 2;
        }
    }
}
