using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.UIFlow.Results;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class RewardResultsViewTests
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
            public void Continue() { }
        }

        [Test]
        public void Refresh_NullPresenter_ThrowsArgumentNullException()
        {
            var view = new RewardResultsView();

            Assert.Throws<ArgumentNullException>(() => view.Refresh(null));
        }

        [Test]
        public void Refresh_SetsDisplayedGold()
        {
            var state = new FakeStateReader { Gold = 150 };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());
            var view = new RewardResultsView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedGold, Is.EqualTo(150));
        }

        [Test]
        public void Refresh_SetsDisplayedWeaponRewardCount()
        {
            var state = new FakeStateReader { WeaponIds = new List<int> { 1, 2 } };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());
            var view = new RewardResultsView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedWeaponRewardCount, Is.EqualTo(2));
        }

        [Test]
        public void Refresh_WhenNoWeapons_SetsCountToZero()
        {
            var state = new FakeStateReader { WeaponIds = new List<int>() };
            var presenter = new RewardResultsPresenter(state, new FakeRouter());
            var view = new RewardResultsView();

            view.Refresh(presenter);

            Assert.That(view.DisplayedWeaponRewardCount, Is.EqualTo(0));
        }
    }
}
