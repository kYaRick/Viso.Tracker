#!/usr/bin/env dotnet run

#:package Spectre.Console

using System.Diagnostics;
using Spectre.Console;

/// <summary>
/// Builds and publishes Viso.Tracker to GitHub Pages (docs/ folder)
/// 
/// This script:
/// 1. Cleans previous builds
/// 2. Publishes Blazor WASM in Release mode
/// 3. Copies wwwroot to docs/
/// 4. Commits and pushes changes
/// 
/// Usage: dotnet run scripts/PublishGitHubPages.cs
/// </summary>
class PublishGitHubPages
{
    static async Task Main(string[] args)
    {
        try
        {
            var repoRoot = GetRepositoryRoot();
            var projectPath = Path.Combine(repoRoot, "srcs", "Viso.Tracker");
            var wwwrootPath = Path.Combine(repoRoot, "builds", "Viso.Tracker", "Release", "net10.0", "wwwroot");
            var docsPath = Path.Combine(repoRoot, "docs");

            if (!Directory.Exists(projectPath))
            {
                AnsiConsole.MarkupLine("[red]❌ Project path not found[/]");
                return;
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("[cyan]Publishing to GitHub Pages...[/]", async ctx =>
                {
                    // Step 1: Clean
                    ctx.Status("[blue]🧹 Cleaning previous builds...[/]");
                    await RunCommand("dotnet", $"clean \"{projectPath}\" --configuration Release", projectPath);

                    // Step 2: Publish
                    ctx.Status("[blue]🔨 Building Release version...[/]");
                    await RunCommand("dotnet", $"publish \"{projectPath}\" --configuration Release --no-self-contained", projectPath);

                    if (!Directory.Exists(wwwrootPath))
                    {
                        AnsiConsole.MarkupLine("[red]❌ wwwroot path not found after publish[/]");
                        return;
                    }

                    // Step 3: Copy to docs/
                    ctx.Status("[blue]📋 Copying to docs/ folder...[/]");
                    CopyDirectory(wwwrootPath, docsPath, overwrite: true);

                    // Step 4: Add .nojekyll to prevent Jekyll processing
                    ctx.Status("[blue]⚙️  Adding .nojekyll...[/]");
                    File.WriteAllText(Path.Combine(docsPath, ".nojekyll"), "");

                    AnsiConsole.MarkupLine("[green]✅ Published successfully to docs/[/]");
                    AnsiConsole.MarkupLine("[cyan]📌 Next steps:[/]");
                    AnsiConsole.MarkupLine("[cyan]  1. Review changes: git status[/]");
                    AnsiConsole.MarkupLine("[cyan]  2. Stage files: git add docs/[/]");
                    AnsiConsole.MarkupLine("[cyan]  3. Commit: git commit -m 'build: publish to github pages'[/]");
                    AnsiConsole.MarkupLine("[cyan]  4. Push: git push origin main[/]");
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
            throw new InvalidOperationException($"Command failed: {command} {arguments}\n{error}");
        }
    }

    static void CopyDirectory(string sourceDir, string destDir, bool overwrite = false)
    {
        // Create destination directory if it doesn't exist
        Directory.CreateDirectory(destDir);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        }

        // Recursively copy subdirectories
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir, overwrite);
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
