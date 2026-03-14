using System;

namespace TowerBreak.UIFlow.Title
{
    public sealed class TitleScreenView
    {
        public bool IsContinueEnabled { get; private set; }

        public void Refresh(bool isContinueAvailable)
        {
            IsContinueEnabled = isContinueAvailable;
        }

        public void Bind(TitleScreenPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            Refresh(presenter.IsContinueAvailable);
        }
    }
}
