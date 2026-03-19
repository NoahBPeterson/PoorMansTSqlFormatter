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
using System.IO;
using System.Text;
using PoorMansTSqlFormatterLib;
using PoorMansTSqlFormatterLib.Formatters;

namespace PoorMansTSqlFormatterNppPlugin
{
    internal static class Main
    {
        internal const string PluginName = "Poor Man's T-Sql Formatter";

        private static SqlFormattingManager _formattingManager;

        internal static void CommandMenuInit()
        {
            // Get the plugin config directory from Notepad++
            StringBuilder sbIniFilePath = new StringBuilder(Npp.MAX_PATH);
            Npp.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Npp.MAX_PATH, sbIniFilePath);
            string iniFolder = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFolder))
                Directory.CreateDirectory(iniFolder);

            string settingsPath = Path.Combine(iniFolder, PluginName + ".ini.xml");
            Settings.Initialize(settingsPath);
            Settings.EnsureSettingsFileExists();

            _formattingManager = CreateFormattingManager(Settings.GetOptions());

            PluginBase.SetCommand(0, "Format T-SQL Code", FormatSqlCommand);
            PluginBase.SetCommand(1, "T-SQL Formatting Options...", FormattingOptionsCommand);
        }

        internal static void PluginCleanUp()
        {
        }

        internal static void FormatSqlCommand()
        {
            // Always reload settings before formatting, in case user edited the file
            Settings.InvalidateCache();
            _formattingManager = CreateFormattingManager(Settings.GetOptions());

            IntPtr currentScintilla = PluginBase.GetCurrentScintilla();

            // Get selection length (Scintilla returns buffer size including null terminator)
            int selectionBufferLength = (int)Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_GETSELTEXT, 0, 0);

            if (selectionBufferLength > 1)
            {
                // Get the selected text
                StringBuilder textBuffer = new StringBuilder(selectionBufferLength);
                Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_GETSELTEXT, 0, textBuffer);

                if (FormatAndWarn(textBuffer, out StringBuilder outBuffer))
                {
                    // Replace the selection with the formatted content
                    Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_REPLACESEL, 0, outBuffer);
                }
            }
            else
            {
                // No selection - format the entire document
                int docBufferLength = (int)Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_GETTEXT, 0, 0);
                int docCursorPosition = (int)Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_GETCURRENTPOS, 0, 0);
                StringBuilder textBuffer = new StringBuilder(docBufferLength);
                Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_GETTEXT, docBufferLength, textBuffer);

                if (FormatAndWarn(textBuffer, out StringBuilder outBuffer))
                {
                    // Approximate the cursor position proportionally
                    int newPosition = (docBufferLength > 1)
                        ? (int)Math.Round(1.0 * docCursorPosition * outBuffer.Length / (docBufferLength - 1), 0, MidpointRounding.AwayFromZero)
                        : 0;
                    Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_SETTEXT, 0, outBuffer);
                    Sci.SendMessage(currentScintilla, (uint)SciMsg.SCI_SETSEL, newPosition, newPosition);
                }
            }
        }

        private static bool FormatAndWarn(StringBuilder textBuffer, out StringBuilder outBuffer)
        {
            bool errorsEncountered = false;
            outBuffer = new StringBuilder(_formattingManager.Format(textBuffer.ToString(), ref errorsEncountered));

            if (errorsEncountered)
            {
                int result = Npp.MessageBox(
                    PluginBase.nppData._nppHandle,
                    "Errors found during SQL parsing. Would you like to apply formatting anyway?",
                    "Errors found. Continue?",
                    Npp.MB_OKCANCEL | Npp.MB_ICONWARNING);

                if (result != Npp.IDOK)
                    return false;
            }

            return true;
        }

        internal static void FormattingOptionsCommand()
        {
            string settingsPath = Settings.SettingsFilePath;
            if (string.IsNullOrEmpty(settingsPath))
                return;

            Settings.EnsureSettingsFileExists();

            // Open the settings file in Notepad++ for editing
            Npp.SendMessage(
                PluginBase.nppData._nppHandle,
                (uint)NppMsg.NPPM_DOOPEN,
                0,
                settingsPath);

            Npp.MessageBox(
                PluginBase.nppData._nppHandle,
                "The settings file has been opened in Notepad++.\n\n" +
                "Edit the OptionsSerialized value (comma-separated Key=Value pairs).\n" +
                "Save the file, then use Format T-SQL Code - new settings apply automatically.\n\n" +
                "Available options (only non-default values need to be listed):\n" +
                "  IndentString=\\t\n" +
                "  SpacesPerTab=4\n" +
                "  MaxLineWidth=999\n" +
                "  ExpandCommaLists=True\n" +
                "  TrailingCommas=False\n" +
                "  SpaceAfterExpandedComma=False\n" +
                "  ExpandBooleanExpressions=True\n" +
                "  ExpandBetweenConditions=True\n" +
                "  ExpandCaseStatements=True\n" +
                "  ExpandInLists=True\n" +
                "  UppercaseKeywords=True\n" +
                "  BreakJoinOnSections=False\n" +
                "  KeywordStandardization=False\n" +
                "  NewClauseLineBreaks=1\n" +
                "  NewStatementLineBreaks=2",
                "Poor Man's T-SQL Formatter - Options",
                Npp.MB_OK);
        }

        private static SqlFormattingManager CreateFormattingManager(TSqlStandardFormatterOptions options)
        {
            var formatter = new TSqlStandardFormatter(options);
            formatter.ErrorOutputPrefix = "--WARNING: Parsing error --" + Environment.NewLine;
            return new SqlFormattingManager(formatter);
        }
    }
}
