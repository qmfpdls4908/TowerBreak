using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Title;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class TitleScreenViewTests
    {
        private sealed class FakeRouter : ITitleFlowRouter
        {
            public void StartNewGame() { }
            public void ContinueGame() { }
        }

        private sealed class FakeSaveStateReader : ITitleSaveStateReader
        {
            public bool ReturnValue;

            public bool HasSaveData() => ReturnValue;
        }

        [Test]
        public void Refresh_WhenContinueAvailable_EnablesContinue()
        {
            var view = new TitleScreenView();

            view.Refresh(isContinueAvailable: true);

            Assert.That(view.IsContinueEnabled, Is.True);
        }

        [Test]
        public void Refresh_WhenContinueNotAvailable_DisablesContinue()
        {
            var view = new TitleScreenView();

            view.Refresh(isContinueAvailable: false);

            Assert.That(view.IsContinueEnabled, Is.False);
        }

        [Test]
        public void Bind_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new TitleScreenView();

            Assert.Throws<ArgumentNullException>(() => view.Bind(null));
        }

        [Test]
        public void Bind_WhenPresenterHasSaveData_EnablesContinue()
        {
            var reader = new FakeSaveStateReader { ReturnValue = true };
            var presenter = new TitleScreenPresenter(new FakeRouter(), reader);
            var view = new TitleScreenView();

            view.Bind(presenter);

            Assert.That(view.IsContinueEnabled, Is.True);
        }

        [Test]
        public void Bind_WhenPresenterHasNoSaveData_DisablesContinue()
        {
            var reader = new FakeSaveStateReader { ReturnValue = false };
            var presenter = new TitleScreenPresenter(new FakeRouter(), reader);
            var view = new TitleScreenView();

            view.Bind(presenter);

            Assert.That(view.IsContinueEnabled, Is.False);
        }
    }
}
