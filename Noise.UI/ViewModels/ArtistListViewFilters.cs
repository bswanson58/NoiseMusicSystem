using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

                case ArtistFilterType.FilterArtistList:
                    retValue = new ArtistFilterList( onView );
                    break;
            }

            return retValue;
        }
    }

    public interface IArtistFilter {
        ArtistFilterType            FilterType { get; set; }
        bool                        DoesArtistMatch( UiArtist artist );

        string                      FilterText { get; set; }
        void                        SetFilterList( IEnumerable<string> filterList );

        ReactiveCommand<Unit, Unit> Clear { get; }
        void                        ClearFilter();

        event                       EventHandler FilterCleared;
    }

    abstract class ArtistFilterBase : ReactiveObject, IArtistFilter {
        private readonly ICollectionView    mCollectionView;
        private readonly List<String>       mFilterList;
        private string                      mFilterText;

        public  ArtistFilterType            FilterType { get; set; }
        public  ReactiveCommand<Unit, Unit> Clear { get; }
        public  event EventHandler          FilterCleared = delegate { };

        protected ArtistFilterBase( ICollectionView view ) {
            mCollectionView = view;

            mFilterList = new List<string>();
            Clear = ReactiveCommand.Create( OnClearFilter );
        }

        public abstract bool DoesArtistMatch( UiArtist artist );

        public string FilterText {
            get => mFilterText;
            set => UpdateFilter( value );
        }

        public void SetFilterList( IEnumerable<string> filterList ) {
            mFilterList.Clear();
            mFilterList.AddRange( filterList );

            mCollectionView.Refresh();
        }

        protected IEnumerable<string> GetFilterList() => mFilterList;

        private void OnClearFilter() {
            ClearFilter();
        }

        public void ClearFilter() {
            mFilterList.Clear();
            FilterText = String.Empty;

            FilterCleared( this, EventArgs.Empty );
        }

        private void UpdateFilter( string text ) {
            mFilterText = text;

            mCollectionView.Refresh();
            this.RaisePropertyChanged( nameof( FilterText ));
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
                retValue = FilterText.Equals( artist.Genre, StringComparison.OrdinalIgnoreCase );
            }

            return ( retValue );
        }
    }

    class ArtistFilterList : ArtistFilterBase {
        public ArtistFilterList( ICollectionView view ) :
            base( view ) {
            FilterType = ArtistFilterType.FilterArtistList;
        }

        public override bool DoesArtistMatch( UiArtist artist ) {
            var retValue = true;

            if( GetFilterList().Any()) {
                retValue = GetFilterList().Any( filter => artist.Name.Equals( filter, StringComparison.CurrentCultureIgnoreCase ));
            }

            return retValue;
        }
    }
}
