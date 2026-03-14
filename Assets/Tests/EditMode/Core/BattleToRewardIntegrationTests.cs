using System;
using System.Collections.Generic;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Bridge;
using TowerBreak.Core.Events;
using TowerBreak.Core.State;
using TowerBreak.EventBus;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Results;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class BattleToRewardIntegrationTests
    {
        private PlayerInventoryState inventory;
        private PlayerWalletState wallet;
        private PlayerSessionState sessionState;
        private TestGameDataProvider gameDataProvider;
        private EventBus<CombatEndedEvent> combatEndedBus;

        [SetUp]
        public void SetUp()
        {
            inventory = new PlayerInventoryState();
            wallet = new PlayerWalletState();
            sessionState = new PlayerSessionState(inventory, wallet);
            gameDataProvider = new TestGameDataProvider();
            combatEndedBus = new EventBus<CombatEndedEvent>();
        }

        [TearDown]
        public void TearDown()
        {
            inventory = null;
            wallet = null;
            sessionState = null;
            gameDataProvider = null;
            combatEndedBus = null;
        }

        [Test]
        public void CombatEndedEvent_CanBePublishedAndReceived()
        {
            bool received = false;
            CombatEndedEvent receivedEvent = default;
            combatEndedBus.Subscribe(evt =>
            {
                received = true;
                receivedEvent = evt;
            });

            CombatEndedEvent evt = new CombatEndedEvent(true, 1);
            combatEndedBus.Publish(evt);

            Assert.That(received, Is.True);
            Assert.That(receivedEvent.IsVictory, Is.True);
            Assert.That(receivedEvent.FloorId, Is.EqualTo(1));
        }

        [Test]
        public void CombatResultBridge_Constructor_WhenSessionStateNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatResultBridge(null, gameDataProvider, combatEndedBus));
        }

        [Test]
        public void CombatResultBridge_Constructor_WhenGameDataProviderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatResultBridge(sessionState, null, combatEndedBus));
        }

        [Test]
        public void CombatResultBridge_Constructor_WhenEventBusNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatResultBridge(sessionState, gameDataProvider, null));
        }

        [Test]
        public void CombatResultBridge_OnCombatEnded_WhenVictory_ResolvesAndStoresReward()
        {
            sessionState.StartNewRun();
            CombatResultBridge bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
            bridge.Initialize();

            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            Assert.That(sessionState.LastBattleReward, Is.Not.Null);
            Assert.That(sessionState.LastBattleReward.GoldAmount, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void CombatResultBridge_OnCombatEnded_WhenDefeat_StoresNullReward()
        {
            sessionState.StartNewRun();
            CombatResultBridge bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
            bridge.Initialize();

            combatEndedBus.Publish(new CombatEndedEvent(false, 1));

            Assert.That(sessionState.LastBattleReward, Is.Null);
        }

        [Test]
        public void RewardResultsStateReader_WhenRewardAvailable_ReturnsCorrectGold()
        {
            sessionState.StartNewRun();
            RewardBundle reward = new RewardBundle(100, new List<int>());
            sessionState.SetLastBattleReward(reward);
            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);

            int gold = reader.GoldEarned;

            Assert.That(gold, Is.EqualTo(100));
        }

        [Test]
        public void RewardResultsStateReader_WhenRewardAvailable_ReturnsGrantedWeaponIds()
        {
            sessionState.StartNewRun();
            RewardBundle reward = new RewardBundle(0, new List<int> { 1, 2 });
            sessionState.SetLastBattleReward(reward);
            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);

            IReadOnlyList<int> weaponIds = reader.GrantedWeaponIds;

            Assert.That(weaponIds.Count, Is.EqualTo(2));
            Assert.That(weaponIds[0], Is.EqualTo(1));
            Assert.That(weaponIds[1], Is.EqualTo(2));
        }

        [Test]
        public void RewardResultsStateReader_WhenNoReward_ReturnsZeroGold()
        {
            sessionState.StartNewRun();
            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);

            int gold = reader.GoldEarned;

            Assert.That(gold, Is.EqualTo(0));
        }

        [Test]
        public void RewardResultsStateReader_WhenNoReward_ReturnsEmptyWeaponIds()
        {
            sessionState.StartNewRun();
            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);

            IReadOnlyList<int> weaponIds = reader.GrantedWeaponIds;

            Assert.That(weaponIds.Count, Is.EqualTo(0));
        }

        [Test]
        public void PlayerSessionState_SetLastBattleReward_StoresValue()
        {
            RewardBundle reward = new RewardBundle(50, new List<int>());
            sessionState.SetLastBattleReward(reward);

            Assert.That(sessionState.LastBattleReward.GoldAmount, Is.EqualTo(50));
        }

        [Test]
        public void PlayerSessionState_ClearLastBattleReward_ResetsToNull()
        {
            RewardBundle reward = new RewardBundle(50, new List<int>());
            sessionState.SetLastBattleReward(reward);
            sessionState.ClearLastBattleReward();

            Assert.That(sessionState.LastBattleReward, Is.Null);
        }

        [Test]
        public void PlayerSessionState_LastBattleReward_DefaultsToNull()
        {
            Assert.That(sessionState.LastBattleReward, Is.Null);
        }

        [Test]
        public void CombatResultBridge_EndToEnd_VictoryResolvesRewardAndStateReaderCanRead()
        {
            sessionState.StartNewRun();
            CombatResultBridge bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
            bridge.Initialize();

            combatEndedBus.Publish(new CombatEndedEvent(true, 1));

            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);
            Assert.That(reader.GoldEarned, Is.GreaterThanOrEqualTo(0));
            Assert.That(reader.GrantedWeaponIds, Is.Not.Null);
        }

        [Test]
        public void CombatResultBridge_EndToEnd_DefeatNoRewardAndStateReaderReadsEmpty()
        {
            sessionState.StartNewRun();
            CombatResultBridge bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
            bridge.Initialize();

            combatEndedBus.Publish(new CombatEndedEvent(false, 1));

            RewardResultsStateReader reader = new RewardResultsStateReader(sessionState);
            Assert.That(reader.GoldEarned, Is.EqualTo(0));
            Assert.That(reader.GrantedWeaponIds.Count, Is.EqualTo(0));
        }

        private class TestGameDataProvider : IGameDataProvider
        {
            public TowerBreakerGameData GetGameData()
            {
                TowerBreakerGameData data = new TowerBreakerGameData();
                data.Floors = new List<FloorRow>
                {
                    new FloorRow { Id = 1, RewardTableId = 1 }
                };
                data.RewardTables = new List<RewardTableRow>
                {
                    new RewardTableRow
                    {
                        Id = 1,
                        GuaranteedGold = 100,
                        WeaponDropChance = 0.5f,
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
                return data;
            }
        }
    }
}
