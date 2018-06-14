# Information Collected And Transmitted By EarTrumpet

First, a reminder: EarTrumpet is provided "as is", without warranty of any kind, express or
implied, including but not limited to the warranties of merchantability,
fitness for a particular purpose and noninfringement.

With that out of the way, here's a breakdown of all the information we may collect **at crash time** via Bugsnag.

### Application-Level
Includes:
* Exception information
  * Could, in rare cases, contain paths to applications on your computer
* Machine name ℹ
* Host name ℹ
* Version number (e.g. 2.0.x.x)

### Operating System-Level
Includes:
* Architecture (e.g. 32-bit)
* Version (e.g. Windows 10)
* Build (e.g. 17134.1.amd64fre.rs4_release.180410-1804)
* Available processors/cores (e.g. 8 cores)
* Machine Name (e.g. MyFastPC) ℹ
* .NET Framework Common Language Runtime version (e.g. 4.0.30319.42000)
* Light/Dark mode configuration (light/dark)
* Right-to-Left configuration (on/off)
* Transparency configuration (on/off)
* Accent color configuration (on/off)
* System Animations configuration (on/off)

ℹ We filed a bug with Bugsnag on the lack of filtering for these items. We don't want this data and we're actively working to remove the collection of this data.

### Third-Party Policies

* Bugsnag https://docs.bugsnag.com/legal/privacy-policy/
* Microsoft Store https://docs.microsoft.com/en-us/legal/windows/agreements/store-policies