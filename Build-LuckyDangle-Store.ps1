$ErrorActionPreference = "Stop"

# ============================================================
# Lucky Dangle - Microsoft Store Production Builder
#
# PURPOSE:
#   Build the next Microsoft Store package automatically.
#
# VERSIONING:
#   Automatically increments the third numeric component.
#
#   Example:
#       0.1.3.0 -> 0.1.4.0
#       0.1.4.0 -> 0.1.5.0
#
# IMPORTANT:
# - Source manifest is updated ONLY after successful packaging.
# - Does NOT use devcert.pfx.
# - Does NOT install the package.
# - Does NOT create a duplicate StartupTask.
# - Preserves the existing LuckyCharmStartup from the source manifest.
# - Creates:
#       StorePackage\StoreUpload\LuckyDangle-x64.msix
#       StorePackage\StoreUpload\LuckyDangle-x64.msixupload
# ============================================================

$ProjectRoot = $PSScriptRoot

$SourceManifest = Join-Path `
    $ProjectRoot `
    "StorePackage\Package.appxmanifest"

$PackageRoot = Join-Path `
    $ProjectRoot `
    "StorePackage\StorePackageRoot"

$PublishRoot = Join-Path `
    $ProjectRoot `
    "bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

$AssetsSource = Join-Path `
    $ProjectRoot `
    "StorePackage\Assets"

$SupportSource = Join-Path `
    $ProjectRoot `
    "Assets\Support"

$OutputDir = Join-Path `
    $ProjectRoot `
    "StorePackage\StoreUpload"

# ============================================================
# Microsoft Store identity
# ============================================================

$StorePackageName = "RohitKumarMallick.LuckyDangle"

$StorePublisher = `
    "CN=5E0658A7-ADE9-4F1C-9547-6C3A82C8ED3B"

$StorePublisherDisplayName = `
    "Rohit Kumar Mallick"

# ============================================================
# Header
# ============================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "     Lucky Dangle - Microsoft Store Builder" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# 1. Validate tools and source files
# ============================================================

Write-Host "[1/8] Checking tools and source files..." `
    -ForegroundColor Yellow

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet CLI was not found."
}

if (-not (Get-Command winapp -ErrorAction SilentlyContinue)) {
    throw "winapp CLI was not found."
}

if (-not (Test-Path $SourceManifest)) {
    throw "Source manifest not found: $SourceManifest"
}

if (-not (Test-Path $AssetsSource)) {
    throw "Store assets folder not found: $AssetsSource"
}

if (-not (Test-Path $SupportSource)) {
    Write-Host `
        "WARNING: Support assets folder not found: $SupportSource" `
        -ForegroundColor Yellow
}

# ============================================================
# 2. Read and increment version
#
# IMPORTANT:
# The source manifest is NOT saved yet.
# We only save it after the Store package is successfully created.
# ============================================================

[xml]$sourceManifestXml = `
    Get-Content $SourceManifest -Raw

$sourceIdentityNode = `
    $sourceManifestXml.Package.Identity

if ($null -eq $sourceIdentityNode) {
    throw "Package Identity was not found in the source manifest."
}

$currentVersion = `
    [version]$sourceIdentityNode.Version

$newVersion = [version]::new(
    $currentVersion.Major,
    $currentVersion.Minor,
    $currentVersion.Build + 1,
    0
)

Write-Host "Current version : $currentVersion"
Write-Host "New version     : $newVersion"
Write-Host "Store identity  : $StorePackageName"
Write-Host "Store publisher : $StorePublisher"
Write-Host ""

# ============================================================
# 3. Publish application
# ============================================================

Write-Host "[2/8] Publishing Release build..." `
    -ForegroundColor Yellow

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

Write-Host "Publish succeeded." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# 4. Rebuild Store PackageRoot
# ============================================================

Write-Host "[3/8] Rebuilding Store PackageRoot..." `
    -ForegroundColor Yellow

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

if (-not (Test-Path (Join-Path $PackageRoot "LuckyDangle.exe"))) {
    throw "LuckyDangle.exe was not copied into StorePackageRoot."
}

Write-Host "Application copied." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# 5. Copy assets and create Store manifest
# ============================================================

