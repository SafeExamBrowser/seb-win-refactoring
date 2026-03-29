# Requires -RunAsAdministrator

$msbuildDir = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin"

if (!(Test-Path "$msbuildDir\MSBuild.exe")) {
    Write-Error "MSBuild.exe not found at $msbuildDir! Please verify your Visual Studio installation path."
    return
}

$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")

if ($currentPath -notlike "*$msbuildDir*") {
    $newPath = "$currentPath;$msbuildDir"
    [Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
    Write-Host "SUCCESS: MSBuild added to System PATH: $msbuildDir" -ForegroundColor Green
    Write-Host "IMPORTANT: You MUST restart your terminal (or Visual Studio) for this to work." -ForegroundColor Yellow
} else {
    Write-Host "INFO: MSBuild is already in the System PATH." -ForegroundColor Cyan
}

# Also add to the current session so you can test immediately (only for this window)
$env:Path += ";$msbuildDir"
Write-Host "Current session updated. You can now type 'msbuild' to test." -ForegroundColor White
