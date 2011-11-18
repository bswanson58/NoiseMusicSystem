using System.Linq;
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

			CreateMap<DbArtist, RoArtistInfo>()
				.ForMember( dest => dest.ArtistId, opt => opt.MapFrom( src => src.DbId ))
				.ForMember( dest => dest.ArtistImage, opt => opt.Ignore())
				.ForMember( dest => dest.BandMembers, opt => opt.Ignore())
				.ForMember( dest => dest.Biography, opt => opt.Ignore())
				.ForMember( dest => dest.SimilarArtists, opt => opt.Ignore())
				.ForMember( dest => dest.TopAlbums, opt => opt.Ignore());

			CreateMap<ArtistSupportInfo, RoArtistInfo>()
				.ForMember( dest => dest.Biography, opt => opt.MapFrom( src => src.Biography != null ? src.Biography.Text : "" ))
				.ForMember( dest => dest.ArtistId, opt => opt.Ignore())
				.ForMember( dest => dest.BandMembers, opt => opt.MapFrom( src => src.BandMembers != null ? src.BandMembers.GetItems().ToArray() : new string[0]))
				.ForMember( dest => dest.ArtistImage, opt => opt.Ignore())
				.ForMember( dest => dest.Website, opt => opt.Ignore())
				.ForMember( dest => dest.SimilarArtists, opt => opt.MapFrom( src => src.SimilarArtist != null ? src.SimilarArtist.GetItems().ToArray() : new string[0]))
				.ForMember( dest => dest.TopAlbums, opt => opt.MapFrom( src => src.TopAlbums != null ? src.TopAlbums.GetItems().ToArray() : new string[0]));

			CreateMap<DbAlbum, RoAlbumInfo>()
				.ForMember( dest => dest.AlbumId, opt => opt.MapFrom( src => src.DbId ))
				.ForMember( dest => dest.AlbumCover, opt => opt.Ignore());

			CreateMap<AlbumSupportInfo, RoAlbumInfo>()
				.ForMember( dest => dest.AlbumId, opt => opt.Ignore())
				.ForMember( dest => dest.AlbumCover, opt => opt.Ignore());
		}
	}

	public static class MappingConfiguration {
		public static void Configure() {
			Mapper.AddProfile( new RemoteModelProfile());
//			Mapper.Initialize( x => x.AddProfile<RemoteModelProfile>() );

			Mapper.AssertConfigurationIsValid();
		}
	}
}
