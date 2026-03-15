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
            public void CloseEquipment() { }
            public void CloseReroll() { }
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
        public void Initialize_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new LobbyView();

            Assert.Throws<ArgumentNullException>(() => view.Initialize(null));
        }

        [Test]
        public void Initialize_SetsDisplayedFloorId()
        {
            var state = new FakeStateReader { FloorId = 2 };
            var wallet = new TowerBreak.Meta.State.PlayerWalletState(1000);
            var presenter = new LobbyPresenter(new FakeRouter(), state, wallet);
            var view = new LobbyView();

            view.Initialize(presenter);

            Assert.That(view.DisplayedFloorId, Is.EqualTo(2));
        }

        [Test]
        public void Initialize_SetsDisplayedGold()
        {
            var state = new FakeStateReader { GoldAmount = 250 };
            var wallet = new TowerBreak.Meta.State.PlayerWalletState(250);
            var presenter = new LobbyPresenter(new FakeRouter(), state, wallet);
            var view = new LobbyView();

            view.Initialize(presenter);

            Assert.That(view.DisplayedGold, Is.EqualTo(250));
        }

        [Test]
        public void Initialize_WhenNoWeapon_SetsIsWeaponEquippedFalse()
        {
            var state = new FakeStateReader { WeaponId = null };
            var wallet = new TowerBreak.Meta.State.PlayerWalletState(0);
            var presenter = new LobbyPresenter(new FakeRouter(), state, wallet);
            var view = new LobbyView();

            view.Initialize(presenter);

            Assert.That(view.IsWeaponEquipped, Is.False);
        }

        [Test]
        public void Initialize_WhenWeaponEquipped_SetsIsWeaponEquippedTrue()
        {
            var state = new FakeStateReader { WeaponId = 5 };
            var wallet = new TowerBreak.Meta.State.PlayerWalletState(0);
            var presenter = new LobbyPresenter(new FakeRouter(), state, wallet);
            var view = new LobbyView();

            view.Initialize(presenter);

            Assert.That(view.IsWeaponEquipped, Is.True);
        }

        [Test]
        public void Refresh_UpdatesDisplayedValues()
        {
            var state = new FakeStateReader { FloorId = 1, GoldAmount = 100 };
            var wallet = new TowerBreak.Meta.State.PlayerWalletState(100);
            var presenter = new LobbyPresenter(new FakeRouter(), state, wallet);
            var view = new LobbyView();

            view.Initialize(presenter);
            Assert.That(view.DisplayedFloorId, Is.EqualTo(1));

            state.FloorId = 5;
            state.GoldAmount = 500;
            view.Refresh();

            Assert.That(view.DisplayedFloorId, Is.EqualTo(5));
            Assert.That(view.DisplayedGold, Is.EqualTo(500));
        }
    }
}
