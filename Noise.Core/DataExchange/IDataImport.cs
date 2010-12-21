using System.Xml.Linq;

namespace Noise.Core.DataExchange {
	public interface IDataImport {
		int		Import( XElement rootElement, bool eliminateDuplicates );
	}
}
