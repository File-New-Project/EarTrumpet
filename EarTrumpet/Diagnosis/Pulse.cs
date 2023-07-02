using System;
using System.Net.Http;
using System.Threading.Tasks;
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
            _timer.Elapsed += async (_, __) => await TrySendPulseAsync();
            _timer.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
                _ = TrySendPulseAsync();
            }
        }

        private async Task TrySendPulseAsync()
        {
            try
            {
                await _http.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://api.file-new-project.com/eartrumpet/pulse/"));
            }
            catch
            {
                // Do nothing
            }
        }
    }
}
