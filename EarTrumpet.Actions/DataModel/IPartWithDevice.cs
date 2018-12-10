using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.DataModel
{
    interface IPartWithDevice
    {
        Device Device { get; set; }
    }
}
