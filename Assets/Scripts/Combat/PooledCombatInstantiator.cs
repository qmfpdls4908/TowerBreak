using System;
using System.Collections.Generic;

using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class PooledCombatInstantiator : ICombatInstantiator
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> poolByPrefab = new();

        public GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (!poolByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                poolByPrefab.Add(prefab, pool);
            }

            GameObject instance;
            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
                instance.transform.SetParent(parent, false);
                instance.transform.position = position;
                instance.SetActive(true);
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, parent);
                instance.SetActive(true);
            }

            return instance;
        }

        public void Release(GameObject prefab, GameObject instance)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!poolByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                poolByPrefab.Add(prefab, pool);
            }

            instance.SetActive(false);
            instance.transform.SetParent(null, false);
            pool.Enqueue(instance);
        }
    }
}
