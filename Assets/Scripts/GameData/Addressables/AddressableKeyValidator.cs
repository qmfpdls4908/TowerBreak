using System;

namespace TowerBreak.GameData.Addressables
{
    public sealed class AddressableKeyValidator
    {
        public string ValidateRequired(string key, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Addressable key is required.", parameterName);
            }

            return key;
        }
    }
}
