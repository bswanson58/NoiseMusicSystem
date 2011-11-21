//
//  RemoteQueueClient.m
//  Noisie
//
//  Created by William Swanson on 11/16/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteQueueClient.h"
#import "RoPlayQueueTrack.h"
#import "PlayQueueListResult.h"
#import "PlayQueueListDelegate.h"
#import "BaseServerResult.h"
#import "Events.h"

@interface RemoteQueueClient ()

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   PlayQueueListDelegate   *mPlayQueueListDelegate;

- (void) initObjectMappings;

- (void) onPlayQueueList:(PlayQueueListResult *) result;

@end

@implementation RemoteQueueClient

@synthesize mClient;
@synthesize mPlayQueueListDelegate;

- (void) dealloc {
    self.mClient = nil;
    self.mPlayQueueListDelegate = nil;
    
    [super dealloc];
}

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[[RKObjectManager alloc] initWithBaseURL:serverAddress] autorelease];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];
}

- (void) enqueueAlbum:(NSNumber *)albumId {
    NSMutableString *path = [NSMutableString stringWithString:@"enqueueAlbum"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:albumId, @"album", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self];
}

- (void) enqueueTrack:(NSNumber *)trackId {
    NSMutableString *path = [NSMutableString stringWithString:@"enqueueTrack"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:trackId, @"track", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self];
}

- (void) requestPlayQueueList {
    [self.mClient loadObjectsAtResourcePath:@"queueList"
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[PlayQueueListResult class]]
                                   delegate:self.mPlayQueueListDelegate];
}

- (void) onPlayQueueList:(PlayQueueListResult *)result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventPlayQueueListUpdate object:result.Tracks];
    }
}

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {
    NSLog( @"%@", @"RemoteQueueClient:didLoadObjects called!" );
}  

- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteQueueClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[BaseServerResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoPlayQueueTrack class]];
    [mapping mapAttributes:@"TrackId", @"TrackName", @"AlbumName", @"ArtistName", @"IsPlaying", @"HasPlayed", @"IsFaulted", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    mapping = [RKObjectMapping mappingForClass:[PlayQueueListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Tracks" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoPlayQueueTrack class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    self.mPlayQueueListDelegate = [[[PlayQueueListDelegate alloc] initWithPlayQueueListBlock:^(PlayQueueListResult *result) { [self onPlayQueueList:result]; } ] autorelease];
}

@end
