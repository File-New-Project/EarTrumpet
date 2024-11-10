using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1060:Move pinvokes to native methods class")]
[assembly: SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible")]

// e.g. throw new NotImplementedException
[assembly: SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
[assembly: SuppressMessage("Interoperability", "WFAC010:Remove high DPI settings from App.manifest and configure via Application.SetHighDpiMode API or 'ApplicationHighDpiMode' project property")]