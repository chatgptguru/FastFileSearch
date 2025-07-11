using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FastFileSearch
{
    public static class Win32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll")]
        public static extern uint SHGetImageList(int iImageList, ref Guid riid, out IntPtr ppv);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("shell32.dll")]
        public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [DllImport("user32.dll")]
        public static extern bool TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern bool DestroyMenu(IntPtr hMenu);

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        public const uint SHGFI_DISPLAYNAME = 0x200;
        public const uint SHGFI_TYPENAME = 0x400;

        public const int SHIL_LARGE = 0x0;
        public const int SHIL_SMALL = 0x1;
        public const int SHIL_EXTRALARGE = 0x2;
        public const int SHIL_JUMBO = 0x4;

        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [ComImport]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItem
        {
            void BindToHandler(IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport]
        [Guid("000214e4-0000-0000-c000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IContextMenu
        {
            int QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            int InvokeCommand(IntPtr pici);
            int GetCommandString(uint idCmd, uint uType, IntPtr pReserved, StringBuilder pszName, uint cchMax);
        }

        public enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000,
            PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            PARENTRELATIVE = 0x80080001
        }
    }

    public static class IconExtractor
    {
        public static Icon GetFileIcon(string filePath, bool largeIcon = false)
        {
            Win32.SHFILEINFO shinfo = new Win32.SHFILEINFO();
            uint flags = Win32.SHGFI_ICON | (largeIcon ? Win32.SHGFI_LARGEICON : Win32.SHGFI_SMALLICON);
            
            if (System.IO.Directory.Exists(filePath))
                flags |= Win32.SHGFI_USEFILEATTRIBUTES;

            IntPtr hImgSmall = Win32.SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            
            if (hImgSmall != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = Icon.FromHandle(shinfo.hIcon);
                Icon clonedIcon = (Icon)icon.Clone();
                Win32.DestroyIcon(shinfo.hIcon);
                return clonedIcon;
            }

            return SystemIcons.Application;
        }

        public static Bitmap GetThumbnail(string filePath, int width = 96, int height = 96)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return null;

                string ext = System.IO.Path.GetExtension(filePath).ToLower();
                
                // For image files, create actual thumbnail
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
                {
                    using (var image = Image.FromFile(filePath))
                    {
                        return new Bitmap(image, width, height);
                    }
                }
                
                // For other files, return large icon
                using (var icon = GetFileIcon(filePath, true))
                {
                    return icon.ToBitmap();
                }
            }
            catch
            {
                return GetFileIcon(filePath, true).ToBitmap();
            }
        }
    }
}