Add-Type -AssemblyName System.Drawing

$assets = ".\StorePackage\Assets"

function Resize-TrimmedPng {
    param(
        [string]$InputFile,
        [int]$CanvasWidth,
        [int]$CanvasHeight,
        [double]$Fill = 0.88
    )

    $inputPath = Join-Path $assets $InputFile
    $backupPath = "$inputPath.backup"

    Write-Host ""
    Write-Host "Processing $InputFile ..."

    if (!(Test-Path $inputPath)) {
        Write-Host "  SKIPPED - file not found"
        return
    }

    # Backup original once
    if (!(Test-Path $backupPath)) {
        Copy-Item $inputPath $backupPath
    }

    $source = [System.Drawing.Bitmap]::new($inputPath)

    # Find non-transparent bounds
    $minX = $source.Width
    $minY = $source.Height
    $maxX = -1
    $maxY = -1

    for ($y = 0; $y -lt $source.Height; $y++) {
        for ($x = 0; $x -lt $source.Width; $x++) {

            $pixel = $source.GetPixel($x, $y)

            if ($pixel.A -gt 8) {
                if ($x -lt $minX) { $minX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }

    if ($maxX -lt 0) {
        Write-Host "  SKIPPED - no visible artwork found"
        $source.Dispose()
        return
    }

    $cropWidth = $maxX - $minX + 1
    $cropHeight = $maxY - $minY + 1

    Write-Host "  Original: $($source.Width)x$($source.Height)"
    Write-Host "  Artwork:  $cropWidth x $cropHeight"

    # Crop artwork
    $cropped = $source.Clone(
        [System.Drawing.Rectangle]::new(
            $minX,
            $minY,
            $cropWidth,
            $cropHeight
        ),
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
    )

    $source.Dispose()

    # Leave a small margin
    $targetWidth = [int]($CanvasWidth * $Fill)
    $targetHeight = [int]($CanvasHeight * $Fill)

    # Preserve aspect ratio
    $scale = [Math]::Min(
        $targetWidth / [double]$cropWidth,
        $targetHeight / [double]$cropHeight
    )

    $newWidth = [Math]::Max(1, [int]($cropWidth * $scale))
    $newHeight = [Math]::Max(1, [int]($cropHeight * $scale))

    $result = [System.Drawing.Bitmap]::new(
        $CanvasWidth,
        $CanvasHeight,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
    )

    $graphics = [System.Drawing.Graphics]::FromImage($result)

    $graphics.Clear([System.Drawing.Color]::Transparent)

    $graphics.InterpolationMode =
        [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

    $graphics.PixelOffsetMode =
        [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $graphics.SmoothingMode =
        [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

    $graphics.CompositingQuality =
        [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    $x = [int](($CanvasWidth - $newWidth) / 2)
    $y = [int](($CanvasHeight - $newHeight) / 2)

    $graphics.DrawImage(
        $cropped,
        $x,
        $y,
        $newWidth,
        $newHeight
    )

    $graphics.Dispose()
    $cropped.Dispose()

    $result.Save(
        $inputPath,
        [System.Drawing.Imaging.ImageFormat]::Png
    )

    $result.Dispose()

    Write-Host "  New artwork: $newWidth x $newHeight"
    Write-Host "  Canvas:      $CanvasWidth x $CanvasHeight"
    Write-Host "  DONE"
}


# ============================================================
# APP LIST
# ============================================================

Resize-TrimmedPng `
    "AppList.png" `
    44 `
    44 `
    0.88

Resize-TrimmedPng `
    "AppList.scale-200.png" `
    88 `
    88 `
    0.88

Resize-TrimmedPng `
    "AppList.targetsize-24_altform-unplated.png" `
    24 `
    24 `
    0.88


# ============================================================
# MAIN TILE
# ============================================================

Resize-TrimmedPng `
    "MedTile.png" `
    150 `
    150 `
    0.90

Resize-TrimmedPng `
    "MedTile.scale-200.png" `
    300 `
    300 `
    0.90


# ============================================================
# STORE LOGO
# ============================================================

Resize-TrimmedPng `
    "StoreLogo.png" `
    50 `
    50 `
    0.88

Resize-TrimmedPng `
    "StoreLogo.scale-200.png" `
    100 `
    100 `
    0.88


# ============================================================
# WIDE TILE
# ============================================================

Resize-TrimmedPng `
    "WideTile.png" `
    310 `
    150 `
    0.82

Resize-TrimmedPng `
    "WideTile.scale-200.png" `
    620 `
    300 `
    0.82


Write-Host ""
Write-Host "=============================================="
Write-Host "LuckyCharm icon assets updated."
Write-Host "Original files have .backup copies."
Write-Host "=============================================="