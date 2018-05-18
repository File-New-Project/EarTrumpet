namespace EarTrumpet.Interop.MMDeviceAPI
{
    public enum AudioSessionDisconnectReason
    {
        DeviceRemoval = 0,
        ServerShutdown = 1,
        FormatChanged = 2,
        SessionLogoff = 3,
        SessionDisconnected = 4,
        ExclusiveModeOverride = 5
    }
}