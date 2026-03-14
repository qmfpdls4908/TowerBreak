using System;
using System.Collections.Generic;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Router;
using TowerBreak.Core.State;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Growth;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class GrowthToLobbyIntegrationTests
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

        // --- GrowthFlowRouter null guards ---

        [Test]
        public void GrowthFlowRouter_Constructor_WhenSessionStateNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthFlowRouter(null, gameDataProvider, sceneLoader, () => 1, new Random(0)));
        }

        [Test]
        public void GrowthFlowRouter_Constructor_WhenGameDataProviderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthFlowRouter(sessionState, null, sceneLoader, () => 1, new Random(0)));
        }

        [Test]
        public void GrowthFlowRouter_Constructor_WhenSceneLoaderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthFlowRouter(sessionState, gameDataProvider, null, () => 1, new Random(0)));
        }

        [Test]
        public void GrowthFlowRouter_Constructor_WhenInstanceIdProviderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthFlowRouter(sessionState, gameDataProvider, sceneLoader, null, new Random(0)));
        }

        [Test]
        public void GrowthFlowRouter_Constructor_WhenRandomNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthFlowRouter(sessionState, gameDataProvider, sceneLoader, () => 1, null));
        }

        // --- Equip ---

        [Test]
        public void GrowthFlowRouter_Equip_WhenNoCandidateReward_ThrowsInvalidOperationException()
        {
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            Assert.Throws<InvalidOperationException>(() => router.Equip());
        }

        [Test]
        public void GrowthFlowRouter_Equip_WhenRewardWithNoWeapons_ThrowsInvalidOperationException()
        {
            RewardBundle reward = new RewardBundle(100, new List<int>());
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            Assert.Throws<InvalidOperationException>(() => router.Equip());
        }

        [Test]
        public void GrowthFlowRouter_Equip_WhenCandidateAvailable_AddsWeaponToInventory()
        {
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));

            router.Equip();

            Assert.That(inventory.Equipment.Count, Is.EqualTo(1));
        }

        [Test]
        public void GrowthFlowRouter_Equip_WhenCandidateAvailable_EquipsWeaponInInventory()
        {
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));

            router.Equip();

            Assert.That(inventory.EquippedWeaponInstanceId, Is.EqualTo(100));
        }

        [Test]
        public void GrowthFlowRouter_Equip_WhenCandidateAvailable_EquipsCorrectWeaponId()
        {
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));

            router.Equip();

            inventory.TryGetEquipment(100, out OwnedEquipment eq);
            Assert.That(eq.WeaponId, Is.EqualTo(20));
        }

        // --- Reroll ---

        [Test]
        public void GrowthFlowRouter_Reroll_WhenNoEquippedWeapon_ThrowsInvalidOperationException()
        {
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            Assert.Throws<InvalidOperationException>(() => router.Reroll());
        }

        [Test]
        public void GrowthFlowRouter_Reroll_WhenEquippedWeapon_DeductsGoldFromWallet()
        {
            wallet.AddGold(500);
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 2, new Random(0));

            router.Reroll();

            Assert.That(wallet.Gold, Is.LessThan(500));
        }

        [Test]
        public void GrowthFlowRouter_Reroll_WhenInsufficientGold_ThrowsInvalidOperationException()
        {
            // wallet has 0 gold, reroll costs 50
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 2, new Random(0));

            Assert.Throws<InvalidOperationException>(() => router.Reroll());
        }

        // --- Continue ---

        [Test]
        public void GrowthFlowRouter_Continue_WhenVictoryAndCanContinue_AdvancesFloor()
        {
            sessionState.StartNewRun(); // floor=1
            RewardBundle reward = new RewardBundle(50, new List<int>());
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            router.Continue();

            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(2));
        }

        [Test]
        public void GrowthFlowRouter_Continue_WhenVictoryAndRunComplete_EndsRun()
        {
            sessionState.StartNewRun();
            sessionState.AdvanceToNextFloor(); // floor=2
            sessionState.AdvanceToNextFloor(); // floor=3 (last in test data)
            RewardBundle reward = new RewardBundle(50, new List<int>());
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            router.Continue();

            Assert.That(sessionState.IsRunActive, Is.False);
        }

        [Test]
        public void GrowthFlowRouter_Continue_WhenDefeat_EndsRun()
        {
            sessionState.StartNewRun(); // floor=1
            // No reward = defeat
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            router.Continue();

            Assert.That(sessionState.IsRunActive, Is.False);
        }

        [Test]
        public void GrowthFlowRouter_Continue_AlwaysLoadsLobbyScene()
        {
            sessionState.StartNewRun();
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            router.Continue();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void GrowthFlowRouter_Continue_WhenVictoryAndCanContinue_RunRemainsActive()
        {
            sessionState.StartNewRun();
            RewardBundle reward = new RewardBundle(50, new List<int>());
            sessionState.SetLastBattleReward(reward);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 1, new Random(0));

            router.Continue();

            Assert.That(sessionState.IsRunActive, Is.True);
        }

        // --- End-to-end: GrowthPresenter with StateReader + FlowRouter ---

        [Test]
        public void GrowthPresenter_WithStateReader_ReflectsEquippedWeapon()
        {
            OwnedEquipment eq = new OwnedEquipment(1, 10);
            inventory.AddEquipment(eq);
            inventory.EquipWeapon(1);
            GrowthStateReader stateReader = new GrowthStateReader(sessionState, gameDataProvider);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));
            GrowthScreenPresenter presenter = new GrowthScreenPresenter(stateReader, router);

            Assert.That(presenter.HasEquippedWeapon, Is.True);
            Assert.That(presenter.EquippedWeaponId, Is.EqualTo(10));
        }

        [Test]
        public void GrowthPresenter_WithStateReader_ReflectsCandidateWeapon()
        {
            RewardBundle reward = new RewardBundle(0, new List<int> { 20 });
            sessionState.SetLastBattleReward(reward);
            GrowthStateReader stateReader = new GrowthStateReader(sessionState, gameDataProvider);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));
            GrowthScreenPresenter presenter = new GrowthScreenPresenter(stateReader, router);

            Assert.That(presenter.HasCandidateWeapon, Is.True);
            Assert.That(presenter.CandidateWeaponId, Is.EqualTo(20));
        }

        [Test]
        public void GrowthPresenter_WithNoEquipmentOrReward_ShowsNoWeapons()
        {
            GrowthStateReader stateReader = new GrowthStateReader(sessionState, gameDataProvider);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));
            GrowthScreenPresenter presenter = new GrowthScreenPresenter(stateReader, router);

            Assert.That(presenter.HasEquippedWeapon, Is.False);
            Assert.That(presenter.HasCandidateWeapon, Is.False);
            Assert.That(presenter.IsComparisonAvailable, Is.False);
        }

        [Test]
        public void GrowthPresenter_OnContinue_LoadsLobbyScene()
        {
            sessionState.StartNewRun();
            GrowthStateReader stateReader = new GrowthStateReader(sessionState, gameDataProvider);
            GrowthFlowRouter router = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => 100, new Random(0));
            GrowthScreenPresenter presenter = new GrowthScreenPresenter(stateReader, router);

            presenter.OnContinue();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
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
