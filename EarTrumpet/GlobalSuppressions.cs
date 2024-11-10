using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1060:Move pinvokes to native methods class")]
[assembly: SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible")]
[assembly: SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
[assembly: SuppressMessage("Interoperability", "WFAC010:Remove high DPI settings from App.manifest and configure via Application.SetHighDpiMode API or 'ApplicationHighDpiMode' project property")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure")]
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Scope = "member", Target = "~M:EarTrumpet.App.CreateSettingsExperience~System.Windows.Window")]
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Scope = "member", Target = "~M:EarTrumpet.App.CreateMixerExperience~System.Windows.Window")]
