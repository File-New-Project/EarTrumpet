using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EarTrumpet.Services;

namespace EarTrumpet.Installers
{
    public sealed class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAudioSessionService>()
                .ImplementedBy<AudioSessionService>()
            );

            container.Register(
                Component.For<IAudioDeviceService>()
                .ImplementedBy<AudioDeviceService>()
            );

            container.Register(
                Component.For<IAudioService>()
                .ImplementedBy<AudioService>()
            );
        }
    }
}
