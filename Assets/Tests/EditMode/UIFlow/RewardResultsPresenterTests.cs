using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.UIFlow.Results;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class RewardResultsPresenterTests
    {
        private sealed class FakeStateReader : IRewardResultsStateReader
        {
            public int Gold;
            public IReadOnlyList<int> WeaponIds = new List<int>();

            public int GoldEarned => Gold;
            public IReadOnlyList<int> GrantedWeaponIds => WeaponIds;
        }

        private sealed class FakeRouter : IRewardResultsFlowRouter
        {
            public int ContinueCount;

            public void Continue() => ContinueCount++;
        }

        [Test]
        public void Constructor_NullStateReader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RewardResultsPresenter(null, new FakeRouter()));
        }

        [Test]
        public void Constructor_NullRouter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RewardResultsPresenter(new FakeStateReader(), null));
        }

        [Test]
        public void GoldEarned_ReturnsStateReaderValue()
        {
            var state = new FakeStateReader { Gold = 300 };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());

            Assert.That(presenter.GoldEarned, Is.EqualTo(300));
        }

        [Test]
        public void GrantedWeaponIds_ReturnsStateReaderList()
        {
            var state = new FakeStateReader { WeaponIds = new List<int> { 1, 2 } };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());

            Assert.That(presenter.GrantedWeaponIds, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void HasWeaponRewards_WhenNoWeapons_ReturnsFalse()
        {
            var state = new FakeStateReader { WeaponIds = new List<int>() };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());

            Assert.That(presenter.HasWeaponRewards, Is.False);
        }

        [Test]
        public void HasWeaponRewards_WhenWeaponsGranted_ReturnsTrue()
        {
            var state = new FakeStateReader { WeaponIds = new List<int> { 5 } };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());

            Assert.That(presenter.HasWeaponRewards, Is.True);
        }

        [Test]
        public void WeaponRewardCount_ReturnsGrantedWeaponCount()
        {
            var state = new FakeStateReader { WeaponIds = new List<int> { 1, 2, 3 } };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());

            Assert.That(presenter.WeaponRewardCount, Is.EqualTo(3));
        }

        [Test]
        public void OnContinue_CallsRouterContinue()
        {
            var router = new FakeRouter();
            var presenter = new RewardResultsPresenter(new FakeStateReader(), router);

            presenter.OnContinue();

            Assert.That(router.ContinueCount, Is.EqualTo(1));
        }
    }
}
