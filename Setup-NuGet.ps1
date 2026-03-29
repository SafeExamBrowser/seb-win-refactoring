# Requires -RunAsAdministrator

$destDir = "C:\Tools\NuGet"
$destPath = Join-Path $destDir "nuget.exe"

if (!(Test-Path $destDir)) {
    New-Item -Path $destDir -ItemType Directory | Out-Null
    Write-Host "Created directory: $destDir" -ForegroundColor Cyan
}

if (!(Test-Path $destPath)) {
    Write-Host "Downloading latest nuget.exe..." -ForegroundColor Cyan
    $url = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    Invoke-WebRequest -Uri $url -OutFile $destPath
    Write-Host "SUCCESS: nuget.exe downloaded to $destPath" -ForegroundColor Green
} else {
    Write-Host "INFO: nuget.exe already exists at $destPath" -ForegroundColor Cyan
}

$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
if ($currentPath -notlike "*$destDir*") {
    $newPath = "$currentPath;$destDir"
    [Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
    Write-Host "SUCCESS: NuGet added to System PATH: $destDir" -ForegroundColor Green
    Write-Host "Please restart your terminal/IDE for changes to take effect." -ForegroundColor Yellow
} else {
    Write-Host "INFO: NuGet folder is already in the System PATH." -ForegroundColor Cyan
}
