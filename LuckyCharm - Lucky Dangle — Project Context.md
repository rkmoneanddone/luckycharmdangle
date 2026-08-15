# LuckyCharm / Lucky Dangle — Project Context

## 1. Project Identity

**Project:** LuckyCharm / Lucky Dangle  
**Repository:** `rkmoneanddone/luckycharmdangle`  
**GitHub:** https://github.com/rkmoneanddone/luckycharmdangle  
**Default branch:** `main`

Lucky Dangle is a Windows desktop application that displays a decorative lucky charm / dangle on the desktop.

The project is currently a **WPF application targeting .NET 10 for Windows**.

The executable assembly/root namespace is:

```text
LuckyDangle
```

The application icon is:

```text
Assets\LuckyDangle.ico
```

---

# 2. Source of Truth

For all implementation questions, the **GitHub repository is the primary source of truth**.

Do not assume that remembered code is newer than the repository.

Before modifying an existing feature:

1. Inspect the current GitHub implementation.
2. Understand how the existing code works.
3. Make the smallest appropriate change.
4. Preserve existing behavior unless the user explicitly asks to change it.
5. Do not recreate architecture that already exists.

The ChatGPT Project context is primarily for:
- design decisions
- interaction rules
- artwork rules
- product intent
- conventions
- important constraints
- decisions that may not be obvious from source code

The repository is the authoritative source for:
- current C# code
- XAML
- resource declarations
- dangle classes
- artwork files
- picker implementation
- current implementation details

---

# 3. Current Technical Stack

The project currently uses:

- C#
- WPF
- .NET 10 for Windows
- Nullable reference types enabled
- Implicit usings enabled
- `WinExe`
- `LuckyDangle` assembly name
- `LuckyDangle` root namespace

Current project configuration includes:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

---

# 4. High-Level Architecture

The dangle system is intentionally designed around an interface/base-class architecture.

Main pieces include:

```text
Dangles/
├── IDangle.cs
├── ImageDangleBase.cs
├── DangleCatalog.cs
├── DangleFactory.cs
└── individual dangle classes
```

The UI includes:

```text
CharmPickerWindow.xaml
CharmPickerWindow.xaml.cs
```

Artwork is stored under:

```text
Assets/Dangles/
```

The project currently contains separate artwork and C# classes for multiple dangle designs.

---

# 5. IDangle Contract

`IDangle` defines the common metadata and rendering contract for every dangle.

Current important properties are:

```text
Id
Name
Description
Category
Collection
IsPremium
IsSeasonal
HangPointX
```

Every dangle must implement:

```text
void Render(Canvas canvas)
```

### Meaning of important metadata

### Id

Unique machine-readable identity.

Example:

```text
rakhi_classic
```

Do not casually change an existing dangle's ID after it is established.

### Name

Human-readable display name.

### Description

Short description of the design.

### Category

Broad classification such as:

```text
Festive
Luck
Spiritual
```

### Collection

Family/group of designs.

Examples:

```text
Rakhi
Lucky Charms
```

### IsPremium

Whether the specific design is commercially premium.

### IsSeasonal

Whether the design is seasonal, such as:

- Rakhi
- Diwali
- Christmas
- other seasonal designs

### HangPointX

This is extremely important.

`HangPointX` identifies the horizontal point from which the dangle should visually appear to hang.

The UI uses this value to center the artwork correctly.

It is NOT simply assumed that the visual artwork's bounding-box center is the correct hanging point.

---

# 6. ImageDangleBase

Most image-based dangles derive from:

```text
ImageDangleBase
```

The base class provides common rendering behavior.

Important configurable properties include:

```text
ImageWidth
ImageHeight
ImageLeft
ImageTop
ImageStretch
ImageScaleX
HangPointX
AssetPath
```

The current defaults include approximately:

```text
ImageWidth  = 120
ImageHeight = 160
ImageLeft   = 0
ImageTop    = 0
ImageStretch = Uniform
ImageScaleX = 1.0
```

The base implementation renders the artwork into a WPF `Canvas`.

Do not duplicate rendering infrastructure inside every dangle class unless a design genuinely requires custom rendering.

Prefer subclass-level metadata/positioning overrides when possible.

---

# 7. HangPointX — Core Design Rule

`HangPointX` is one of the most important concepts in the project.

The desktop dangle has a physical/visual hanging point.

The artwork must be positioned so that:

```text
HangPointX
```

corresponds to the actual top hanging point of the charm.

The picker explicitly centers previews using:

```text
preview center - dangle.HangPointX
```

Therefore:

**Do not "fix" artwork alignment by randomly changing Canvas positions in the picker.**

