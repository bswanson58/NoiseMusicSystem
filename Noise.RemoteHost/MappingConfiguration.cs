using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.RemoteDto;

namespace Noise.RemoteHost {
	public class RemoteModelProfile : Profile {
		public override string ProfileName {
			get { return( "RemoteHostModel" ); }
		}

		protected override void Configure() {
			CreateMap<DbArtist, RoArtist>()
				.ForMember( dest => dest.Genre, opt => opt.Ignore());

			CreateMap<DbAlbum, RoAlbum>()
				.ForMember( dest => dest.ArtistId, opt => opt.MapFrom( src => src.Artist ))
				.ForMember( dest => dest.Genre, opt => opt.Ignore());

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

			CreateMap<DbAlbum, RoAlbumInfo>()
				.ForMember( dest => dest.AlbumId, opt => opt.MapFrom( src => src.DbId ))
				.ForMember( dest => dest.AlbumCover, opt => opt.Ignore());

			CreateMap<AlbumSupportInfo, RoAlbumInfo>()
				.ForMember( dest => dest.AlbumId, opt => opt.Ignore())
				.ForMember( dest => dest.AlbumCover, opt => opt.Ignore());

			CreateMap<PlayQueueTrack, RoPlayQueueTrack>()
				.ForMember( dest => dest.ArtistName, opt => opt.MapFrom( src => src.Artist.Name ))
				.ForMember( dest => dest.AlbumName, opt => opt.MapFrom( src => src.Album.Name ))
				.ForMember( dest => dest.TrackId, opt => opt.MapFrom( src => src.Track.DbId ))
				.ForMember( dest => dest.TrackName, opt => opt.MapFrom( src => src.Track.Name ))
				.ForMember( dest => dest.DurationMilliseconds, opt => opt.MapFrom( src =>src.Track.DurationMilliseconds ))
				.ForMember( dest => dest.IsStrategySourced, opt => opt.MapFrom( src => src.StrategySource != eStrategySource.User ));

			CreateMap<SearchResultItem, RoSearchResultItem>()
				.ForMember( dest => dest.ArtistId, opt => opt.MapFrom( src => src.Artist != null ? src.Artist.DbId : Constants.cDatabaseNullOid ))
				.ForMember( dest => dest.ArtistName, opt => opt.MapFrom( src => src.Artist != null ? src.Artist.Name : "" ))
				.ForMember( dest => dest.AlbumId, opt => opt.MapFrom( src => src.Album != null ? src.Album.DbId : Constants.cDatabaseNullOid ))
				.ForMember( dest => dest.AlbumName, opt => opt.MapFrom( src => src.Album != null ? src.Album.Name : "" ))
				.ForMember( dest => dest.TrackId, opt => opt.MapFrom( src => src.Track != null ? src.Track.DbId : Constants.cDatabaseNullOid ))
				.ForMember( dest => dest.TrackName, opt => opt.MapFrom( src => src.Track != null ? src.Track.Name : "" ))
				.ForMember( dest => dest.CanPlay, opt => opt.Ignore());
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
