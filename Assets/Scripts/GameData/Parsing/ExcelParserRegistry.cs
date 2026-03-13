using System;
using System.Collections.Generic;

namespace TowerBreak.GameData
{
    public sealed class ExcelParserRegistry
    {
        private readonly List<IExcelValueParser> parsers = new();

        public IReadOnlyList<IExcelValueParser> Parsers => parsers;

        public void Register(IExcelValueParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            parsers.Add(parser);
        }

        public bool TryParse(in ExcelParseContext context, out object value)
        {
            for (int index = 0; index < parsers.Count; index++)
            {
                IExcelValueParser parser = parsers[index];
                if (!parser.CanParse(context.TargetType))
                {
                    continue;
                }

                value = parser.Parse(context);
                return true;
            }

            value = null;
            return false;
        }
    }
}
