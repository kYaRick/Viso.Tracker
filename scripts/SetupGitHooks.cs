#!/usr/bin/env dotnet run

#:package Spectre.Console

using System.Diagnostics;
using Spectre.Console;

/// <summary>
/// Configures Git hooks for the repository.
/// Runs automatically after clone or manually if needed.
/// 
/// Usage: dotnet scripts/SetupGitHooks.cs
/// </summary>
class SetupGitHooks
{
    static int Main(string[] args)
    {
        try
        {
            AnsiConsole.Write(new Rule("[cyan]Setting Up Git Hooks[/]").RuleStyle("grey").LeftJustified());

            var repoRoot = FindGitRoot();
            if (repoRoot == null)
            {
                AnsiConsole.MarkupLine("[red]❌ Not a git repository[/]");
                return 1;
            }

            // Configure git to use .githooks directory
            var result = RunGitCommand("config", "core.hooksPath", ".githooks", repoRoot);
            if (result != 0)
            {
                AnsiConsole.MarkupLine("[red]❌ Failed to configure git hooks path[/]");
                return 1;
            }

            AnsiConsole.MarkupLine($"[green]✅ Configured:[/] [cyan]core.hooksPath[/] = [yellow].githooks[/]");

            // Ensure .githooks exists and write commit-msg hook
            var hooksDir = Path.Combine(repoRoot, ".githooks");
            if (!Directory.Exists(hooksDir))
            {
                Directory.CreateDirectory(hooksDir);
            }

            var commitMsgHook = Path.Combine(hooksDir, "commit-msg");
            var hookContent = @"#!/bin/sh
# Git commit-msg hook for Conventional Commits validation

dotnet \"$(git rev-parse --show-toplevel)/scripts/ValidateCommitMessage.cs\" \"$1\"";

            File.WriteAllText(commitMsgHook, hookContent.Replace("\r\n", "\n"));

            // Make hooks executable on Unix-like systems
            if (!OperatingSystem.IsWindows())
            {
                foreach (var hook in Directory.GetFiles(hooksDir))
                {
                    RunCommand("chmod", $"+x \"{hook}\"");
                    AnsiConsole.MarkupLine($"[green]✅ Made executable:[/] [cyan]{Path.GetFileName(hook)}[/]");
                }
            }

            AnsiConsole.Write(new Rule().RuleStyle("grey"));
            AnsiConsole.MarkupLine("[green]✅ Git hooks configured successfully![/]");
            AnsiConsole.MarkupLine("[yellow]💡 All commits will now be validated for Conventional Commits format[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
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

    static int RunGitCommand(string command, params string[] args)
    {
        var repoRoot = FindGitRoot() ?? Directory.GetCurrentDirectory();
        var allArgs = string.Join(" ", args);
        return RunCommand("git", $"-C \"{repoRoot}\" {command} {allArgs}");
    }

    static int RunCommand(string command, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        process?.WaitForExit();
        return process?.ExitCode ?? -1;
    }
}
