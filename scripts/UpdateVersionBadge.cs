#!/usr/bin/env dotnet run

#:package Spectre.Console

using System.Text.RegularExpressions;
using Spectre.Console;

/// <summary>
/// Updates version badges in README.md from Directory.Build.props
/// Automatically extracts version and updates badge URLs
/// 
/// Usage: dotnet run scripts/UpdateVersionBadge.cs
/// </summary>
class UpdateVersionBadge
{
    static async Task Main(string[] args)
    {
        try
        {
            var repoRoot = GetRepositoryRoot();
            var buildPropsPath = Path.Combine(repoRoot, "Directory.Build.props");
            var readmePath = Path.Combine(repoRoot, "README.md");

            if (!File.Exists(buildPropsPath))
            {
                AnsiConsole.MarkupLine("[red]❌ Directory.Build.props not found[/]");
                return;
            }

            if (!File.Exists(readmePath))
            {
                AnsiConsole.MarkupLine("[red]❌ README.md not found[/]");
                return;
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("[blue]Updating version badge...[/]", async ctx =>
                {
                    // Read version from Directory.Build.props
                    ctx.Status("[blue]Extracting version from Directory.Build.props...[/]");
                    var buildPropsContent = File.ReadAllText(buildPropsPath);
                    var versionMatch = Regex.Match(buildPropsContent, @"<Version>([\d.]+(?:-[\w]+)?)<\/Version>");

                    if (!versionMatch.Success)
                    {
                        AnsiConsole.MarkupLine("[red]❌ Version tag not found in Directory.Build.props[/]");
                        return;
                    }

                    var version = versionMatch.Groups[1].Value;
                    AnsiConsole.MarkupLine($"[cyan]Found version: {version}[/]");

                    // Update README.md
                    ctx.Status("[blue]Updating README.md badge...[/]");
                    var readmeContent = File.ReadAllText(readmePath);

                    // Replace version badge - flexible pattern that matches various formats
                    var badgePattern = @"\[!\[Version\]\(https:\/\/img\.shields\.io\/badge\/Version-[^\)]+\)\]\(#\)";
                    var newBadge = $"[![Version](https://img.shields.io/badge/Version-{version}-orange)](#)";

                    var updatedContent = Regex.Replace(readmeContent, badgePattern, newBadge);

                    // If pattern didn't match, try simple replacement
                    if (updatedContent == readmeContent)
                    {
                        AnsiConsole.MarkupLine("[yellow]⚠️  Version badge pattern not found, skipping update[/]");
                        return;
                    }

                    // Write back
                    File.WriteAllText(readmePath, updatedContent);
                    AnsiConsole.MarkupLine($"[green]✅ Updated README.md with version {version}[/]");
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            Environment.Exit(1);
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