Write-Host "[4/8] Copying Store assets..." `
    -ForegroundColor Yellow

Copy-Item `
    $AssetsSource `
    $PackageRoot `
    -Recurse `
    -Force

if (Test-Path $SupportSource) {

    $PackageAssetsPath = `
        Join-Path $PackageRoot "Assets"

    Copy-Item `
        $SupportSource `
        $PackageAssetsPath `
        -Recurse `
        -Force
}

Write-Host "Assets copied." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# Create Store manifest
#
# IMPORTANT:
# - Start from the corrected source manifest.
# - Change ONLY Store identity/publisher/display/version.
# - DO NOT ADD LuckyCharmStartup here.
#
# The source manifest already contains the startup task.
# ============================================================

Write-Host "[5/8] Creating Store manifest..." `
    -ForegroundColor Yellow

$StoreManifestPath = `
    Join-Path $PackageRoot "Package.appxmanifest"

[xml]$storeManifestXml = `
    Get-Content $SourceManifest -Raw

# ------------------------------------------------------------
# Store identity
# ------------------------------------------------------------

$storeManifestXml.Package.Identity.Name = `
    $StorePackageName

$storeManifestXml.Package.Identity.Publisher = `
    $StorePublisher

$storeManifestXml.Package.Identity.Version = `
    $newVersion.ToString()

# ------------------------------------------------------------
# Publisher display name
# ------------------------------------------------------------

$storeManifestXml.Package.Properties.PublisherDisplayName = `
    $StorePublisherDisplayName

# ------------------------------------------------------------
# Store display name
# ------------------------------------------------------------

$storeManifestXml.Package.Properties.DisplayName = `
    "Lucky Dangle"

# ------------------------------------------------------------
# Save Store manifest ONLY inside StorePackageRoot
# ------------------------------------------------------------

$storeManifestXml.Save(
    $StoreManifestPath
)

Write-Host ""
Write-Host "Store manifest:" `
    -ForegroundColor Green

Write-Host "  Name      : $StorePackageName"
Write-Host "  Publisher : $StorePublisher"
Write-Host "  Version   : $newVersion"
Write-Host "  Display   : Lucky Dangle"

Write-Host ""

# ============================================================
# 6. Verify Store manifest
# ============================================================

Write-Host "[6/8] Verifying Store package..." `
    -ForegroundColor Yellow

if (-not (Test-Path $StoreManifestPath)) {
    throw "Store manifest was not created."
}

$ExePath = `
    Join-Path $PackageRoot "LuckyDangle.exe"

if (-not (Test-Path $ExePath)) {
    throw "LuckyDangle.exe is missing."
}

[xml]$verifiedStoreManifestXml = `
    Get-Content $StoreManifestPath -Raw

$verifiedIdentity = `
    $verifiedStoreManifestXml.Package.Identity

if ($verifiedIdentity.Name -ne $StorePackageName) {
    throw "Store package Name mismatch."
}

if ($verifiedIdentity.Publisher -ne $StorePublisher) {
    throw "Store package Publisher mismatch."
}

if ($verifiedIdentity.Version -ne $newVersion.ToString()) {
    throw "Store package Version mismatch. Expected $newVersion, found $($verifiedIdentity.Version)."
}

$verifiedPublisherDisplayName = `
    $verifiedStoreManifestXml.Package.Properties.PublisherDisplayName

if ($verifiedPublisherDisplayName -ne $StorePublisherDisplayName) {
    throw "PublisherDisplayName mismatch."
}

$verifiedDisplayName = `
    $verifiedStoreManifestXml.Package.Properties.DisplayName

if ($verifiedDisplayName -ne "Lucky Dangle") {
    throw "Store DisplayName mismatch."
}

Write-Host "Store identity verified." `
    -ForegroundColor Green

# ------------------------------------------------------------
# Verify Applications wrapper
# ------------------------------------------------------------

