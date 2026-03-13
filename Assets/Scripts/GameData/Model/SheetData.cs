using System;
using System.Collections.Generic;

namespace TowerBreak.GameData
{
    [Serializable]
    public sealed class SheetData
    {
        public SheetData(string name, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<CellValue>> rows)
        {
            Name = name;
            Headers = headers;
            Rows = rows;
        }

        public string Name { get; }

        public IReadOnlyList<string> Headers { get; }

        public IReadOnlyList<IReadOnlyList<CellValue>> Rows { get; }
    }
}
