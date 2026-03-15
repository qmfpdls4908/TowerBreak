using System;
using System.Collections.Generic;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Bridge;
using TowerBreak.Core.Events;
using TowerBreak.Core.Router;
using TowerBreak.Core.State;
using TowerBreak.EventBus;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Growth;
using TowerBreak.UIFlow.Lobby;
using TowerBreak.UIFlow.Results;
using TowerBreak.UIFlow.Title;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class FullLoopIntegrationTests
    {
        private TestSceneLoader sceneLoader;
        private PlayerInventoryState inventory;
        private PlayerWalletState wallet;
        private PlayerSessionState sessionState;
        private TestGameDataProvider gameDataProvider;
        private EventBus<CombatEndedEvent> combatEndedBus;
        private CombatResultBridge bridge;
        private int nextInstanceId;

        private TitleFlowRouter titleRouter;
        private LobbyFlowRouter lobbyRouter;
        private RewardResultsFlowRouter rewardRouter;
        private GrowthFlowRouter growthRouter;

        private TitleSaveStateReader titleStateReader;
        private LobbyStateReader lobbyStateReader;
        private RewardResultsStateReader rewardStateReader;
        private GrowthStateReader growthStateReader;

        [SetUp]
        public void SetUp()
        {
            sceneLoader = new TestSceneLoader();
            inventory = new PlayerInventoryState();
            wallet = new PlayerWalletState();
            sessionState = new PlayerSessionState(inventory, wallet);
            gameDataProvider = new TestGameDataProvider();
            combatEndedBus = new EventBus<CombatEndedEvent>();
            nextInstanceId = 1;

            bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
            bridge.Initialize();

            titleRouter = new TitleFlowRouter(sceneLoader, sessionState);
            lobbyRouter = new LobbyFlowRouter(sceneLoader, sessionState);
            rewardRouter = new RewardResultsFlowRouter(sceneLoader);
            growthRouter = new GrowthFlowRouter(
                sessionState, gameDataProvider, sceneLoader, () => nextInstanceId++, new Random(0));

            titleStateReader = new TitleSaveStateReader(sessionState);
            lobbyStateReader = new LobbyStateReader(sessionState);
            rewardStateReader = new RewardResultsStateReader(sessionState);
            growthStateReader = new GrowthStateReader(sessionState, gameDataProvider);
        }

        [TearDown]
        public void TearDown()
        {
            bridge.Dispose();
            bridge = null;
            combatEndedBus = null;
            sessionState = null;
            inventory = null;
            wallet = null;
            sceneLoader = null;
            gameDataProvider = null;
            titleRouter = null;
            lobbyRouter = null;
            rewardRouter = null;
            growthRouter = null;
            titleStateReader = null;
            lobbyStateReader = null;
            rewardStateReader = null;
            growthStateReader = null;
        }

        // --- Victory loop ---

        [Test]
        public void VictoryLoop_FullPath_AdvancesFloorAndReturnsToLobby()
        {
            // Title: start new game -> lobby
            titleRouter.StartNewGame();
            Assert.That(sessionState.IsRunActive, Is.True);
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(1));

            // Lobby: open challenge -> battle
            lobbyRouter.OpenChallenge();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"));
            Assert.That(sessionState.PendingBattleFloorId, Is.EqualTo(1));

            // Combat ended (victory) -> reward resolved
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));
            Assert.That(sessionState.LastBattleReward, Is.Not.Null);

            // Reward: continue -> growth
            rewardRouter.Continue();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Growth"));

            // Growth: continue -> floor advances, returns to lobby
            growthRouter.Continue();
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(2));
            Assert.That(sessionState.IsRunActive, Is.True);
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void VictoryLoop_SceneSequence_MatchesExpectedFlow()
        {
            titleRouter.StartNewGame();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));

            lobbyRouter.OpenChallenge();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"));

            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            rewardRouter.Continue();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Growth"));

            growthRouter.Continue();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void TwoVictoryLoops_AdvancesFromFloor1ToFloor3()
        {
            // Loop 1: floor 1 -> floor 2
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));
            growthRouter.Continue();
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(2));

            // Loop 2: floor 2 -> floor 3
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 2));
            growthRouter.Continue();
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(3));

            Assert.That(sessionState.IsRunActive, Is.True);
        }

        // --- Defeat loop ---

        [Test]
        public void DefeatLoop_FullPath_EndsRunAndReturnsToLobby()
        {
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();

            // Combat ended (defeat) -> no reward
            combatEndedBus.Publish(new CombatEndedEvent(false, 1));
            Assert.That(sessionState.LastBattleReward, Is.Null);

            // Reward screen still navigates to growth
            rewardRouter.Continue();
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Growth"));

            // Growth continue on defeat -> run ends
            growthRouter.Continue();
            Assert.That(sessionState.IsRunActive, Is.False);
            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void DefeatLoop_NoReward_GrowthStateHasNoCandidateWeapon()
        {
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(false, 1));

            Assert.That(growthStateReader.CandidateWeaponId, Is.Null);
            Assert.That(rewardStateReader.GoldEarned, Is.EqualTo(0));
        }

        // --- State continuity ---

        [Test]
        public void VictoryLoop_RewardReader_ReturnsPositiveGold()
        {
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            Assert.That(rewardStateReader.GoldEarned, Is.GreaterThan(0));
        }

        [Test]
        public void VictoryLoop_CandidateWeaponAvailable_AfterReward()
        {
            // WeaponDropChance=1.0f guarantees weapon drop
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            Assert.That(growthStateReader.CandidateWeaponId, Is.Not.Null);
        }

        [Test]
        public void VictoryLoop_Equip_WeaponAppearsInLobbyState()
        {
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            growthRouter.Equip();

            Assert.That(lobbyStateReader.EquippedWeaponId, Is.Not.Null);
        }

        [Test]
        public void VictoryLoop_EquipThenContinue_EquippedWeaponPersistsAfterFloorAdvance()
        {
            titleRouter.StartNewGame();
            lobbyRouter.OpenChallenge();
            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            growthRouter.Equip();
            growthRouter.Continue();

            // Floor advanced, run still active, weapon still equipped
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(2));
            Assert.That(lobbyStateReader.EquippedWeaponId, Is.Not.Null);
        }

        // --- Presenter wiring ---

        [Test]
        public void FullLoop_AllPresenters_CanBeAssembledFromSameSessionState()
        {
            titleRouter.StartNewGame();

            TitleScreenPresenter titlePresenter = new TitleScreenPresenter(titleRouter, titleStateReader);
            LobbyPresenter lobbyPresenter = new LobbyPresenter(lobbyRouter, lobbyStateReader, sessionState.Wallet);
            RewardResultsPresenter rewardPresenter = new RewardResultsPresenter(rewardStateReader, rewardRouter);
            GrowthScreenPresenter growthPresenter = new GrowthScreenPresenter(growthStateReader, growthRouter);

            Assert.That(titlePresenter.IsContinueAvailable, Is.True);
            Assert.That(lobbyPresenter.CurrentFloorId, Is.EqualTo(1));
            Assert.That(rewardPresenter.GoldEarned, Is.EqualTo(0)); // no combat yet
            Assert.That(growthPresenter.HasEquippedWeapon, Is.False); // no equip yet
        }

        // --- Null guard ---

        [Test]
        public void FullLoopAssembly_WhenSessionStateNull_TitleRouterThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TitleFlowRouter(sceneLoader, null));
        }

        // --- Helpers ---

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
                    new WeaponRow { Id = 1, BaseAttack = 10, AttackSpeed = 1.0f, PushPower = 1.0f, Rarity = WeaponRarity.Common },
                    new WeaponRow { Id = 2, BaseAttack = 20, AttackSpeed = 1.5f, PushPower = 1.5f, Rarity = WeaponRarity.Common }
                };
                data.RewardTables = new List<RewardTableRow>
                {
                    new RewardTableRow
                    {
                        Id = 1,
                        GuaranteedGold = 100,
                        WeaponDropChance = 1.0f,
                        FallbackRewardId = 0
                    }
                };
                data.RewardEntries = new List<RewardEntryRow>
                {
                    new RewardEntryRow
                    {
                        RewardTableId = 1,
                        RewardId = 1,
                        RewardType = RewardType.Weapon,
                        TargetItemId = 1,
                        Weight = 100
                    }
                };
                data.RerollCosts = new List<RerollCostRow>
                {
                    new RerollCostRow
                    {
                        Rarity = WeaponRarity.Common,
                        GoldCost = 50,
                        RollCount = 3,
                        MinBonus = 1,
                        MaxBonus = 5
                    }
                };
                data.FloorWaves = new List<FloorWaveRow>();
                data.Enemies = new List<EnemyRow>();
                data.EnhancementCosts = new List<EnhancementCostRow>();
                return data;
            }
        }
    }
}
