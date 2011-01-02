using System;
using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.UI.Dto;

namespace Noise.UI {
	public class ViewModelProfile : Profile {
		public override string ProfileName {
			get { return( "ViewModel" ); }
		}

		protected override void Configure() {
			CreateMap<DbArtist, UiArtist>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayName, opt => opt.MapFrom( src => src.Name ))
				.ForMember( dest => dest.SortName, opt => opt.MapFrom( src => src.Name ))
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ));

			CreateMap<DbAlbum, UiAlbum>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.IsExpanded, opt => opt.Ignore())
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ));

			CreateMap<DbTrack, UiTrack>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore())
				.ForMember( dest => dest.Duration, opt => opt.MapFrom( src => new TimeSpan( 0, 0, 0, 0, src.DurationMilliseconds )))
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ));

			CreateMap<DbInternetStream, UiInternetStream>()
				.ForMember( dest => dest.IsLinked, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.MapFrom( src => src.IsFavorite ))
				.ForMember( dest => dest.UiRating, opt => opt.MapFrom( src => src.Rating ));
		}
	}

	public static class MappingConfiguration {
		public static void Configure() {
			Mapper.Initialize( x => x.AddProfile<ViewModelProfile>() );

			Mapper.AssertConfigurationIsValid();
		}
	}
}
