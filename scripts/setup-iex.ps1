$url = "https://raw.githubusercontent.com/kYaRick/Viso.Tracker/main/scripts/setup.ps1"
$urlWithNoCache = $url + "?v=" + (Get-Date).Ticks
$rawCode = Invoke-RestMethod -Uri $urlWithNoCache -UseBasicParsing


if ($rawCode -match "(?s)Param\s*\(.*") {
    $cleanCode = $Matches[0]
} else {
    $cleanCode = $rawCode
}

try {
    $script = [scriptblock]::Create($cleanCode)
    & $script
} catch {
    Write-Host "❌ Error creating script block: $($_.Exception.Message)" -ForegroundColor Red
}
