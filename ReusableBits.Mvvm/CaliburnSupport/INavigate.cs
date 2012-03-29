namespace ReusableBits.Mvvm.CaliburnSupport {
	public interface INavigate {
		void	NavigateHome();
		void	NavigateTo( object screen );
		void	NavigateReturn( object fromScreen, bool closeScreen );
	}
}
