using System;

namespace TowerBreak.GameData
{
    public interface IExcelValueParser
    {
        bool CanParse(Type targetType);

        object Parse(in ExcelParseContext context);
    }
}
