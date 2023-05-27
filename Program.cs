using MoxmoeApp.MoeUtils;
using Spectre.Console;
using static MoxmoeApp.MoeUtils.TerminalUserInterface;

namespace MoxmoeApp;

internal class Program
{
    private static TaskbarIndicator _taskbarIndicator = null!;

    private static Repacker _repacker = null!;
    
    private static void Main(string[] args)
    {
        _taskbarIndicator = new TaskbarIndicator();
        _taskbarIndicator.ResetProgressState();
        AnsiConsole.Write(WelcomePanel);
        _repacker = new Repacker();
        _repacker.InitConfig(Path.Combine(Environment.CurrentDirectory, "config.conf"));

        ProgressBar.Instance.Start(ctx =>
        {
            LogLine("[yellow]开始提取图片并打包文件...[/]");
            var currVal = 0;
            var totVal = _repacker.FileList.Count;
            var task = ctx.AddTask("[green]Kox.moe[/]", maxValue: totVal);
            foreach (var filePath in _repacker.FileList)
            {
                _repacker.Repack(filePath);
                task.Increment(1);
                _taskbarIndicator.SetProgressValue(currVal++, totVal);
            }
        });

        _taskbarIndicator.ResetProgressState();
        LogLine("[yellow]开始清理缓存文件...[/]");
        FileSystem.DeleteDirectoryIfExists(_repacker.CacheDir);
        LogLine("[green]所有转换任务完成！[/]");
    }
}