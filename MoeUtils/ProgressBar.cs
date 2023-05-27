using Spectre.Console;
using Spectre.Console.Rendering;

namespace MoxmoeApp.MoeUtils;

public class ProgressBar
{
    public static readonly Progress Instance = AnsiConsole.Progress()
        .Columns(
            new TaskDescriptionColumn(),
            new SpinnerColumn(),
            new ProgressBarColumn(),
            new CounterColumn(),
            new PercentageColumn(),
            new NaiveSpeedColumn(),
            new TextColumn("ETD:"),
            new ElapsedTimeColumn(),
            new TextColumn("ETA:"),
            new RemainingTimeColumn()
        );

    private sealed class TextColumn : ProgressColumn
    {
        private readonly string _s;

        public TextColumn(string s)
        {
            _s = s;
        }

        /// <inheritdoc />
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            return new Markup(_s).RightJustified();
        }
    }

    private sealed class CounterColumn : ProgressColumn
    {
        /// <inheritdoc />
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            var style = (int)task.Value == (int)task.MaxValue ? "green" : "blue";
            return new Markup($"[{style}]{task.Value}/{task.MaxValue}[/]").RightJustified();
        }

        /// <inheritdoc />
        public override int? GetColumnWidth(RenderOptions options)
        {
            return 5;
        }
    }

    private sealed class NaiveSpeedColumn : ProgressColumn
    {
        /// <inheritdoc />
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            if (task.Speed == null) return new Markup("[red](?/s)[/]");

            return new Markup($"[red]({task.Speed.Value:F2}/s)[/]");
        }
    }
}