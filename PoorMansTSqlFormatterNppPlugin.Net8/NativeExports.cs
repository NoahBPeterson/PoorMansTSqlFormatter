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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PoorMansTSqlFormatterNppPlugin
{
    public static class NativeExports
    {
        [UnmanagedCallersOnly(EntryPoint = "isUnicode", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static int IsUnicode()
        {
            return 1; // BOOL TRUE
        }

        [UnmanagedCallersOnly(EntryPoint = "setInfo", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void SetInfo(NppData notepadPlusData)
        {
            PluginBase.nppData = notepadPlusData;
            Main.CommandMenuInit();
        }

        [UnmanagedCallersOnly(EntryPoint = "getFuncsArray", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe IntPtr GetFuncsArray(int* nbF)
        {
            IntPtr result = PluginBase.GetNativeFuncItemsPointer(out int count);
            *nbF = count;
            return result;
        }

        [UnmanagedCallersOnly(EntryPoint = "messageProc", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static uint MessageProc(uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        private static IntPtr _ptrPluginName = IntPtr.Zero;

        [UnmanagedCallersOnly(EntryPoint = "getName", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static IntPtr GetName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [UnmanagedCallersOnly(EntryPoint = "beNotified", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void BeNotified(SCNotification* notifyCode)
        {
            uint code = notifyCode->nmhdr.code;

            if (code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                PluginBase.RefreshFuncItemCmdIDs();
            }
            else if (code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                Main.PluginCleanUp();
                if (_ptrPluginName != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_ptrPluginName);
                    _ptrPluginName = IntPtr.Zero;
                }
            }
        }
    }
}
