using System;

using NUnit.Framework;

using TowerBreak.UIFlow.Growth;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class GrowthScreenViewTests
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
            public void Equip() { }
            public void Reroll() { }
            public void Continue() { }
        }

        [Test]
        public void Refresh_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new GrowthScreenView();

            Assert.Throws<ArgumentNullException>(() => view.Refresh(null));
        }

        [Test]
        public void Refresh_SetsIsComparisonAvailable()
        {
            var state = new FakeStateReader { EquippedId = 1, CandidateId = 2 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());
            var view = new GrowthScreenView();

            view.Refresh(presenter);

            Assert.That(view.IsComparisonAvailable, Is.True);
        }

        [Test]
        public void Refresh_SetsDisplayedEquippedWeaponId()
        {
            var state = new FakeStateReader { EquippedId = 4 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());
            var view = new GrowthScreenView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedEquippedWeaponId, Is.EqualTo(4));
        }

        [Test]
        public void Refresh_SetsDisplayedCandidateWeaponId()
        {
            var state = new FakeStateReader { CandidateId = 9 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());
            var view = new GrowthScreenView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedCandidateWeaponId, Is.EqualTo(9));
        }

        [Test]
        public void Refresh_SetsDisplayedDeltaAttack()
        {
            var state = new FakeStateReader { DeltaAtk = 3 };
            var presenter = new GrowthScreenPresenter(state, new FakeRouter());
            var view = new GrowthScreenView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedDeltaAttack, Is.EqualTo(3));
        }
    }
}
