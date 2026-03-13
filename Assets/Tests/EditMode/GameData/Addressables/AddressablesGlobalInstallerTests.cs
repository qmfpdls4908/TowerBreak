using NUnit.Framework;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

namespace TowerBreak.GameData.Tests.Addressables
{
    public sealed class AddressablesGlobalInstallerTests
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
        public void Install_RegistersAddressableProviderIntoGlobalContainer()
        {
            AddressablesGlobalInstaller.Install();

            IAddressableAssetProvider provider = DIContainer.ResolveFromRegistered<IAddressableAssetProvider>();
            AddressableKeyValidator validator = DIContainer.ResolveFromRegistered<AddressableKeyValidator>();
            IAddressableAssetLoader loader = DIContainer.ResolveFromRegistered<IAddressableAssetLoader>();

            Assert.That(provider, Is.Not.Null);
            Assert.That(validator, Is.Not.Null);
            Assert.That(loader, Is.Not.Null);
            Assert.That(DIContainer.RegisteredContainerCount, Is.EqualTo(1));
        }
    }
}
