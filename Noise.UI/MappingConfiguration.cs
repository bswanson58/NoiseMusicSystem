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
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore())
				.ForMember( dest => dest.UiIsFavorite, opt => opt.Ignore())
				.ForMember( dest => dest.UiRating, opt => opt.Ignore());

			CreateMap<DbAlbum, UiAlbum>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore())
				.ForMember( dest => dest.DisplayGenre, opt => opt.Ignore())
				.ForMember( dest => dest.IsExpanded, opt => opt.Ignore())
				.ForMember( dest => dest.IsSelected, opt => opt.Ignore());

			CreateMap<DbTrack, UiTrack>()
				.ForMember( dest => dest.IsSelected, opt => opt.UseValue( false ))
				.ForMember( dest => dest.Duration, opt => opt.MapFrom( src => new TimeSpan( 0, 0, 0, 0, src.DurationMilliseconds )));
		}
	}

	public static class MappingConfiguration {
		public static void Configure() {
			Mapper.Initialize( x => x.AddProfile<ViewModelProfile>() );

			Mapper.AssertConfigurationIsValid();
		}
	}
}
