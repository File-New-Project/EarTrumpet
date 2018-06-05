using Bugsnag;
using Bugsnag.Clients;
using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;

namespace EarTrumpet.Services
{
    class ErrorReportingService
    {
        internal static void Initialize()
        {
            WPFClient.Config.AppVersion = App.Current.HasIdentity() ? Package.Current.Id.Version.ToVersionString() : "DevInternal";
            WPFClient.Start();

            WPFClient.Config.BeforeNotify(OnBeforeNotify);
        }

        private static bool OnBeforeNotify(Event error)
        {
            error.Metadata.AddToTab("Device", "machineName", "<redacted>");
            error.Metadata.AddToTab("Device", "hostname", "<redacted>");

            return true;
        }
    }
}
