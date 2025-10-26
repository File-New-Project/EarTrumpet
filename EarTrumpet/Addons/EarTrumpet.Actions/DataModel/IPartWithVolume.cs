using EarTrumpet.Actions.DataModel.Enum;

namespace EarTrumpet.Actions.DataModel;

public interface IPartWithVolume
{
    double Volume { get; set; }
    VolumeUnit Unit { get; set; }
    SetVolumeKind Option { get; set; }
}
