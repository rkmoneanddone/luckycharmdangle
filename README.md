# Lucky Dangle for Windows

A tiny transparent desktop companion that brings interactive lucky charms
to your Windows desktop.

Lucky Dangle stays on top of your desktop and lets you choose, move,
pull, swing, and randomly change between lucky dangles.

## Current Version

**0.1.0**

## Features

- Borderless transparent WPF desktop window
- Always-on-top
- Remembers its last desktop position
- Remembers the last selected dangle
- Interactive click-and-drag pull
- Spring and swing physics
- Right-click context menu
- Change Dangle
- Random Dangle
- Dangle collections
- Free, Premium, Seasonal, and category filtering
- Interactive dangle picker
- Persistent preferences
- Custom Lucky Dangle application icon
- Self-contained Windows release

## Dangle Collections

Current collections include:

- Lucky Charms
- Rakhi

### Individual Dangles

- Classic Lucky Coin
- Maneki Neko
- Classic Rakhi
- Royal Rakhi
- Peacock Rakhi

More dangles and collections can be added through the data-driven
dangle catalog.

## Technology

- C#
- .NET 10
- WPF
- Windows

## Project Structure

```text
LuckyDangle/
│
├── Assets/
│   ├── Dangles/
│   │   └── maneki_neko.png
│   └── LuckyDangle.ico
│
├── Dangles/
│   ├── DangleCatalog.cs
│   ├── DangleFactory.cs
│   ├── IDangle.cs
│   ├── ImageDangleBase.cs
│   ├── LuckyCoinDangle.cs
│   ├── ManekiNekoDangle.cs
│   ├── PeacockRakhiDangle.cs
│   ├── RakhiDangle.cs
│   └── RoyalRakhiDangle.cs
│
├── App.xaml
├── App.xaml.cs
├── CharmPickerWindow.xaml
├── CharmPickerWindow.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── LuckyDangle.csproj
├── Styles.xaml
└── README.md
```

## Build

Install the .NET 10 SDK on Windows.

Restore dependencies:

```powershell
dotnet restore
```

Build the application:

```powershell
dotnet build -c Release
```

Run the application:

```powershell
dotnet run
```

## Publish a Self-Contained Windows Build

To create a standalone Windows x64 release that does not require the
.NET runtime to be installed:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

The published application will be available under:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

Run the published application:

```text
LuckyDangle.exe
```

The self-contained release includes the required .NET runtime, so
users do not need to separately install .NET to run the published
application.

## Application Data

Lucky Dangle stores selected preferences locally on the Windows
computer.

Currently persisted preferences include:

- Selected dangle
- Window position

These preferences allow Lucky Dangle to restore the user's previous
desktop experience after restarting the application.

## Development

Lucky Dangle is currently developed as a Windows desktop application
using C# and WPF on .NET 10.

The project uses a data-driven dangle catalog so that additional
collections and dangles can be added without changing the core desktop
interaction system.

## Developer

**Rohit Kumar Mallick**

## Support

For questions, feedback, bug reports, or feature suggestions:

**wishugreens@gmail.com**

## Documentation

- [About Lucky Dangle](ABOUT.md)
- [Privacy Policy](PRIVACY.md)
- [Terms of Use](TERMS.md)
- [Support](SUPPORT.md)

## License

Copyright © 2026 Rohit Kumar Mallick.

All rights reserved.
