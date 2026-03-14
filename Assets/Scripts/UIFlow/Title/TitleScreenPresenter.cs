using System;

namespace TowerBreak.UIFlow.Title
{
    public sealed class TitleScreenPresenter
    {
        private readonly ITitleFlowRouter _router;
        private readonly ITitleSaveStateReader _saveState;

        public TitleScreenPresenter(ITitleFlowRouter router, ITitleSaveStateReader saveState)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _saveState = saveState ?? throw new ArgumentNullException(nameof(saveState));
        }

        public bool IsContinueAvailable => _saveState.HasSaveData();

        public void OnStartNewGame()
        {
            _router.StartNewGame();
        }

        public void OnContinueGame()
        {
            if (!IsContinueAvailable)
            {
                return;
            }

            _router.ContinueGame();
        }
    }
}
