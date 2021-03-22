using EarTrumpet.DataModel.Storage;

namespace EarTrumpet.Actions.DataModel
{
    public class LocalVariablesContainer
    {
        public bool this[string key]
        {
            get => _settings.Get($"{s_localVariablePrefix}{key}", false);
            set => _settings.Set($"{s_localVariablePrefix}{key}", value);
        }

        private const string s_localVariablePrefix = "LocalVariable.";
        private readonly ISettingsBag _settings;

        public LocalVariablesContainer(ISettingsBag settings)
        {
            _settings = settings;
        }
    }
}
