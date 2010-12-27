﻿using System.Windows;
using System.Windows.Controls;

// from: http://www.scottlogic.co.uk/blog/colin/2010/12/a-simplified-grid-markup-for-silverlight-and-wpf/

namespace Noise.UI.Behaviours {
	public class GridUtils {
		/// <summary>
		/// Identified the RowDefinitions attached property
		/// </summary>
		public static readonly DependencyProperty RowDefinitionsProperty =
        DependencyProperty.RegisterAttached( "RowDefinitions", typeof( string ), typeof( GridUtils ),
				new PropertyMetadata( "", OnRowDefinitionsPropertyChanged ));

		/// <summary>
		/// Gets the value of the RowDefinitions property
		/// </summary>
		public static string GetRowDefinitions( DependencyObject d ) {
			return (string)d.GetValue( RowDefinitionsProperty );
		}

		/// <summary>
		/// Sets the value of the RowDefinitions property
		/// </summary>
		public static void SetRowDefinitions( DependencyObject d, string value ) {
			d.SetValue( RowDefinitionsProperty, value );
		}

		/// <summary>
		/// Handles property changed event for the RowDefinitions property, constructing
		/// the required RowDefinitions elements on the grid which this property is attached to.
		/// </summary>
		private static void OnRowDefinitionsPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			var targetGrid = d as Grid;

			if( targetGrid != null ) {
				// construct the required row definitions
				targetGrid.RowDefinitions.Clear();
				var rowDefs = e.NewValue as string;

				if( rowDefs != null ) {
					var rowDefArray = rowDefs.Split( ',' );
					foreach( string rowDefinition in rowDefArray ) {
						if( rowDefinition.Trim() == "" ) {
							targetGrid.RowDefinitions.Add( new RowDefinition());
						}
						else {
							targetGrid.RowDefinitions.Add( new RowDefinition { Height = ParseLength( rowDefinition ) } );
						}
					}
				}
			}
		}

		/// <summary>
		/// Identifies the ColumnDefinitions attached property
		/// </summary>
		public static readonly DependencyProperty ColumnDefinitionsProperty =
        DependencyProperty.RegisterAttached( "ColumnDefinitions", typeof( string ), typeof( GridUtils ),
				new PropertyMetadata( "", OnColumnDefinitionsPropertyChanged ));

		/// <summary>
		/// Gets the value of the ColumnDefinitions property
		/// </summary>
		public static string GetColumnDefinitions( DependencyObject d ) {
			return (string)d.GetValue( ColumnDefinitionsProperty );
		}

		/// <summary>
		/// Sets the value of the ColumnDefinitions property
		/// </summary>
		public static void SetColumnDefinitions( DependencyObject d, string value ) {
			d.SetValue( ColumnDefinitionsProperty, value );
		}

		/// <summary>
		/// Handles property changed event for the ColumnDefinitions property, constructing
		/// the required ColumnDefinitions elements on the grid which this property is attached to.
		/// </summary>
		private static void OnColumnDefinitionsPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			var targetGrid = d as Grid;

			if( targetGrid != null ) {
				// construct the required column definitions
				targetGrid.ColumnDefinitions.Clear();
				var columnDefs = e.NewValue as string;

				if( columnDefs != null ) {
					var columnDefArray = columnDefs.Split( ',' );
					foreach( string columnDefinition in columnDefArray ) {
						if( columnDefinition.Trim() == "" ) {
							targetGrid.ColumnDefinitions.Add( new ColumnDefinition());
						}
						else {
							targetGrid.ColumnDefinitions.Add( new ColumnDefinition { Width = ParseLength( columnDefinition ) } );
						}
					}
				}
			}
		}

		/// <summary>
		/// Parses a string to create a GridLength
		/// </summary>
		private static GridLength ParseLength( string length ) {
			length = length.Trim();

			if( length.ToLowerInvariant().Equals( "auto" ) ) {
				return new GridLength( 0, GridUnitType.Auto );
			}

			if( length.Contains( "*" ) ) {
				length = length.Replace( "*", "" );

				if( string.IsNullOrWhiteSpace( length )) {
					return( new GridLength( 1.0, GridUnitType.Star ));
				}

				return new GridLength( double.Parse( length ), GridUnitType.Star );
			}

			return new GridLength( double.Parse( length ), GridUnitType.Pixel );
		}
	}
}
