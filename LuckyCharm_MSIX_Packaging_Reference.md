# LuckyCharm --- MSIX Packaging & Signing Reference

This is the repeatable reference for creating, signing, testing, and
upgrading the LuckyCharm Windows MSIX package.

## 1. Working Environment

Project:

`E:\projects\LuckyDangle`

Verified tooling:

-   .NET SDK `10.0.400`
-   Windows App Development CLI `0.6.0`
-   WPF target: `net10.0-windows`
-   Runtime: `win-x64`
-   Publish mode: self-contained

Verify the CLI:

``` powershell
winapp --version
```

Expected:

``` text
0.6.0
```

The `winapp` CLI provides the packaging and certificate commands used
below. We do not need to manually locate `makeappx.exe` or
`signtool.exe` for this workflow.

------------------------------------------------------------------------

## 2. Package Identity vs Display Name

The package identity is intentionally:

``` xml
<Identity
    Name="LuckyDangle"
    Publisher="CN=Rohit Kumar Mallick"
    Version="0.1.1.0" />
```

The user-facing application name is:

``` xml
<DisplayName>LuckyCharm</DisplayName>
```

and the visual element also uses:

``` xml
<uap:VisualElements
    DisplayName="LuckyCharm"
```

### IMPORTANT

Do not casually change:

``` xml
Name="LuckyDangle"
```

Windows uses the package identity to determine whether a package is an
update to an existing installation.

Users see `LuckyCharm`; Windows internally identifies the package as
`LuckyDangle`.

------------------------------------------------------------------------

## 3. Versioning Is Critical

Windows rejected a package when the installed package and the new
package both had version `0.1.0.0` but different contents.

Therefore every update package must have a higher version.

Example:

``` text
0.1.0.0
0.1.1.0
0.1.2.0
0.2.0.0
```

For the next release, change the manifest:

``` xml
Version="0.1.1.0"
```

to a higher version such as:

``` xml
Version="0.1.2.0"
```

Do not reuse an existing version for changed package contents.

The successful local upgrade test was:

``` text
0.1.0.0 → 0.1.1.0
```

without uninstalling the existing package.

------------------------------------------------------------------------

## 4. Important Project Locations

``` text
E:\projects\LuckyDangle\
├── Assets\
│   └── Support\
│       └── lucky_charm_upi_qr.jpg
│
├── StorePackage\
│   ├── Assets\
│   ├── Package.appxmanifest
│   └── PackageRoot\
│
├── bin\Release\
│   └── net10.0-windows\
│       └── win-x64\
│           └── publish\
│
└── devcert.pfx
```

The application Support QR is sourced from:

``` text
Assets\Support\lucky_charm_upi_qr.jpg
```

and must also exist in:

``` text
StorePackage\PackageRoot\Assets\Support\
```

------------------------------------------------------------------------

# 5. Standard Release Packaging Procedure

## Step A --- Increment the manifest version

Edit:

``` text
StorePackage\Package.appxmanifest
```

Example:

``` xml
Version="0.1.2.0"
```

Keep:

``` xml
Name="LuckyDangle"
Publisher="CN=Rohit Kumar Mallick"
```

and the user-facing name:

``` xml
<DisplayName>LuckyCharm</DisplayName>
```

------------------------------------------------------------------------

## Step B --- Publish the current Release build

From:

``` powershell
PS E:\projects\LuckyDangle>
```

run:

``` powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Publish output:

``` text
bin\Release\net10.0-windows\win-x64\publish\
```

------------------------------------------------------------------------

## Step C --- Clear the old PackageRoot

``` powershell
Remove-Item .\StorePackage\PackageRoot\* -Recurse -Force
```

This avoids accidentally packaging stale application files.

------------------------------------------------------------------------

## Step D --- Copy the current application build

``` powershell
Copy-Item `
    .\bin\Release\net10.0-windows\win-x64\publish\* `
    .\StorePackage\PackageRoot\ `
    -Recurse `
    -Force
```

------------------------------------------------------------------------

## Step E --- Copy package assets

``` powershell
Copy-Item `
    .\StorePackage\Assets `
    .\StorePackage\PackageRoot\ `
    -Recurse `
    -Force
```

------------------------------------------------------------------------

## Step F --- Copy the current manifest

``` powershell
Copy-Item `
    .\StorePackage\Package.appxmanifest `
    .\StorePackage\PackageRoot\ `
    -Force
```

------------------------------------------------------------------------

## Step G --- Copy application support assets

``` powershell
Copy-Item `
    .\Assets\Support `
    .\StorePackage\PackageRoot\Assets\ `
    -Recurse `
    -Force
```

Verify:

``` powershell
Get-ChildItem .\StorePackage\PackageRoot\Assets\Support
```

Expected:

``` text
lucky_charm_upi_qr.jpg
```

------------------------------------------------------------------------

# 6. Verify PackageRoot Before Packaging

Verify the executable:

``` powershell
Get-Item .\StorePackage\PackageRoot\LuckyDangle.exe |
    Select-Object Name, Length, LastWriteTime
```

