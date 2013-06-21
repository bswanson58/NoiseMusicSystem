﻿using System;

namespace ReusableBits.Ui.Localization {
	// from: http://www.codeproject.com/Articles/249369/Advanced-WPF-Localization

	/// <summary>
	/// Invokes a method to obtain a localized value.
	/// </summary>
	public class MethodLocalizedValue : LocalizedValue {
		private readonly LocalizationCallback	mMethod;
		private readonly object					mParameter;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodLocalizedValue"/> class.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="method">The method.</param>
		/// <param name="parameter">The parameter to pass to the method.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is null.</exception>
		public MethodLocalizedValue( LocalizedProperty property, LocalizationCallback method, object parameter )
			: base( property ) {
			if( method == null ) {
				throw new ArgumentNullException( "method" );
			}

			mMethod = method;

			mParameter = parameter;
		}

		/// <summary>
		/// Retrieves the localized value from resources or by other means.
		/// </summary>
		/// <returns>
		/// The localized value.
		/// </returns>
		protected override object GetLocalizedValue() {
			var culture = Property.GetCulture();

			var uiCulture = Property.GetUICulture();

			return mMethod( culture, uiCulture, mParameter );
		}
	}
}
