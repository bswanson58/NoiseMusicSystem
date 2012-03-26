using Autofac;
using Caliburn.Micro.Autofac;

namespace Noise.TenFooter {
	public class AppBootstrapper : AutofacBootstrapper<ShellViewModel> {
		protected override void ConfigureBootstrapper() {
			base.ConfigureBootstrapper();

			//  override namespace naming convention
			EnforceNamespaceConvention = false;
			//  change our view model base type
			ViewModelBaseType = typeof( IShell );
		}

		protected override void ConfigureContainer( ContainerBuilder builder ) {

		}
	}
}
