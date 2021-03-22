using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.DataModel
{
    public interface IPartWithDevice
    {
        Device Device { get; set; }
    }
}
