using System;

namespace TowerBreak.DI
{
    public abstract class DIGlobalInstaller
    {
        public void Install(DIContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            InstallBindings(container);
        }

        protected abstract void InstallBindings(DIContainer container);
    }
}
