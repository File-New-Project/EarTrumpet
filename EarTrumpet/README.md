# EarTrumpet Technical Documentation
## Overview
EarTrumpet is a Windows Presentation Foundation (WPF) app consisting of a notification area icon with a context menu, flyout and a volume mixer window. Changing the default device is available on the context menu. Hotkeys can be assigned in settings to invoke the flyout, volume mixer or open settings. The flyout and volume mixer have effectively the same controls, in both cases there is a hidden popup on each app session (type <kbd>space</kbd> or right click) that reveal sliders for each item in the app group (e.g. multiple Firefox sessions running).

EarTrumpet uses the Windows Multimedia Device API to replace the system volume experience, enabling richer control of apps and devices through a modern UI with independent app controls.

The flyout is created at startup and held ready for display. The volume mixer and settings windows are single-instance and created on demand.

## EarTrumpet.DataModel.AppInformation
`AppInformationFactory` produces `IAppInfo` object from a process id (PID). Data (display name, icon location, background color, etc.) is uniquely extracted from desktop and modern apps. 

### Zombie processes
Lingering processes--processes that are dead but not yet cleaned up by the system--could show up in EarTrumpet.

#### Edge Content Process Repro
1. Start Legacy Edge, navigate to Youtube Page 1.
2. Add a tab and navigate to Youtube Page 2.
3. Close Page 1 tab.

Observe: Zombie process is still in EarTrumpet.

## EarTrumpet.DataModel.Audio
### IAudioDeviceManager
Manages an automatically updating observable collection of devices with a default device.

### IAudioDevice
Represents an audio device and its associated apps.

### IAudioDeviceSesson
Represents an app with an open audio session.

## EarTrumpet.DataModel.Storage
Settings are stored in a key/value store that is backed by Windows Storage or the registry, if the app is packaged or not respectively. `StorageFactory.GetSettings()` is used to retrieve and persist settings.

### Windows Storage reliablity
We have observed a high rate of failures (via telemetry) from the `Windows.Storage.ApplicationData.Current.LocalSettings.Values` API.

## EarTrumpet.DataModel
### SystemSettings
Accesses the user-configurable Windows settings that represent the user's personalization, accessibility, and globalization settings.

### ProcessWatcherService
`ProcessWatcherService` uses a background thread to wait (via `WaitForMultipleObjects`) on a list of process handles--processes EarTrumpet has audio sessions for--and waits to be signalled. This thread only waits for 5 seconds at a time, then dispatches notifications for any terminated processes.

## EarTrumpet.DataModel.WindowsAudio
The Windows audio implementation of `IAudioDeviceManager` and related interfaces.

### Persisted output devices
Windows has the facility to set the persisted playback device on a per-application basis. This setting is persisted however it is not reliably applied at startup. EarTrumpet mitigates this for the user by manually setting the persisted playback device.

### Peak values
To provide good performance, audio metering is sampled on a background thread, and then dispatched on the foreground thread as a batch.

## EarTrumpet.Diagnosis
### Bugsnag
We use Bugsnag as our error reporting service. Secure (TLS) connections are made to notify.bugsnag.com at notification time.

### CircularBufferTraceListener
Contains a small internal buffer of log messages that are only shown at the users' request.

### ErrorReporter
Encapsulates the Bugsnag connection and manages the metadata that is sent at notification time.

### LocalDataExporter
Transforms an `IAudioDeviceManager` into a string for debug purposes at the users' request.

### SnapshotData
Contains metadata dictionaries populated during crash notification or at the users' request.

## EarTrumpet.Extensibility
Contains support for loading add-on assemblies.

Add-ons must implement a single `IAddonLifecycle` in an external add-on assembly.

## EarTrumpet.Extensibility.Hosting
`AddonManager` uses an `AddonResolver` to load all applicable add-ons into an `AddonHost` (`AddonManager.Host`) where they can be accessed directly.

### Assembly loading
`AppDomain.CurrentDomain.AssemblyResolve` locates dependent assemblies residing in add-on directories.

### Version selection
Add-ons are placed in the `Versions\[EarTrumpet version]` folder. An add-on can support multiple versions of EarTrumpet in one package.

If an compatible EarTrumpet version can't be found, the add-on won't be loaded. See `AddonResolver` for more details.

## EarTrumpet.Extensibility.Shared
Hosts **required** static global data.

### ServiceBus
Used for cross add-on communication. An add-on could register at `ApplicationLifecycleEvent.Startup` and another could retrieve the service at `ApplicationLifecycleEvent.Startup2`.

## EarTrumpet.Interop
Contains Platform Invoke (P/Invoke) declarations and interfaces.

## EarTrumpet.Interop.Helpers
Contains P/Invoke wrappers.

## EarTrumpet.Interop.MMDeviceAPI
Contains core audio COM interfaces and declarations, generated from `mmdevapi.idl`. This is done by exporting to a Type Library (TLB), importing into Visual Studio, and then copying the generated artifacts. 

## EarTrumpet.UI
## EarTrumpet.UI.Helpers
### ShellNotifyIcon
`Shell_NotifyIcon` is called directly because `System.Windows.Forms.NotifyIcon` does not support the newer `NOTIFICATIONDATAW` structure containing the `guidItem` member. This allows the Windows Shell to migrate notification icon settings when the app install location changes.

#### Listening to mouse wheel scroll events
The `Shell_NotifyIcon` API, as of Windows 10 1903, doesn't emit scroll events. To capture scroll events, EarTrumpet uses the following scheme:

