using System;

namespace EarTrumpet.Interop
{
    [Flags]
    enum SFGAO : uint
    {
        /// <summary>Objects can be copied</summary>
        /// <remarks>DROPEFFECT_COPY</remarks>
        CANCOPY = 0x1,
        /// <summary>Objects can be moved</summary>
        /// <remarks>DROPEFFECT_MOVE</remarks>
        CANMOVE = 0x2,
        /// <summary>Objects can be linked</summary>
        /// <remarks>
        /// DROPEFFECT_LINK.
        ///
        /// If this bit is set on an item in the shell folder, a
        /// 'Create Shortcut' menu item will be added to the File
        /// menu and context menus for the item.  If the user selects
        /// that command, your IContextMenu::InvokeCommand() will be called
        /// with 'link'.
        /// That flag will also be used to determine if 'Create Shortcut'
        /// should be added when the item in your folder is dragged to another
        /// folder.
        /// </remarks>
        CANLINK = 0x4,
        /// <summary>supports BindToObject(IID_IStorage)</summary>
        STORAGE = 0x00000008,
        /// <summary>Objects can be renamed</summary>
        CANRENAME = 0x00000010,
        /// <summary>Objects can be deleted</summary>
        CANDELETE = 0x00000020,
        /// <summary>Objects have property sheets</summary>
        HASPROPSHEET = 0x00000040,

        // unused = 0x00000080,

        /// <summary>Objects are drop target</summary>
        DROPTARGET = 0x00000100,
        CAPABILITYMASK = 0x00000177,
        // unused = 0x00000200,
        // unused = 0x00000400,
        // unused = 0x00000800,
        // unused = 0x00001000,
        /// <summary>Object is encrypted (use alt color)</summary>
        ENCRYPTED = 0x00002000,
        /// <summary>'Slow' object</summary>
        ISSLOW = 0x00004000,
        /// <summary>Ghosted icon</summary>
        GHOSTED = 0x00008000,
        /// <summary>Shortcut (link)</summary>
        LINK = 0x00010000,
        /// <summary>Shared</summary>
        SHARE = 0x00020000,
        /// <summary>Read-only</summary>
        READONLY = 0x00040000,
        /// <summary> Hidden object</summary>
        HIDDEN = 0x00080000,
        DISPLAYATTRMASK = 0x000FC000,
        /// <summary> May contain children with SFGAO_FILESYSTEM</summary>
        FILESYSANCESTOR = 0x10000000,
        /// <summary>Support BindToObject(IID_IShellFolder)</summary>
        FOLDER = 0x20000000,
        /// <summary>Is a win32 file system object (file/folder/root)</summary>
        FILESYSTEM = 0x40000000,
        /// <summary>May contain children with SFGAO_FOLDER (may be slow)</summary>
        HASSUBFOLDER = 0x80000000,
        CONTENTSMASK = 0x80000000,
        /// <summary>Invalidate cached information (may be slow)</summary>
        VALIDATE = 0x01000000,
        /// <summary>Is this removeable media?</summary>
        REMOVABLE = 0x02000000,
        /// <summary> Object is compressed (use alt color)</summary>
        COMPRESSED = 0x04000000,
        /// <summary>Supports IShellFolder, but only implements CreateViewObject() (non-folder view)</summary>
        BROWSABLE = 0x08000000,
        /// <summary>Is a non-enumerated object (should be hidden)</summary>
        NONENUMERATED = 0x00100000,
        /// <summary>Should show bold in explorer tree</summary>
        NEWCONTENT = 0x00200000,
        /// <summary>Obsolete</summary>
        CANMONIKER = 0x00400000,
        /// <summary>Obsolete</summary>
        HASSTORAGE = 0x00400000,
        /// <summary>Supports BindToObject(IID_IStream)</summary>
        STREAM = 0x00400000,
        /// <summary>May contain children with SFGAO_STORAGE or SFGAO_STREAM</summary>
        STORAGEANCESTOR = 0x00800000,
        /// <summary>For determining storage capabilities, ie for open/save semantics</summary>
        STORAGECAPMASK = 0x70C50008,
        /// <summary>
        /// Attributes that are masked out for PKEY_SFGAOFlags because they are considered
        /// to cause slow calculations or lack context
        /// (SFGAO_VALIDATE | SFGAO_ISSLOW | SFGAO_HASSUBFOLDER and others)
        /// </summary>
        PKEYSFGAOMASK = 0x81044000,
    }
}