If a specific artwork is visually hanging from the wrong location, first investigate:

```text
HangPointX
ImageLeft
ImageTop
ImageWidth
ImageHeight
ImageScaleX
```

and the artwork itself.

---

# 8. Desktop Dangle Interaction — LOCKED BEHAVIOR

The Lucky Dangle desktop interaction has been intentionally designed and should be treated as locked unless the user explicitly requests a change.

### Default placement

The dangle initially appears at the **top-right of the desktop**.

### Independent repositioning

The entire dangle can be repositioned independently.

### Anchor behavior

The top anchor remains fixed relative to the dangle.

The user can pull/move the charm:

- left
- right
- up
- down

without breaking the relationship between the top anchor and the charm.

The anchor is part of the dangle's visual positioning system.

Do not introduce behavior where the anchor remains fixed to the screen while the charm moves independently.

---

# 9. Artwork Rules — VERY IMPORTANT

Future Lucky Dangle artwork must follow these rules.

## No long hanging cord/string

Artwork should **NOT contain a long top hanging string/cord**.

Only a small/minimal top attachment should be present.

Reason:

The actual desktop application handles the hanging/anchor relationship.

A long artificial string in the artwork wastes valuable display width/height and makes the charm itself smaller.

The goal is to maximize the usable size of the actual charm artwork.

### Preferred artwork composition

```text
small top attachment
        ↓
     actual charm
```

Not:

```text
long string
    ↓
    ↓
    ↓
    ↓
actual charm
```

---

# 10. Artwork Should Be Desktop-Friendly

New dangle artwork should be designed specifically for the Lucky Dangle desktop display.

Priorities:

1. Clear silhouette
2. Strong visual identity
3. Good readability at desktop-widget size
4. Minimal unnecessary transparent space
5. Minimal top attachment
6. Charm itself should occupy as much useful area as practical
7. Correct visual hanging point
8. Transparent background where appropriate
9. No unnecessary long cord/string

Do not create artwork that looks beautiful only when viewed as a large poster but becomes tiny when rendered inside the desktop dangle.

---

# 11. Charm Picker

The project has a dedicated:

```text
CharmPickerWindow
```

implemented using:

```text
CharmPickerWindow.xaml
CharmPickerWindow.xaml.cs
```

The picker provides:

- collection gallery
- individual dangle gallery
- filters
- premium/free/seasonal filtering
- category filtering
- collection navigation
- dangle selection

Current filter concepts include:

```text
All
Free
Premium
Seasonal
Festive
Luck
```

---

# 12. Picker Design Philosophy

The picker is intended to feel like a **premium charm collection**, not a generic file selector.

The current visual direction uses:

- navy
- ivory
- champagne
- gold
- muted premium typography
- rounded cards
- gold borders
- premium collection presentation

Important theme colors currently used include:

```text
Navy       #17243A
NavyLight  #243149
NavySoft   #31415A
Ivory      #FFF9EC
IvoryBright #FFFDF7
Champagne  #E6D4A7
Gold       #C9A34A
GoldBright #D9B65C
GoldDark   #8B681E
GoldSoft   #F5E7C2
TextDark   #17243A
TextMuted  #6E7B8F
```

When modifying the picker, preserve this premium visual language unless the user explicitly asks for a redesign.

---

# 13. Current Picker Sizing / Layout Conventions

The collection gallery and individual dangle gallery have deliberately tuned sizing.

### Collection cards

Current implementation uses approximately:

```text
Width  = 165
Height = 300
Margin = 6
```

Collection preview:

```text
Height = 215
```

Preview canvas:

```text
Width  = 120
Height = 215
```

Collection previews are centered according to `HangPointX`.

### Individual dangle cards

The individual dangle gallery has been tuned separately and should not automatically inherit collection-card dimensions.

Previously established project UI sizing uses approximately:

```text
Individual card:
Width  = 136
Height = 235
```

Artwork preview:

```text
Height = 145
```

Preview canvas:

```text
Width  = 100
Height = 155
```

These dimensions should be treated as intentional starting points.

Do not casually change them while fixing unrelated behavior.

---

# 14. Collection Architecture

`DangleCatalog` is the master list.

The catalog currently groups designs into collections such as:

```text
RAKHI COLLECTION

LUCKY CHARMS COLLECTION
```

The picker automatically derives collection views from the dangle metadata.

Therefore:

**Collection membership should be expressed through the dangle's `Collection` property.**

Do not hard-code individual dangles into the picker just to make them appear in a collection.

---

# 15. DangleCatalog Rule

`DangleCatalog` contains the master list of dangles.

Current architecture intentionally makes this simple:

