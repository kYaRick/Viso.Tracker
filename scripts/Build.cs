#!/usr/bin/env dotnet

using System;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

// Viso.Tracker Build Script
// Run this from the project root with: dotnet scripts/Build.cs
// Or with GitHub Pages patch: dotnet scripts/Build.cs

// Robust path detection
var rootPath = Directory.GetCurrentDirectory();
var propsFile = "Directory.Build.props";

if (!File.Exists(Path.Combine(rootPath, propsFile)))
{
    rootPath = Path.GetFullPath(Path.Combine(rootPath, ".."));
}

if (!File.Exists(Path.Combine(rootPath, propsFile)))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[!] Error: Could not find {propsFile}");
    Console.ResetColor();
    Environment.Exit(1);
}

var versionPropsPath = Path.Combine(rootPath, propsFile);
var versionProps = XDocument.Load(versionPropsPath);
var version = versionProps.Descendants("Version").First().Value;

var projectFile = Path.Combine(rootPath, "srcs/Viso.Tracker/Viso.Tracker.csproj");
var configuration = "Release";
var outputPath = Path.Combine(rootPath, "publish");

Console.WriteLine("======================================");
Console.WriteLine($"   Viso.Tracker Build v{version}         ");
Console.WriteLine("======================================");

try
{
    Step("Restoring dependencies...", "dotnet", $"restore {projectFile}");
    Step("Building and Publishing project...", "dotnet", $"publish {projectFile} -c {configuration} -o {outputPath} -p:Version={version} --no-restore");

    var wwwroot = Path.Combine(outputPath, "wwwroot");

    Console.WriteLine("\n[*] Applying GitHub Pages configuration...");

    // 1. Add .nojekyll to allow folders starting with underscore (like _framework)
    File.WriteAllText(Path.Combine(wwwroot, ".nojekyll"), "");
    Console.WriteLine(" - [OK] Created .nojekyll");

    // 2. Patch index.html for GitHub Pages base href
    var indexPath = Path.Combine(wwwroot, "index.html");
    if (File.Exists(indexPath))
    {
        Console.WriteLine("\n[PATCH] Updating index.html for GitHub Pages...");
        var indexContent = File.ReadAllText(indexPath);

        var baseHrefPattern = @"<base\s+href=[""']\/[""']\s*\/?>";
        var newBaseHref = "<base href=\"/Viso.Tracker/\" />";

        if (Regex.IsMatch(indexContent, baseHrefPattern))
        {
            indexContent = Regex.Replace(indexContent, baseHrefPattern, newBaseHref);
            Console.WriteLine(" - [OK] Patched <base href> to /Viso.Tracker/");
        }

        File.WriteAllText(indexPath, indexContent);

        // Copy index.html to 404.html for SPA routing support
        File.Copy(indexPath, Path.Combine(wwwroot, "404.html"), true);
        Console.WriteLine(" - [OK] Created 404.html for SPA routing");
    }

    Console.WriteLine($"\n[SUCCESS] Build completed! Static site ready in '{wwwroot}/' folder.");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[FAILED] Build failed: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

void Step(string message, string command, string args)
{
    Console.WriteLine($"\n>> {message}");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"> {command} {args}");
    Console.ResetColor();

    var psi = new ProcessStartInfo
    {
        FileName = command,
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = false
    };

    using var process = Process.Start(psi);
    process?.WaitForExit();

    if (process?.ExitCode != 0)
    {
        throw new Exception($"Command '{command} {args}' exited with code {process?.ExitCode}");
    }
}
