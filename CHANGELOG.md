# Changelog

## vNext
- Disable Alt+Space on the flyout window
- Limit the internal log buffer
- Fix a rendering issue when the DPI was greater than 100% and there were more devices than would fit in the flyout without a scrollbar
- Fix a rendering issue where the Notification Area icon becomes blurry at DPIs geater than 100%
- Fix the icon and name of recording devices in 'Listen' mode
- More High Contrast theme color updates
- Per-Monitor DPI support
- Fixes to Notification Area icon scaling
- Fix overflow flyout at greater than 100% DPI

## 2.0.5.0
- Fixed System Sounds icon on ARM64
- High Contrast colors updated
- Added collection of debug information when device enumeration fails

## 2.0.4.0
- Scoped the mouse wheel scrolling to only over the Notification Area icon.
- Added support for RS3.
- Fixed a bug that caused the flyout to show 'It doesn't look like you have any playback devices.' when removing the default device.
- Fixed a crash during adding/removing devices.
- Removed non-fatal errors from telemetry.

## 2.0.3.0
- Fixed an issue with certain apps (e.g. Sea of Thieves) not appearing correctly
- Added additional languages (Arabic, Spanish, Hungarian, Korean, Turkish, and Ukrainian)

## 2.0.2.0
- Changes to telemetry 

## 2.0.1.0
- Fixed a crash when a device is quickly added/removed
- Fixed a crash with multi-process audio sessions
- Fixed a crash launching web links
- Fixed a crash when closing EarTrumpet windows
- Fixed a crash when expanding/collapsing the main window
- Fixed a crash when Taskbar auto-hide is in use
- Fixed a crash when the registry has invalid personalization data
- Fixed a crash when calling application data storage APIs

## 2.0.0.0
- Added middle click on notification area icon to mute
- Added ability to use mouse wheel to change device volume when window is open
- Added multi-channel peak metering
- Added ability to move apps between devices
- Added ability to view multiple devices
- Added volume mixer window
- Enhanced app session grouping
- Enhanced keyboarding
- Added keyboard shortcut to open flyout
- Added support for Windows light/dark mode
- Added Sounds, Recording, etc. links to context menu
- Enhanced animations and detail work
- Additional fixes for RTL, Accessibility, and apps without icons

## 1.5.3.0
- Improved slider performance
- Fixed Acrylic Blur compatibility for Windows 10 1803

## 1.5.2.0

## 1.5.1.0

## 1.5.0.0

## 1.4.4.0

## 1.4.3.0

## 1.4.2.0

## 1.4.1.0

## 1.4.0.0

## 1.3.2.0
- Fixed changing audio devices in Windows 10 (RS1)

## 1.3.1.0
- Fixed DWM scaling issue where window appeared in the wrong position
- Fixed UI issues when no audio devices found
- Fixed changing audio devices in Windows 10 (TH1) and Windows 10 November Update (TH2)
- Fixed multiple sessions not appearing for some applications
- Added company metadata for Task Manager

## 1.3.0.0
- Fixed Speech Runtime display
- Fixed positioning when Taskbar auto-hide is enabled
- Installer/uninstaller now checks if the app is running
- Added ability to change default audio device (right-click the tray icon)
- Added ability to mute apps/audio device
- Added default audio device master volume slider

## 1.2.0.0
- Fixed issue with a number of apps not appearing in Ear Trumpet when using background audio services (e.g. iHeartRadio)
- Fixed issue with a number of apps not appearing in Ear Trumpet when playing protected media (e.g. Netflix)
- Fixed issue with apps not showing due to unexpected logo/icon paths (e.g. Skype Translator)
- Added base localization to Ear Trumpet (defaults to English for now - feel free to provide translations as pull requests)

## 1.1.1.0

## 1.1.0.0
- Fixed DPI scaling issue
- Fixed Ear Trumpet not displaying correctly when the Taskbar was in a different location or not on the primary monitor
- Initial fix for modern app missing extracted logo
- Ear Trumpet will now only allow one open instance
- GitHub readme updated with details and minimum versions
- Installer no longer allows installs on Windows versions before Windows 10
- Fixed issue with Windows 10 tablet mode
- Fixed Ear Trumpet window not having the correct border and drop shadow 

## 1.0.0.0
- Initial release