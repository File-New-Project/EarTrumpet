using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace EarTrumpet.Installers
{
    public sealed class ViewsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<MainWindow>().LifestyleSingleton()
            );
        }
    }
}
