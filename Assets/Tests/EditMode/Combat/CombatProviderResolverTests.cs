using System.Threading.Tasks;

using NUnit.Framework;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

using UnityEngine;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatProviderResolverTests
    {
        [SetUp]
        public void SetUp()
        {
            DIGlobalContext.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            DIGlobalContext.Reset();
        }

        [Test]
        public void Resolve_WhenDIHasProvider_ReturnsDIRegisteredProvider()
        {
            StubProvider stub = new();
            DIGlobalContext.EnsureContainer().Register<IAddressableAssetProvider>(stub);

            IAddressableAssetProvider resolved = CombatProviderResolver.Resolve();

            Assert.That(resolved, Is.SameAs(stub));
        }

        [Test]
        public void Resolve_WhenDIHasNoProvider_ReturnsCombatDebugProvider()
        {
            IAddressableAssetProvider resolved = CombatProviderResolver.Resolve();

            Assert.That(resolved, Is.InstanceOf<CombatDebugAddressableAssetProvider>());
        }

        private sealed class StubProvider : IAddressableAssetProvider
        {
            public Task<T> LoadAssetAsync<T>(string key)
                where T : Object
            {
                return Task.FromResult<T>(null);
            }

            public void Release(Object asset)
            {
            }
        }
    }
}
