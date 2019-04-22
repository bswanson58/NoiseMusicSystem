namespace Noise.Infrastructure.Interfaces {
	public interface IBlobStorageManager {
		void			SetResolver( IBlobStorageResolver resolver );
		bool			Initialize( string rootStoragePath );

		bool			OpenStorage();
		bool			CreateStorage();
		void			CloseStorage();
		void			DeleteStorage();

		bool			IsOpen { get; }
		IBlobStorage	GetStorage();
	}
}
