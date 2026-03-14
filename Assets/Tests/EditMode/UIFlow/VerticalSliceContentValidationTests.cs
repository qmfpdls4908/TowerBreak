using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.UIFlow.Tests
{
    [TestFixture]
    public sealed class VerticalSliceContentValidationTests
    {
        private TowerBreakerGameData _data;

        [SetUp]
        public void SetUp()
        {
            _data = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
        }

        [Test]
        public void GameData_CanBeLoadedFromResources()
        {
            Assert.IsNotNull(_data,
                "TowerBreakerGameData.asset must exist at Assets/Resources/TowerBreakerGameData.asset");
        }

        [Test]
        public void FloorRows_ContainsFloors1Through3()
        {
            Assert.IsNotNull(_data);
            var ids = _data.Floors.Select(f => f.Id).ToList();
            Assert.Contains(1, ids, "FloorRow with Id=1 required");
            Assert.Contains(2, ids, "FloorRow with Id=2 required");
            Assert.Contains(3, ids, "FloorRow with Id=3 required");
        }

        [Test]
        public void EnemyRows_ContainsBothMvpArchetypes()
        {
            Assert.IsNotNull(_data);
            var archetypes = _data.Enemies.Select(e => e.Archetype).ToHashSet();
            Assert.IsTrue(archetypes.Contains(EnemyArchetype.BasicMelee),
                "EnemyRow with Archetype=BasicMelee required");
            Assert.IsTrue(archetypes.Contains(EnemyArchetype.ArmoredPusher),
                "EnemyRow with Archetype=ArmoredPusher required");
        }

        [Test]
        public void WeaponRows_ContainsBothMvpArchetypes()
        {
            Assert.IsNotNull(_data);
            var archetypes = _data.Weapons.Select(w => w.Archetype).ToHashSet();
            Assert.IsTrue(archetypes.Contains(WeaponArchetype.Claw),
                "WeaponRow with Archetype=Claw required");
            Assert.IsTrue(archetypes.Contains(WeaponArchetype.Lance),
                "WeaponRow with Archetype=Lance required");
        }

        [Test]
        public void RewardTableRows_ContainsOnePerMvpFloor()
        {
            Assert.IsNotNull(_data);
            Assert.GreaterOrEqual(_data.RewardTables.Count, 3,
                "At least one RewardTableRow per floor 1-3 required");
        }

        [Test]
        public void RewardEntryRows_EachTableHasAtLeastOneEntry()
        {
            Assert.IsNotNull(_data);
            Assert.Greater(_data.RewardEntries.Count, 0, "At least one RewardEntryRow required");
            foreach (var table in _data.RewardTables)
            {
                var count = _data.RewardEntries.Count(e => e.RewardTableId == table.Id);
                Assert.Greater(count, 0,
                    $"RewardTableId={table.Id} has no matching RewardEntries");
            }
        }

        [Test]
        public void RerollCostRows_ContainsCommonRarity()
        {
            Assert.IsNotNull(_data);
            var rarities = _data.RerollCosts.Select(r => r.Rarity).ToHashSet();
            Assert.IsTrue(rarities.Contains(WeaponRarity.Common),
                "RerollCostRow for WeaponRarity.Common required");
        }

        [Test]
        public void Scene_Bootstrap_FileExists()
        {
            var path = Path.Combine(Application.dataPath, "Scenes", "Bootstrap.unity");
            Assert.IsTrue(File.Exists(path),
                "Placeholder scene required at Assets/Scenes/Bootstrap.unity");
        }

        [Test]
        public void Scene_Lobby_FileExists()
        {
            var path = Path.Combine(Application.dataPath, "Scenes", "Lobby.unity");
            Assert.IsTrue(File.Exists(path),
                "Placeholder scene required at Assets/Scenes/Lobby.unity");
        }

        [Test]
        public void Scene_Battle_FileExists()
        {
            var path = Path.Combine(Application.dataPath, "Scenes", "Battle.unity");
            Assert.IsTrue(File.Exists(path),
                "Placeholder scene required at Assets/Scenes/Battle.unity");
        }

        [Test]
        public void PrefabDirectory_UI_Exists()
        {
            var path = Path.Combine(Application.dataPath, "Prefabs", "UI");
            Assert.IsTrue(Directory.Exists(path),
                "Assets/Prefabs/UI/ directory required for UI prefab surface");
        }
    }
}
