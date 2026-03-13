using System;

namespace TowerBreak.GameData
{
    public readonly struct ExcelParseContext
    {
        public ExcelParseContext(
            Type targetType,
            string text,
            object rawValue,
            string sheetName,
            int rowIndex,
            string columnName)
        {
            TargetType = targetType;
            Text = text;
            RawValue = rawValue;
            SheetName = sheetName;
            RowIndex = rowIndex;
            ColumnName = columnName;
        }

        public Type TargetType { get; }

        public string Text { get; }

        public object RawValue { get; }

        public string SheetName { get; }

        public int RowIndex { get; }

        public string ColumnName { get; }
    }
}
