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
using System.Xml;
using PoorMansTSqlFormatterLib.Formatters;

namespace PoorMansTSqlFormatterNppPlugin
{
    /// <summary>
    /// Simple XML-based settings storage for .NET 8 NativeAOT compatibility.
    /// Replaces the .NET Framework ApplicationSettingsBase approach.
    /// Settings file is a simple XML document that can be edited directly.
    /// </summary>
    internal static class Settings
    {
        private static string _settingsPath;
        private static TSqlStandardFormatterOptions _cachedOptions;

        internal static string SettingsFilePath => _settingsPath;

        internal static void Initialize(string settingsPath)
        {
            _settingsPath = settingsPath;
            _cachedOptions = null;
        }

        internal static TSqlStandardFormatterOptions GetOptions()
        {
            if (_cachedOptions != null)
                return _cachedOptions;

            _cachedOptions = LoadOptions();
            return _cachedOptions;
        }

        internal static void SetOptions(TSqlStandardFormatterOptions options)
        {
            _cachedOptions = options;
            SaveOptions(options);
        }

        internal static void InvalidateCache()
        {
            _cachedOptions = null;
        }

        private static TSqlStandardFormatterOptions LoadOptions()
        {
            if (string.IsNullOrEmpty(_settingsPath) || !File.Exists(_settingsPath))
                return new TSqlStandardFormatterOptions();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_settingsPath);

                XmlElement optionsNode = (XmlElement)doc.SelectSingleNode("//Setting[@name='OptionsSerialized']");
                if (optionsNode != null && !string.IsNullOrEmpty(optionsNode.InnerText))
                {
                    return new TSqlStandardFormatterOptions(optionsNode.InnerText);
                }

                // Try loading individual settings (backward compatibility with old format)
                return LoadLegacyOptions(doc);
            }
            catch
            {
                return new TSqlStandardFormatterOptions();
            }
        }

        private static TSqlStandardFormatterOptions LoadLegacyOptions(XmlDocument doc)
        {
            var options = new TSqlStandardFormatterOptions();

            string GetSettingValue(string name, string defaultValue)
            {
                XmlElement node = (XmlElement)doc.SelectSingleNode($"//Setting[@name='{name}']");
                return node != null ? node.InnerText : defaultValue;
            }

            bool GetBool(string name, bool defaultValue) =>
                bool.TryParse(GetSettingValue(name, defaultValue.ToString()), out bool result) ? result : defaultValue;

            int GetInt(string name, int defaultValue) =>
                int.TryParse(GetSettingValue(name, defaultValue.ToString()), out int result) ? result : defaultValue;

            options.ExpandCommaLists = GetBool("ExpandCommaLists", true);
            options.TrailingCommas = GetBool("TrailingCommas", false);
            options.ExpandBooleanExpressions = GetBool("ExpandBooleanExpressions", true);
            options.ExpandCaseStatements = GetBool("ExpandCaseStatements", true);
            options.ExpandBetweenConditions = GetBool("ExpandBetweenConditions", true);
            options.UppercaseKeywords = GetBool("UppercaseKeywords", true);
            options.SpaceAfterExpandedComma = GetBool("SpaceAfterExpandedComma", false);
            options.KeywordStandardization = GetBool("KeywordStandardization", false);
            options.BreakJoinOnSections = GetBool("BreakJoinOnSections", false);
            options.ExpandInLists = GetBool("ExpandInLists", true);
            options.IndentString = GetSettingValue("IndentString", "\\t");
            options.SpacesPerTab = GetInt("SpacesPerTab", 4);
            options.MaxLineWidth = GetInt("MaxLineWidth", 999);
            options.NewClauseLineBreaks = GetInt("NewClauseLineBreaks", 1);
            options.NewStatementLineBreaks = GetInt("NewStatementLineBreaks", 2);

            return options;
        }

        private static void SaveOptions(TSqlStandardFormatterOptions options)
        {
            if (string.IsNullOrEmpty(_settingsPath))
                return;

            try
            {
                XmlDocument doc = new XmlDocument();

                if (File.Exists(_settingsPath))
                {
                    doc.Load(_settingsPath);
                }
                else
                {
                    doc.AppendChild(doc.CreateElement("Settings"));
                }

                SetSettingValue(doc, "OptionsSerialized", options.ToSerializedString());
                doc.Save(_settingsPath);
            }
            catch
            {
                // Silently fail on save errors
            }
        }

        private static void SetSettingValue(XmlDocument doc, string name, string value)
        {
            XmlElement node = (XmlElement)doc.SelectSingleNode($"//Setting[@name='{name}']");
            if (node == null)
            {
                node = doc.CreateElement("Setting");
                node.SetAttribute("name", name);
                doc.DocumentElement.AppendChild(node);
            }
            node.InnerText = value;
        }

        /// <summary>
        /// Creates a human-readable settings file with comments if it doesn't exist.
        /// </summary>
        internal static void EnsureSettingsFileExists()
        {
            if (string.IsNullOrEmpty(_settingsPath))
                return;

            if (File.Exists(_settingsPath))
                return;

            try
            {
                string dir = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Save default options to create the file
                SaveOptions(new TSqlStandardFormatterOptions());
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
