using UnityEngine;

namespace TowerBreak.DI
{
    public abstract class DIInstaller : MonoBehaviour
    {
        public DIContainer Container { get; private set; }

        protected virtual void Awake()
        {
            Container = new DIContainer();
            DIContainer.AddContainer(Container);
            InstallBindings(Container);
        }

        protected virtual void OnDestroy()
        {
            DIContainer.RemoveContainer(Container);
            Container = null;
        }

        protected abstract void InstallBindings(DIContainer container);
    }
}
