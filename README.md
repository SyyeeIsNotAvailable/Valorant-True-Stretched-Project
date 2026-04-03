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

- [QRes.exe](https://www.majorgeeks.com/files/details/qres.html) — drop it anywhere (but the best is to put it in the same folder as the app), the app will ask you to locate it
- A custom resolution created in your GPU control panel (NVIDIA / AMD) (VERY IMPORTANT)
- GPU scaling set to **Full-screen** + **GPU** in your driver settings (ALSO VERY IMPORTANT)

## Build

```bash
git clone https://github.com/SyyeeIsNotAvailable/Valorant-True-Stretched-Project.git
cd Valorant-True-Stretched-Project
dotnet build
dotnet run
```


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
