using System;

namespace TowerBreak.DI
{
    public static class DIGlobalContext
    {
        private static DIContainer container;

        public static DIContainer EnsureContainer()
        {
            if (container != null)
            {
                return container;
            }

            container = new DIContainer();
            DIContainer.AddContainer(container);
            return container;
        }

        public static void Install(DIGlobalInstaller installer)
        {
            if (installer == null)
            {
                throw new ArgumentNullException(nameof(installer));
            }

            installer.Install(EnsureContainer());
        }

        public static void Reset()
        {
            if (container == null)
            {
                return;
            }

            DIContainer.RemoveContainer(container);
            container = null;
        }
    }
}
