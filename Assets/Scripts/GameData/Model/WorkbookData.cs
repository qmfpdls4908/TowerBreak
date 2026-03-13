using System;
using System.Collections.Generic;

namespace TowerBreak.GameData
{
    [Serializable]
    public sealed class WorkbookData
    {
        public WorkbookData(string sourcePath, IReadOnlyList<SheetData> sheets)
        {
            SourcePath = sourcePath;
            Sheets = sheets;
        }

        public string SourcePath { get; }

        public IReadOnlyList<SheetData> Sheets { get; }
    }
}
