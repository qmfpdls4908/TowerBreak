using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerBreak.Core
{
    public sealed class SceneLoader : ISceneLoader
    {
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            if (CoroutineRunner.Instance == null)
            {
                GameObject runner = new("SceneLoaderCoroutineRunner");
                runner.AddComponent<CoroutineRunner>();
            }

            CoroutineRunner.Instance.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action onComplete)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
