using System;
using System.Windows;

namespace Topics.Radical.Windows.Presentation.Boot
{
    public class WindsorApplicationBootstrapper<TShellView> :
        WindsorApplicationBootstrapper
        where TShellView : Window
    {
        public WindsorApplicationBootstrapper()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorApplicationBootstrapper"/> class.
        /// </summary>
        /// <param name="assemblyScannerFilter">The filter delegate to include or exclude assemblies from the scanning operation.</param>
        public WindsorApplicationBootstrapper(Func<string, AssemblyScanner.FilterResults> assemblyScannerFilter)
            : base(assemblyScannerFilter)
        {
            this.UsingAsShell<TShellView>();
        }
    }
}
