$ErrorActionPreference = "Stop"

# ============================================================
# LuckyCharm - Dangle Image Optimizer
#
# Input:
#   E:\projects\LuckyDangle\Assets\Dangles
#
# Goal:
#   Maximum 250 KB per PNG
#
# Strategy:
#   1. Preserve original aspect ratio
#   2. Preserve transparency
#   3. Strip unnecessary metadata
#   4. Apply maximum PNG compression
#   5. If still > 250 KB, gradually reduce resolution
#   6. Never distort the image
#   7. Keep the highest possible resolution under the limit
#
# Requires:
#   ImageMagick ("magick" command)
# ============================================================

$DangleFolder = "E:\projects\LuckyDangle\Assets\Dangles"

$MaxBytes = 250KB

$BackupFolder = Join-Path $DangleFolder "_originals"

$TempFolder = Join-Path $DangleFolder "_optimized_temp"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "       LuckyCharm Dangle Image Optimizer" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# Check ImageMagick
# ============================================================

if (-not (Get-Command magick -ErrorAction SilentlyContinue)) {

    Write-Host "ERROR: ImageMagick was not found." -ForegroundColor Red
    Write-Host ""
    Write-Host "Install ImageMagick and make sure 'magick' works in PowerShell."
    Write-Host ""
    exit 1
}

# ============================================================
# Check source folder
# ============================================================

if (-not (Test-Path $DangleFolder)) {

    Write-Host "ERROR: Dangle folder not found:" -ForegroundColor Red
    Write-Host $DangleFolder
    exit 1
}

# ============================================================
# Create backup/temp folders
# ============================================================

if (-not (Test-Path $BackupFolder)) {

    New-Item `
        -ItemType Directory `
        -Path $BackupFolder `
        -Force |
        Out-Null
}

if (Test-Path $TempFolder) {

    Remove-Item `
        $TempFolder `
        -Recurse `
        -Force
}

New-Item `
    -ItemType Directory `
    -Path $TempFolder `
    -Force |
    Out-Null

# ============================================================
# Find PNG files
# ============================================================

$Images = Get-ChildItem `
    -Path $DangleFolder `
    -Filter "*.png" `
    -File

if ($Images.Count -eq 0) {

    Write-Host "No PNG files found." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($Images.Count) PNG files."
Write-Host ""

# ============================================================
# Process each image
# ============================================================

foreach ($Image in $Images) {

    Write-Host "------------------------------------------------" `
        -ForegroundColor DarkGray

    Write-Host "Processing: $($Image.Name)" `
        -ForegroundColor Cyan

    $OriginalSize = $Image.Length

    Write-Host "Original size : $([math]::Round($OriginalSize / 1KB, 1)) KB"

    # --------------------------------------------------------
    # Backup original
    # --------------------------------------------------------

    $BackupPath = Join-Path `
        $BackupFolder `
        $Image.Name

    if (-not (Test-Path $BackupPath)) {

        Copy-Item `
            $Image.FullName `
            $BackupPath `
            -Force

        Write-Host "Original backed up." `
            -ForegroundColor DarkGray
    }

    # --------------------------------------------------------
    # Temporary working file
    # --------------------------------------------------------

    $TempPath = Join-Path `
        $TempFolder `
        $Image.Name

    # --------------------------------------------------------
    # First pass:
    # Lossless optimization / metadata stripping
    #
    # No resizing yet.
    # --------------------------------------------------------

    & magick `
        $Image.FullName `
        -strip `
        -define png:compression-level=9 `
        -define png:compression-filter=5 `
        -define png:compression-strategy=1 `
        $TempPath

    if ($LASTEXITCODE -ne 0) {

        Write-Host "ImageMagick failed." -ForegroundColor Red
        continue
    }

    $CurrentSize = (Get-Item $TempPath).Length

    # --------------------------------------------------------
    # If still too large:
    # gradually reduce resolution.
    #
    # Aspect ratio is ALWAYS preserved.
    # --------------------------------------------------------

    $Scale = 100

    while ($CurrentSize -gt $MaxBytes -and $Scale -gt 25) {

        $Scale -= 5

        Write-Host `
            "Still $([math]::Round($CurrentSize / 1KB, 1)) KB -> trying ${Scale}%..."

        & magick `
            $Image.FullName `
            -strip `
            -resize "${Scale}%" `
            -filter Lanczos `
            -define png:compression-level=9 `
            -define png:compression-filter=5 `
            -define png:compression-strategy=1 `
            $TempPath

        if ($LASTEXITCODE -ne 0) {

            Write-Host "Resize failed." -ForegroundColor Red
            break
        }

        $CurrentSize = (Get-Item $TempPath).Length
    }

    # --------------------------------------------------------
    # Final check
    # --------------------------------------------------------

    if (-not (Test-Path $TempPath)) {

        Write-Host "No optimized image produced." `
            -ForegroundColor Red

        continue
    }

    $FinalSize = (Get-Item $TempPath).Length

    if ($FinalSize -gt $MaxBytes) {

        Write-Host ""
        Write-Host "WARNING: Could not reach 250 KB." `
            -ForegroundColor Red

        Write-Host `
            "Best result: $([math]::Round($FinalSize / 1KB, 1)) KB"

        Write-Host "Original image has NOT been replaced."

        continue
    }

    # --------------------------------------------------------
    # Replace original with optimized version
    # --------------------------------------------------------

    Copy-Item `
        $TempPath `
        $Image.FullName `
        -Force

    # --------------------------------------------------------
    # Get final dimensions
    # --------------------------------------------------------

    $IdentifyOutput = & magick identify `
        -format "%wx%h" `
        $Image.FullName

    $Reduction =
        [math]::Round(
            (1 - ($FinalSize / $OriginalSize)) * 100,
            1
        )

    Write-Host ""
    Write-Host "Optimized successfully." `
        -ForegroundColor Green

    Write-Host "Final size    : $([math]::Round($FinalSize / 1KB, 1)) KB"
    Write-Host "Dimensions    : $IdentifyOutput"
    Write-Host "Reduction     : $Reduction%"
    Write-Host "Resolution    : ${Scale}% of original"
}

# ============================================================
# Cleanup
# ============================================================

if (Test-Path $TempFolder) {

    Remove-Item `
        $TempFolder `
        -Recurse `
        -Force
}

# ============================================================
# Final report
# ============================================================

Write-Host ""
Write-Host "================================================" `
    -ForegroundColor Green

Write-Host "          OPTIMIZATION COMPLETE" `
    -ForegroundColor Green

Write-Host "================================================" `
    -ForegroundColor Green

Write-Host ""
Write-Host "Maximum allowed: 250 KB"
Write-Host ""

$FinalImages = Get-ChildItem `
    -Path $DangleFolder `
    -Filter "*.png" `
    -File

foreach ($File in $FinalImages) {

    $SizeKB = [math]::Round(
        $File.Length / 1KB,
        1
    )

    if ($File.Length -le $MaxBytes) {

        Write-Host `
            ("{0,-45} {1,8} KB   OK" -f $File.Name, $SizeKB) `
            -ForegroundColor Green
    }
    else {

        Write-Host `
            ("{0,-45} {1,8} KB   TOO LARGE" -f $File.Name, $SizeKB) `
            -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Original images are safely stored in:"
Write-Host $BackupFolder -ForegroundColor Yellow
Write-Host ""