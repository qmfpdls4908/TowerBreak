using UnityEngine;

namespace TowerBreak.Combat
{
    public readonly struct EnemyDeathEvent
    {
        public int EnemyId { get; }
        public int FloorNumber { get; }
        public GameObject EnemyObject { get; }

        public EnemyDeathEvent(int enemyId, int floorNumber, GameObject enemyObject)
        {
            EnemyId = enemyId;
            FloorNumber = floorNumber;
            EnemyObject = enemyObject;
        }
    }
}
