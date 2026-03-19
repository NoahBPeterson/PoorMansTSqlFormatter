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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PoorMansTSqlFormatterNppPlugin
{
    internal delegate void NppFuncItemDelegate();

    internal static class PluginBase
    {
        internal static NppData nppData;
        internal static List<FuncItem> _funcItems = new List<FuncItem>();
        internal static IntPtr _nativeFuncItemsPtr = IntPtr.Zero;

        // Must keep delegate references alive to prevent GC collection
        internal static List<NppFuncItemDelegate> _delegates = new List<NppFuncItemDelegate>();

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            _delegates.Add(functionPointer);

            FuncItem funcItem = new FuncItem();
            funcItem._itemName = commandName;
            funcItem._pFunc = Marshal.GetFunctionPointerForDelegate(functionPointer);
            funcItem._cmdID = 0;
            funcItem._init2Check = 0;
            funcItem._pShKey = IntPtr.Zero;

            _funcItems.Add(funcItem);
        }

        internal static IntPtr GetNativeFuncItemsPointer(out int count)
        {
            count = _funcItems.Count;

            if (_nativeFuncItemsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_nativeFuncItemsPtr);
            }

            int sizeOfFuncItem = Marshal.SizeOf<FuncItem>();
            _nativeFuncItemsPtr = Marshal.AllocHGlobal(sizeOfFuncItem * count);

            for (int i = 0; i < count; i++)
            {
                IntPtr itemPtr = IntPtr.Add(_nativeFuncItemsPtr, i * sizeOfFuncItem);
                Marshal.StructureToPtr(_funcItems[i], itemPtr, false);
            }

            return _nativeFuncItemsPtr;
        }

        internal static void RefreshFuncItemCmdIDs()
        {
            if (_nativeFuncItemsPtr == IntPtr.Zero) return;

            int sizeOfFuncItem = Marshal.SizeOf<FuncItem>();
            for (int i = 0; i < _funcItems.Count; i++)
            {
                IntPtr itemPtr = IntPtr.Add(_nativeFuncItemsPtr, i * sizeOfFuncItem);
                FuncItem updated = Marshal.PtrToStructure<FuncItem>(itemPtr);
                _funcItems[i] = updated;
            }
        }

        internal static IntPtr GetCurrentScintilla()
        {
            Npp.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out int curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }
    }
}
