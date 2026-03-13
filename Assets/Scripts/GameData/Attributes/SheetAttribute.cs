using System;

namespace TowerBreak.GameData
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SheetAttribute : Attribute
    {
        public SheetAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
