#!/usr/bin/env dotnet run

#:package Spectre.Console

using Spectre.Console;

/// <summary>
/// Installs Git hooks for commit message validation.
/// 
/// Usage: dotnet scripts/InstallGitHooks.cs
/// </summary>
class InstallGitHooks
{
    static int Main(string[] args)
    {
        try
        {
            AnsiConsole.Write(new Rule("[cyan]Installing Git Hooks[/]").RuleStyle("grey").LeftJustified());

            var repoRoot = FindGitRoot();
            if (repoRoot == null)
            {
                AnsiConsole.MarkupLine("[red]❌ Not a git repository[/]");
                return 1;
            }

            var hooksDir = Path.Combine(repoRoot, ".git", "hooks");
            if (!Directory.Exists(hooksDir))
            {
                Directory.CreateDirectory(hooksDir);
            }

            // Create commit-msg hook
            var commitMsgHook = Path.Combine(hooksDir, "commit-msg");
            var hookContent = @"#!/bin/sh
# Git commit-msg hook for Conventional Commits validation

dotnet \"$(git rev-parse --show-toplevel)/scripts/ValidateCommitMessage.cs\" \"$1\"";

            File.WriteAllText(commitMsgHook, hookContent.Replace("\r\n", "\n"));

            // Make executable on Unix-like systems
            if (!OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start("chmod", $"+x {commitMsgHook}");
            }

            AnsiConsole.MarkupLine($"[green]✅ Installed:[/] [cyan]commit-msg[/] hook");
            AnsiConsole.MarkupLine($"[grey]   Location: {commitMsgHook}[/]");

            AnsiConsole.Write(new Rule().RuleStyle("grey"));
            AnsiConsole.MarkupLine("[green]✅ Git hooks installed successfully![/]");
            AnsiConsole.MarkupLine("[yellow]💡 All commits will now be validated for Conventional Commits format[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    static string? FindGitRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}
