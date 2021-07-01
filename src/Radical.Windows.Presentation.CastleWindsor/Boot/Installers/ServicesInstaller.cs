using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.ComponentModel.Composition;

namespace Topics.Radical.Windows.Presentation.Boot.Installers
{
    /// <summary>
    /// A Windsor installer.
    /// </summary>
    [Export(typeof(IWindsorInstaller))]
    public class ServicesInstaller : IWindsorInstaller
    {
        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            var conventions = container.Resolve<BootstrapConventions>();
            var currentDirectory = Helpers.EnvironmentHelper.GetCurrentDirectory();
            var types = container.Resolve<Type[]>();

            container.Register
            (
                Types.From(types)
                    .Where(t => conventions.IsService(t) && !conventions.IsExcluded(t))
                    .WithService.Select((type, baseTypes) => conventions.SelectServiceContracts(type))
                    .Configure(r =>
                   {
                       r.Overridable();
                       r.PropertiesIgnore(conventions.IgnorePropertyInjection);
                   })
            );
        }
    }
}