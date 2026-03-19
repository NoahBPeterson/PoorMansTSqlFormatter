# Building the .NET 8 NativeAOT Notepad++ Plugin

## Prerequisites

- **.NET 8 SDK** (or .NET 10 SDK) installed on Windows
- **Visual Studio Build Tools** or **Visual Studio** with "Desktop development with C++" workload
  (required for NativeAOT linking on Windows)

## Build Steps

### For 64-bit Notepad++ (most common)

```powershell
cd PoorMansTSqlFormatterNppPlugin.Net8
dotnet publish -c Release -r win-x64
```

### For 32-bit Notepad++

```powershell
cd PoorMansTSqlFormatterNppPlugin.Net8
dotnet publish -c Release -r win-x86
```

The output will be a single native DLL at:
```
bin\Release\net8.0-windows\win-x64\native\PoorMansTSqlFormatterNppPlugin.dll
```
(or `win-x86\native\` for 32-bit)

## Installation

1. Navigate to your Notepad++ plugins directory:
   - Usually `C:\Program Files\Notepad++\plugins\`

2. Create a folder named `PoorMansTSqlFormatterNppPlugin`:
   ```
   C:\Program Files\Notepad++\plugins\PoorMansTSqlFormatterNppPlugin\
   ```

3. Copy the built `PoorMansTSqlFormatterNppPlugin.dll` into that folder:
   ```
   C:\Program Files\Notepad++\plugins\PoorMansTSqlFormatterNppPlugin\PoorMansTSqlFormatterNppPlugin.dll
   ```

4. Restart Notepad++.

## Usage

After installation, the plugin appears under `Plugins > Poor Man's T-Sql Formatter`:

- **Format T-SQL Code**: Formats the selected text as T-SQL. If nothing is selected,
  formats the entire document. If parsing errors are found, you'll be prompted before
  formatting is applied.

- **T-SQL Formatting Options...**: Opens the settings XML file in Notepad++ for editing.
  A message box shows all available options and their defaults. Settings are automatically
  picked up the next time you format code.

## Settings

Settings are stored in an XML file in the Notepad++ plugin config directory:
```
%APPDATA%\Notepad++\plugins\config\Poor Man's T-Sql Formatter.ini.xml
```

The settings file uses the same serialization format as the original plugin, so you can
migrate existing settings by copying the `OptionsSerialized` value.

### Available Options

| Option | Default | Description |
|--------|---------|-------------|
| IndentString | `\t` | Characters used for indentation (`\t` for tab, `\s` for space) |
| SpacesPerTab | 4 | Tab display width for line-length calculations |
| MaxLineWidth | 999 | Target maximum line width |
| ExpandCommaLists | True | Put each comma-separated item on its own line |
| TrailingCommas | False | Place commas at end of line instead of beginning |
| SpaceAfterExpandedComma | False | Add space after commas in expanded lists |
| ExpandBooleanExpressions | True | Put AND/OR on separate lines |
| ExpandBetweenConditions | True | Expand BETWEEN conditions |
| ExpandCaseStatements | True | Expand CASE/WHEN statements |
| ExpandInLists | True | Expand IN lists |
| UppercaseKeywords | True | Uppercase SQL keywords |
| BreakJoinOnSections | False | Break JOIN ON conditions |
| KeywordStandardization | False | Standardize keywords (e.g., INNER JOIN -> JOIN) |
| NewClauseLineBreaks | 1 | Number of line breaks before new clauses |
| NewStatementLineBreaks | 2 | Number of line breaks between statements |

## Targeting .NET 10

To build with .NET 10 instead of .NET 8, edit the `.csproj` file and change:
```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

Then build with the .NET 10 SDK installed.

## How It Works

This plugin uses **.NET NativeAOT** to compile to a single native DLL. This means:
- **No .NET runtime required** at runtime - the DLL is fully self-contained
- **No .NET Framework required** - completely independent of .NET Framework
- Single file deployment - just one DLL to copy
- Same formatting engine as the original plugin
