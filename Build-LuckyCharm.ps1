param(
    [switch]$Install
)

$ErrorActionPreference = "Stop"

$ProjectRoot       = $PSScriptRoot
$ManifestSource    = Join-Path $ProjectRoot "StorePackage\Package.appxmanifest"
$PackageRoot       = Join-Path $ProjectRoot "StorePackage\PackageRoot"
$PublishRoot       = Join-Path $ProjectRoot "bin\Release\net10.0-windows\win-x64\publish"
$AssetsSource      = Join-Path $ProjectRoot "StorePackage\Assets"
$SupportSource     = Join-Path $ProjectRoot "Assets\Support"
$CertPath          = Join-Path $ProjectRoot "devcert.pfx"
$OutputDir         = Join-Path $ProjectRoot "StorePackage"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "        LuckyCharm Release Builder" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# =================================================
# 1. Read current version
# =================================================

if (-not (Test-Path $ManifestSource)) {
    throw "Manifest not found: $ManifestSource"
}

[xml]$sourceManifestXml = Get-Content $ManifestSource -Raw

$identityNode = $sourceManifestXml.Package.Identity

if ($null -eq $identityNode) {
    throw "Could not find Package Identity in manifest."
}

$currentVersion = [version]$identityNode.Version

# 0.1.1.0 -> 0.1.2.0
$newVersion = [version]::new(
    $currentVersion.Major,
    $currentVersion.Minor,
    $currentVersion.Build + 1,
    0
)

Write-Host "Current version : $currentVersion"
Write-Host "New version     : $newVersion"
Write-Host ""

# =================================================
# 2. Check tools
# =================================================

Write-Host "[1/8] Checking tools..." -ForegroundColor Yellow

if (-not (Get-Command winapp -ErrorAction SilentlyContinue)) {
    throw "winapp CLI was not found."
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet CLI was not found."
}

# =================================================
# 3. Check / create development certificate
# =================================================

