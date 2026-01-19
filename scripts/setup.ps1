Param(
    # Parameters for non-interactive and custom setup:
    # --- Start of parameters ---
    [Parameter(HelpMessage="Repository URL")]
    [string]$Url = "https://github.com/kYaRick/Viso.Tracker.git",

    [Parameter(HelpMessage="Target directory name")]
    [string]$Target = "Viso.Tracker",

    [Parameter(HelpMessage="Action: 1-Clone Only, 2-Full Setup (Clone + SDK)")]
    [int]$Action = 0,

    [Parameter(HelpMessage=".NET Channel: STS, LTS, or Preview")]
    [string]$Channel = "STS",

    [Parameter(HelpMessage="Skip adding .NET to User PATH environment variable")]
    [switch]$SkipPath
    # --- End of parameters --
)

# Force strict mode and stop on errors
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Define local paths
$LocalDotnetDir = Join-Path $HOME ".dotnet"
$IsWin = $PSVersionTable.PSVersion.Major -le 5 -or $IsWindows
$DotnetExe = if ($IsWin) { "dotnet.exe" } else { "dotnet" }
$PathUpdated = $false

# --- Logging functions with emojis ---
function Write-Header { param([string]$Text) Write-Host "`n🚀 $Text" -ForegroundColor Cyan }
function Write-Info   { param([string]$Text) Write-Host "ℹ️  $Text" -ForegroundColor Gray }
function Write-Warn   { param([string]$Text) Write-Host "⚠️  $Text" -ForegroundColor Yellow }
function Write-Ok     { param([string]$Text) Write-Host "✅ $Text" -ForegroundColor Green }
function Write-Err    { param([string]$Text) Write-Host "❌ $Text" -ForegroundColor Red }

# --- Locate dotnet executable ---
function Get-DotnetCommand {
    $localPath = Join-Path $LocalDotnetDir $DotnetExe
    if (Test-Path $localPath) { return $localPath }
    $sysPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($sysPath) { return $sysPath.Definition }
    return $null
}

# --- Clone repository or download ZIP fallback ---
function Fetch-Repository {
    param([string]$RepoUrl, [string]$TargetDir)
    
    if (Test-Path $TargetDir) {
        Write-Warn "Target directory '$TargetDir' already exists. Skipping download."
        return
    }

    # Primary: Git Clone
    if (Get-Command git -ErrorAction SilentlyContinue) {
        Write-Header "Cloning repository via Git..."
        git clone --depth 1 $RepoUrl $TargetDir 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Ok "Successfully cloned to '$TargetDir'"
            return
        }
    }

    # Secondary: ZIP Fallback
    Write-Warn "Git not found or clone failed. Downloading ZIP..."
    $zipUrl = $RepoUrl.Replace(".git", "/archive/refs/heads/main.zip")
    $zipPath = Join-Path $env:TEMP "repo_archive.zip"
    
    Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath
    Write-Info "Extracting archive..."
    Expand-Archive -Path $zipPath -DestinationPath $PWD -Force
    
    $extractedDir = Get-ChildItem -Directory | Where-Object { $_.Name -like "*Viso.Tracker*" } | Select-Object -First 1
    if ($extractedDir) {
        Rename-Item -Path $extractedDir.FullName -NewName $TargetDir
    }
    
    Remove-Item $zipPath -Force
    Write-Ok "Repository prepared in '$TargetDir'"
}

# --- Install .NET SDK and register environment variables ---
function Setup-DotnetSdk {
    param([string]$RepoPath, [bool]$SkipPathRegistration)
    
    $globalJson = Join-Path $RepoPath "global.json"
    $targetVersion = "10.0" 
    
    if (Test-Path $globalJson) {
        try {
            $targetVersion = (Get-Content $globalJson -Raw | ConvertFrom-Json).sdk.version
        } catch {}
    }

    $scriptPath = Join-Path $env:TEMP 'dotnet-install.ps1'
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $scriptPath -UseBasicParsing
    
    $majorChannel = if ($targetVersion -match '^\d+\.\d+') { $Matches[0] } else { "10.0" }
    $installed = $false

    Write-Info "Searching for .NET SDK (Requested: $targetVersion)..."

    # Installation sequence: Channel -> Specific Version -> LTS fallback
    $attempts = @(
        @{ Channel = $majorChannel },
        @{ Version = $targetVersion },
        @{ Channel = "LTS" }
    )

    foreach ($opt in $attempts) {
        if (-not $installed) {
            if ($opt.Channel) {
                & powershell -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Channel $opt.Channel -InstallDir $LocalDotnetDir *>$null
            } else {
                & powershell -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Version $opt.Version -InstallDir $LocalDotnetDir *>$null
            }
            if ($LASTEXITCODE -eq 0) { $installed = $true }
        }
    }

    if ($installed) {
        # Set environment for current process
        $env:DOTNET_ROOT = $LocalDotnetDir
        $env:PATH = "$LocalDotnetDir;" + $env:PATH
        
        # Permanent registration in User PATH
        if (-not $SkipPathRegistration) {
            try {
                $uPath = [Environment]::GetEnvironmentVariable("Path", "User")
                if ($uPath -notlike "*$LocalDotnetDir*") {
                    [Environment]::SetEnvironmentVariable("Path", "$uPath;$LocalDotnetDir", "User")
                    $script:PathUpdated = $true
                    Write-Ok "Registered SDK in User PATH permanently."
                }
            } catch {
                Write-Warn "Failed to update PATH. Add $LocalDotnetDir manually."
            }
        }

        $actualVer = & (Join-Path $LocalDotnetDir $DotnetExe) --version
        Write-Ok "Success. SDK Ready: $actualVer"
    } else {
        Write-Err "Critical failure: Could not install .NET SDK."
        exit 1
    }

    if (Test-Path $scriptPath) { Remove-Item $scriptPath -Force }
}

# --- MAIN EXECUTION ---
try {
    $currentAction = $Action

    if ($currentAction -eq 0) {
        Clear-Host
        Write-Host "====================================" -ForegroundColor Cyan
        Write-Host "      Viso.Tracker Setup Tool" -ForegroundColor Cyan
        Write-Host "====================================" -ForegroundColor Cyan
        Write-Host " 1) Clone/Download Code only"
        Write-Host " 2) Full Setup (Code + .NET SDK)"
        Write-Host " 3) Exit"
        
        $choice = Read-Host "`nSelect an option"
        if ($choice -match '^[1-3]$') {
            $currentAction = [int]$choice
        } else {
            Write-Err "Invalid selection."; exit 1
        }
    }

    if ($currentAction -eq 3) { Write-Ok "Goodbye!"; exit 0 }

    Fetch-Repository -RepoUrl $Url -TargetDir $Target

    if ($currentAction -eq 2) {
        Setup-DotnetSdk -RepoPath $Target -SkipPathRegistration $SkipPath
    }

    Write-Header "SETUP FINISHED SUCCESSFULLY"
    Write-Info "Project folder: $Target"

} catch {
    Write-Err "Critical failure: $($_.Exception.Message)"
} finally {
    if ($PathUpdated) {
        Write-Host "`n⚠️  IMPORTANT: RESTART your terminal/IDE to apply PATH changes." -ForegroundColor Yellow
    }
    Write-Host "Press any key to close this terminal..." -ForegroundColor Gray
    $null = [Console]::ReadKey($true)
    exit
}
