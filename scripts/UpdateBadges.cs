using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Find root directory by looking for global.json
var currentDir = Directory.GetCurrentDirectory();
var rootPath = currentDir;

while (!File.Exists(Path.Combine(rootPath, "global.json")) && Directory.GetParent(rootPath) != null)
{
    rootPath = Directory.GetParent(rootPath)!.FullName;
}

var readmePath = Path.Combine(rootPath, "README.md");
var globalJsonPath = Path.Combine(rootPath, "global.json");
var packagesPropsPath = Path.Combine(rootPath, "Directory.Packages.props");
var buildPropsPath = Path.Combine(rootPath, "Directory.Build.props");

if (!File.Exists(readmePath))
{
    Console.WriteLine("❌ README.md not found!");
    return 1;
}

Console.WriteLine("🔄 Updating all badges in README.md...");

var readmeContent = File.ReadAllText(readmePath);
var originalContent = readmeContent;
var updated = false;

// 1. Update Version Badge from Directory.Build.props
if (File.Exists(buildPropsPath))
{
    try
    {
        var buildPropsXml = XDocument.Load(buildPropsPath);
        var versionElement = buildPropsXml.Descendants("Version").FirstOrDefault();

        if (versionElement != null)
        {
            var version = versionElement.Value;
            var escapedVersion = version.Replace("-", "--");
            var versionPattern = @"\[!\[Version\]\(https://img\.shields\.io/badge/Version-[^\)]+?-orange\)\](\([^\)]*\))?";
            var newBadge = $"[![Version](https://img.shields.io/badge/Version-{escapedVersion}-orange)](#)";

            if (Regex.IsMatch(readmeContent, versionPattern))
            {
                readmeContent = Regex.Replace(readmeContent, versionPattern, newBadge);
                Console.WriteLine($"✅ Version badge updated: {version}");
                updated = true;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Failed to read Version from Directory.Build.props: {ex.Message}");
    }
}

// 2. Update .NET Version Badge from global.json
if (File.Exists(globalJsonPath))
{
    try
    {
        var globalJson = JsonDocument.Parse(File.ReadAllText(globalJsonPath));
        if (globalJson.RootElement.TryGetProperty("sdk", out var sdk) &&
            sdk.TryGetProperty("version", out var versionElement))
        {
            var dotnetVersion = versionElement.GetString();
            var dotnetPattern = @"\[!\[\.NET\]\(https://img\.shields\.io/badge/\.NET-[^-]+-512BD4\)\](\([^\)]*\))?";
            var newBadge = $"[![.NET](https://img.shields.io/badge/.NET-{dotnetVersion}-512BD4)](#)";

            if (Regex.IsMatch(readmeContent, dotnetPattern))
            {
                readmeContent = Regex.Replace(readmeContent, dotnetPattern, newBadge);
                Console.WriteLine($"✅ .NET badge updated: {dotnetVersion}");
                updated = true;
            }
            else
            {
                // Add badge after Version badge if it doesn't exist
                var versionBadge = @"(\[!\[Version\]\([^\)]+\)\]\([^\)]*\))";
                if (Regex.IsMatch(readmeContent, versionBadge))
                {
                    readmeContent = Regex.Replace(readmeContent, versionBadge, $"$1\n{newBadge}");
                    Console.WriteLine($"✅ .NET badge added: {dotnetVersion}");
                    updated = true;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Failed to read .NET version from global.json: {ex.Message}");
    }
}

// 3. Update MudBlazor Version Badge from Directory.Packages.props
if (File.Exists(packagesPropsPath))
{
    try
    {
        var packagesXml = XDocument.Load(packagesPropsPath);
        var mudBlazorPackage = packagesXml.Descendants("PackageVersion")
            .FirstOrDefault(e => e.Attribute("Include")?.Value == "MudBlazor");

        if (mudBlazorPackage != null)
        {
            var mudBlazorVersion = mudBlazorPackage.Attribute("Version")?.Value;
            var mudBlazorPattern = @"\[!\[MudBlazor\]\(https://img\.shields\.io/badge/MudBlazor-[^-]+-594ae2\)\](\([^\)]*\))?";
            var newBadge = $"[![MudBlazor](https://img.shields.io/badge/MudBlazor-{mudBlazorVersion}-594ae2)](#)";

            if (Regex.IsMatch(readmeContent, mudBlazorPattern))
            {
                readmeContent = Regex.Replace(readmeContent, mudBlazorPattern, newBadge);
                Console.WriteLine($"✅ MudBlazor badge updated: {mudBlazorVersion}");
                updated = true;
            }
            else
            {
                // Add badge after .NET badge if it doesn't exist
                var dotnetBadge = @"(\[!\[\.NET\]\([^\)]+\)\]\([^\)]*\))";
                if (Regex.IsMatch(readmeContent, dotnetBadge))
                {
                    readmeContent = Regex.Replace(readmeContent, dotnetBadge, $"$1\n{newBadge}");
                    Console.WriteLine($"✅ MudBlazor badge added: {mudBlazorVersion}");
                    updated = true;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Failed to read MudBlazor version from Directory.Packages.props: {ex.Message}");
    }
}

// 4. Ensure GitHub static badges are present (add if missing)
var repoOwner = "kYaRick";
var repoName = "Viso.Tracker";

var staticBadges = new[]
{
    ($"[![Last Commit](https://img.shields.io/github/last-commit/{repoOwner}/{repoName})](https://github.com/{repoOwner}/{repoName}/commits/main)", "Last Commit"),
    ($"[![License](https://img.shields.io/github/license/{repoOwner}/{repoName})](LICENSE)", "License"),
    ($"[![Issues](https://img.shields.io/github/issues/{repoOwner}/{repoName})](https://github.com/{repoOwner}/{repoName}/issues)", "Issues"),
};

foreach (var (badge, name) in staticBadges)
{
    if (!readmeContent.Contains(badge))
    {
        // Add after MudBlazor badge (or last dynamic badge)
        var mudBlazorBadge = @"(\[!\[MudBlazor\]\([^\)]+\)\]\([^\)]*\))";
        var dotnetBadge = @"(\[!\[\.NET\]\([^\)]+\)\]\([^\)]*\))";
        var versionBadge = @"(\[!\[Version\]\([^\)]+\)\]\([^\)]*\))";

        if (Regex.IsMatch(readmeContent, mudBlazorBadge))
        {
            readmeContent = Regex.Replace(readmeContent, mudBlazorBadge, $"$1\n{badge}", RegexOptions.Multiline, TimeSpan.FromSeconds(1));
            Console.WriteLine($"✅ {name} badge added");
            updated = true;
        }
        else if (Regex.IsMatch(readmeContent, dotnetBadge))
        {
            readmeContent = Regex.Replace(readmeContent, dotnetBadge, $"$1\n{badge}", RegexOptions.Multiline, TimeSpan.FromSeconds(1));
            Console.WriteLine($"✅ {name} badge added");
            updated = true;
        }
        else if (Regex.IsMatch(readmeContent, versionBadge))
        {
            readmeContent = Regex.Replace(readmeContent, versionBadge, $"$1\n{badge}", RegexOptions.Multiline, TimeSpan.FromSeconds(1));
            Console.WriteLine($"✅ {name} badge added");
            updated = true;
        }
    }
}

if (updated)
{
    File.WriteAllText(readmePath, readmeContent);
    Console.WriteLine("✅ All badges updated successfully!");

    // Show diff summary
    var addedLines = readmeContent.Split('\n').Except(originalContent.Split('\n')).Count();
    var removedLines = originalContent.Split('\n').Except(readmeContent.Split('\n')).Count();

    if (addedLines > 0 || removedLines > 0)
    {
        Console.WriteLine($"   +{addedLines} -{removedLines} lines changed");
    }

    return 0;
}
else
{
    Console.WriteLine("⚠️  No badges needed updating.");
    return 0;
}
