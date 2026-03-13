using System;

namespace TowerBreak.GameData
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
