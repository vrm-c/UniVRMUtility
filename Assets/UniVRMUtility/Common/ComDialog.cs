using System;
using System.Runtime.InteropServices;

namespace UniVRMUtility
{
    //
    // https://docs.microsoft.com/en-us/windows/win32/learnwin32/example--the-open-dialog-box
    // https://qiita.com/otagaisama-1/items/b0804b9d6d37d82950f7
    //
    public static class ComDialog
    {
        enum SIGDN : uint // not fully defined
        {
            _FILESYSPATH = 0x80058000,
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItem
        {
            void BindToHandler(); // not fully defined
            void GetParent(); // not fully defined
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes();  // not fully defined
            void Compare();  // not fully defined
        }

        [Flags]
        enum FOS // not fully defined
        {
            _FORCEFILESYSTEM = 0x40,
            _PICKFOLDERS = 0x20,
        }

        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IFileDialog
        {
            [PreserveSig]
            UInt32 Show([In] IntPtr hwndParent);
            void SetFileTypes();     // not fully defined
            void SetFileTypeIndex();     // not fully defined
            void GetFileTypeIndex();     // not fully defined
            void Advise(); // not fully defined
            void Unadvise();
            void SetOptions([In] FOS fos);
            void GetOptions(); // not fully defined
            void SetDefaultFolder(); // not fully defined
            void SetFolder(IShellItem psi);
            void GetFolder(); // not fully defined
            void GetCurrentSelection(); // not fully defined
            void SetFileName();  // not fully defined
            void GetFileName();  // not fully defined
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel(); // not fully defined
            void SetFileNameLabel(); // not fully defined
            void GetResult(out IShellItem ppsi);
            void AddPlace(); // not fully defined
            void SetDefaultExtension(); // not fully defined
            void Close(); // not fully defined
            void SetClientGuid();  // not fully defined
            void ClearClientData();
            void SetFilter(); // not fully defined
        }

        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IFileOpenDialog : IFileDialog
        {
            void GetResults(); // not fully defined
            void GetSelectedItems(); // not fully defined
        }

        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        class CLSID_FileOpenDialog
        {
        }

        [ComImport]
        [Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IFileSaveDialog : IFileOpenDialog
        {
            uint SetSaveAsItem(IShellItem psi);
            uint SetProperties();
            uint SetCollectedProperties();
            uint GetProperties();
            uint ApplyProperties();
        }

        [ComImport]
        [Guid("C0B4E2F3-BA21-4773-8DBA-335EC946EB8B")]
        private class CLSID_FileSaveDialog
        {
        }

        public static string Open()
        {
            var dlg = new CLSID_FileOpenDialog() as IFileOpenDialog;
            var hr = dlg.Show(hwndParent: IntPtr.Zero);
            if (hr != 0)
            {
                return null;
            }
            IShellItem pItem;
            dlg.GetResult(out pItem);
            string pszFilePath;
            pItem.GetDisplayName(SIGDN._FILESYSPATH, out pszFilePath);
            return pszFilePath;
        }

        public static string Save()
        {
            var dlg = new CLSID_FileSaveDialog() as IFileSaveDialog;
            var hr = dlg.Show(hwndParent: IntPtr.Zero);
            if (hr != 0)
            {
                return null;
            }
            IShellItem pItem;
            dlg.GetResult(out pItem);
            string pszFilePath;
            pItem.GetDisplayName(SIGDN._FILESYSPATH, out pszFilePath);
            return pszFilePath;
        }
    }
}
