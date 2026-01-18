#!/usr/bin/env dotnet run

#:package Spectre.Console

using System.Text.RegularExpressions;
using Spectre.Console;

#pragma warning disable IL2026 // RequiresUnreferencedCode
#pragma warning disable IL3050 // RequiresDynamicCode

/// <summary>
/// CI automation script to clean up formatting guides from issue descriptions.
/// Supports GitHub (preferred) and GitLab environments.
/// Removes the "Formatting Guide" section that developers may forget to delete.
/// 
/// Usage: dotnet scripts/CleanIssueDescription.cs
/// 
/// GitHub Environment Variables:
/// - GITHUB_TOKEN: GitHub token with repo scope
/// - GITHUB_REPOSITORY: "owner/repo"
/// - ISSUE_NUMBER: Issue number to process (provide via workflow input)
/// - GITHUB_API_URL: Optional (default https://api.github.com)
/// 
/// GitLab Environment Variables:
/// - CI_JOB_TOKEN: GitLab CI token
/// - CI_PROJECT_ID: GitLab project ID
/// - CI_API_V4_URL: GitLab API URL
/// - CI_ISSUE_IID: Issue ID (set by GitLab CI)
/// </summary>
class CleanIssueDescription
{
    static async Task Main(string[] args)
    {
        try
        {
            // Prefer GitHub if available
            var ghToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            var ghRepo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"); // owner/repo
            var ghApi = Environment.GetEnvironmentVariable("GITHUB_API_URL") ?? "https://api.github.com";
            var ghIssue = Environment.GetEnvironmentVariable("ISSUE_NUMBER");

            var glToken = Environment.GetEnvironmentVariable("CI_JOB_TOKEN");
            var glProjectId = Environment.GetEnvironmentVariable("CI_PROJECT_ID");
            var glApiUrl = Environment.GetEnvironmentVariable("CI_API_V4_URL");
            var glIssueIid = Environment.GetEnvironmentVariable("CI_ISSUE_IID");

            if (!string.IsNullOrEmpty(ghToken) && !string.IsNullOrEmpty(ghRepo) && !string.IsNullOrEmpty(ghIssue))
            {
                await ProcessGitHubIssue(ghToken, ghRepo, ghApi, ghIssue);
                return;
            }

            if (!string.IsNullOrEmpty(glToken) && !string.IsNullOrEmpty(glProjectId) && !string.IsNullOrEmpty(glIssueIid))
            {
                await ProcessGitLabIssue(glToken, glProjectId, glApiUrl!, glIssueIid);
                return;
            }

            AnsiConsole.MarkupLine("[yellow]⚠️  Skipping: No supported CI environment variables found[/]");
            return;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[blue]Processing Issue #{issueIid}...[/]", async ctx =>
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", token);

                    // Get issue details
                    ctx.Status("[blue]Fetching issue details...[/]");
                    var issueUrl = $"{apiUrl}/projects/{projectId}/issues/{issueIid}";
                    var response = await client.GetAsync(issueUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Failed to fetch issue: {response.StatusCode}[/]");
                        return;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var issue = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);

                    var description = issue.GetProperty("description").GetString() ?? "";
                    var cleanedDescription = CleanFormattingGuide(description);

                    if (cleanedDescription == description)
                    {
                        AnsiConsole.MarkupLine("[green]✅ No formatting guide found - nothing to clean[/]");
                        return;
                    }

                    // Update issue
                    ctx.Status("[blue]Updating issue description...[/]");
                    var updateUrl = $"{apiUrl}/projects/{projectId}/issues/{issueIid}";
                    var updateContent = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(new { description = cleanedDescription }),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    var updateResponse = await client.PutAsync(updateUrl, updateContent);

                    if (updateResponse.IsSuccessStatusCode)
                    {
                        AnsiConsole.MarkupLine("[green]✅ Successfully removed formatting guide from issue[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Failed to update issue: {updateResponse.StatusCode}[/]");
                    }
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Removes the "Formatting Guide" section and everything after it.
    /// </summary>
    static string CleanFormattingGuide(string description)
    {
        // Match "---" or newlines followed by "## 📚 Formatting Guide" and remove everything after
        var pattern = @"^---\s*\n##\s*📚\s*Formatting Guide[\s\S]*$|^\n---\s*\n##\s*📚\s*Formatting Guide[\s\S]*$";
        var cleaned = Regex.Replace(description, pattern, "", RegexOptions.Multiline).TrimEnd();

        return cleaned;
    }

    static async Task ProcessGitHubIssue(string token, string repo, string api, string issueNumber)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[blue]Processing GitHub Issue #{issueNumber}...[/]", async ctx =>
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "Viso.Tracker-Agent");

                // Get issue
                ctx.Status("[blue]Fetching issue details...[/]");
                var issueUrl = $"{api}/repos/{repo}/issues/{issueNumber}";
                var response = await client.GetAsync(issueUrl);
                if (!response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[red]❌ Failed to fetch issue: {response.StatusCode}[/]");
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var issue = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);
                var description = issue.GetProperty("body").GetString() ?? string.Empty;
                var cleaned = CleanFormattingGuide(description);

                if (cleaned == description)
                {
                    AnsiConsole.MarkupLine("[green]✅ No formatting guide found - nothing to clean[/]");
                    return;
                }

                // Patch issue
                ctx.Status("[blue]Updating issue description...[/]");
                var updateUrl = issueUrl;
                var payload = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { body = cleaned }),
                    System.Text.Encoding.UTF8,
                    "application/json");
                var patch = new HttpRequestMessage(new HttpMethod("PATCH"), updateUrl) { Content = payload };
                var updateResponse = await client.SendAsync(patch);
                if (updateResponse.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]✅ Successfully removed formatting guide from issue[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]❌ Failed to update issue: {updateResponse.StatusCode}[/]");
                }
            });
    }

    static async Task ProcessGitLabIssue(string token, string projectId, string apiUrl, string issueIid)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[blue]Processing GitLab Issue #{issueIid}...[/]", async ctx =>
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", token);

                // Get issue details
                ctx.Status("[blue]Fetching issue details...[/]");
                var issueUrl = $"{apiUrl}/projects/{projectId}/issues/{issueIid}";
                var response = await client.GetAsync(issueUrl);
                if (!response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[red]❌ Failed to fetch issue: {response.StatusCode}[/]");
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var issue = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);
                var description = issue.GetProperty("description").GetString() ?? string.Empty;
                var cleaned = CleanFormattingGuide(description);

                if (cleaned == description)
                {
                    AnsiConsole.MarkupLine("[green]✅ No formatting guide found - nothing to clean[/]");
                    return;
                }

                // Update issue
                ctx.Status("[blue]Updating issue description...[/]");
                var updateUrl = $"{apiUrl}/projects/{projectId}/issues/{issueIid}";
                var updateContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { description = cleaned }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var updateResponse = await client.PutAsync(updateUrl, updateContent);
                if (updateResponse.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]✅ Successfully removed formatting guide from issue[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]❌ Failed to update issue: {updateResponse.StatusCode}[/]");
                }
            });
    }
}
