
namespace Noise.Infrastructure.RemoteDto {
	public class BaseResult {
		public bool		Success { get; set; }
		public string	ErrorMessage { get; set; }

		public BaseResult() {
			ErrorMessage = "";
		}
	}
}
