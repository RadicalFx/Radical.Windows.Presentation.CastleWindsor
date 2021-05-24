using Castle;
using Castle.Facilities;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Naming;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace Topics.Radical.Windows.Presentation.Boot
{
    public class WindsorApplicationBootstrapper : ApplicationBootstrapper
    {
        IWindsorContainer container;
        Func<string, AssemblyScanner.FilterResults> assemblyScannerFilter = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorApplicationBootstrapper"/> class.
        /// </summary>
        public WindsorApplicationBootstrapper()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorApplicationBootstrapper"/> class.
        /// </summary>
        /// <param name="assemblyScannerFilter">The filter delegate to include or exclude assemblies from the scanning operation.</param>
        public WindsorApplicationBootstrapper(Func<string, AssemblyScanner.FilterResults> assemblyScannerFilter)
        {
            this.assemblyScannerFilter = assemblyScannerFilter;
        }

        protected override IServiceProvider CreateServiceProvider()
        {
            var nss = new DelegateNamingSubSystem()
            {
                SubSystemHandler = (s, hs) =>
                {
                    if (hs.Any(h => h.ComponentModel.IsOverridable()))
                    {
                        var nonOverridable = hs.Except(hs.Where(h => h.ComponentModel.IsOverridable()));
                        if (nonOverridable.Any())
                        {
                            return nonOverridable.Single();
                        }
                    }

                    return null;
                }
            };

            this.container = new Castle.Windsor.WindsorContainer();

            this.container.Kernel.AddSubSystem(SubSystemConstants.NamingKey, nss);
            this.container.Kernel.Resolver.AddSubResolver(new ArrayResolver(this.container.Kernel, true));

            var wrapper = new ServiceProviderWrapper(this.container);

            var bootConventions = new BootstrapConventions();

            this.container.Register(
                Component.For<IServiceProvider>()
                    .Instance(wrapper)
                    .PropertiesIgnore(pi => bootConventions.IgnorePropertyInjection(pi))
            );

            var scanner = new AssemblyScanner();
            if (assemblyScannerFilter != null)
            {
                scanner.AddAssemblyFilter(assemblyScannerFilter);
            }

            this.container.Register(Component.For<AssemblyScanner>().Instance(scanner));

            var allTypes = scanner.Scan()
                .SelectMany(assembly => assembly.GetTypes())
                .Distinct()
                .ToArray();

            this.container.Register(Component.For<Type[]>().Instance(allTypes));

            this.container.Register(Component.For<IWindsorContainer>().Instance(this.container));
            this.container.Register
            (
                Component.For<BootstrapConventions>()
                    .Instance(bootConventions)
                    .PropertiesIgnore(pi => bootConventions.IgnorePropertyInjection(pi))
            );

            this.container.AddFacility<Castle.Facilities.SubscribeToMessageFacility>();
            this.container.AddFacility<InjectViewInRegionFacility>();

            return wrapper;
        }

        [ImportMany]
        IEnumerable<IWindsorInstaller> Installers { get; set; }

        protected override void OnCompositionContainerComposed(CompositionContainer container, IServiceProvider serviceProvider)
        {
            base.OnCompositionContainerComposed(container, serviceProvider);

            var toInstall = this.Installers.Where(i => this.ShouldInstall(i)).ToArray();
            this.container.Install(toInstall);
        }

        protected virtual Boolean ShouldInstall(IWindsorInstaller installer)
        {
            return true;
        }

        protected override IEnumerable<T> ResolveAll<T>()
        {
            return this.container.ResolveAll<T>();
        }

    }
}
