using System;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Router;
using TowerBreak.Core.State;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class LobbyToBattleIntegrationTests
    {
        private TestSceneLoader sceneLoader;
        private PlayerInventoryState inventory;
        private PlayerWalletState wallet;
        private PlayerSessionState sessionState;

        [SetUp]
        public void SetUp()
        {
            sceneLoader = new TestSceneLoader();
            inventory = new PlayerInventoryState();
            wallet = new PlayerWalletState();
            sessionState = new PlayerSessionState(inventory, wallet);
        }

        [TearDown]
        public void TearDown()
        {
            sceneLoader = null;
            inventory = null;
            wallet = null;
            sessionState = null;
        }

        [Test]
        public void LobbyFlowRouter_OpenChallenge_WhenRunActive_LoadsBattleScene()
        {
            sessionState.StartNewRun();
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);

            router.OpenChallenge();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"));
        }

        [Test]
        public void LobbyFlowRouter_OpenChallenge_WhenRunNotActive_ThrowsInvalidOperationException()
        {
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);

            Assert.Throws<InvalidOperationException>(() => router.OpenChallenge());
        }

        [Test]
        public void LobbyFlowRouter_OpenChallenge_SetsPendingBattleFloorId()
        {
            sessionState.StartNewRun();
            sessionState.AdvanceToNextFloor();
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);

            router.OpenChallenge();

            Assert.That(sessionState.PendingBattleFloorId, Is.EqualTo(2));
        }

        [Test]
        public void LobbyPresenter_OnOpenChallenge_WhenRunActive_RouterOpensChallenge()
        {
            sessionState.StartNewRun();
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);
            LobbyStateReader stateReader = new LobbyStateReader(sessionState);
            LobbyPresenter presenter = new LobbyPresenter(router, stateReader, sessionState.Wallet);

            presenter.OnOpenChallenge();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"));
        }

        [Test]
        public void PlayerSessionState_PendingBattleFloorId_DefaultsToNull()
        {
            Assert.That(sessionState.PendingBattleFloorId, Is.Null);
        }

        [Test]
        public void PlayerSessionState_SetPendingBattleFloorId_StoresValue()
        {
            sessionState.SetPendingBattleFloorId(5);

            Assert.That(sessionState.PendingBattleFloorId, Is.EqualTo(5));
        }

        [Test]
        public void PlayerSessionState_ClearPendingBattleFloorId_ResetsToNull()
        {
            sessionState.SetPendingBattleFloorId(5);
            sessionState.ClearPendingBattleFloorId();

            Assert.That(sessionState.PendingBattleFloorId, Is.Null);
        }

        [Test]
        public void PlayerSessionState_SetPendingBattleFloorId_WhenInvalidFloor_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => sessionState.SetPendingBattleFloorId(0));
            Assert.Throws<ArgumentException>(() => sessionState.SetPendingBattleFloorId(-1));
        }

        [Test]
        public void LobbyToBattleFlow_EndToEnd_FloorContextPreserved()
        {
            sessionState.StartNewRun();
            sessionState.AdvanceToNextFloor();
            sessionState.AdvanceToNextFloor();

            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);
            LobbyStateReader stateReader = new LobbyStateReader(sessionState);
            LobbyPresenter presenter = new LobbyPresenter(router, stateReader, sessionState.Wallet);

            Assert.That(presenter.CurrentFloorId, Is.EqualTo(3), "Presenter should show floor 3");

            presenter.OnOpenChallenge();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"), "Should load Battle scene");
            Assert.That(sessionState.PendingBattleFloorId, Is.EqualTo(3), "Pending battle should be floor 3");
            Assert.That(sessionState.IsRunActive, Is.True, "Run should still be active");
        }

        [Test]
        public void LobbyFlowRouter_Constructor_WhenSceneLoaderNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LobbyFlowRouter(null, sessionState));
        }

        [Test]
        public void LobbyFlowRouter_Constructor_WhenSessionStateNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LobbyFlowRouter(sceneLoader, null));
        }

        [Test]
        public void LobbyFlowRouter_OpenChallenge_WhenOnFloor1_SetsPendingTo1()
        {
            sessionState.StartNewRun();
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);

            router.OpenChallenge();

            Assert.That(sessionState.PendingBattleFloorId, Is.EqualTo(1));
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
    }
}
