using UnityEngine;

namespace TowerBreak.Combat
{
    public interface ICombatInstantiator
    {
        GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent);
    }
}
