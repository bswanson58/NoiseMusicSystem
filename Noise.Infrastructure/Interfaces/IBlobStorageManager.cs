namespace Noise.Infrastructure.Interfaces {
    public interface IInPlaceStorageManager : IBlobStorageManager { }

	public interface IBlobStorageManager {
		void			SetResolver( IBlobStorageResolver resolver );
		bool			Initialize( ILibraryConfiguration libraryConfiguration );

		bool			OpenStorage();
		bool			CreateStorage();
		void			CloseStorage();
		void			DeleteStorage();

		bool			IsOpen { get; }
		IBlobStorage	GetStorage();
	}
}
