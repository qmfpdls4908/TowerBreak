using System;

namespace TowerBreak.Core
{
    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
        void LoadSceneAsync(string sceneName, Action onComplete = null);
    }
}
