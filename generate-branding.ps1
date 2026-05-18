Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = "Stop"
$srcLogo = [System.Drawing.Image]::FromFile("D:\seb-win-refactoring\XobinLogo.png")
Write-Host "Source logo loaded: $($srcLogo.Width)x$($srcLogo.Height)"

function Resize-Image([System.Drawing.Image]$src, [int]$w, [int]$h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $bmp.SetResolution(96, 96)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    
    $srcRatio = $src.Width / $src.Height
    $dstRatio = $w / $h
    if ($srcRatio -gt $dstRatio) {
        $drawW = $w
        $drawH = [int]($w / $srcRatio)
        $drawX = 0
        $drawY = [int](($h - $drawH) / 2)
    } else {
        $drawH = $h
        $drawW = [int]($h * $srcRatio)
        $drawX = [int](($w - $drawW) / 2)
        $drawY = 0
    }
    $g.DrawImage($src, $drawX, $drawY, $drawW, $drawH)
    $g.Dispose()
    return $bmp
}

function Create-IcoFile([System.Drawing.Image]$src, [string]$path) {
    $sizes = @(16, 24, 32, 48, 64, 128, 256)
    $pngDataList = @()
    
    foreach ($size in $sizes) {
        $bmp = Resize-Image $src $size $size
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngDataList += ,($ms.ToArray())
        $ms.Dispose()
        $bmp.Dispose()
    }
    
    $headerSize = 6
    $dirEntrySize = 16
    $dataOffset = $headerSize + ($sizes.Count * $dirEntrySize)
    
    $fs = [System.IO.File]::Create($path)
    $bw = New-Object System.IO.BinaryWriter($fs)
    
    # ICO header
    $bw.Write([uint16]0)       # reserved
    $bw.Write([uint16]1)       # type: icon
    $bw.Write([uint16]$sizes.Count)
    
    $currentOffset = $dataOffset
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $size = $sizes[$i]
        $data = $pngDataList[$i]
        
        $bw.Write([byte]$(if ($size -ge 256) { 0 } else { $size }))  # width
        $bw.Write([byte]$(if ($size -ge 256) { 0 } else { $size }))  # height
        $bw.Write([byte]0)     # color palette
        $bw.Write([byte]0)     # reserved
        $bw.Write([uint16]1)   # color planes
        $bw.Write([uint16]32)  # bits per pixel
        $bw.Write([uint32]$data.Length)
        $bw.Write([uint32]$currentOffset)
        
        $currentOffset += $data.Length
    }
    
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $bw.Write($pngDataList[$i])
    }
    
    $bw.Flush()
    $bw.Close()
    $fs.Close()
    Write-Host "Created ICO: $path ($([System.IO.FileInfo]::new($path).Length) bytes)"
}

function Create-SplashScreen([System.Drawing.Image]$logo, [string]$path) {
    $w = 800; $h = 450
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $bmp.SetResolution(96, 96)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::White)
    
    # Draw logo on the left-center area
    $logoSize = 140
    $logoResized = Resize-Image $logo $logoSize $logoSize
    $logoX = 160
    $logoY = [int](($h - $logoSize) / 2) - 30
    $g.DrawImage($logoResized, $logoX, $logoY, $logoSize, $logoSize)
    
    # Draw "Xolock" text below/right of logo
    $fontFamily = "Segoe UI"
    $titleFont = New-Object System.Drawing.Font($fontFamily, 48, [System.Drawing.FontStyle]::Bold)
    $titleBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(51, 51, 51))
    $titleSize = $g.MeasureString("Xolock", $titleFont)
    $titleX = $logoX + $logoSize + 30
    $titleY = [int](($h - $titleSize.Height) / 2) - 30
    $g.DrawString("Xolock", $titleFont, $titleBrush, $titleX, $titleY)
    
    # Subtitle
    $subFont = New-Object System.Drawing.Font($fontFamily, 14, [System.Drawing.FontStyle]::Regular)
    $subBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 120, 120))
    $subText = "by Xobin Technologies Pvt. Ltd."
    $subSize = $g.MeasureString($subText, $subFont)
    $subX = $titleX
    $subY = $titleY + $titleSize.Height + 5
    $g.DrawString($subText, $subFont, $subBrush, $subX, $subY)
    
    # Bottom line accent
    $accentBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 120, 212))
    $g.FillRectangle($accentBrush, 0, $h - 4, $w, 4)
    
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $logoResized.Dispose()
    $titleFont.Dispose(); $titleBrush.Dispose()
    $subFont.Dispose(); $subBrush.Dispose(); $accentBrush.Dispose()
    Write-Host "Created Splash: $path"
}

