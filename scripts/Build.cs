#!/usr/bin/env dotnet run

#:package Spectre.Console

using System.Diagnostics;
using Spectre.Console;

/// <summary>
/// Builds Viso.Tracker Blazor WebAssembly project
/// 
/// Usage: 
///   dotnet run scripts/Build.cs                     # Local debug build
///   dotnet run scripts/Build.cs --release           # Local release build
/// </summary>
class Build
{
    static async Task Main(string[] args)
    {
        try
        {
            var configuration = args.Contains("--release") ? "Release" : "Debug";
            var publishOutput = configuration == "Release" ? "publish" : "bin";
            var repoRoot = GetRepositoryRoot();
            var projectPath = Path.Combine(repoRoot, "srcs", "Viso.Tracker");

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[cyan]Building {configuration}...[/]", async ctx =>
                {
                    ctx.Status("[blue]🔨 Building project...[/]");
                    var buildArgs = $"publish \"{projectPath}\" --configuration {configuration} --output \"{publishOutput}\"";
                    await RunCommand("dotnet", buildArgs, repoRoot);

                    AnsiConsole.MarkupLine($"[green]✅ Build completed successfully to {publishOutput}/[/]");
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            Environment.Exit(1);
        }
    }

    static async Task RunCommand(string command, string arguments, string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException($"Failed to start process: {command}");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Build failed: {error}");
        }
    }

    static string GetRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(currentDir, "Directory.Build.props")))
        {
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                throw new InvalidOperationException("Repository root not found");
            currentDir = parent.FullName;
        }
        return currentDir;
    }
}
