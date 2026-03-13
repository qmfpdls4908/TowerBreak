using System;
using System.Threading.Tasks;

using NUnit.Framework;

using TowerBreak.GameData.Addressables;

using UnityEngine;

namespace TowerBreak.GameData.Tests.Addressables
{
    public sealed class AddressableAssetProviderTests
    {
        [Test]
        public void ValidateRequired_WithBlankKey_ThrowsArgumentException()
        {
            AddressableKeyValidator validator = new();

            Assert.Throws<ArgumentException>(() => validator.ValidateRequired("   ", "key"));
        }

        [Test]
        public void ValidateRequired_WithValue_ReturnsOriginalKey()
        {
            AddressableKeyValidator validator = new();

            string key = validator.ValidateRequired("enemy/basic", "key");

            Assert.That(key, Is.EqualTo("enemy/basic"));
        }

        [Test]
        public void LoadAssetAsync_WithBlankKey_ThrowsArgumentException()
        {
            AddressableAssetProvider provider = new(new StubAddressableAssetLoader(), new AddressableKeyValidator());

            Assert.ThrowsAsync<ArgumentException>(async () => await provider.LoadAssetAsync<Texture2D>(string.Empty));
        }

        [Test]
        public void LoadAssetAsync_WhenLoaderReturnsNull_ThrowsInvalidOperationException()
        {
            AddressableAssetProvider provider = new(new StubAddressableAssetLoader(), new AddressableKeyValidator());

            Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.LoadAssetAsync<Texture2D>("ui/icon"));
        }

        [Test]
        public async Task LoadAssetAsync_WhenLoaderReturnsAsset_ReturnsLoadedAsset()
        {
            Texture2D asset = new(4, 4);
            AddressableAssetProvider provider = new(new StubAddressableAssetLoader(asset), new AddressableKeyValidator());

            Texture2D loaded = await provider.LoadAssetAsync<Texture2D>("ui/icon");

            Assert.That(loaded, Is.SameAs(asset));

            UnityEngine.Object.DestroyImmediate(asset);
        }

        private sealed class StubAddressableAssetLoader : IAddressableAssetLoader
        {
            private readonly UnityEngine.Object asset;

            public StubAddressableAssetLoader(UnityEngine.Object asset = null)
            {
                this.asset = asset;
            }

            public Task<T> LoadAssetAsync<T>(string key)
                where T : UnityEngine.Object
            {
                return Task.FromResult(asset as T);
            }

            public void Release(UnityEngine.Object asset)
            {
            }
        }
    }
}
