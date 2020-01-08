using System;
using System.ComponentModel;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
    public enum ArtistFilterType {
        FilterText,
        FilterGenre,
        FilterArtistList
    }

    public static class ArtistFilterFactory {
        public static IArtistFilter CreateArtistFilter( ArtistFilterType ofType, ICollectionView onView ) {
            var retValue = default( IArtistFilter );

            switch( ofType ) {
                case ArtistFilterType.FilterText:
                    retValue = new ArtistFilterText( onView );
                    break;
            }

            return retValue;
        }
    }

    public interface IArtistFilter {
        bool    DoesArtistMatch( UiArtist artist );

        void    UpdateFilter( string text );
    }

    public abstract class ArtistFilterBase : IArtistFilter {
        private readonly ICollectionView    mCollectionView;
        protected string                    mFilterText;

        protected ArtistFilterBase( ICollectionView view ) {
            mCollectionView = view;
        }

        public abstract bool DoesArtistMatch( UiArtist artist );

        public void UpdateFilter( string text ) {
            mFilterText = text;

            mCollectionView.Refresh();
        }
    }

    public class ArtistFilterText : ArtistFilterBase {
        public ArtistFilterText( ICollectionView view ) :
            base( view ) { }

        public override bool DoesArtistMatch( UiArtist artist ) {
            var retValue = true;

            if(!string.IsNullOrWhiteSpace( mFilterText )) {
                if(( artist.Name.IndexOf( mFilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) &&
                   ( artist.Genre.IndexOf( mFilterText, StringComparison.OrdinalIgnoreCase ) == -1 )) {
                    retValue = false;
                }
            }

            return ( retValue );
        }
    }
}
