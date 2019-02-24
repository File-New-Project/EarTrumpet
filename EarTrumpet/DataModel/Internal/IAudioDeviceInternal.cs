namespace EarTrumpet.DataModel.Internal
{
    interface IAudioDeviceInternal
    {
        void UpdatePeakValue();
        void MoveHiddenAppsToDevice(string appId, string id);
        void UnhideSessionsForProcessId(int processId);
    }
}
