using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using TowerBreak.Combat;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat.Tests
{
    public class EnemyControllerTests
    {
        [Test]
        public void Constructor_WithNullEnemyRow_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                var controller = new EnemyController(null);
            });
        }
    }
}
