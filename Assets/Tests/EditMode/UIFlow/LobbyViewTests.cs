using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class LobbyViewTests
    {
        private sealed class FakeRouter : ILobbyFlowRouter
        {
            public void OpenChallenge() { }
            public void OpenEquipment() { }
            public void OpenReroll() { }
        }

        private sealed class FakeStateReader : ILobbyStateReader
        {
            public int FloorId;
            public int GoldAmount;
            public int? WeaponId;

            public int CurrentFloorId => FloorId;
            public int Gold => GoldAmount;
            public int? EquippedWeaponId => WeaponId;
        }

        [Test]
        public void Refresh_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new LobbyView();

            Assert.Throws<ArgumentNullException>(() => view.Refresh(null));
        }

        [Test]
        public void Refresh_SetsDisplayedFloorId()
        {
            var state = new FakeStateReader { FloorId = 2 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);
            var view = new LobbyView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedFloorId, Is.EqualTo(2));
        }

        [Test]
        public void Refresh_SetsDisplayedGold()
        {
            var state = new FakeStateReader { GoldAmount = 250 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);
            var view = new LobbyView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedGold, Is.EqualTo(250));
        }

        [Test]
        public void Refresh_WhenNoWeapon_SetsIsWeaponEquippedFalse()
        {
            var state = new FakeStateReader { WeaponId = null };
            var presenter = new LobbyPresenter(new FakeRouter(), state);
            var view = new LobbyView();

            view.Refresh(presenter);

            Assert.That(view.IsWeaponEquipped, Is.False);
        }

        [Test]
        public void Refresh_WhenWeaponEquipped_SetsIsWeaponEquippedTrue()
        {
            var state = new FakeStateReader { WeaponId = 5 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);
            var view = new LobbyView();

            view.Refresh(presenter);

            Assert.That(view.IsWeaponEquipped, Is.True);
        }
    }
}