1. Windows Shell generates `WM_MOUSEMOVE` message in response to cursor movement.
2. EarTrumpet calls `RegisterRawInputDevices` to request `WM_INPUT` (global raw mouse input) messages.
3. `WM_INPUT` messages are processed until the cursor leaves the tray icon bounds (as defined by `Shell_NotifyIconGetRect`).

## EarTrumpet.UI.Themes
Contains WPF XAML resources used to configure the UI to resemble Windows themed UI.

EarTrumpet supports all the Windows theme configurations:
- High contrast
- Light or dark theme (system or app)
- Accent color (applied only to dark theme)
- Transparency (applied to all sans high contrast)

Historical insufficient solutions:
- Split Styles and DataTemplates
- Replacing a `ResourceDictionary` of Brushes

`Theme:Brush` applies colors at runtime using a special declaration in XAML.

### Brush
Theme brush values are specified on elements in XAML:
```xml
<TextBlock Theme:Brush.Foreground="SystemAccent" />
```

The simplest value is a static color: `Red` or `#FFaabbcc` or `SystemAccentDark1`. These colors will apply to all theme configurations.

Values can be set for Light, Dark and HighContrast configurations:
```
Light=LightChromeWhite, Dark=DarkChromeWhite, HighContrast=Highlight
```

The same value can also be written as:
```
Theme={Theme}ChromeWhite, HighContrast=Highlight
```

`{Theme}` is a variable that is replaced with `Light` or `Dark` as applicable.

The color channel alpha may be optionally modified using a value between `0.0-1.0`:
```
Color/<TransparencySetting:On|Off>
Color/<TransparencySetting:On>/<TransparencySetting:Off>
```

Example:
```
Light=LightChromeWhite/0.8
Light=LightChromeWhite/0.8/1
```

This allows colors to be opaque when transparency isn't being used.

Brush values reference colors located by:
- `System.Windows.Media.ColorConverter.ConvertFromString("*ColorName*")` (.e.g `#aa000000`)
- `EarTrumpet.Interop.ImmersiveSystemColors.Lookup("*ColorName*")` API (e.g. `SystemAccent1`)
- `System.Windows.Media.Colors.*ColorName*` (e.g. `Red`)
- `System.Windows.Media.SystemColors.*ColorName*` (e.g. `HotTrack`, for high contrast)
- A reference like `<Theme:Ref Key="*ColorName*">` in `App.xaml`.

#### Theme Manager references
Complex brush values can be built up using nested `<Theme:Ref.Rules>` and then referenced like a static color.

The `Rules` collection is traversed, evaluating each `Rule` using the following scheme:
1. Evaluate `On` condition
  - If True: Use `Value` or evaluate `Rules`
  - If False: Continue to next `Rule`

Once a `Rules` collection is entered, it must terminate with a `Value` or continue to a deeper `Rules` collection, not go back to the parent looking for a match.

If no rule is found at the end, this is an programming error. Ensure that the last rule is `On=Any` (the default) to prevent this.

#### Theme:Options.Source
Set at the top level window and may be `App` or `System`. This specifies the theme configuration source (from `SystemSettings`). Taskbar and flyout UI use the `System` theme, while other top-level windows use the `App` theme.

#### Theme:Options.Scope
This is used to specify different colors between the flyout and other experiences irrespective of theme configuration.

Adding scope requires that the same tokens (Light, Dark, HighContrast) be specified equally for both scopes (even if they are the same).

Example:
```
Flyout:Theme=Red, Flyout:HighContrast=HotTrack, :Theme=Blue, :HighContrast=HotTrack
```

In this instance the flyout will use the red color, while other surfaces will use blue. High contrast will always be `HotTrack`.

#### Theme:OS
Contains OS-specific dependency properties (only `IsWindows11` at this time) used for theming across multiple OS versions.

#### Adding a new Theme:Brush.*Property*
Because Brush uses Reflection, properties are identified by string name only. This is to avoid having to special case certain elemenets when a property exists with the same name on many common types of elements.

Adding a new brush should be accomplished by copying a block in `Brush.cs`.

### Moving audio sessions between devices
The view-model layer virtualizes moving sessions across devices. This virtualization is necessary for the case where the user moves a session that has an `Inactive` state. In this case, Windows will not move the session to the new device until the next time the stream becomes `Active`, resulting in incorrect placement in the UI.

## Build
EarTrumpet only targets x86. Some P/Invokes will need to be updated if targeting x64 or other platforms.

See [Compiling](../COMPILING.md) for more information.

### Prebuild script: prebuild.ps1
Updates `AssemblyInfo.cs` (binary file version info) and the package `AppxManifest.xml` are kept in sync. `Version.txt` is the source of truth for the version.

## Debugging
EarTrumpet can be debugged via the `EarTrumpet` project, or via the `EarTrumpet.Package` project, which is slower but enables debugging the packaged app.

### VSDebug configuration
`VSDebug` mode enables the following features over `Debug` mode:
- `TemporaryAppItemViewModel` is marked with a red background color.
- Avoid the app identity check which causes a handled startup exception.

## Miscellaneous test reminders
- Taskbar top/left/right/bottom (consider RTL!)
- Taskbar auto-hide
- Per-monitor DPI (Settings > change the scale for only one display on a multi-display system)
- Theme pivots: light/dark, Use accent color, Use transparency
- High contrast
- UIAutomation / accessibility
- Keyboard, Touch, Mouse input
- Move taskbar to non-primary display (different DPI)
- Move an audio session that is not currently playing sound
- Remote desktop
- Device add/remove