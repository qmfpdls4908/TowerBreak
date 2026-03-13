using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TowerBreak.DI.Tests
{
    public sealed class DIContainerTests
    {
        [SetUp]
        public void SetUp()
        {
            ClearRegisteredContainers();
        }

        [TearDown]
        public void TearDown()
        {
            ClearRegisteredContainers();
        }

        [Test]
        public void ResolveFromRegistered_ReturnsNewestRegisteredContainerValue()
        {
            DIContainer rootContainer = new();
            DIContainer sceneContainer = new();

            rootContainer.Register<ITestService>(new TestService("root"));
            sceneContainer.Register<ITestService>(new TestService("scene"));

            DIContainer.AddContainer(rootContainer);
            DIContainer.AddContainer(sceneContainer);

            ITestService resolved = DIContainer.ResolveFromRegistered<ITestService>();

            Assert.That(resolved.Name, Is.EqualTo("scene"));
        }

        [Test]
        public void RemoveContainer_FallsBackToPreviousContainerValue()
        {
            DIContainer rootContainer = new();
            DIContainer sceneContainer = new();

            rootContainer.Register<ITestService>(new TestService("root"));
            sceneContainer.Register<ITestService>(new TestService("scene"));

            DIContainer.AddContainer(rootContainer);
            DIContainer.AddContainer(sceneContainer);
            DIContainer.RemoveContainer(sceneContainer);

            ITestService resolved = DIContainer.ResolveFromRegistered<ITestService>();

            Assert.That(resolved.Name, Is.EqualTo("root"));
        }

        [Test]
        public void Inject_FillsAnnotatedFieldAndProperty()
        {
            DIContainer container = new();
            Consumer consumer = new();

            container.Register<ITestService>(new TestService("default"));
            container.Register<ITestService>(new TestService("named"), "special");
            DIContainer.AddContainer(container);

            DIContainer.Inject(consumer);

            Assert.That(consumer.FieldService, Is.Not.Null);
            Assert.That(consumer.FieldService.Name, Is.EqualTo("default"));
            Assert.That(consumer.PropertyService, Is.Not.Null);
            Assert.That(consumer.PropertyService.Name, Is.EqualTo("named"));
        }

        [Test]
        public void Installer_AwakeRegistersContainer_AndDestroyRemovesIt()
        {
            GameObject gameObject = new("DIInstallerTest");
            TestInstaller installer = gameObject.AddComponent<TestInstaller>();

            installer.InvokeAwake();

            ITestService resolved = DIContainer.ResolveFromRegistered<ITestService>();
            Assert.That(resolved.Name, Is.EqualTo("scene"));
            Assert.That(DIContainer.RegisteredContainerCount, Is.EqualTo(1));

            installer.InvokeOnDestroy();

            Assert.That(DIContainer.RegisteredContainerCount, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => DIContainer.ResolveFromRegistered<ITestService>());

            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        private static void ClearRegisteredContainers()
        {
            FieldInfo containersField = typeof(DIContainer).GetField("Containers", BindingFlags.Static | BindingFlags.NonPublic);
            object containersValue = containersField?.GetValue(null);
            if (containersValue is System.Collections.IList list)
            {
                list.Clear();
            }
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

        private sealed class Consumer
        {
            [DIInject]
            public ITestService FieldService;

            [DIInject("special")]
            public ITestService PropertyService { get; private set; }
        }

        private sealed class TestInstaller : DIInstaller
        {
            public void InvokeAwake()
            {
                Awake();
            }

            public void InvokeOnDestroy()
            {
                OnDestroy();
            }

            protected override void InstallBindings(DIContainer container)
            {
                container.Register<ITestService>(new TestService("scene"));
            }
        }
    }
}
