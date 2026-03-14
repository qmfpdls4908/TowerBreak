using System;
using TowerBreak.UIFlow.Results;

namespace TowerBreak.Core.Router
{
    public sealed class RewardResultsFlowRouter : IRewardResultsFlowRouter
    {
        private readonly ISceneLoader _sceneLoader;

        public RewardResultsFlowRouter(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        public void Continue()
        {
            _sceneLoader.LoadScene("Growth");
        }
    }
}
