using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Title;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class TitleScreenPresenterTests
    {
        private sealed class FakeRouter : ITitleFlowRouter
        {
            public int StartNewGameCallCount;
            public int ContinueGameCallCount;

            public void StartNewGame() => StartNewGameCallCount++;
            public void ContinueGame() => ContinueGameCallCount++;
        }

        private sealed class FakeSaveStateReader : ITitleSaveStateReader
        {
            public bool ReturnValue;

            public bool HasSaveData() => ReturnValue;
        }

        [Test]
        public void Constructor_NullRouter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TitleScreenPresenter(null, new FakeSaveStateReader()));
        }

        [Test]
        public void Constructor_NullSaveStateReader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TitleScreenPresenter(new FakeRouter(), null));
        }

        [Test]
        public void IsContinueAvailable_WhenHasSaveData_ReturnsTrue()
        {
            var reader = new FakeSaveStateReader { ReturnValue = true };
            var presenter = new TitleScreenPresenter(new FakeRouter(), reader);

            Assert.That(presenter.IsContinueAvailable, Is.True);
        }

        [Test]
        public void IsContinueAvailable_WhenNoSaveData_ReturnsFalse()
        {
            var reader = new FakeSaveStateReader { ReturnValue = false };
            var presenter = new TitleScreenPresenter(new FakeRouter(), reader);

            Assert.That(presenter.IsContinueAvailable, Is.False);
        }

        [Test]
        public void OnStartNewGame_CallsRouterStartNewGame()
        {
            var router = new FakeRouter();
            var presenter = new TitleScreenPresenter(router, new FakeSaveStateReader());

            presenter.OnStartNewGame();

            Assert.That(router.StartNewGameCallCount, Is.EqualTo(1));
        }

        [Test]
        public void OnStartNewGame_DoesNotCallRouterContinueGame()
        {
            var router = new FakeRouter();
            var presenter = new TitleScreenPresenter(router, new FakeSaveStateReader());

            presenter.OnStartNewGame();

            Assert.That(router.ContinueGameCallCount, Is.EqualTo(0));
        }

        [Test]
        public void OnContinueGame_WhenContinueAvailable_CallsRouterContinueGame()
        {
            var router = new FakeRouter();
            var reader = new FakeSaveStateReader { ReturnValue = true };
            var presenter = new TitleScreenPresenter(router, reader);

            presenter.OnContinueGame();

            Assert.That(router.ContinueGameCallCount, Is.EqualTo(1));
        }

        [Test]
        public void OnContinueGame_WhenContinueNotAvailable_DoesNotCallRouter()
        {
            var router = new FakeRouter();
            var reader = new FakeSaveStateReader { ReturnValue = false };
            var presenter = new TitleScreenPresenter(router, reader);

            presenter.OnContinueGame();

            Assert.That(router.ContinueGameCallCount, Is.EqualTo(0));
        }
    }
}
