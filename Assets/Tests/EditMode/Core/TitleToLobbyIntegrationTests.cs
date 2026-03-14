using System;
using NUnit.Framework;
using TowerBreak.Core;
using TowerBreak.Core.Router;
using TowerBreak.Core.State;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Title;

namespace TowerBreak.Core.Tests
{
    [TestFixture]
    public class TitleToLobbyIntegrationTests
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
        public void TitleFlowRouter_StartNewGame_LoadsLobbyScene()
        {
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);

            router.StartNewGame();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void TitleFlowRouter_StartNewGame_InitializesSessionState()
        {
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);

            router.StartNewGame();

            Assert.That(sessionState.IsRunActive, Is.True);
            Assert.That(sessionState.CurrentFloorId, Is.EqualTo(1));
        }

        [Test]
        public void TitleFlowRouter_ContinueGame_WhenRunActive_LoadsLobbyScene()
        {
            sessionState.StartNewRun();
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);

            router.ContinueGame();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void TitleFlowRouter_ContinueGame_WhenRunNotActive_ThrowsInvalidOperationException()
        {
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);

            Assert.Throws<InvalidOperationException>(() => router.ContinueGame());
        }

        [Test]
        public void TitleSaveStateReader_HasSaveData_WhenRunActive_ReturnsTrue()
        {
            sessionState.StartNewRun();
            TitleSaveStateReader reader = new TitleSaveStateReader(sessionState);

            bool result = reader.HasSaveData();

            Assert.That(result, Is.True);
        }

        [Test]
        public void TitleSaveStateReader_HasSaveData_WhenRunNotActive_ReturnsFalse()
        {
            TitleSaveStateReader reader = new TitleSaveStateReader(sessionState);

            bool result = reader.HasSaveData();

            Assert.That(result, Is.False);
        }

        [Test]
        public void TitleScreenPresenter_IsContinueAvailable_WhenRunActive_ReturnsTrue()
        {
            sessionState.StartNewRun();
            TitleSaveStateReader saveReader = new TitleSaveStateReader(sessionState);
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);
            TitleScreenPresenter presenter = new TitleScreenPresenter(router, saveReader);

            bool result = presenter.IsContinueAvailable;

            Assert.That(result, Is.True);
        }

        [Test]
        public void TitleScreenPresenter_IsContinueAvailable_WhenRunNotActive_ReturnsFalse()
        {
            TitleSaveStateReader saveReader = new TitleSaveStateReader(sessionState);
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);
            TitleScreenPresenter presenter = new TitleScreenPresenter(router, saveReader);

            bool result = presenter.IsContinueAvailable;

            Assert.That(result, Is.False);
        }

        [Test]
        public void TitleScreenPresenter_OnStartNewGame_LoadsLobbyAndStartsRun()
        {
            TitleSaveStateReader saveReader = new TitleSaveStateReader(sessionState);
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);
            TitleScreenPresenter presenter = new TitleScreenPresenter(router, saveReader);

            presenter.OnStartNewGame();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
            Assert.That(sessionState.IsRunActive, Is.True);
        }

        [Test]
        public void TitleScreenPresenter_OnContinueGame_WhenAvailable_LoadsLobby()
        {
            sessionState.StartNewRun();
            TitleSaveStateReader saveReader = new TitleSaveStateReader(sessionState);
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);
            TitleScreenPresenter presenter = new TitleScreenPresenter(router, saveReader);

            presenter.OnContinueGame();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Lobby"));
        }

        [Test]
        public void TitleScreenPresenter_OnContinueGame_WhenNotAvailable_DoesNotLoadScene()
        {
            TitleSaveStateReader saveReader = new TitleSaveStateReader(sessionState);
            TitleFlowRouter router = new TitleFlowRouter(sceneLoader, sessionState);
            TitleScreenPresenter presenter = new TitleScreenPresenter(router, saveReader);

            presenter.OnContinueGame();

            Assert.That(sceneLoader.LastLoadedScene, Is.Null);
        }

        [Test]
        public void LobbyStateReader_CurrentFloorId_ReturnsSessionValue()
        {
            sessionState.StartNewRun();
            sessionState.AdvanceToNextFloor();
            LobbyStateReader reader = new LobbyStateReader(sessionState);

            int result = reader.CurrentFloorId;

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void LobbyStateReader_Gold_ReturnsWalletValue()
        {
            wallet.AddGold(100);
            LobbyStateReader reader = new LobbyStateReader(sessionState);

            int result = reader.Gold;

            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void LobbyStateReader_EquippedWeaponId_WhenNoWeapon_ReturnsNull()
        {
            LobbyStateReader reader = new LobbyStateReader(sessionState);

            int? result = reader.EquippedWeaponId;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void LobbyFlowRouter_OpenChallenge_WhenRunActive_LoadsBattleScene()
        {
            sessionState.StartNewRun();
            LobbyFlowRouter router = new LobbyFlowRouter(sceneLoader, sessionState);

            router.OpenChallenge();

            Assert.That(sceneLoader.LastLoadedScene, Is.EqualTo("Battle"));
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
