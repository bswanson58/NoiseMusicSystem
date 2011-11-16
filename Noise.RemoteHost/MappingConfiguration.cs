using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.RemoteDto;

namespace Noise.RemoteHost {
	public class RemoteModelProfile : Profile {
		public override string ProfileName {
			get { return( "RemoteHostModel" ); }
		}

		protected override void Configure() {
			CreateMap<DbAlbum, RoAlbum>()
				.ForMember( dest => dest.ArtistId, opt => opt.MapFrom( src => src.Artist ));

			CreateMap<DbTrack, RoTrack>()
				.ForMember( dest => dest.ArtistId, opt => opt.Ignore())
				.ForMember( dest => dest.AlbumId, opt => opt.MapFrom( src => src.Album ));
		}
	}

	public static class MappingConfiguration {
		public static void Configure() {
			Mapper.Initialize( x => x.AddProfile<RemoteModelProfile>() );

			Mapper.AssertConfigurationIsValid();
		}
	}
}
