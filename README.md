# Lucky Dangle for Windows

A tiny transparent desktop companion that hangs from the top-right corner.

## Current MVP

- Borderless transparent WPF window
- Always-on-top
- Automatically positions at the top-right of the primary monitor
- Click-drag to pull the dangle
- Release to let it swing and settle using simple damped-pendulum physics
- Right-click context menu
- Swing action
- Random action placeholder
- Exit action

## Next

1. Data-driven charm catalog
2. Transparent PNG/WebP charm artwork
3. Charm picker
4. Size control
5. Position/monitor selection
6. Start with Windows
7. System tray icon
8. Save preferences
9. Installer
10. Optional charm packs

## Build

Install the .NET 8 SDK on Windows, then:

```powershell
dotnet restore
dotnet build -c Release
dotnet run
```
