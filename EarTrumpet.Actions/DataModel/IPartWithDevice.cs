using EarTrumpet_Actions.DataModel.Serialization;

namespace EarTrumpet_Actions.DataModel
{
    public interface IPartWithDevice
    {
        Device Device { get; set; }
    }
}
