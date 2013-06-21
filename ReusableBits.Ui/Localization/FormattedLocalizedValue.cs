using System;

namespace ReusableBits.Ui.Localization {
	// from: http://www.codeproject.com/Articles/249369/Advanced-WPF-Localization

	/// <summary>
	/// Formats a list of objects to produce a string value.
	/// </summary>
	public class FormattedLocalizedValue : LocalizedValue {
		private	readonly string		mFormatString;
		private readonly object[]	mArgs;

		/// <summary>
		/// Initializes a new instance of the <see cref="FormattedLocalizedValue"/> class.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="formatString">The format string.</param>
		/// <param name="args">The args.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="formatString"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
		public FormattedLocalizedValue( LocalizedProperty property, string formatString, params object[] args )
			: base( property ) {
			if( formatString == null ) {
				throw new ArgumentNullException( "formatString" );
			}

			if( args == null ) {
				throw new ArgumentNullException( "args" );
			}

			mFormatString = formatString;

			mArgs = args;
		}

		/// <summary>
		/// Retrieves the localized value from resources or by other means.
		/// </summary>
		/// <returns>
		/// The localized value.
		/// </returns>
		protected override object GetLocalizedValue() {
			var culture = Property.GetCulture();

			return string.Format( culture, mFormatString, mArgs );
		}
	}
}