```text
Add a new dangle HERE only.
```

The picker, categories and related selection functionality derive their data from the catalog.

When adding a new dangle, the catalog must be updated.

The expected registration pattern is:

```csharp
new SomeDangle(),
```

inside:

```text
DangleCatalog.AllDangles
```

Place the new registration in the appropriate collection section.

---

# 16. Adding a New Dangle — REQUIRED PROCESS

Whenever a new dangle is created, the implementation must include ALL of the following.

### 1. Artwork

Add:

```text
Assets/Dangles/<asset>.png
```

The artwork must follow the Lucky Dangle artwork rules.

### 2. C# class

Create:

```text
Dangles/<DangleClass>.cs
```

The class should follow the established `ImageDangleBase` pattern unless custom rendering is required.

### 3. `.csproj` resource

Add the exact resource declaration:

```xml
<Resource Include="Assets\Dangles\<asset>.png" />
```

### 4. DangleCatalog

Add:

```csharp
new <DangleClass>(),
```

to:

```text
DangleCatalog.AllDangles
```

### 5. Metadata

Set:

```text
Id
Name
Description
Category
Collection
IsPremium
IsSeasonal
```

appropriately.

### 6. HangPointX

Verify and set the correct hanging point.

Do not assume the image center is always correct.

---

# 17. New Dangle Response Convention

When the user asks for a new dangle, provide the integration information together with the artwork/implementation.

Always include:

### C# file

```text
Filename:
Dangles/<ClassName>.cs
```

### `.csproj`

Provide the exact line:

```xml
<Resource Include="Assets\Dangles\<asset>.png" />
```

### Catalog

Provide the exact line:

```csharp
new <ClassName>(),
```

This is a project convention and should not be forgotten.

---

# 18. Existing Dangle Families

The repository currently contains multiple dangles including designs such as:

### Rakhi

- Divine Rakhi
- Lotus Grace
- Shree Ganesha
- Kundan Heart
- Royal Peacock
- Lucky Elephant
- Bhai Mere Hero Rakhi

### Lucky Charms

- Evil Eye Shield
- Lucky Coin
- Maneki Neko
- Nimbu Mirchi
- Nazar Guardian
- Hamsa Khamsa
- Cornicello
- Jet Stone Higuerilla

The exact current class names and artwork filenames should always be checked in GitHub before adding or modifying anything.

---

# 19. Existing Artwork Naming Convention

Artwork filenames generally use lowercase snake_case.

Examples:

```text
maneki_neko.png
lucky_coin.png
nazar_guardian.png
hamsa_khamsa.png
cornicello.png
rakhi_divine.png
rakhi_kundan_heart.png
rakhi_shree_ganesha.png
```

Prefer this convention for future assets.

---

# 20. Existing C# Naming Convention

Dangle classes use PascalCase.

Examples:

```text
ManekiNekoDangle
LuckyCoinDangle
NazarGuardianDangle
HamsaKhamsaDangle
CornicelloDangle
```

The usual pattern is:

```text
<DesignName>Dangle
```

---

# 21. Do Not Break Existing Metadata Semantics

The project has intentionally separated:

```text
Category
Collection
Premium status
Seasonal status
```

Do not collapse these concepts into one property.

For example:

```text
Collection = Rakhi
Category   = Festive
IsSeasonal = true
```

can all legitimately describe the same dangle.

---

# 22. Premium / Free Architecture

The catalog provides:

```text
GetFree()
GetPremium()
```

based on:

```text
IsPremium
```

Therefore, premium status belongs to the individual dangle.

Do not implement premium filtering separately in the picker.

The picker should consume the catalog's filtering API.

---

# 23. Seasonal Architecture

Seasonal dangles are identified through:

```text
IsSeasonal
```

The catalog provides:

```text
GetSeasonal()
```

The picker exposes a Seasonal filter.

Do not create special-case UI code for each festival unless explicitly required.

---

# 24. Category Architecture

Categories are derived from the dangle metadata.

The catalog provides:

```text
GetCategory(string category)
GetCategories()
```

Category comparison is case-insensitive.

When adding a new category, prefer reusing an existing meaningful category if it fits.

Do not create many near-duplicate categories.

---

# 25. DangleFactory

`DangleFactory` exists as part of the dangle architecture.

Before creating a new mechanism for instantiating or resolving dangles, inspect the current `DangleFactory` implementation.

Do not bypass existing architecture unnecessarily.

---

# 26. Code Modification Philosophy

When changing Lucky Dangle code:

### Prefer

- small targeted changes
- existing abstractions
- existing metadata
- existing rendering pipeline
- existing catalog
- existing picker mechanisms
- minimal regression risk

