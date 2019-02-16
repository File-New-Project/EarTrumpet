using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.DataModel
{
    interface IPartWithApp
    {
        AppRef App { get; set; }
    }
}
