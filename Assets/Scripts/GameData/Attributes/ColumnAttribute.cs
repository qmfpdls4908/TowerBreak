using System;

namespace TowerBreak.GameData
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