$verifiedApplication = `
    $verifiedStoreManifestXml.Package.Applications.Application

if ($null -eq $verifiedApplication) {
    throw "Application node was not found inside Applications."
}

# ------------------------------------------------------------
# Verify startup task already exists
#
# IMPORTANT:
# We VERIFY it.
# We DO NOT CREATE it.
# ------------------------------------------------------------

$verifiedStartupExtension =
    $verifiedApplication.Extensions.ChildNodes |
        Where-Object {
            $_.LocalName -eq "Extension" -and
            $_.Category -eq "windows.startupTask"
        } |
        Select-Object -First 1

if ($null -eq $verifiedStartupExtension) {
    throw "Windows startup extension was not found."
}

$verifiedStartupTask =
    $verifiedStartupExtension.ChildNodes |
        Where-Object {
            $_.LocalName -eq "StartupTask" -and
            $_.TaskId -eq "LuckyCharmStartup"
        } |
        Select-Object -First 1

if ($null -eq $verifiedStartupTask) {
    throw "LuckyCharm startup task was not found."
}

if ($verifiedStartupTask.Enabled -ne "true") {
    throw "LuckyCharm startup task is not enabled."
}

if ($verifiedStartupTask.DisplayName -ne "LuckyCharm") {
    throw "LuckyCharm startup task DisplayName mismatch."
}

Write-Host "Windows startup task verified." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# 7. Create Store MSIX
#
# Microsoft Store handles production signing.
#
# No devcert.pfx is used here.
# ============================================================

Write-Host "[7/8] Creating Store MSIX..." `
    -ForegroundColor Yellow

if (-not (Test-Path $OutputDir)) {

    New-Item `
        -ItemType Directory `
        -Path $OutputDir `
        -Force |
        Out-Null
}

$OutputMsix = Join-Path `
    $OutputDir `
    "LuckyDangle-$newVersion-x64.msix"

if (Test-Path $OutputMsix) {
    Remove-Item $OutputMsix -Force
}

Write-Host ""
Write-Host "Package root : $PackageRoot"
Write-Host "Manifest     : $StoreManifestPath"
Write-Host "Output       : $OutputMsix"
Write-Host ""

& winapp package `
    $PackageRoot `
    --manifest $StoreManifestPath `
    --output $OutputMsix

if ($LASTEXITCODE -ne 0) {
    throw "Store MSIX creation failed."
}

if (-not (Test-Path $OutputMsix)) {
    throw "Store MSIX was not created."
}

Write-Host ""
Write-Host "MSIX package creation completed." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# Create MSIXUPLOAD
#
# Contents:
#
#   LuckyDangle-<VERSION>-x64.msix
#   LuckyDangle.appxsym
#
# Then ZIP -> .msixupload
# ============================================================

Write-Host "[8/8] Creating MSIXUPLOAD..." `
    -ForegroundColor Yellow

$UploadZip = Join-Path `
    $OutputDir `
    "LuckyDangle-$newVersion-x64.zip"

$FinalUpload = Join-Path `
    $OutputDir `
    "LuckyDangle-$newVersion-x64.msixupload"

$TempUploadDir = Join-Path `
    $OutputDir `
    "_upload_temp"

# ------------------------------------------------------------
# Clean previous temporary/output files
# ------------------------------------------------------------

if (Test-Path $TempUploadDir) {

    Remove-Item `
        $TempUploadDir `
        -Recurse `
        -Force
}

if (Test-Path $UploadZip) {
    Remove-Item $UploadZip -Force
}

if (Test-Path $FinalUpload) {
    Remove-Item $FinalUpload -Force
}

New-Item `
    -ItemType Directory `
    -Path $TempUploadDir `
    -Force |
    Out-Null

# ------------------------------------------------------------
# Copy MSIX
# ------------------------------------------------------------

Copy-Item `
    $OutputMsix `
    $TempUploadDir `
    -Force

# ------------------------------------------------------------
# Create .appxsym from public PDB
# ------------------------------------------------------------

