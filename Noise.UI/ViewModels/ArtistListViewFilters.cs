using System;
using System.ComponentModel;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using Noise.UI.Dto;
using ReactiveUI;

namespace Noise.UI.ViewModels {
    public enum ArtistFilterType {
        FilterText,
        FilterGenre,
        FilterArtistList
    }

    public class ArtistFilterTemplateSelector : DataTemplateSelector {
        public DataTemplate     TextTemplate { get; set; }
        public DataTemplate     GenreTemplate { get; set; }
        public DataTemplate     ArtistListTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = default( DataTemplate );

            if( item is IArtistFilter filter ) {
                switch( filter.FilterType ) {
                    case ArtistFilterType.FilterText:
                        retValue = TextTemplate;
                        break;

                    case ArtistFilterType.FilterGenre:
                        retValue = GenreTemplate;
                        break;

                    case ArtistFilterType.FilterArtistList:
                        retValue = ArtistListTemplate;
                        break;
                }
            }

            return retValue;
        }
    }

    public static class ArtistFilterFactory {
        public static IArtistFilter CreateArtistFilter( ArtistFilterType ofType, ICollectionView onView ) {
            var retValue = default( IArtistFilter );

            switch( ofType ) {
                case ArtistFilterType.FilterText:
                    retValue = new ArtistFilterText( onView );
                    break;

                case ArtistFilterType.FilterGenre:
                    retValue = new ArtistFilterGenre( onView );
                    break;
            }

            return retValue;
        }
    }

    public interface IArtistFilter {
        ArtistFilterType            FilterType { get; set; }
        bool                        DoesArtistMatch( UiArtist artist );

        string                      FilterText { get; set; }
        ReactiveCommand<Unit, Unit> Clear { get; }
        void                        ClearFilter();

        event                       EventHandler FilterCleared;
    }

    abstract class ArtistFilterBase : IArtistFilter {
        private readonly ICollectionView    mCollectionView;
        private string                      mFilterText;

        public  ArtistFilterType            FilterType { get; set; }
        public  ReactiveCommand<Unit, Unit> Clear { get; }
        public  event EventHandler          FilterCleared = delegate { };

        protected ArtistFilterBase( ICollectionView view ) {
            mCollectionView = view;

            Clear = ReactiveCommand.Create( OnClearFilter );
        }

        public abstract bool DoesArtistMatch( UiArtist artist );

        public string FilterText {
            get => mFilterText;
            set => UpdateFilter( value );
        }

        private void OnClearFilter() {
            ClearFilter();
        }

        public void ClearFilter() {
            FilterText = String.Empty;

            FilterCleared( this, EventArgs.Empty );
        }

        private void UpdateFilter( string text ) {
            mFilterText = text;

            mCollectionView.Refresh();
        }
    }

    class ArtistFilterText : ArtistFilterBase {
        public ArtistFilterText( ICollectionView view ) :
            base( view ) {
            FilterType = ArtistFilterType.FilterText;
        }

        public override bool DoesArtistMatch( UiArtist artist ) {
            var retValue = true;

            if(!string.IsNullOrWhiteSpace( FilterText )) {
                if(( artist.Name.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) &&
                   ( artist.Genre.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 )) {
                    retValue = false;
                }
            }

            return ( retValue );
        }
    }

    class ArtistFilterGenre : ArtistFilterBase {
        public ArtistFilterGenre( ICollectionView view ) :
            base( view ) {
            FilterType = ArtistFilterType.FilterGenre;
        }

        public override bool DoesArtistMatch( UiArtist artist ) {
            var retValue = true;

            if(!string.IsNullOrWhiteSpace( FilterText )) {
                if( artist.Genre.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                    retValue = false;
                }
            }

            return ( retValue );
        }
    }
}
