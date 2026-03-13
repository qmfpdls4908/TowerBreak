using System;

using NUnit.Framework;

namespace TowerBreak.DI.Tests
{
    public sealed class DIGlobalInstallerTests
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
        public void Install_WithNullInstaller_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DIGlobalContext.Install(null));
        }

        [Test]
        public void Install_RegistersDependenciesIntoGlobalContainer()
        {
            DIGlobalContext.Install(new TestGlobalInstaller());

            ITestService service = DIContainer.ResolveFromRegistered<ITestService>();

            Assert.That(service.Name, Is.EqualTo("global"));
            Assert.That(DIContainer.RegisteredContainerCount, Is.EqualTo(1));
        }

        [Test]
        public void EnsureContainer_CalledMultipleTimes_ReusesSingleRegisteredContainer()
        {
            DIContainer first = DIGlobalContext.EnsureContainer();
            DIContainer second = DIGlobalContext.EnsureContainer();

            Assert.That(second, Is.SameAs(first));
            Assert.That(DIContainer.RegisteredContainerCount, Is.EqualTo(1));
        }

        private interface ITestService
        {
            string Name { get; }
        }

        private sealed class TestService : ITestService
        {
            public TestService(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class TestGlobalInstaller : DIGlobalInstaller
        {
            protected override void InstallBindings(DIContainer container)
            {
                container.Register<ITestService>(new TestService("global"));
            }
        }
    }
}
