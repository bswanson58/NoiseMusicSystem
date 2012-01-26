namespace Noise.UI.Support {
	public class DialogServiceResolver {
		private static readonly IDialogService	mDefaultContext = new DialogService();
		private static			IDialogService	mCurrent;

		public static IDialogService Current {
			get { return( mCurrent ?? ( mCurrent = mDefaultContext )); }
			set { mCurrent = value; }
		}

		public IDialogService DialogService() {
			return( Current );
		}
	}
}
