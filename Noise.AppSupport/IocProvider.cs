using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class IocProvider : IIoc {
		private readonly CompositionContainer	mCompositionContainer;

		public IocProvider() {
			var catalog = new DirectoryCatalog(  @".\" );

			mCompositionContainer = new CompositionContainer( catalog );
		}

		public void ComposeParts( object o ) {
			mCompositionContainer.ComposeParts( o );
		}
	}
}
