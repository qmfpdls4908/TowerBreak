using System;
using System.Collections.Generic;
using System.Linq;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public static class WaveSpawnPlanner
    {
        public static WaveSpawnPlan CreatePlan(FloorRow floor, IReadOnlyList<FloorWaveRow> floorWaves, IReadOnlyList<EnemyRow> enemies)
        {
            if (floor == null)
            {
                throw new ArgumentNullException(nameof(floor));
            }

            if (floorWaves == null)
            {
                throw new ArgumentNullException(nameof(floorWaves));
            }

            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            Dictionary<int, EnemyRow> enemyById = enemies.ToDictionary(enemy => enemy.Id);

            List<WaveSpawnEntry> entries = floorWaves
                .Where(wave => wave.FloorId == floor.Id)
                .OrderBy(wave => wave.WaveIndex)
                .ThenBy(wave => wave.SpawnOrder)
                .Select(wave => CreateEntry(wave, enemyById))
                .ToList();

            return new WaveSpawnPlan(floor.Id, entries);
        }

        private static WaveSpawnEntry CreateEntry(FloorWaveRow wave, IReadOnlyDictionary<int, EnemyRow> enemyById)
        {
            if (!enemyById.TryGetValue(wave.EnemyId, out EnemyRow enemy))
            {
                throw new InvalidOperationException($"Enemy id '{wave.EnemyId}' was not found for wave planning.");
            }

            if (string.IsNullOrWhiteSpace(enemy.PrefabKey))
            {
                throw new InvalidOperationException($"Enemy id '{wave.EnemyId}' is missing a prefab key.");
            }

            return new WaveSpawnEntry
            {
                WaveIndex = wave.WaveIndex,
                EnemyId = wave.EnemyId,
                SpawnOrder = wave.SpawnOrder,
                SpawnTime = wave.SpawnTime,
                Quantity = wave.Quantity,
                PrefabKey = enemy.PrefabKey
            };
        }
    }
}
