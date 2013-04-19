namespace ReusableBits.Ui.Controls {
	public class StatusMessage {
		public	string		Message { get; private set; }
		public	string		TemplateName { get; private set; }
		public	bool		ExtendActiveDisplay { get; set; }

		public StatusMessage( string message ) {
			Message = message;

			TemplateName = string.Empty;
		}

		public StatusMessage( string message, string templateName ) :
			this( message ) {
			TemplateName = templateName;
		}
	}
}
