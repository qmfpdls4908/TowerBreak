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

        [Test]
        public void Update_MovesLeft_BasedOnMoveSpeed()
        {
            // Arrange
            var enemyData = new EnemyRow 
            { 
                MoveSpeed = 5f 
            };
            
            var gameObject = new GameObject("TestEnemy");
            var controller = gameObject.AddComponent<EnemyController>();
            controller.Initialize(enemyData);
            
            Vector3 initialPosition = gameObject.transform.position;
            
            // Act
            controller.Update();
            
            // Assert
            Assert.That(gameObject.transform.position.x, Is.LessThan(initialPosition.x));
        }
    }
}
