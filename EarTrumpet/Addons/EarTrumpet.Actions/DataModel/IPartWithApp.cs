using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.DataModel;

internal interface IPartWithApp
{
    AppRef App { get; set; }
}
