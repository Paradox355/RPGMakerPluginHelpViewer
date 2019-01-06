using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PluginGuides
{
    static class SystemIcons
    {
        private const int Folder = 4;
        private const int Floppy = 6;
        private const int Hard = 8;
        private const int Net = 9;
        private const int CD = 11;
        private const int RAM = 12;
        private const int Unknown = 53;
        private const int FolderFiles = 126;
        private const int Refresh = 238;

        [DllImport("shell32.dll", EntryPoint = "ExtractIcon")]

        extern static IntPtr ExtractIcon(
            IntPtr hInst,
            string lpszExeFileName,
            int nIconIndex);

        const string ShellIconsLib = @"C:\WINDOWS\System32\shell32.dll";

        static public Icon GetIcon(int index)
        {
            IntPtr Hicon = ExtractIcon(
                IntPtr.Zero, ShellIconsLib, index);
            Icon icon = Icon.FromHandle(Hicon);
            return icon;
        }


        static public Icon FolderIcon
        {
            get { return GetIcon(Folder); }
        }

        static public Icon FloppyDisk
        {
            get { return GetIcon(Floppy); }
        }

        static public Icon HardDisk
        {
            get { return GetIcon(Hard); }
        }

        static public Icon NetDisk
        {
            get { return GetIcon(Net); }
        }

        static public Icon CDROM
        {
            get { return GetIcon(CD); }
        }

        static public Icon RamDisk
        {
            get { return GetIcon(RAM); }
        }

        static public Icon UnknownDisk
        {
            get { return GetIcon(Unknown); }
        }

        static public Icon FolderFilesIcon
        {
            get { return GetIcon(FolderFiles); }
        }

        static public Icon RefreshIcon
        {
            get { return GetIcon(Refresh); }
        }

        static public ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
    }

}
