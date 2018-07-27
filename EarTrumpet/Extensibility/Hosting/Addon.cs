namespace EarTrumpet.Extensibility.Hosting
{
    public class Addon
    {
        public string DisplayName => _path;

        private string _path;

        public Addon(string path)
        {
            _path = path;
        }
    }
}
