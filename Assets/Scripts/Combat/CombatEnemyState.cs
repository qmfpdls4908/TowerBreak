namespace TowerBreak.Combat
{
    public sealed class CombatEnemyState
    {
        public CombatEnemyState(int enemyId, int health, float pressure)
        {
            EnemyId = enemyId;
            Health = health;
            Pressure = pressure;
        }

        public int EnemyId { get; }

        public int Health { get; }

        public float Pressure { get; }
    }
}
