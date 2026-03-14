using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Battle;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class BattleHudPresenterTests
    {
        private sealed class FakeStateReader : IBattleHudStateReader
        {
            public int Health;
            public int Floor;
            public int Wave;
            public bool DangerActive;
            public bool WallDefeated;

            public int PlayerHealth => Health;
            public int FloorId => Floor;
            public int WaveNumber => Wave;
            public bool IsDangerActive => DangerActive;
            public bool IsWallDefeated => WallDefeated;
        }

        [Test]
        public void Constructor_NullStateReader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BattleHudPresenter(null));
        }

        [Test]
        public void PlayerHealth_ReturnsStateReaderValue()
        {
            var state = new FakeStateReader { Health = 5 };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.PlayerHealth, Is.EqualTo(5));
        }

        [Test]
        public void FloorLabel_ContainsFloorId()
        {
            var state = new FakeStateReader { Floor = 2 };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.FloorLabel, Does.Contain("2"));
        }

        [Test]
        public void WaveLabel_ContainsWaveNumber()
        {
            var state = new FakeStateReader { Wave = 3 };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.WaveLabel, Does.Contain("3"));
        }

        [Test]
        public void IsDangerBannerVisible_WhenSafe_ReturnsFalse()
        {
            var state = new FakeStateReader { DangerActive = false, WallDefeated = false };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.IsDangerBannerVisible, Is.False);
        }

        [Test]
        public void IsDangerBannerVisible_WhenDangerActive_ReturnsTrue()
        {
            var state = new FakeStateReader { DangerActive = true, WallDefeated = false };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.IsDangerBannerVisible, Is.True);
        }

        [Test]
        public void IsDangerBannerVisible_WhenWallDefeated_ReturnsTrue()
        {
            var state = new FakeStateReader { DangerActive = false, WallDefeated = true };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.IsDangerBannerVisible, Is.True);
        }

        [Test]
        public void DangerBannerText_WhenSafe_ReturnsEmpty()
        {
            var state = new FakeStateReader { DangerActive = false, WallDefeated = false };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.DangerBannerText, Is.Empty);
        }

        [Test]
        public void DangerBannerText_WhenDangerActive_ReturnsDanger()
        {
            var state = new FakeStateReader { DangerActive = true, WallDefeated = false };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.DangerBannerText, Does.Contain("DANGER"));
        }

        [Test]
        public void DangerBannerText_WhenWallDefeated_ReturnsDefeat()
        {
            var state = new FakeStateReader { DangerActive = false, WallDefeated = true };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.DangerBannerText, Does.Contain("DEFEAT"));
        }

        [Test]
        public void DangerBannerText_WhenBothDangerAndDefeated_PrioritisesDefeat()
        {
            var state = new FakeStateReader { DangerActive = true, WallDefeated = true };
            var presenter = new BattleHudPresenter(state);

            Assert.That(presenter.DangerBannerText, Does.Contain("DEFEAT"));
        }

        [Test]
        public void FloorLabel_SameInputProducesSameOutput()
        {
            var state = new FakeStateReader { Floor = 1 };
            var presenter = new BattleHudPresenter(state);

            string first = presenter.FloorLabel;
            string second = presenter.FloorLabel;

            Assert.That(first, Is.EqualTo(second));
        }
    }
}
