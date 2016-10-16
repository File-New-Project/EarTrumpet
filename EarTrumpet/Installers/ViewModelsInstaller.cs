using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EarTrumpet.ViewModels;

namespace EarTrumpet.Installers
{
    public sealed class ViewModelsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAudioMixerViewModel>()
                .ImplementedBy<AudioMixerViewModel>()
            );
        }
    }
}
