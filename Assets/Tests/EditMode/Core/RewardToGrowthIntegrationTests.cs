using System;
using System.Collections.Generic;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Router;
using TowerBreak.Core.State;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Results;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class RewardToGrowthIntegrationTests
    {
        private TestSceneLoader sceneLoader;
        private PlayerInventoryState inventory;
        private PlayerWalletState wallet;
        private PlayerSessionState sessionState;
        private TestGameDataProvider gameDataProvider;

        [SetUp]
        public void SetUp()
        {
            sceneLoader = new TestSceneLoader();
            inventory = new PlayerInventoryState();
            wallet = new PlayerWalletState();
            sessionState = new PlayerSessionState(inventory, wallet);
            gameDataProvider = new TestGameDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            sceneLoader = null;
            inventory = null;
            wallet = null;
            sessionState = null;
            gameDataProvider = null;
        }

        // --- RewardResultsFlowRouter ---

        [Test]
        public void RewardResultsFlowRouter_Constructor_WhenSceneLoaderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RewardResultsFlowRouter(null));
        }

        [Test]
        public void RewardResultsFlowRouter_Continue_LoadsGrowthScene()
        {
            RewardResultsFlowRouter router = new RewardResultsFlowRouter(sceneLoader);

            router.Continue();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Growth"));
        }

        [Test]
        public void RewardResultsPresenter_OnContinue_LoadsGrowthScene()
        {
            RewardBundle reward = new RewardBundle(100, new List<int>());
            sessionState.SetLastBattleReward(reward);
            RewardResultsStateReader stateReader = new RewardResultsStateReader(sessionState);
            RewardResultsFlowRouter router = new RewardResultsFlowRouter(sceneLoader);
            RewardResultsPresenter presenter = new RewardResultsPresenter(stateReader, router);

            presenter.OnContinue();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Growth"));
        }

        // --- GrowthStateReader ---

        [Test]
        public void GrowthStateReader_Constructor_WhenSessionStateNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GrowthStateReader(null, gameDataProvider));
        }

        [Test]
        public void GrowthStateReader_Constructor_WhenGameDataProviderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GrowthStateReader(sessionState, null));
        }

        [Test]
        public void GrowthStateReader_WhenNoEquippedWeapon_EquippedWeaponIdIsNull()
        {
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.EquippedWeaponId, Is.Null);
        }

        [Test]
        public void GrowthStateReader_WhenEquippedWeapon_ReturnsEquippedWeaponTemplateId()
        {
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.EquippedWeaponId, Is.EqualTo(10));
        }

        [Test]
        public void GrowthStateReader_WhenNoReward_CandidateWeaponIdIsNull()
        {
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.CandidateWeaponId, Is.Null);
        }

        [Test]
        public void GrowthStateReader_WhenRewardWithNoWeapons_CandidateWeaponIdIsNull()
        {
            RewardBundle reward = new RewardBundle(100, new List<int>());
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.CandidateWeaponId, Is.Null);
        }

        [Test]
        public void GrowthStateReader_WhenRewardWithWeapon_ReturnsCandidateWeaponId()
        {
            RewardBundle reward = new RewardBundle(100, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.CandidateWeaponId, Is.EqualTo(20));
        }

        [Test]
        public void GrowthStateReader_WhenNoEquippedAndNoCandidate_DeltasAreZero()
        {
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.DeltaAttack, Is.EqualTo(0));
            Assert.That(reader.DeltaAttackSpeed, Is.EqualTo(0f));
            Assert.That(reader.DeltaPushPower, Is.EqualTo(0f));
        }

        [Test]
        public void GrowthStateReader_WhenOnlyEquippedNoCandidate_DeltasAreZero()
        {
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.DeltaAttack, Is.EqualTo(0));
        }

        [Test]
        public void GrowthStateReader_WhenOnlyCandidateNoEquipped_DeltasAreZero()
        {
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.DeltaAttack, Is.EqualTo(0));
        }

        [Test]
        public void GrowthStateReader_WhenBothEquippedAndCandidate_ReturnsDeltaAttack()
        {
            // equipped: weaponId=10 (BaseAttack=5), candidate: weaponId=20 (BaseAttack=15)
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.DeltaAttack, Is.EqualTo(10)); // 15 - 5 = 10
        }

        [Test]
        public void GrowthStateReader_WhenBothEquippedAndCandidate_ReturnsDeltaAttackSpeed()
        {
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader reader = new GrowthStateReader(sessionState, gameDataProvider);

            Assert.That(reader.DeltaAttackSpeed, Is.EqualTo(0.5f).Within(0.001f)); // 1.5f - 1.0f
        }

        private class TestSceneLoader : ISceneLoader
        {
            public string LastLoadedScene { get; private set; }

            public void LoadScene(string sceneName)
            {
                LastLoadedScene = sceneName;
            }

            public void LoadSceneAsync(string sceneName, Action onComplete = null)
            {
                LastLoadedScene = sceneName;
                onComplete?.Invoke();
            }
        }

        private class TestGameDataProvider : IGameDataProvider
        {
            public TowerBreakerGameData GetGameData()
            {
                TowerBreakerGameData data = new TowerBreakerGameData();
                data.Floors = new List<FloorRow>
                {
                    new FloorRow { Id = 1, RewardTableId = 1 },
                    new FloorRow { Id = 2, RewardTableId = 1 },
                    new FloorRow { Id = 3, RewardTableId = 1 }
                };
                data.Weapons = new List<WeaponRow>
                {
                    new WeaponRow { Id = 10, BaseAttack = 5, AttackSpeed = 1.0f, PushPower = 1.0f, Rarity = WeaponRarity.Common },
                    new WeaponRow { Id = 20, BaseAttack = 15, AttackSpeed = 1.5f, PushPower = 1.5f, Rarity = WeaponRarity.Common }
                };
                data.RerollCosts = new List<RerollCostRow>
                {
                    new RerollCostRow { Rarity = WeaponRarity.Common, GoldCost = 50, RollCount = 3, MinBonus = 1, MaxBonus = 5 }
                };
                data.RewardTables = new List<RewardTableRow>();
                data.RewardEntries = new List<RewardEntryRow>();
                data.FloorWaves = new List<FloorWaveRow>();
                data.Enemies = new List<EnemyRow>();
                data.EnhancementCosts = new List<EnhancementCostRow>();
                return data;
            }
        }
    }
}
