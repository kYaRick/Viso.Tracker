#requires -Version 7.0

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ==========================================
# Viso.Tracker Bootstrap (PowerShell)
# Emoji-rich, color-coded interactive setup
# ==========================================

$RepoOwner = 'kYaRick'
$RepoName  = 'Viso.Tracker'
$DefaultRepoUrl = "https://github.com/$RepoOwner/$RepoName.git"
$DefaultCloneDir = $RepoName

function Write-Header {
    param([string]$Text)
    Write-Host "🚀 $Text" -ForegroundColor Cyan
}

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ️  $Text" -ForegroundColor Cyan
}

function Write-Warn {
    param([string]$Text)
    Write-Host "⚠️  $Text" -ForegroundColor Yellow
}

function Write-Ok {
    param([string]$Text)
    Write-Host "✅ $Text" -ForegroundColor Green
}

function Write-Err {
    param([string]$Text)
    Write-Host "❌ $Text" -ForegroundColor Red
}

function Ensure-Git {
    if (Get-Command git -ErrorAction SilentlyContinue) { return $true }
    Write-Warn 'Git not found. Will attempt ZIP download fallback.'
    return $false
}

function Clone-Repo {
    param(
        [string]$RepoUrl,
        [string]$TargetDir
    )

    if (Test-Path $TargetDir) {
        Write-Warn "Target directory '$TargetDir' already exists. Skipping clone."
        return
    }

    if (Ensure-Git) {
        Write-Header 'Cloning repository (git) 📦'
        git clone --depth 1 $RepoUrl $TargetDir
        Write-Ok "Cloned to '$TargetDir'"
        return
    }

    Write-Header 'Downloading repository ZIP 📦'
    $zipUrl = "https://github.com/$RepoOwner/$RepoName/archive/refs/heads/main.zip"
    $zipPath = Join-Path $PWD "$RepoName.zip"
    Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath
    $tmpDir = Join-Path $PWD "$RepoName-main"
    Expand-Archive -Path $zipPath -DestinationPath $PWD
    Remove-Item $zipPath -Force
    if (-not (Test-Path $tmpDir)) {
        Write-Err 'ZIP expansion failed.'
        throw 'ZIP expansion failed.'
    }
    Rename-Item -Path $tmpDir -NewName $TargetDir
    Write-Ok "Downloaded and prepared '$TargetDir'"
}

function Read-RequiredSdkVersion {
    param([string]$RepoDir)
    $globalJson = Join-Path $RepoDir 'global.json'
    if (-not (Test-Path $globalJson)) {
        Write-Err 'global.json not found; cannot determine required SDK.'
        throw 'global.json missing'
    }
    $json = Get-Content $globalJson -Raw | ConvertFrom-Json
    $version = $json.sdk.version
    if (-not $version) {
        Write-Err 'No sdk.version found in global.json.'
        throw 'sdk.version missing'
    }
    return $version
}

function Get-DotnetSdks {
    try {
        $out = & dotnet --list-sdks 2>$null
        if (-not $out) { return @() }
        return $out
    } catch {
        return @()
    }
}

function Has-RequiredSdk {
    param([string]$Required)
    $sdks = Get-DotnetSdks
    # Parse major.minor from required version (e.g., "10.0.0" -> "10.0")
    $requiredParts = $Required -split '\.'
    if ($requiredParts.Length -lt 2) { return $false }
    $requiredMajorMinor = "$($requiredParts[0]).$($requiredParts[1])"
    
    foreach ($line in $sdks) {
        # Lines like: 8.0.100 [C:\...] or 10.0.100-preview.1 [...]
        $ver = ($line -split '\s+')[0]
        # Check if installed SDK matches major.minor
        if ($ver -match "^$requiredMajorMinor\.") { return $true }
    }
    return $false
}

function Install-DotnetSdk {
    param([string]$Version)
    Write-Header "Installing .NET SDK $Version 🧰"

    $installDir = Join-Path $HOME '.dotnet'
    if (-not (Test-Path $installDir)) { New-Item -ItemType Directory -Path $installDir | Out-Null }

    $scriptUrl = 'https://dot.net/v1/dotnet-install.ps1'
    $scriptPath = Join-Path $PWD 'dotnet-install.ps1'

    try {
        Invoke-WebRequest -Uri $scriptUrl -OutFile $scriptPath
    } catch {
        Write-Err 'Failed to download dotnet-install.ps1.'
        throw
    }

    try {
        & $scriptPath -Version $Version -InstallDir $installDir
    } catch {
        Write-Err "dotnet-install.ps1 failed: $($_.Exception.Message)"
        throw
    } finally {
        if (Test-Path $scriptPath) { Remove-Item $scriptPath -Force }
    }

    # Update PATH for current session
    $env:DOTNET_ROOT = $installDir
    if (-not ($env:PATH -split ';' | Where-Object { $_ -eq $installDir })) {
        $env:PATH = "$installDir;$env:PATH"
    }

    Write-Ok ".NET SDK $Version installed to $installDir"
}

function Option-CloneOnly {
    param([string]$RepoUrl, [string]$TargetDir)
    Write-Header 'Option 1: Clone Only'
    Clone-Repo -RepoUrl $RepoUrl -TargetDir $TargetDir
    Write-Ok 'Done. You can now open the repo and run dotnet commands.'
}

function Option-CloneAndSetupSdk {
    param([string]$RepoUrl, [string]$TargetDir)
    Write-Header 'Option 2: Clone & Setup SDK'
    Clone-Repo -RepoUrl $RepoUrl -TargetDir $TargetDir
    $required = Read-RequiredSdkVersion -RepoDir $TargetDir
    Write-Info "Required SDK per global.json: $required"
    if (Has-RequiredSdk -Required $required) {
        Write-Ok "Required .NET SDK $required is already installed."
    } else {
        Install-DotnetSdk -Version $required
        if (Has-RequiredSdk -Required $required) {
            Write-Ok "Verified .NET SDK $required is available."
        } else {
            Write-Warn 'Could not verify SDK via dotnet --list-sdks. Ensure PATH includes $HOME/.dotnet.'
        }
    }
    Write-Ok 'Environment ready.'
}

function Show-Menu {
    Clear-Host
    Write-Header 'Viso.Tracker Setup'
    Write-Host 'Choose an option:' -ForegroundColor Cyan
    Write-Host '  1) 📦 Clone repository only'
    Write-Host '  2) 🧰 Clone repository and setup required .NET SDK'
    Write-Host '  3) ❎ Exit'
}

# --- Interactive flow ---
Show-Menu
$choice = Read-Host 'Enter choice (1-3)'
$repoUrl = Read-Host "Repository URL (default: $DefaultRepoUrl)"
if (-not $repoUrl) { $repoUrl = $DefaultRepoUrl }
$targetDir = Read-Host "Target folder (default: $DefaultCloneDir)"
if (-not $targetDir) { $targetDir = $DefaultCloneDir }

try {
    switch ($choice) {
        '1' { Option-CloneOnly -RepoUrl $repoUrl -TargetDir $targetDir }
        '2' { Option-CloneAndSetupSdk -RepoUrl $repoUrl -TargetDir $targetDir }
        default { Write-Info 'Bye!'; return }
    }

    Write-Host ''
    Write-Ok 'Next steps:'
    Write-Host '  - Open the folder in VS Code or run:' -ForegroundColor Green
    Write-Host "      cd '$targetDir'" -ForegroundColor Green
    Write-Host '      dotnet build' -ForegroundColor Green
} catch {
    Write-Err "Setup failed: $($_.Exception.Message)"
    throw
}
