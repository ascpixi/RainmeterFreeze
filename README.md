<h1 align="center">❄ RainmeterFreeze</h1>
<p align="center">RainmeterFreeze is a lightweight app that freezes <a href="https://www.rainmeter.net/">Rainmeter</a> widgets when they're not visible to save CPU.</p>

## 💻 Setup
In order to run RainmeterFreeze, you'll need .NET Core 3.1 (or higher) - you probably already have it installed, but if you don't, you can get it from here: <a href="https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-3.1.24-windows-x64-installer">64-bit</a> | <a href="https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-3.1.24-windows-x86-installer">32-bit</a>

RainmeterFreeze doesn't require any installation - simply run the .exe file! If you want, you can also move the .exe into your Startup folder so that it starts alongside Rainmeter:
- Press `WIN` + `R`
- Type in "`shell:startup`" and press Enter
- Drag and Drop `RainmeterFreeze.exe` into the folder
- You're all set! (/≧▽≦)/

## ❓ Usage
RainmeterFreeze creates a tray icon that you can right click to configure - you can select from 3 algorithms ("Freeze when...") that dictate when to freeze Rainmeter, and from
2 modes that dictate *how* Rainmeter will be frozen.

#### **Freeze when... (Algorithms)**
- **Not on desktop** - if any window is in focus, Rainmeter will be frozen, even if the window itself does not cover the desktop. This can sometimes lead to glitchy behaviours with certain widgets.
- **Foreground window is maximized** - if the window that is in focus is maximized, Rainmeter will be frozen. This is the default.
- **When in full-screen mode** - Rainmeter will only be frozen if the window in focus covers the full area of the screen - for example, games running in full-screen mode.

#### **Mode**
- **Suspend** - Rainmeter will be suspended using Windows's thread suspend function. This will completely halt Rainmeter and ensure it stays at 0% CPU usage.
- **Low Priority** - Rainmeter will be set to have a lower priority than all other running processes.

## 🐛 Bugs
If you found a bug or want to file a feature request, please feel free to request them in the [Issues](https://github.com/ascpixi/RainmeterFreeze/issues) section! In order
for us to help you, we will need a **stacktrace** - fortunately, that's really easy to find!
- Press `WIN` + `R`
- Type in "`%appdata%/RainmeterFreeze/stacktrace.log`"
- A text file should open containing the stacktrace.
