using System;
using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.UI.Dto;

namespace Noise.UI {
	public class ViewModelProfile : Profile {
		public override string ProfileName => ( "ViewModel" );

	    public ViewModelProfile() {
			CreateMap<DbArtist, UiArtist>()
				.ForMember( dest => dest.ActiveYears, opt => opt.Ignore())
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayName, opt => opt.MapFrom( src => src.Name ))
				.ForMember( dest => dest.SortName, opt => opt.MapFrom( src => src.Name ))
				.ForMember( dest => dest.Rating, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ))
				.ForMember( dest => dest.FavoriteValue, opt => opt.Ignore())
				.ForMember( dest => dest.Website, opt => opt.Ignore());

			CreateMap<DbAlbum, UiAlbum>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.Rating, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ))
				.ForMember( dest => dest.FavoriteValue, opt => opt.Ignore());

			CreateMap<DbTrack, UiTrack>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore())
				.ForMember( dest => dest.Duration, opt => opt.MapFrom( src => new TimeSpan( 0, 0, 0, 0, src.DurationMilliseconds )))
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ))
                .ForMember( dest => dest.HasUserTags, opt => opt.Ignore());

			CreateMap<DbInternetStream, UiInternetStream>()
				.ForMember( dest => dest.IsLinked, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ));

			CreateMap<DbTag, UiCategory>()
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ))
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore());

			CreateMap<UiArtist, DbArtist>()
				.ForMember( dest => dest.DbId, opt => opt.Ignore())
				.ForMember( dest => dest.AlbumCount, opt => opt.Ignore())
				.ForMember( dest => dest.CalculatedGenre, opt => opt.Ignore())
				.ForMember( dest => dest.CalculatedRating, opt => opt.Ignore())
				.ForMember( dest => dest.DateAdded, opt => opt.Ignore())
				.ForMember( dest => dest.DateAddedTicks, opt => opt.Ignore())
				.ForMember( dest => dest.ExternalGenre, opt => opt.Ignore())
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.HasFavorites, opt => opt.Ignore())
				.ForMember( dest => dest.IsUserRating, opt => opt.Ignore())
				.ForMember( dest => dest.LastChangeTicks, opt => opt.Ignore())
				.ForMember( dest => dest.LastPlayedTicks, opt => opt.Ignore())
				.ForMember( dest => dest.LastViewedTicks, opt => opt.Ignore())
				.ForMember( dest => dest.PlayCount, opt => opt.Ignore())
				.ForMember( dest => dest.Rating, opt => opt.Ignore())
				.ForMember( dest => dest.UserGenre, opt => opt.Ignore())
				.ForMember( dest => dest.UserRating, opt => opt.Ignore())
				.ForMember( dest => dest.Version, opt => opt.Ignore())
				.ForMember( dest => dest.ViewCount, opt => opt.Ignore());
		}
	}
}
