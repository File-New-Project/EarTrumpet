using System;
using System.Net.Http;
using System.Timers;

namespace EarTrumpet.Diagnosis
{
    internal class Pulse
    {
        private readonly Timer _timer;
        private readonly HttpClient _http;

        public Pulse()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd($"EarTrumpet/{App.PackageVersion}");

            _timer = new Timer();
            _timer.Elapsed += (_, __) => TrySendPulse();
            _timer.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
                TrySendPulse();
            }
        }

        private void TrySendPulse()
        {
            try
            {
                _http.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://api.file-new-project.com/eartrumpet/pulse/"));
            }
            finally
            {
            }
        }
    }
}
