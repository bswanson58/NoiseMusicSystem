//
//  RemoteDataClient.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteDataClient.h"
#import "RoArtist.h"
#import "RoArtistInfo.h"
#import "RoAlbum.h"
#import "RoAlbumInfo.h"
#import "RoTrack.h"
#import "RoFavorite.h"
#import "ArtistListResult.h"
#import "ArtistListDelegate.h"
#import "ArtistInfoResult.h"
#import "ArtistInfoDelegate.h"
#import "AlbumListResult.h"
#import "AlbumListDelegate.h"
#import "AlbumInfoResult.h"
#import "AlbumInfoDelegate.h"
#import "TrackListResult.h"
#import "TrackListDelegate.h"
#import "FavoriteListResult.h"
#import "FavoritesListDelegate.h"
#import "Events.h"

@interface RemoteDataClient ()

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   ArtistListDelegate      *mArtistListDelegate;
@property (nonatomic, retain)   ArtistInfoDelegate      *mArtistInfoDelegate;
@property (nonatomic, retain)   AlbumListDelegate       *mAlbumListDelegate;
@property (nonatomic, retain)   AlbumInfoDelegate       *mAlbumInfoDelegate;
@property (nonatomic, retain)   TrackListDelegate       *mTrackListDelegate;
@property (nonatomic, retain)   FavoriteListDelegate    *mFavoriteListDelegate;

- (void) initObjectMappings;
- (void) onArtistList:(ArtistListResult *) result;
- (void) onArtistInfo:(ArtistInfoResult *) result;
- (void) onAlbumList:(AlbumListResult *) result;
- (void) onAlbumInfo:(AlbumInfoResult *) result;
- (void) onTrackList:(TrackListResult *) result;
- (void) onFavoriteList:(FavoriteListResult *) result;

@end

@implementation RemoteDataClient

@synthesize mClient;
@synthesize mArtistListDelegate;
@synthesize mArtistInfoDelegate;
@synthesize mAlbumListDelegate;
@synthesize mAlbumInfoDelegate;
@synthesize mTrackListDelegate;
@synthesize mFavoriteListDelegate;

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mClient = nil;
    self.mArtistListDelegate = nil;
    self.mArtistInfoDelegate = nil;
    self.mAlbumListDelegate = nil;
    self.mAlbumInfoDelegate = nil;
    self.mTrackListDelegate = nil;
    self.mFavoriteListDelegate = nil;
    
    [super dealloc];
}

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[[RKObjectManager alloc] initWithBaseURL:serverAddress] autorelease];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];
}

- (void) requestArtistList {
    [self.mClient loadObjectsAtResourcePath:@"artists" 
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[ArtistListResult class]]
                                   delegate:self.mArtistListDelegate];
}

- (void) onArtistList:(ArtistListResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistListUpdate object:result];
    }
}

- (void) requestArtistInfo:(NSNumber *)artistId {
    NSMutableString *path = [NSMutableString stringWithString:@"artist"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:artistId, @"artist", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[ArtistInfoResult class]]
                                   delegate:self.mArtistInfoDelegate];
}

- (void) onArtistInfo:(ArtistInfoResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistInfoUpdate object:result.ArtistInfo];
    }
}

- (void) requestAlbumList:(NSNumber *)forArtist {
    NSMutableString *path = [NSMutableString stringWithString:@"albums"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:forArtist, @"artist", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[AlbumListResult class]]
                                   delegate:self.mAlbumListDelegate];
}

- (void) onAlbumList:(AlbumListResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventAlbumListUpdate object:result];
    }
}

- (void) requestAlbumInfo:(NSNumber *)albumId {
    NSMutableString *path = [NSMutableString stringWithString:@"album"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:albumId, @"album", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[AlbumInfoResult class]]
                                   delegate:self.mAlbumInfoDelegate];
}

- (void) onAlbumInfo:(AlbumInfoResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventAlbumInfoUpdate object:result.AlbumInfo];
    }
}

- (void) requestTrackList:(NSNumber *)forAlbum {
    NSMutableString *path = [NSMutableString stringWithString:@"tracks"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:forAlbum, @"album", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[TrackListResult class]]
                                   delegate:self.mTrackListDelegate];
}

- (void) onTrackList:(TrackListResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventTrackListUpdate object:result];
    }
}

- (void) requestFavoriteList {
    [self.mClient loadObjectsAtResourcePath:@"favorites" 
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[FavoriteListResult class]]
                                   delegate:self.mFavoriteListDelegate];
}

- (void) onFavoriteList:(FavoriteListResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventFavoritesListUpdate object:result];
    }
}

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {
    NSLog( @"%@", @"RemoteDataClient:didLoadObjects called!" );
}  
   
- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteDataClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[RoArtist class]];
    [mapping mapAttributes:@"DbId", @"Name", @"AlbumCount", @"Rating", @"Genre", @"IsFavorite", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[ArtistListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Artists" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoArtist class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoAlbum class]];
    [mapping mapAttributes:@"DbId", @"Name", @"ArtistId", @"TrackCount", @"Rating", @"PublishedYear", @"Genre", @"IsFavorite", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[AlbumListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", @"ArtistId", nil];
    [mapping mapRelationship:@"Albums" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoAlbum class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoTrack class]];
    [mapping mapAttributes:@"DbId", @"Name", @"ArtistId", @"AlbumId", @"DurationMilliseconds", @"Rating", @"TrackNumber", @"VolumeName", @"IsFavorite", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[TrackListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", @"ArtistId", @"AlbumId", nil];
    [mapping mapRelationship:@"Tracks" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoTrack class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoFavorite class]];
    [mapping mapAttributes:@"ArtistId", @"AlbumId", @"TrackId", @"Artist", @"Album", @"Track", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[FavoriteListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Favorites" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoFavorite class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    mapping = [RKObjectMapping mappingForClass:[RoArtistInfo class]];
    [mapping mapAttributes:@"ArtistId", @"Website", @"Biography", @"ArtistImage", @"BandMembers", @"TopAlbums", @"SimilarArtists", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[ArtistInfoResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"ArtistInfo" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoArtistInfo class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoAlbumInfo class]];
    [mapping mapAttributes:@"AlbumId", @"AlbumCover", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[AlbumInfoResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"AlbumInfo" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoAlbumInfo class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    self.mArtistListDelegate = [[[ArtistListDelegate alloc] initWithArtistListBlock:^(ArtistListResult *result) { [self onArtistList:result]; } ] autorelease];
    self.mArtistInfoDelegate = [[[ArtistInfoDelegate alloc] initWithArtistInfoBlock:^(ArtistInfoResult *result) { [self onArtistInfo:result]; } ] autorelease];
    self.mAlbumListDelegate = [[[AlbumListDelegate alloc] initWithAlbumListBlock:^(AlbumListResult *result) { [self onAlbumList:result]; } ] autorelease];
    self.mAlbumInfoDelegate = [[[AlbumInfoDelegate alloc] initWithAlbumInfoBlock:^(AlbumInfoResult *result) { [self onAlbumInfo:result]; } ] autorelease];
    self.mTrackListDelegate = [[[TrackListDelegate alloc] initWithTrackListBlock:^(TrackListResult *result) { [self onTrackList:result]; } ] autorelease];
    self.mFavoriteListDelegate = [[[FavoriteListDelegate alloc] initWithFavoriteListBlock:^(FavoriteListResult *result) { [self onFavoriteList:result]; } ] autorelease];
}

@end
