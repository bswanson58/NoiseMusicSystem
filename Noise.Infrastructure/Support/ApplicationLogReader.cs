using System;
using System.IO;
using System.Text;

namespace Noise.Infrastructure.Support {
	public class ApplicationLogReader {
		private readonly StringBuilder	mLogText;

		public ApplicationLogReader() {
			mLogText = new StringBuilder();
		}

		public string LogText {
			get{ return( mLogText.ToString()); }
		}

		public bool ReadLog( string logName ) {
			bool	retValue = false;

			mLogText.Clear();

			try {
				if( File.Exists( logName )) {
					var	stream = new StreamReader( logName );

					while(!stream.EndOfStream ) {
						var line = stream.ReadLine();

						if( line.Contains( "------" )) {
							mLogText.Clear();
						}
						else {
							mLogText.Append( line );
							mLogText.AppendLine();
						}
					}

					stream.Close();

					retValue = true;
				}
			}
			catch( Exception ex ) {
				
			}

			return( retValue );
		}
	}
}
