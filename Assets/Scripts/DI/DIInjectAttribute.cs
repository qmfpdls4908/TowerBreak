using System;

namespace TowerBreak.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DIInjectAttribute : Attribute
    {
        public DIInjectAttribute(string key = null)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
