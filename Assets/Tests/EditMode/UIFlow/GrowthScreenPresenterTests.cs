using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Growth;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class GrowthScreenPresenterTests
    {
        private sealed class FakeStateReader : IGrowthStateReader
        {
            public int? EquippedId;
            public int? CandidateId;
            public int DeltaAtk;
            public float DeltaSpd;
            public float DeltaPush;

            public int? EquippedWeaponId => EquippedId;
            public int? CandidateWeaponId => CandidateId;
            public int DeltaAttack => DeltaAtk;
            public float DeltaAttackSpeed => DeltaSpd;
            public float DeltaPushPower => DeltaPush;
        }

        private sealed class FakeRouter : IGrowthFlowRouter
        {
            public int EquipCount;
            public int RerollCount;
            public int ContinueCount;

            public void Equip() => EquipCount++;
            public void Reroll() => RerollCount++;
            public void Continue() => ContinueCount++;
        }

        [Test]
        public void Constructor_NullStateReader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthScreenPresenter(null, new FakeRouter()));
        }

        [Test]
        public void Constructor_NullRouter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GrowthScreenPresenter(new FakeStateReader(), null));
        }

        [Test]
        public void HasEquippedWeapon_WhenNull_ReturnsFalse()
        {
            var state = new FakeStateReader { EquippedId = null };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.HasEquippedWeapon, Is.False);
        }

        [Test]
        public void HasEquippedWeapon_WhenSet_ReturnsTrue()
        {
            var state = new FakeStateReader { EquippedId = 3 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.HasEquippedWeapon, Is.True);
        }

        [Test]
        public void HasCandidateWeapon_WhenNull_ReturnsFalse()
        {
            var state = new FakeStateReader { CandidateId = null };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.HasCandidateWeapon, Is.False);
        }

        [Test]
        public void HasCandidateWeapon_WhenSet_ReturnsTrue()
        {
            var state = new FakeStateReader { CandidateId = 7 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.HasCandidateWeapon, Is.True);
        }

        [Test]
        public void IsComparisonAvailable_WhenBothPresent_ReturnsTrue()
        {
            var state = new FakeStateReader { EquippedId = 1, CandidateId = 2 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.IsComparisonAvailable, Is.True);
        }

        [Test]
        public void IsComparisonAvailable_WhenNoEquipped_ReturnsFalse()
        {
            var state = new FakeStateReader { EquippedId = null, CandidateId = 2 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.IsComparisonAvailable, Is.False);
        }

        [Test]
        public void IsComparisonAvailable_WhenNoCandidate_ReturnsFalse()
        {
            var state = new FakeStateReader { EquippedId = 1, CandidateId = null };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.IsComparisonAvailable, Is.False);
        }

        [Test]
        public void DeltaAttack_ReturnsStateReaderValue()
        {
            var state = new FakeStateReader { DeltaAtk = 5 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());

            Assert.That(presenter.DeltaAttack, Is.EqualTo(5));
        }

        [Test]
        public void OnEquip_CallsRouterEquip()
        {
            var router = new FakeRouter();
            var presenter = new GrowthScreenPresenter(new FakeStateReader(), router);

            presenter.OnEquip();

            Assert.That(router.EquipCount, Is.EqualTo(1));
        }

        [Test]
        public void OnEquip_DoesNotCallOtherRouterMethods()
        {
            var router = new FakeRouter();
            var presenter = new GrowthScreenPresenter(new FakeStateReader(), router);

            presenter.OnEquip();

            Assert.That(router.RerollCount, Is.EqualTo(0));
            Assert.That(router.ContinueCount, Is.EqualTo(0));
        }

        [Test]
        public void OnReroll_CallsRouterReroll()
        {
            var router = new FakeRouter();
            var presenter = new GrowthScreenPresenter(new FakeStateReader(), router);

            presenter.OnReroll();

            Assert.That(router.RerollCount, Is.EqualTo(1));
        }

        [Test]
        public void OnContinue_CallsRouterContinue()
        {
            var router = new FakeRouter();
            var presenter = new GrowthScreenPresenter(new FakeStateReader(), router);

            presenter.OnContinue();

            Assert.That(router.ContinueCount, Is.EqualTo(1));
        }
    }
}
