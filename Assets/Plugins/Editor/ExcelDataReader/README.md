# ExcelDataReader DLL bundle

Place the following editor-only DLLs in this folder:

- `ExcelDataReader.dll`
- `ExcelDataReader.DataSet.dll`
- `System.Text.Encoding.CodePages.dll`

The default importer implementation in `Assets/Scripts/GameData/Editor/Excel/ExcelWorkbookReader.cs` expects these files.

Recommended source:
- NuGet package `ExcelDataReader` 3.8.0
- NuGet package `ExcelDataReader.DataSet` 3.8.0
- NuGet package `System.Text.Encoding.CodePages`

You can also run the bootstrap script at `Tools/setup-excel-reader.ps1` to download and extract them automatically.
