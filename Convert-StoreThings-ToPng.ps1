$SourceFolder = "E:\projects\LuckyDangle\StoreThings"

Add-Type -AssemblyName System.Drawing

$extensions = @(
    "*.jpg",
    "*.jpeg",
    "*.webp",
    "*.bmp",
    "*.gif",
    "*.tif",
    "*.tiff"
)

foreach ($pattern in $extensions) {

    Get-ChildItem `
        -Path $SourceFolder `
        -Filter $pattern `
        -File |
    ForEach-Object {

        $inputFile = $_.FullName
        $outputFile = Join-Path `
            $SourceFolder `
            ($_.BaseName + ".png")

        Write-Host "Converting: $($_.Name) -> $($_.BaseName).png"

        $image = [System.Drawing.Image]::FromFile($inputFile)

        try {
            $image.Save(
                $outputFile,
                [System.Drawing.Imaging.ImageFormat]::Png
            )
        }
        finally {
            $image.Dispose()
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All images converted to PNG." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green