Verify the manifest:

``` powershell
Get-Item .\StorePackage\PackageRoot\Package.appxmanifest
```

Verify version:

``` powershell
Select-String `
    -Path .\StorePackage\PackageRoot\Package.appxmanifest `
    -Pattern 'Version='
```

Verify identity/display name:

``` powershell
Select-String `
    -Path .\StorePackage\PackageRoot\Package.appxmanifest `
    -Pattern 'LuckyCharm|LuckyDangle|Version'
```

Verify icon assets:

``` powershell
Get-ChildItem .\StorePackage\PackageRoot\Assets\*.png |
    Select-Object Name, Length
```

Verify Support:

``` powershell
Get-ChildItem .\StorePackage\PackageRoot\Assets\Support
```

------------------------------------------------------------------------

# 7. Windows Icon Assets

Current asset dimensions:

``` text
AppList.png                                  44 × 44
AppList.scale-200.png                        88 × 88
AppList.targetsize-24_altform-unplated.png   24 × 24
MedTile.png                                 150 × 150
MedTile.scale-200.png                       300 × 300
StoreLogo.png                                50 × 50
StoreLogo.scale-200.png                     100 × 100
WideTile.png                                310 × 150
WideTile.scale-200.png                      620 × 300
```

Manifest references:

``` xml
<Logo>Assets\StoreLogo.png</Logo>
```

``` xml
Square150x150Logo="Assets\MedTile.png"
Square44x44Logo="Assets\AppList.png"
```

The icon artwork can be replaced later without changing the package
identity.

------------------------------------------------------------------------

# 8. Create an Unsigned MSIX

Useful for testing package creation before introducing signing:

``` powershell
winapp package .\StorePackage\PackageRoot `
    --manifest .\StorePackage\PackageRoot\Package.appxmanifest `
    --output .\StorePackage\LuckyCharm-0.1.2.0-x64.msix
```

Use the actual manifest version in the filename.

------------------------------------------------------------------------

# 9. Development Certificate

For local testing, a self-signed development certificate was generated
with:

``` powershell
winapp cert generate --publisher "CN=Rohit Kumar Mallick"
```

It creates:

``` text
E:\projects\LuckyDangle\devcert.pfx
```

The CLI reports that the default password is:

``` text
password
```

Verify it:

``` powershell
winapp cert info .\devcert.pfx
```

The important field must be:

``` text
Subject: CN=Rohit Kumar Mallick
```

This must match the manifest:

``` xml
Publisher="CN=Rohit Kumar Mallick"
```

The tested certificate had a private key and a one-year validity period.

------------------------------------------------------------------------

# 10. Development Certificate Warning

`devcert.pfx` is a self-signed development certificate.

It is for:

-   local testing
-   development
-   testing MSIX installation on trusted machines

It is NOT a production/public signing certificate.

It is not automatically trusted on another user's computer.

For a future public release:

-   Microsoft Store distribution uses the Store publishing/signing
    process.
-   Direct-download distribution should use a trusted production
    code-signing certificate.

Do not publish `devcert.pfx` as part of a public installer.

Keep the private certificate out of GitHub/source control.

------------------------------------------------------------------------

# 11. Trust the Development Certificate

On a test machine:

``` powershell
winapp cert install .\devcert.pfx
```

This installs/trusts the development certificate and may require
elevation.

You do not need to run this from `C:\`; running it from:

``` text
E:\projects\LuckyDangle
```

works because `.\devcert.pfx` points to the project certificate.

------------------------------------------------------------------------

# 12. Create and Sign the MSIX

The tested one-command workflow is:

``` powershell
winapp package .\StorePackage\PackageRoot `
    --manifest .\StorePackage\PackageRoot\Package.appxmanifest `
    --output .\StorePackage\LuckyCharm-0.1.1.0-x64.msix `
    --cert .\devcert.pfx `
    --cert-password password
```

For the next release, change the output filename to the new version, for
example:

``` powershell
winapp package .\StorePackage\PackageRoot `
    --manifest .\StorePackage\PackageRoot\Package.appxmanifest `
    --output .\StorePackage\LuckyCharm-0.1.2.0-x64.msix `
    --cert .\devcert.pfx `
    --cert-password password
```

Successful output includes:

``` text
MSIX package creation completed.
Package has been signed
```

------------------------------------------------------------------------

# 13. Install / Upgrade Test

Install the signed package:

``` powershell
Add-AppxPackage .\StorePackage\LuckyCharm-0.1.2.0-x64.msix
```

If the existing installation has the same package identity but a lower
version, Windows can upgrade it in place.

The tested upgrade was:

``` text
Installed:  LuckyDangle 0.1.0.0
New:        LuckyDangle 0.1.1.0
Result:     Successful upgrade
```

Do not uninstall the old package merely to test an update.

------------------------------------------------------------------------

# 14. Verify Installed Package

``` powershell
Get-AppxPackage -Name LuckyDangle |
    Select-Object Name, Version, PackageFullName, InstallLocation