if (-not (Test-Path $CertPath)) {

    Write-Host "Development certificate not found." -ForegroundColor Yellow
    Write-Host "Generating certificate..."

    & winapp cert generate `
        --publisher "CN=Rohit Kumar Mallick"

    if ($LASTEXITCODE -ne 0) {
        throw "Development certificate generation failed."
    }

    if (-not (Test-Path $CertPath)) {
        throw "Certificate was not created: $CertPath"
    }
}

Write-Host "Tools and certificate OK." -ForegroundColor Green
Write-Host ""

# =================================================
# 4. Publish
# =================================================

Write-Host "[2/8] Publishing Release build..." -ForegroundColor Yellow

& dotnet publish `
    -c Release `
    -r win-x64 `
    --self-contained true

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

if (-not (Test-Path $PublishRoot)) {
    throw "Publish folder was not created: $PublishRoot"
}

Write-Host "Publish succeeded." -ForegroundColor Green
Write-Host ""

# =================================================
# 5. Rebuild PackageRoot
# =================================================

Write-Host "[3/8] Rebuilding PackageRoot..." -ForegroundColor Yellow

if (Test-Path $PackageRoot) {

    Get-ChildItem $PackageRoot -Force |
        Remove-Item -Recurse -Force
}
else {

    New-Item `
        -ItemType Directory `
        -Path $PackageRoot `
        -Force |
        Out-Null
}

Copy-Item `
    (Join-Path $PublishRoot "*") `
    $PackageRoot `
    -Recurse `
    -Force

Write-Host "Application copied." -ForegroundColor Green
Write-Host ""

# =================================================
# 6. Copy assets + create NEW package manifest
# =================================================

Write-Host "[4/8] Copying package assets..." -ForegroundColor Yellow

# Store/package artwork
Copy-Item `
    $AssetsSource `
    $PackageRoot `
    -Recurse `
    -Force

# Support / donation assets
if (Test-Path $SupportSource) {

    $PackageAssetsPath = Join-Path $PackageRoot "Assets"

    Copy-Item `
        $SupportSource `
        $PackageAssetsPath `
        -Recurse `
        -Force
}
else {

    Write-Host `
        "WARNING: Support folder not found: $SupportSource" `
        -ForegroundColor Yellow
}

# -------------------------------------------------
# IMPORTANT:
# Do NOT modify the source manifest yet.
# Create the upgraded manifest only inside PackageRoot.
# -------------------------------------------------

$PackageManifestPath = Join-Path `
    $PackageRoot `
    "Package.appxmanifest"

[xml]$newPackageManifestXml = Get-Content `
    $ManifestSource `
    -Raw

$newPackageManifestXml.Package.Identity.Version =
    $newVersion.ToString()

$newPackageManifestXml.Save(
    $PackageManifestPath
)

Write-Host "Assets and upgraded manifest copied." -ForegroundColor Green
Write-Host ""

# =================================================
# 7. Verify PackageRoot
# =================================================

Write-Host "[5/8] Verifying package..." -ForegroundColor Yellow

$ExePath = Join-Path `
    $PackageRoot `
    "LuckyDangle.exe"

if (-not (Test-Path $ExePath)) {
    throw "LuckyDangle.exe is missing from PackageRoot."
}

if (-not (Test-Path $PackageManifestPath)) {
    throw "Package.appxmanifest is missing from PackageRoot."
}

# Read XML into a DIFFERENT variable.
# Do NOT reuse PackageManifestPath.
[xml]$verifiedManifestXml = Get-Content `
    $PackageManifestPath `
    -Raw

$packageVersion =
    $verifiedManifestXml.Package.Identity.Version

if ($packageVersion -ne $newVersion.ToString()) {

    throw `
        "PackageRoot manifest version mismatch. Expected $newVersion, found $packageVersion."
}

# Verify support QR
$QrPath = Join-Path `
    $PackageRoot `
    "Assets\Support\lucky_charm_upi_qr.jpg"

if (-not (Test-Path $QrPath)) {

    Write-Host `
        "WARNING: UPI QR not found: $QrPath" `
        -ForegroundColor Yellow
}

Write-Host "PackageRoot verified." -ForegroundColor Green
Write-Host "Manifest : $PackageManifestPath"
Write-Host "Version  : $packageVersion"
Write-Host ""

# =================================================
# 8. Create + sign MSIX
# =================================================

Write-Host "[6/8] Creating and signing MSIX..." -ForegroundColor Yellow

$OutputPackage = Join-Path `
    $OutputDir `
    "LuckyCharm-$newVersion-x64.msix"

if (Test-Path $OutputPackage) {

    Remove-Item `
        $OutputPackage `
        -Force
}

# -------------------------------------------------
# VERY IMPORTANT:
# Pass the PATH variable here.
# -------------------------------------------------

Write-Host ""
Write-Host "Package root : $PackageRoot"
Write-Host "Manifest     : $PackageManifestPath"
Write-Host "Output       : $OutputPackage"
Write-Host ""

& winapp package `
    $PackageRoot `
    --manifest $PackageManifestPath `
    --output $OutputPackage `
    --cert $CertPath `
    --cert-password password

if ($LASTEXITCODE -ne 0) {
    throw "MSIX creation/signing failed."
}

if (-not (Test-Path $OutputPackage)) {
    throw "MSIX was not created: $OutputPackage"
}

Write-Host ""
Write-Host "MSIX created and signed successfully." -ForegroundColor Green
Write-Host ""

# =================================================
# 9. ONLY NOW update source manifest
# =================================================

Write-Host "[7/8] Updating source manifest version..." -ForegroundColor Yellow

$sourceManifestXml.Package.Identity.Version =
    $newVersion.ToString()

$sourceManifestXml.Save(
    $ManifestSource
)

Write-Host `
    "Source manifest updated to $newVersion." `
    -ForegroundColor Green

Write-Host ""

# =================================================
# 10. Final verification
# =================================================

$PackageInfo = Get-Item $OutputPackage

Write-Host "================================================" -ForegroundColor Green
Write-Host "SUCCESS" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Version : $newVersion"
Write-Host "Package : $($PackageInfo.FullName)"
Write-Host "Size    : $($PackageInfo.Length) bytes"
Write-Host ""

# =================================================
# 11. Optional installation / upgrade
# =================================================

if ($Install) {

    Write-Host "[8/8] Installing/upgrading LuckyCharm..." -ForegroundColor Yellow
    Write-Host ""

    Add-AppxPackage `
        $OutputPackage

    Write-Host ""
    Write-Host "Installation/upgrade completed." -ForegroundColor Green
    Write-Host ""

    $InstalledPackage =
        Get-AppxPackage -Name LuckyDangle |
        Select-Object `
            Name,
            Version,
            PackageFullName,
            InstallLocation

    $InstalledPackage |
        Format-List
}
else {

    Write-Host "[8/8] Installation skipped." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Use:"
    Write-Host ""
    Write-Host ".\Build-LuckyCharm.ps1 -Install"
    Write-Host ""
    Write-Host "to build AND install/upgrade."
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "LuckyCharm release complete." -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan