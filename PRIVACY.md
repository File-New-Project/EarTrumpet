# Information Collected And Transmitted By EarTrumpet

First, a reminder: EarTrumpet is provided "as is", without warranty of any kind, express or
implied, including but not limited to the warranties of merchantability,
fitness for a particular purpose and noninfringement.

With that out of the way, here's a breakdown of all the information we may [collect](./EarTrumpet/Diagnosis/SnapshotData.cs) **at crash time** via Bugsnag.

### Application-Level
Includes:
* Exception information
  * Could, in rare cases, contain paths to applications on your computer
* Version number (e.g. 2.0.x.x)
* App state (e.g. is shutting down)
* App identity present (true/false)
* Time between starting and crashing (e.g. 30 minutes)

### Operating System-Level
Includes:
* Architecture (e.g. 32-bit)
* Windows Build (e.g. 17134.1.amd64fre.rs4_release.180410-1804)
* Available processors/cores (e.g. 8 cores)
* .NET Framework Common Language Runtime version (e.g. 4.0.30319.42000)
* Light/Dark mode configuration (light/dark)
* Right-to-Left configuration (on/off)
* Transparency configuration (on/off)
* Accent color configuration (on/off)
* System Animations configuration (on/off)
* High Contrast theme configuration (on/off)
* Language and region (e.g. en-US)

### Third-Party Policies

* Bugsnag https://docs.bugsnag.com/legal/privacy-policy/
* Microsoft Store https://docs.microsoft.com/en-us/legal/windows/agreements/store-policies