$PdbPath = `
    Join-Path $PublishRoot "LuckyDangle.pdb"

if (Test-Path $PdbPath) {

    Write-Host "Public symbol source found."

    $SymbolsTempDir = `
        Join-Path $OutputDir "_symbols_temp"

    if (Test-Path $SymbolsTempDir) {

        Remove-Item `
            $SymbolsTempDir `
            -Recurse `
            -Force
    }

    New-Item `
        -ItemType Directory `
        -Path $SymbolsTempDir `
        -Force |
        Out-Null

    Copy-Item `
        $PdbPath `
        $SymbolsTempDir `
        -Force

    $SymbolsZip = `
        Join-Path $OutputDir "LuckyDangle.appxsym.zip"

    $AppxSymPath = `
        Join-Path $TempUploadDir "LuckyDangle.appxsym"

    if (Test-Path $SymbolsZip) {
        Remove-Item $SymbolsZip -Force
    }

    if (Test-Path $AppxSymPath) {
        Remove-Item $AppxSymPath -Force
    }

    Compress-Archive `
        -Path (Join-Path $SymbolsTempDir "*") `
        -DestinationPath $SymbolsZip `
        -CompressionLevel Optimal

    Move-Item `
        $SymbolsZip `
        $AppxSymPath `
        -Force

    Remove-Item `
        $SymbolsTempDir `
        -Recurse `
        -Force

    Write-Host "Public symbols included." `
        -ForegroundColor Green
}
else {

    Write-Host `
        "No PDB found. Continuing without public symbols." `
        -ForegroundColor Yellow
}

# ------------------------------------------------------------
# Create outer MSIXUPLOAD archive
# ------------------------------------------------------------

Write-Host "Creating upload archive..."

Compress-Archive `
    -Path (Join-Path $TempUploadDir "*") `
    -DestinationPath $UploadZip `
    -CompressionLevel Optimal

if (-not (Test-Path $UploadZip)) {
    throw "MSIXUPLOAD archive could not be created."
}

# ------------------------------------------------------------
# Rename ZIP -> MSIXUPLOAD
# ------------------------------------------------------------

Move-Item `
    $UploadZip `
    $FinalUpload `
    -Force

# ------------------------------------------------------------
# Clean temporary folder
# ------------------------------------------------------------

Remove-Item `
    $TempUploadDir `
    -Recurse `
    -Force

# ============================================================
# FINAL PACKAGE VERIFICATION
# ============================================================

$MsixInfo = Get-Item $OutputMsix
$UploadInfo = Get-Item $FinalUpload

if (-not (Test-Path $FinalUpload)) {
    throw "Final MSIXUPLOAD was not created."
}

# ============================================================
# ONLY NOW update source manifest version
#
# This prevents the source version from being advanced if
# packaging fails.
# ============================================================

[xml]$finalSourceManifestXml = `
    Get-Content $SourceManifest -Raw

$finalSourceIdentity = `
    $finalSourceManifestXml.Package.Identity

if ($null -eq $finalSourceIdentity) {
    throw "Could not reopen source manifest for version update."
}

if ($finalSourceIdentity.Version -ne $currentVersion.ToString()) {
    throw "Source manifest changed unexpectedly during packaging."
}

$finalSourceIdentity.Version = `
    $newVersion.ToString()

$finalSourceManifestXml.Save(
    $SourceManifest
)

# ============================================================
# FINAL OUTPUT
# ============================================================

Write-Host ""
Write-Host "================================================" `
    -ForegroundColor Green

Write-Host "       STORE PACKAGE READY" `
    -ForegroundColor Green

Write-Host "================================================" `
    -ForegroundColor Green

Write-Host ""
Write-Host "Previous version : $currentVersion"
Write-Host "New version      : $newVersion"
Write-Host ""

Write-Host "Store name       : Lucky Dangle"
Write-Host "Package identity : $StorePackageName"
Write-Host "Publisher        : $StorePublisher"
Write-Host ""

Write-Host "MSIX:"
Write-Host "  $($MsixInfo.FullName)"
Write-Host "  $($MsixInfo.Length) bytes"

Write-Host ""

Write-Host "MSIXUPLOAD:"
Write-Host "  $($UploadInfo.FullName)"
Write-Host "  $($UploadInfo.Length) bytes"

Write-Host ""

Write-Host "Source manifest updated to:"
Write-Host "  Version = $newVersion"

Write-Host ""

Write-Host "IMPORTANT:" `
    -ForegroundColor Yellow

Write-Host "Upload the .msixupload file to Partner Center."
Write-Host "Do NOT upload the local development MSIX."

Write-Host ""

Write-Host "================================================" `
    -ForegroundColor Cyan

Write-Host "Done." `
    -ForegroundColor Cyan

Write-Host "================================================" `
    -ForegroundColor Cyan