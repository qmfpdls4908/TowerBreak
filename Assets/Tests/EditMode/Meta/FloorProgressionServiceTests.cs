using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Progression;

namespace TowerBreak.Meta.Tests
{
    public sealed class FloorProgressionServiceTests
    {
        // ---- helpers ----

        private static FloorRow MakeFloor(int id)
        {
            return new FloorRow { Id = id };
        }

        private static List<FloorRow> ThreeFloors()
        {
            return new List<FloorRow> { MakeFloor(1), MakeFloor(2), MakeFloor(3) };
        }

        // ---- null / empty guards ----

        [Test]
        public void Advance_NullFloors_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FloorProgressionService.Advance(1, BattleOutcome.Clear, null));
        }

        [Test]
        public void Advance_EmptyFloors_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                FloorProgressionService.Advance(1, BattleOutcome.Clear, new List<FloorRow>()));
        }

        [Test]
        public void Advance_UnknownFloorId_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                FloorProgressionService.Advance(99, BattleOutcome.Clear, ThreeFloors()));
        }

        // ---- clear: middle floor ----

        [Test]
        public void Advance_ClearMiddleFloor_CanContinueIsTrue()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(1, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.CanContinue, Is.True);
        }

        [Test]
        public void Advance_ClearMiddleFloor_ReturnsNextFloorId()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(1, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.NextFloorId, Is.EqualTo(2));
        }

        [Test]
        public void Advance_ClearMiddleFloor_IsRunCompleteIsFalse()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(1, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.IsRunComplete, Is.False);
        }

        [Test]
        public void Advance_ClearMiddleFloor_CurrentFloorIdPreserved()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(2, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.CurrentFloorId, Is.EqualTo(2));
        }

        // ---- clear: last floor ----

        [Test]
        public void Advance_ClearLastFloor_IsRunCompleteIsTrue()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(3, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.IsRunComplete, Is.True);
        }

        [Test]
        public void Advance_ClearLastFloor_NextFloorIdIsNull()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(3, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.NextFloorId, Is.Null);
        }

        [Test]
        public void Advance_ClearLastFloor_CanContinueIsFalse()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(3, BattleOutcome.Clear, ThreeFloors());

            Assert.That(result.CanContinue, Is.False);
        }

        // ---- fail: any floor ----

        [Test]
        public void Advance_FailAnyFloor_CanContinueIsFalse()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(1, BattleOutcome.Fail, ThreeFloors());

            Assert.That(result.CanContinue, Is.False);
        }

        [Test]
        public void Advance_FailAnyFloor_NextFloorIdIsNull()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(2, BattleOutcome.Fail, ThreeFloors());

            Assert.That(result.NextFloorId, Is.Null);
        }

        [Test]
        public void Advance_FailAnyFloor_IsRunCompleteIsFalse()
        {
            FloorProgressionResult result = FloorProgressionService.Advance(1, BattleOutcome.Fail, ThreeFloors());

            Assert.That(result.IsRunComplete, Is.False);
        }

        // ---- single floor ----

        [Test]
        public void Advance_SingleFloor_ClearMakesRunComplete()
        {
            List<FloorRow> floors = new() { MakeFloor(10) };

            FloorProgressionResult result = FloorProgressionService.Advance(10, BattleOutcome.Clear, floors);

            Assert.That(result.IsRunComplete, Is.True);
            Assert.That(result.CanContinue, Is.False);
            Assert.That(result.NextFloorId, Is.Null);
        }

        [Test]
        public void Advance_SingleFloor_FailNotRunComplete()
        {
            List<FloorRow> floors = new() { MakeFloor(10) };

            FloorProgressionResult result = FloorProgressionService.Advance(10, BattleOutcome.Fail, floors);

            Assert.That(result.IsRunComplete, Is.False);
            Assert.That(result.CanContinue, Is.False);
        }

        // ---- determinism ----

        [Test]
        public void Advance_SameInput_AlwaysReturnsSameResult()
        {
            FloorProgressionResult r1 = FloorProgressionService.Advance(1, BattleOutcome.Clear, ThreeFloors());
            FloorProgressionResult r2 = FloorProgressionService.Advance(1, BattleOutcome.Clear, ThreeFloors());

            Assert.That(r1.NextFloorId, Is.EqualTo(r2.NextFloorId));
            Assert.That(r1.IsRunComplete, Is.EqualTo(r2.IsRunComplete));
            Assert.That(r1.CanContinue, Is.EqualTo(r2.CanContinue));
        }
    }
}
