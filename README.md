# Viso.Tracker ⏱️

[![Version](https://img.shields.io/badge/Version-0.1.0--alpha-orange)](#)
[![.NET](https://img.shields.io/badge/.NET-10.0.0-512BD4)](#)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-8.15.0-594ae2)](#)

Minimal, fast time tracking built with Blazor WebAssembly and MudBlazor. Capture time and review grouped history with daily and grand totals.

> [!IMPORTANT]
> UI is implemented with MudBlazor components only, with CSS isolation. The project targets AOT publish and aggressive trimming.

![Viso Tracker Repository Logo](./docs/assets/Viso.Tracker.Repo.Logo.png)

## ✨ Features

- 📅 Date picker defaults to today
- 🗂️ Project dropdown (e.g., Viso Internal, Client A, Client B, Personal Development)
- ⏳ Hours numeric input and 📝 work description textarea
- 💾 Save button to add entries
- 📋 History grouped by date: Date | Project | Hours | Description
- ➕ Daily totals and a grand total across all entries

## 🗺️ Roadmap

### Current & Upcoming

- [x] Implement core UI and local data model (in-memory)
- [x] Theme toggle (light/dark mode with custom green palette)
- [x] Setup script for automated environment preparation`
- [ ] Delete entries functionality with confirmation dialog
- [ ] SQLite WASM persistence (full TZ requirements)
- [ ] Add persistence layer (IndexedDB by default; SQLite WASM for full compliance; or external backend)
- [ ] Write unit and bUnit tests; integrate C# build scripts
- [ ] Optimize for AOT and trimming

## 🖼️ Demo

![Viso Tracker Demo](./docs/assets/Viso.Tracker.Demo.gif)

## 🧭 Spec Alignment

This implementation follows SPEC-0001:

- Scope: Time Entry form + grouped history with totals
- Components: MudDatePicker, MudSelect, MudNumericField/MudTextField, MudButton, MudTable
- Accessibility: WCAG 2.2 AA (labels, aria, focus, contrast, error states)

## 🧰 Tech Stack

- Runtime: .NET 10 (Blazor WebAssembly Standalone)
- Language: C# 14 with nullable reference types
- UI: MudBlazor (latest)
- Styling: Modern vanilla CSS with CSS isolation; no SCSS/Sass
- Testing: xUnit, bUnit, NSubstitute, Shouldly
- Automation: C#-based scripts executed via `dotnet run`

## 🚀 Getting Started

Prerequisites: .NET SDK 10

> [!TIP]
> Initial scaffolding is next; once created under `srcs/`, run the app locally with `dotnet run` and follow SPEC-0001 to validate behavior.

Suggested dev flow:

1) Create the Blazor WASM app under `srcs/` and add MudBlazor
2) Implement the Time Entry form and grouped history
3) Add tests (xUnit/bUnit) and C# automation in `scripts/`

### ⚡ Quick Start (Bootstrap Script)

Use the one-liner to clone and optionally set up the required .NET SDK:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
irm https://raw.githubusercontent.com/kYaRick/Viso.Tracker/main/scripts/setup.ps1 | iex
```

This launches an interactive menu:

## 🗄️ Persistence Options

> [!WARNING]
> GitHub Pages is static hosting. Server-side databases are not available.

- ✅ Default (recommended): IndexedDB in-browser storage (offline-capable, minimal bundle impact)
- 🔁 Optional: SQLite WASM in-browser (OPFS/IndexedDB). Heavier bundle and integration; use only if SQL is required
- 🌐 Alternative: External backend API (ASP.NET Core + SQL) hosted separately; front-end remains static on GitHub Pages

## 🌐 Deployment (GitHub Pages)

- Set the Blazor `index.html` base href to your repo subpath (e.g., `/Viso.Tracker/`)
- Publish and copy the generated `wwwroot` to `docs/` on the `main` branch
- Configure Pages: Source → Deploy from a branch → Branch `main` → Folder `/docs`

> [!CAUTION]
> Verify all assets load under the repository subpath; mismatched base href breaks routing and static files.

## 🔧 Automation Scripts

All automation is C#-based and executable via `dotnet run`. Available scripts in `scripts/`:

### Update All Badges
Automatically updates all badges in README.md:
- **Version** from `Directory.Build.props`
- **.NET** from `global.json`
- **MudBlazor** from `Directory.Packages.props`
- **GitHub badges** (Last Commit, License, Issues) - auto-generated links

```powershell
dotnet run scripts/UpdateBadges.cs
```

> [!TIP]
> Run this script after changing versions or before committing to keep badges up-to-date.

### Update Version Badge (Legacy)
Extracts version from `Directory.Build.props` and updates the version badge in README.md:
```powershell
dotnet run scripts/UpdateVersionBadge.cs
```
> [!NOTE]
> This is now included in `UpdateBadges.cs`. Use the unified script instead.

### Publish to GitHub Pages

**Option 1: GitHub Action (Recommended)**  
Manually trigger workflow via GitHub UI:
1. Go to **Actions** tab on GitHub
2. Select **"Publish to GitHub Pages"** workflow
3. Click **"Run workflow"** → **"Run workflow"**
4. Wait for completion and check `docs/` folder

**Option 2: Local Script**  
Builds Release version, publishes wwwroot to `docs/` folder:
```powershell
dotnet run scripts/PublishGitHubPages.cs
```
> [!TIP]
> After running locally, review changes with `git status`, stage with `git add docs/`, commit, and push to trigger Pages deployment.

### Validate Commit Messages
Ensures commit messages follow Conventional Commits format:
```powershell
dotnet run scripts/ValidateCommitMessage.cs --commit-format
```

### Setup Git Hooks
Installs pre-commit hooks to auto-validate commit messages:
```powershell
dotnet run scripts/SetupGitHooks.cs
# or
dotnet run scripts/InstallGitHooks.cs
```

## 🧑‍💻 Contributing & Commits

> [!IMPORTANT]
> All commit messages must follow Conventional Commits (v1.0.0).
>
> Format: `<type>(<scope>): <description>`
>
> Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`, `wip` (use sparingly)
>
> Rules: lowercase `type`, imperative present (`add` not `added`), no period at the end.
>
> [!TIP]
> Install hooks to enforce validation:
> ```powershell
> dotnet scripts/SetupGitHooks.cs
> # or
> dotnet scripts/InstallGitHooks.cs
> ```
>
> View format help:
> ```powershell
> dotnet scripts/ValidateCommitMessage.cs --commit-format
> ```

—
Made with ❤️ for 🇺🇦
