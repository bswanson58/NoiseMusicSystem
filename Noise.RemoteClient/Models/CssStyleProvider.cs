﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagicGradients;
using MagicGradients.Parser;
using MagicGradients.Xaml;
using Newtonsoft.Json;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Xamarin.Essentials.Interfaces;

namespace Noise.RemoteClient.Models {
    class CssStyleProvider : ICssStyleProvider {
        private readonly IPreferences               mPreferences;
        private readonly IPlatformLog               mLog;
        private readonly DimensionsTypeConverter    mDimensionsTypeConverter;
        private readonly List<CssStyle>             mStyles;
        private string                              mPreferenceName;
        
        public  CssStyle                            CurrentStyle { get; private set; }
        public  GradientCollection                  CurrentGradient { get; }
        public  Dimensions                          GradientSize { get; private set; }

        public CssStyleProvider( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
            mDimensionsTypeConverter = new DimensionsTypeConverter();

            mStyles = new List<CssStyle>();
            CurrentGradient = new GradientCollection();
        }

        public void Initialize( string preferenceName ) {
            mPreferenceName = preferenceName;

            SelectDefaultStyle();
        }

        private void SelectDefaultStyle() {
            if(!mStyles.Any()) {
                LoadStyles();
            }

            if( mStyles.Any()) {
                var preferenceStyle = mPreferences.Get( mPreferenceName, String.Empty );

                SetStyle( String.IsNullOrWhiteSpace( preferenceStyle ) ? mStyles.FirstOrDefault() : 
                                                                         mStyles.FirstOrDefault( s => s.Name.Equals( preferenceStyle )));

                if( CurrentStyle == null ) {
                    SetStyle( mStyles.FirstOrDefault());
                }
            }
        }

        public void SelectNextStyle() {
            if( CurrentStyle != null ) {
                var index = mStyles.IndexOf( CurrentStyle );

                index++;

                if(( index > 0 ) &&
                   ( index < mStyles.Count )) {
                    SetStyle( mStyles[index]);
                }
                else {
                    SetStyle( mStyles.FirstOrDefault());
                }
            }
        }

        public void SelectPreviousStyle() {
            if( CurrentStyle != null ) {
                var index = mStyles.IndexOf( CurrentStyle );

                index--;

                if(( index >= 0 ) &&
                   ( index < mStyles.Count )) {
                    SetStyle( mStyles[index]);
                }
                else {
                    SetStyle( mStyles.LastOrDefault());
                }
            }
        }

        private void SetStyle( CssStyle style ) {
            if( style != null ) {
                try {
                    var parser = new CssGradientParser();
                    var gradients = parser.ParseCss( style.Css );

                    CurrentGradient.Gradients = new GradientElements<Gradient>( gradients );

                    if( String.IsNullOrWhiteSpace( style.Size )) {
                        GradientSize = Dimensions.Prop( 1, 1 );
                    }
                    else {
                        GradientSize = (Dimensions)mDimensionsTypeConverter.ConvertFromInvariantString( style.Size );
                    }

                    CurrentStyle = style;
                    mPreferences.Set( mPreferenceName, CurrentStyle.Name );
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( SetStyle ), ex );
                }
            }
        }

        private void LoadStyles() {
            try {
                mStyles.Clear();

                using( var stream = GetType().Assembly.GetManifestResourceStream( ResourceName( "CssStyles.json" ))) {
                    if( stream != null ) {
                        using( var reader = new StreamReader( stream )) {
                            var data = JsonConvert.DeserializeObject<CssStyleFile>( reader.ReadToEnd());

                            mStyles.AddRange( data.Styles );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( LoadStyles ), ex );
            }
        }

        private string ResourceName( string sourceName ) {
            var retValue = sourceName;

            if(!String.IsNullOrWhiteSpace( sourceName )) {
                var allResources = GetType().Assembly.GetManifestResourceNames();
                var matchedResource = allResources.FirstOrDefault( r => r.EndsWith( sourceName ));

                if( matchedResource != null ) {
                    retValue = matchedResource;
                }
            }

            return retValue;
        }
    }
}
