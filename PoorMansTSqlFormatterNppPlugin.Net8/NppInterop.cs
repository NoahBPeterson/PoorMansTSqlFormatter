/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting
library for .Net 2.0 and JS, written in C#.
Copyright (C) 2011-2024 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PoorMansTSqlFormatterNppPlugin
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NppData
    {
        public IntPtr _nppHandle;
        public IntPtr _scintillaMainHandle;
        public IntPtr _scintillaSecondHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShortcutKey
    {
        public byte _isCtrl;
        public byte _isAlt;
        public byte _isShift;
        public byte _key;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FuncItem
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string _itemName;
        public IntPtr _pFunc;
        public int _cmdID;
        public int _init2Check;
        public IntPtr _pShKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCNotification
    {
        public NMHDR nmhdr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NMHDR
    {
        public IntPtr hwndFrom;
        public UIntPtr idFrom;
        public uint code;
    }

    public enum NppMsg : uint
    {
        NPPMSG = (0x400 + 1000),
        NPPM_GETCURRENTSCINTILLA = (NPPMSG + 4),
        NPPM_GETPLUGINSCONFIGDIR = (NPPMSG + 46),
        NPPM_DOOPEN = (NPPMSG + 77),
        NPPN_FIRST = 1000,
        NPPN_READY = (NPPN_FIRST + 1),
        NPPN_TBMODIFICATION = (NPPN_FIRST + 2),
        NPPN_SHUTDOWN = (NPPN_FIRST + 4),
    }

    public enum SciMsg : uint
    {
        SCI_GETSELTEXT = 2161,
        SCI_REPLACESEL = 2170,
        SCI_GETTEXT = 2182,
        SCI_SETTEXT = 2181,
        SCI_GETCURRENTPOS = 2008,
        SCI_SETSEL = 2160,
    }

    /// <summary>
    /// Notepad++ API calls (Unicode/UTF-16 strings).
    /// </summary>
    public static class Npp
    {
        [DllImport("user32", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, out int lParam);

        [DllImport("user32", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, string lParam);

        [DllImport("user32", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public const int MAX_PATH = 260;
        public const uint MB_OK = 0x00000000;
        public const uint MB_OKCANCEL = 0x00000001;
        public const uint MB_ICONWARNING = 0x00000030;
        public const int IDOK = 1;
    }

    /// <summary>
    /// Scintilla API calls (ANSI/UTF-8 strings).
    /// </summary>
    public static class Sci
    {
        [DllImport("user32", EntryPoint = "SendMessageA")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32", EntryPoint = "SendMessageA")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("user32", EntryPoint = "SendMessageA")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);

        [DllImport("user32", EntryPoint = "SendMessageA")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);
    }
}
