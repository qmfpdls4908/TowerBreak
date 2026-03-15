using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class LobbyPresenterTests
    {
        private sealed class FakeRouter : ILobbyFlowRouter
        {
            public int OpenChallengeCount;
            public int OpenEquipmentCount;
            public int OpenRerollCount;
            public int CloseEquipmentCount;
            public int CloseRerollCount;

            public void OpenChallenge() => OpenChallengeCount++;
            public void OpenEquipment() => OpenEquipmentCount++;
            public void OpenReroll() => OpenRerollCount++;
            public void CloseEquipment() => CloseEquipmentCount++;
            public void CloseReroll() => CloseRerollCount++;
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
        public void Constructor_NullRouter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LobbyPresenter(null, new FakeStateReader()));
        }

        [Test]
        public void Constructor_NullStateReader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LobbyPresenter(new FakeRouter(), null));
        }

        [Test]
        public void CurrentFloorId_ReturnsStateReaderValue()
        {
            var state = new FakeStateReader { FloorId = 3 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.CurrentFloorId, Is.EqualTo(3));
        }

        [Test]
        public void Gold_ReturnsStateReaderValue()
        {
            var state = new FakeStateReader { GoldAmount = 500 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.Gold, Is.EqualTo(500));
        }

        [Test]
        public void HasEquippedWeapon_WhenNoWeapon_ReturnsFalse()
        {
            var state = new FakeStateReader { WeaponId = null };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.HasEquippedWeapon, Is.False);
        }

        [Test]
        public void HasEquippedWeapon_WhenWeaponEquipped_ReturnsTrue()
        {
            var state = new FakeStateReader { WeaponId = 7 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.HasEquippedWeapon, Is.True);
        }

        [Test]
        public void EquippedWeaponId_WhenNoWeapon_ReturnsNull()
        {
            var state = new FakeStateReader { WeaponId = null };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.EquippedWeaponId, Is.Null);
        }

        [Test]
        public void EquippedWeaponId_WhenWeaponEquipped_ReturnsId()
        {
            var state = new FakeStateReader { WeaponId = 12 };
            var presenter = new LobbyPresenter(new FakeRouter(), state);

            Assert.That(presenter.EquippedWeaponId, Is.EqualTo(12));
        }

        [Test]
        public void OnOpenChallenge_CallsRouterOpenChallenge()
        {
            var router = new FakeRouter();
            var presenter = new LobbyPresenter(router, new FakeStateReader());

            presenter.OnOpenChallenge();

            Assert.That(router.OpenChallengeCount, Is.EqualTo(1));
        }

        [Test]
        public void OnOpenChallenge_DoesNotCallOtherRouterMethods()
        {
            var router = new FakeRouter();
            var presenter = new LobbyPresenter(router, new FakeStateReader());

            presenter.OnOpenChallenge();

            Assert.That(router.OpenEquipmentCount, Is.EqualTo(0));
            Assert.That(router.OpenRerollCount, Is.EqualTo(0));
        }

        [Test]
        public void OnOpenEquipment_CallsRouterOpenEquipment()
        {
            var router = new FakeRouter();
            var presenter = new LobbyPresenter(router, new FakeStateReader());

            presenter.OnOpenEquipment();

            Assert.That(router.OpenEquipmentCount, Is.EqualTo(1));
        }

        [Test]
        public void OnOpenReroll_CallsRouterOpenReroll()
        {
            var router = new FakeRouter();
            var presenter = new LobbyPresenter(router, new FakeStateReader());

            presenter.OnOpenReroll();

            Assert.That(router.OpenRerollCount, Is.EqualTo(1));
        }
    }
}
