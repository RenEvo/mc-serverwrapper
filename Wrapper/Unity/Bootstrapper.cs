using Microsoft.Practices.Unity;
using Wrapper.Engines;
using Wrapper.Managers;

namespace Wrapper.Unity
{
    /// <summary>
    /// Class Bootstrapper
    /// </summary>
    public sealed class Bootstrapper
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Bootstrapper Instance { get; private set; }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <value>The container.</value>
        public IUnityContainer Container { get; private set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            Instance = new Bootstrapper { Container = new UnityContainer() };
            Instance.RegisterServices();
        }

        /// <summary>
        /// Registers the services.
        /// </summary>
        private void RegisterServices()
        {
            // Register any services (implementations) here

            Container.RegisterType<ILogEngine, FileLogEngine>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IBackupEngine, BackupEngine>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPlayerManager, PlayerManager>(new ContainerControlledLifetimeManager());

            if (!string.IsNullOrWhiteSpace(MinecraftSettings.Default.ModProviderRoot) && MinecraftSettings.Default.ModProviderRoot.EndsWith(".git"))
                Container.RegisterType<IGameUpdateManager, GitGameUpdateManager>();
            else
                Container.RegisterType<IGameUpdateManager, GameUpdateManager>();

            Container.RegisterType<IApiEngine, WebApiEngine>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IServerManager, ServerManager>(new ContainerControlledLifetimeManager());
        }
    }
}