### Avoid

- duplicated rendering logic
- hard-coded dangle-specific UI when metadata can drive it
- unnecessary architectural rewrites
- changing dimensions globally to solve one artwork problem
- breaking existing dangle alignment
- changing locked desktop interaction behavior without explicit instruction
- adding long cords to artwork
- moving logic from the catalog into the picker without a strong reason

---

# 27. Build / Release

The project is intended to support a self-contained Windows release.

Known publish approach:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

The application is a Windows executable and uses:

```text
LuckyDangle.ico
```

as its application icon.

---

# 28. Debugging Priority

When a visual problem occurs, diagnose it at the correct layer.

### If artwork is wrong:

Check:

```text
PNG artwork
ImageWidth
ImageHeight
ImageLeft
ImageTop
ImageScaleX
HangPointX
```

### If picker alignment is wrong:

Check:

```text
previewCanvas
HangPointX calculation
Viewbox
card dimensions
```

### If dangle registration is wrong:

Check:

```text
DangleCatalog
.csproj Resource Include
class name
AssetPath
```

### If desktop movement is wrong:

Inspect the desktop window/anchor interaction implementation before modifying artwork or picker code.

Do not use a UI workaround to hide a data/model problem.

---

# 29. Important Product Principle

The Lucky Dangle should feel like a **real desktop lucky charm**, not merely an image floating on the screen.

The visual relationship between:

```text
anchor
↓
dangle
```

is important.

The charm should feel physically suspended.

The artwork itself should therefore be designed around its actual hanging point.

---

# 30. Future Dangle Design Direction

The project is intended to grow into a collection of visually distinct lucky/festive/spiritual charms.

Possible future collections can include:

- Indian spiritual
- Indian festive
- global lucky charms
- protection
- prosperity
- success
- relationships
- cultural/festival collections

New collections should be represented through metadata rather than hard-coded picker logic.

---

# 31. Working With This Project in ChatGPT

When the user says something like:

> "Add a new dangle"

First inspect the current repository implementation and existing dangles.

When the user says:

> "Fix the picker"

Inspect the current `CharmPickerWindow.xaml` and `.xaml.cs` before proposing code.

When the user says:

> "Fix the positioning"

Inspect the current dangle rendering and `HangPointX` behavior before changing dimensions.

When the user says:

> "Create artwork"

Follow the artwork rules in this document, especially:

- no long hanging string
- minimal top attachment
- maximize useful charm size
- transparent background where appropriate
- preserve a clear hanging point

When the user asks for code, use the **current GitHub code as the implementation baseline**, not an older remembered version.

---

# 32. Golden Rules

These are the highest-priority project rules.

1. **GitHub is the source of truth for current code.**
2. **Do not make the user repeatedly explain the existing architecture.**
3. **Inspect existing implementation before changing it.**
4. **Do not break the locked desktop interaction.**
5. **The top anchor stays fixed relative to the dangle.**
6. **The dangle can be independently repositioned.**
7. **Use `HangPointX` for correct visual hanging alignment.**
8. **Do not put a long hanging cord/string in new artwork.**
9. **Keep the top attachment minimal.**
10. **Preserve the premium picker design language.**
11. **Use `DangleCatalog` as the master dangle list.**
12. **Use `Collection` metadata for collection membership.**
13. **Use `Category`, `IsPremium`, and `IsSeasonal` metadata instead of picker-specific hard-coding.**
14. **Every new dangle requires artwork + C# class + `.csproj` resource + catalog registration.**
15. **When creating a new dangle, always give the exact integration lines.**
16. **Prefer targeted changes over unnecessary rewrites.**
17. **Check the current repo before assuming a remembered implementation is still current.**

---

# 33. Current Repository Snapshot

At the time this project context was prepared, the repository contains the Lucky Dangle WPF application and the established dangle architecture described above.

The repository currently includes:

```text
App.xaml
App.xaml.cs
CharmPickerWindow.xaml
CharmPickerWindow.xaml.cs
LuckyDangle.csproj

Dangles/
    IDangle.cs
    ImageDangleBase.cs
    DangleCatalog.cs
    DangleFactory.cs
    ...individual dangle classes...

Assets/
    LuckyDangle.ico
    LuckyDangle.png
    Dangles/
        ...dangle artwork...
```

The exact file contents may evolve.

**Always treat the current GitHub repository as authoritative when this document and the code differ.**

---

# 34. Project Status

LuckyCharm / Lucky Dangle is an actively developed project.

Do not treat existing implementation as disposable prototype code.

Changes should preserve working behavior and existing design decisions unless the user explicitly asks for a redesign or architectural change.