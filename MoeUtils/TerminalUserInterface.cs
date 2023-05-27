using Spectre.Console;
using Spectre.Console.Rendering;

namespace MoxmoeApp.MoeUtils;

public class TerminalUserInterface
{
    public static readonly Panel WelcomePanel = new Panel(
            " [bold cyan]支持 [green][link=https://vol.moe]Vol.moe[/][/] & [green][link=https://mox.moe]Mox.moe[/][/] & [green][link=https://kox.moe]Kox.moe[/][/] 下载的漫画文件转换。[/] ")
        .Header(" [bold green]Mox.moe EPUB Manga Repacker[/] ", Justify.Center)
        .DoubleBorder()
        .BorderStyle("cyan")
        .Padding(1, 2);

    public static Table PathTable(string inputDir, string outputDir, string cacheDir)
    {
        var pathTable = new Table().RoundedBorder();
        pathTable.AddColumn("[yellow]目录类型[/]");
        pathTable.AddColumn("[yellow]目录路径[/]");
        pathTable.AddRow("[cyan]输入目录[/]", inputDir);
        pathTable.AddRow("[cyan]输出目录[/]", outputDir);
        pathTable.AddRow("[cyan]缓存目录[/]", cacheDir);
        return pathTable;
    }

    private static void Print(string s)
    {
        AnsiConsole.Markup(s);
    }

    public static void Write(IRenderable s)
    {
        AnsiConsole.Write(s);
    }

    private static void PrintLine(string s)
    {
        AnsiConsole.MarkupLine(s);
    }

    public static void Log(string s)
    {
        Print($"[blue][[{Utils.GetCurrentTimeFormat()}]][/] {s}");
    }

    public static void LogLine(string s)
    {
        PrintLine($"[blue][[{Utils.GetCurrentTimeFormat()}]][/] {s}");
    }
}