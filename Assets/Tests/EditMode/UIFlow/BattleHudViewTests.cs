using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Battle;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class BattleHudViewTests
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
        public void Refresh_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new BattleHudView();

            Assert.Throws<ArgumentNullException>(() => view.Refresh(null));
        }

        [Test]
        public void Refresh_SetsDisplayedPlayerHealth()
        {
            var state = new FakeStateReader { Health = 4 };
            var presenter = new BattleHudPresenter(state);
            var view = new BattleHudView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedPlayerHealth, Is.EqualTo(4));
        }

        [Test]
        public void Refresh_SetsDisplayedFloorLabel()
        {
            var state = new FakeStateReader { Floor = 2 };
            var presenter = new BattleHudPresenter(state);
            var view = new BattleHudView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedFloorLabel, Does.Contain("2"));
        }

        [Test]
        public void Refresh_SetsDisplayedWaveLabel()
        {
            var state = new FakeStateReader { Wave = 1 };
            var presenter = new BattleHudPresenter(state);
            var view = new BattleHudView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedWaveLabel, Does.Contain("1"));
        }

        [Test]
        public void Refresh_WhenSafe_SetsDangerBannerNotVisible()
        {
            var state = new FakeStateReader { DangerActive = false, WallDefeated = false };
            var presenter = new BattleHudPresenter(state);
            var view = new BattleHudView();

            view.Refresh(presenter);

            Assert.That(view.IsDangerBannerVisible, Is.False);
        }

        [Test]
        public void Refresh_WhenDangerActive_SetsDangerBannerVisible()
        {
            var state = new FakeStateReader { DangerActive = true };
            var presenter = new BattleHudPresenter(state);
            var view = new BattleHudView();

            view.Refresh(presenter);

            Assert.That(view.IsDangerBannerVisible, Is.True);
        }
    }
}
