using System;

namespace TowerBreak.GameData
{
    [Serializable]
    public readonly struct CellValue
    {
        public CellValue(object rawValue, string displayText)
        {
            RawValue = rawValue;
            DisplayText = displayText ?? string.Empty;
        }

        public object RawValue { get; }

        public string DisplayText { get; }
    }
}
