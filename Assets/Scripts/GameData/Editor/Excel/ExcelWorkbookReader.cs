using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;

namespace TowerBreak.GameData.Editor
{
    public sealed class ExcelWorkbookReader : IWorkbookReader
    {
        private static bool encodingProviderRegistered;

        public WorkbookData Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Excel path is required.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Excel file was not found.", filePath);
            }

            EnsureEncodingProviderRegistered();

            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

            DataSet dataSet = reader.AsDataSet();
            List<SheetData> sheets = new(dataSet.Tables.Count);

            for (int tableIndex = 0; tableIndex < dataSet.Tables.Count; tableIndex++)
            {
                DataTable table = dataSet.Tables[tableIndex];
                sheets.Add(ConvertSheet(table));
            }

            return new WorkbookData(filePath, sheets);
        }

        private static void EnsureEncodingProviderRegistered()
        {
            if (encodingProviderRegistered)
            {
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encodingProviderRegistered = true;
        }

        private static SheetData ConvertSheet(DataTable table)
        {
            List<string> headers = new();
            List<IReadOnlyList<CellValue>> rows = new();

            if (table.Rows.Count > 0)
            {
                DataRow headerRow = table.Rows[0];
                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    headers.Add(headerRow[columnIndex]?.ToString()?.Trim() ?? string.Empty);
                }
            }

            for (int rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
            {
                DataRow dataRow = table.Rows[rowIndex];
                List<CellValue> cells = new(table.Columns.Count);
                bool hasAnyValue = false;

                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    object rawValue = dataRow[columnIndex];
                    string displayText = rawValue?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(displayText))
                    {
                        hasAnyValue = true;
                    }

                    cells.Add(new CellValue(rawValue, displayText));
                }

                if (hasAnyValue)
                {
                    rows.Add(cells);
                }
            }

            return new SheetData(table.TableName, headers, rows);
        }
    }
}