function Create-BannerBmp([System.Drawing.Image]$logo, [string]$path) {
    $w = 493; $h = 58
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $bmp.SetResolution(96, 96)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::White)
    
    $logoH = 40
    $logoResized = Resize-Image $logo ([int]($logoH * ($logo.Width / $logo.Height))) $logoH
    $g.DrawImage($logoResized, 10, [int](($h - $logoH) / 2), $logoResized.Width, $logoH)
    
    $font = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(51, 51, 51))
    $g.DrawString("Xolock", $font, $brush, $logoResized.Width + 18, [int](($h - $font.GetHeight($g)) / 2))
    
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Bmp)
    $bmp.Dispose(); $logoResized.Dispose(); $font.Dispose(); $brush.Dispose()
    Write-Host "Created Banner BMP: $path"
}

function Create-DialogBmp([System.Drawing.Image]$logo, [string]$path) {
    $w = 493; $h = 312
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $bmp.SetResolution(96, 96)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::White)
    
    $logoSize = 120
    $logoResized = Resize-Image $logo $logoSize $logoSize
    $logoX = [int](($w - $logoSize) / 2)
    $logoY = [int](($h - $logoSize) / 2) - 40
    $g.DrawImage($logoResized, $logoX, $logoY, $logoSize, $logoSize)
    
    $font = New-Object System.Drawing.Font("Segoe UI", 28, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(51, 51, 51))
    $textSize = $g.MeasureString("Xolock", $font)
    $g.DrawString("Xolock", $font, $brush, [int](($w - $textSize.Width) / 2), $logoY + $logoSize + 15)
    
    $subFont = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Regular)
    $subBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 120, 120))
    $subText = "by Xobin Technologies Pvt. Ltd."
    $subSize = $g.MeasureString($subText, $subFont)
    $g.DrawString($subText, $subFont, $subBrush, [int](($w - $subSize.Width) / 2), $logoY + $logoSize + 15 + $textSize.Height + 5)
    
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Bmp)
    $bmp.Dispose(); $logoResized.Dispose()
    $font.Dispose(); $brush.Dispose(); $subFont.Dispose(); $subBrush.Dispose()
    Write-Host "Created Dialog BMP: $path"
}

# === Generate all ICO files ===
Write-Host "`n=== Generating ICO files ==="
$icoTargets = @(
    "D:\seb-win-refactoring\SafeExamBrowser.Client\SafeExamBrowser.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.Runtime\SafeExamBrowser.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.Service\SafeExamBrowser.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Desktop\Images\SafeExamBrowser.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Desktop\Images\LogNotification.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Mobile\Images\SafeExamBrowser.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Mobile\Images\LogNotification.ico",
    "D:\seb-win-refactoring\SafeExamBrowser.ResetUtility\ResetUtility.ico",
    "D:\seb-win-refactoring\SebWindowsConfig\ConfigurationTool.ico",
    "D:\seb-win-refactoring\Setup\Resources\Application.ico",
    "D:\seb-win-refactoring\Setup\Resources\ConfigurationFile.ico",
    "D:\seb-win-refactoring\Setup\Resources\ConfigurationTool.ico",
    "D:\seb-win-refactoring\Setup\Resources\ResetUtility.ico"
)

foreach ($target in $icoTargets) {
    $dir = [System.IO.Path]::GetDirectoryName($target)
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Create-IcoFile $srcLogo $target
}

# === Generate PNG splash screens ===
Write-Host "`n=== Generating Splash Screen PNGs ==="
Create-SplashScreen $srcLogo "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Desktop\Images\SplashScreen.png"
Create-SplashScreen $srcLogo "D:\seb-win-refactoring\SafeExamBrowser.UserInterface.Mobile\Images\SplashScreen.png"

# === Generate Logo PNG (64x64) ===
Write-Host "`n=== Generating Logo PNG ==="
$logo64 = Resize-Image $srcLogo 64 64
$logo64.Save("D:\seb-win-refactoring\SetupBundle\Resources\Logo.png", [System.Drawing.Imaging.ImageFormat]::Png)
$logo64.Dispose()
Write-Host "Created Logo PNG: D:\seb-win-refactoring\SetupBundle\Resources\Logo.png"

# === Generate BMP files ===
Write-Host "`n=== Generating BMP files ==="
Create-BannerBmp $srcLogo "D:\seb-win-refactoring\Setup\Resources\Banner.bmp"
Create-DialogBmp $srcLogo "D:\seb-win-refactoring\Setup\Resources\Dialog.bmp"

$srcLogo.Dispose()

Write-Host "`n=== All assets generated successfully! ==="
