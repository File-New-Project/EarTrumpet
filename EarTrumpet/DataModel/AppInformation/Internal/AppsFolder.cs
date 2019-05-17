using EarTrumpet.Interop;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class AppsFolder
    {
        public static string ReadDisplayName(string appId)
        {
            var item = Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, appId, typeof(IShellItem2).GUID);
            return item.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
        }
    }
}
