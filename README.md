# Valorant True Stretched

An app to simplify applying a true stretched resolution on Valorant.

## How it works

1. Select your `GameUserSettings.ini` (usually in `%LOCALAPPDATA%\VALORANT\Saved\Config\<id>\Windows\`)
2. Select `QRes.exe`
3. Enter your stretched resolution (e.g. `1440x1080`)
4. Hit **START**

The app will:
- Backup the original `.ini` to `Backup_INI/`
- Patch all resolution keys + force `FullscreenMode=2`, `bShouldLetterbox=False`
- Lock the file as read-only so Valorant can't overwrite it
- Switch Windows resolution via QRes

Hit **STOP** to restore everything.

Paths and resolution are saved between sessions (`settings.json`).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [QRes.exe](https://www.majorgeeks.com/files/details/qres.html) — drop it anywhere, the app will ask you to locate it
- A custom resolution created in your GPU control panel (NVIDIA / AMD)
- GPU scaling set to **Full-screen** + **GPU** in your driver settings

## Build

```bash
git clone https://github.com/<your-username>/ValorantTrueStretched.git
cd ValorantTrueStretched
dotnet build
dotnet run
```

## Publish a standalone .exe

```bash
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

Output will be in `bin/Release/net8.0-windows/win-x64/publish/`.

## Project structure

```
├── App.xaml / App.xaml.cs          # Entry point, exit cleanup
├── MainWindow.xaml / .xaml.cs      # UI + event handlers
├── AppOrchestrator.cs              # Start/Stop lifecycle
├── ConfigManager.cs                # INI backup, patch, lock/unlock
├── ResolutionManager.cs            # QRes wrapper, native res detection
├── UserSettings.cs                 # Persists paths & resolution
├── Styles/DarkTheme.xaml           # WPF theme
├── app.ico                         # Exe icon
└── icon.png                        # Window icon
```

## INI keys patched

| Key | Value |
|-----|-------|
| `ResolutionSizeX/Y` | stretched res |
| `LastUserConfirmedResolutionSizeX/Y` | stretched res |
| `DesiredScreenWidth/Height` | stretched res |
| `LastUserConfirmedDesiredScreenWidth/Height` | stretched res |
| `bShouldLetterbox` | `False` |
| `bLastConfirmedShouldLetterbox` | `False` |
| `bUseVSync` | `False` |
| `bUseDynamicResolution` | `False` |
| `FullscreenMode` | `2` (injected if missing) |
| `LastConfirmedFullscreenMode` | `2` |
| `PreferredFullscreenMode` | `2` |

## License

MIT
