//
//  RemoteDataClient.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteDataClient.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "RoTrack.h"
#import "RoFavorite.h"
#import "ArtistListResult.h"
#import "ArtistListDelegate.h"
#import "AlbumListResult.h"
#import "AlbumListDelegate.h"
#import "TrackListResult.h"
#import "TrackListDelegate.h"
#import "FavoriteListResult.h"
#import "FavoritesListDelegate.h"
#import "Events.h"

@interface RemoteDataClient ()

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   ArtistListDelegate      *mArtistListDelegate;
@property (nonatomic, retain)   AlbumListDelegate       *mAlbumListDelegate;
@property (nonatomic, retain)   TrackListDelegate       *mTrackListDelegate;
@property (nonatomic, retain)   FavoriteListDelegate    *mFavoriteListDelegate;

- (void) initObjectMappings;
- (void) onArtistList:(ArtistListResult *) result;
- (void) onAlbumList:(AlbumListResult *) result;
- (void) onTrackList:(TrackListResult *) result;
- (void) onFavoriteList:(FavoriteListResult *) result;

@end

@implementation RemoteDataClient

@synthesize mClient;
@synthesize mArtistListDelegate;
@synthesize mAlbumListDelegate;
@synthesize mTrackListDelegate;
@synthesize mFavoriteListDelegate;

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mClient = nil;
    self.mArtistListDelegate = nil;
    self.mAlbumListDelegate = nil;
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
    [mapping mapAttributes:@"DbId", @"Name", @"Website", @"AlbumCount", @"Rating", @"Genre", @"IsFavorite", nil];
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

    self.mArtistListDelegate = [[[ArtistListDelegate alloc] initWithArtistListBlock:^(ArtistListResult *result) { [self onArtistList:result]; } ] autorelease];
    self.mAlbumListDelegate = [[[AlbumListDelegate alloc] initWithAlbumListBlock:^(AlbumListResult *result) { [self onAlbumList:result]; } ] autorelease];
    self.mTrackListDelegate = [[[TrackListDelegate alloc] initWithTrackListBlock:^(TrackListResult *result) { [self onTrackList:result]; } ] autorelease];
    self.mFavoriteListDelegate = [[[FavoriteListDelegate alloc] initWithFavoriteListBlock:^(FavoriteListResult *result) { [self onFavoriteList:result]; } ] autorelease];
}

@end
