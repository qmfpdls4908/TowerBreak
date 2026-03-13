using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class UnityCombatInstantiator : ICombatInstantiator
    {
        public GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            instance.SetActive(true);
            return instance;
        }
    }
}
