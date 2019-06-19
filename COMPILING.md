# Compiling EarTrumpet

## Requirements
* [Visual Studio 2017](https://visualstudio.microsoft.com/vs/community/) (or newer)
* [Git for Windows](https://git-scm.com/download/win)
* [Windows 10 Anniversary Update](https://blogs.windows.com/windowsexperience/2016/08/02/how-to-get-the-windows-10-anniversary-update/#GD97Eq04wJA7S4P7.97) (or newer)
* [.NET Framework 4.6.2 Developer Pack](https://www.microsoft.com/net/download/thank-you/net462-developer-pack)

## Step-by-step
1. Install Visual Studio 2017 with the `.NET desktop development` and `Universal Windows Platform development` workloads. Ensure the optional `Windows 10 SDK (10.0.14393.0)` component is also selected.
2. Install the .NET Framework 4.6.2 Developer Pack.
3. Install Git for Windows.
4. Clone the EarTrumpet repository (`git clone https://github.com/File-New-Project/EarTrumpet.git`).
5. Open `EarTrumpet.vs15.sln` in Visual Studio.
6. Change the target platform to `x86` and build the `EarTrumpet.Package` project.
7. You're done. If you plan on submitting your changes to us, please review the [Contributing guide](https://github.com/File-New-Project/EarTrumpet/blob/master/CONTRIBUTING.md) first.