```

Expected pattern:

``` text
Name            : LuckyDangle
Version         : 0.1.2.0
PackageFullName : LuckyDangle_0.1.2.0_x64__...
```

Windows should present the application to the user as:

``` text
LuckyCharm
```

while the internal package identity remains:

``` text
LuckyDangle
```

------------------------------------------------------------------------

# 15. Same-Version Error

If Windows reports:

``` text
0x80073CFB
```

with wording such as:

``` text
The provided package is already installed...
same identity as an already-installed package but the contents are different.
Increment the version number...
```

the package version was reused.

Fix:

1.  Increase `Version` in `Package.appxmanifest`.
2.  Copy the manifest into `PackageRoot`.
3.  Recreate and sign the MSIX.
4.  Install the higher-version package.

------------------------------------------------------------------------

# 16. Complete Copy/Paste Release Recipe

After changing the manifest to the new version:

``` powershell
dotnet publish -c Release -r win-x64 --self-contained true

Remove-Item .\StorePackage\PackageRoot\* -Recurse -Force

Copy-Item `
    .\bin\Release\net10.0-windows\win-x64\publish\* `
    .\StorePackage\PackageRoot\ `
    -Recurse `
    -Force

Copy-Item `
    .\StorePackage\Assets `
    .\StorePackage\PackageRoot\ `
    -Recurse `
    -Force

Copy-Item `
    .\StorePackage\Package.appxmanifest `
    .\StorePackage\PackageRoot\ `
    -Force

Copy-Item `
    .\Assets\Support `
    .\StorePackage\PackageRoot\Assets\ `
    -Recurse `
    -Force

Select-String `
    -Path .\StorePackage\PackageRoot\Package.appxmanifest `
    -Pattern 'Version='

winapp package .\StorePackage\PackageRoot `
    --manifest .\StorePackage\PackageRoot\Package.appxmanifest `
    --output .\StorePackage\LuckyCharm-<VERSION>-x64.msix `
    --cert .\devcert.pfx `
    --cert-password password
```

Replace `<VERSION>` with the exact version in the manifest.

Then:

``` powershell
Add-AppxPackage .\StorePackage\LuckyCharm-<VERSION>-x64.msix
```

Verify:

``` powershell
Get-AppxPackage -Name LuckyDangle |
    Select-Object Name, Version, PackageFullName, InstallLocation
```

------------------------------------------------------------------------

# 17. Certificate / Tool Troubleshooting

## Check CLI

``` powershell
winapp --version
```

Expected working setup:

``` text
0.6.0
```

## Check certificate

``` powershell
winapp cert info .\devcert.pfx
```

Confirm:

``` text
Subject: CN=Rohit Kumar Mallick
Has Private Key: True
```

## Trust certificate

``` powershell
winapp cert install .\devcert.pfx
```

## Check package

``` powershell
Get-Item .\StorePackage\LuckyCharm-*.msix |
    Select-Object Name, Length, LastWriteTime
```

------------------------------------------------------------------------

# 18. Important Rules for Future Releases

1.  Keep package identity `LuckyDangle`.
2.  Keep user-facing display name `LuckyCharm`.
3.  Keep publisher consistent with the certificate.
4.  Increment package version for every update.
5.  Publish a fresh Release build before packaging.
6.  Recreate `PackageRoot` from the fresh publish to avoid stale files.
7.  Copy the manifest after changing its version.
8.  Copy `StorePackage\Assets` into `PackageRoot`.
9.  Copy `Assets\Support` into `PackageRoot\Assets`.
10. Verify the QR file before packaging.
11. Keep `devcert.pfx` out of GitHub.
12. Development signing is for local testing only.
13. Do not use the development certificate as the final public
    production certificate.
14. Do not uninstall the existing package when testing normal upgrades.
15. If the package contents change, use a higher version.

------------------------------------------------------------------------

# 19. Current Known-Good State

Successfully verified:

``` text
.NET SDK:                  10.0.400
Windows App CLI:           0.6.0
Target framework:          net10.0-windows
Architecture:              win-x64
Deployment:                self-contained

Package identity:          LuckyDangle
Display name:              LuckyCharm
Publisher:                 CN=Rohit Kumar Mallick

Development certificate:   devcert.pfx
Certificate password:      password

Successful package:
LuckyCharm-0.1.0.0-x64.msix

Successful signed package:
LuckyCharm-0.1.1.0-x64.msix

Successful Windows upgrade:
0.1.0.0 → 0.1.1.0
```

The signed `0.1.1.0` package installed successfully over the existing
`0.1.0.0` package without uninstalling it.

------------------------------------------------------------------------

# 20. One-Line Mental Model

For future reference:

``` text
CHANGE VERSION
      ↓
dotnet publish
      ↓
REBUILD PackageRoot
      ↓
COPY MANIFEST + ASSETS + SUPPORT
      ↓
winapp package + devcert
      ↓
SIGNED MSIX
      ↓
Add-AppxPackage
      ↓
VERIFY INSTALLED VERSION
```

This is the current known-good LuckyCharm MSIX workflow.
