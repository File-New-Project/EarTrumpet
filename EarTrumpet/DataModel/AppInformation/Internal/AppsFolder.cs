using EarTrumpet.Interop;

namespace EarTrumpet.DataModel.AppInformation.Internal
{
    class AppsFolder
    {
        public static string ReadDisplayName(string appId)
        {
            var iid = typeof(IShellItem2).GUID;
            var shellItem = Shell32.SHCreateItemInKnownFolder(ref FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, appId, ref iid);
            return shellItem.GetString(ref PropertyKeys.PKEY_ItemNameDisplay);
        }
    }
}
