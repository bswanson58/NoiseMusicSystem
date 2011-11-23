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
#import "ServerResultDelegate.h"
#import "BaseServerResult.h"
#import "Events.h"

@interface RemoteQueueClient ()

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   PlayQueueListDelegate   *mPlayQueueListDelegate;
@property (nonatomic, retain)   ServerResultDelegate    *mServerResultDelegate;

- (void) initObjectMappings;

- (void) onPlayQueueList:(PlayQueueListResult *) result;
- (void) onServerResult:(BaseServerResult *) result;

@end

@implementation RemoteQueueClient

@synthesize mClient;
@synthesize mPlayQueueListDelegate;
@synthesize mServerResultDelegate;

- (void) dealloc {
    self.mClient = nil;
    self.mPlayQueueListDelegate = nil;
    self.mServerResultDelegate = nil;
    
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
                                   delegate:self.mServerResultDelegate];
}

- (void) enqueueTrack:(NSNumber *)trackId {
    NSMutableString *path = [NSMutableString stringWithString:@"enqueueTrack"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:trackId, @"track", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self.mServerResultDelegate];
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

- (void) transportCommand:(NSNumber *)command {
    NSMutableString *path = [NSMutableString stringWithString:@"transportCommand"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:command, @"command", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self.mServerResultDelegate];
}

- (void) onServerResult:(BaseServerResult *)result {
    if(![result.Success isEqualToString:@"true"]) {
        NSLog( @"Error from server in RemoteQueueClient: %@", result.ErrorMessage );
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
    [mapping mapAttributes:@"TrackId", @"TrackName", @"AlbumName", @"ArtistName", @"DurationMilliseconds", @"IsPlaying", @"HasPlayed", @"IsFaulted", @"IsStrategySourced", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    mapping = [RKObjectMapping mappingForClass:[PlayQueueListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Tracks" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoPlayQueueTrack class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    self.mPlayQueueListDelegate = [[[PlayQueueListDelegate alloc] initWithPlayQueueListBlock:^(PlayQueueListResult *result) { [self onPlayQueueList:result]; } ] autorelease];
    self.mServerResultDelegate = [[[ServerResultDelegate alloc] initWithServerResultBlock:^(BaseServerResult *result) { [self onServerResult:result]; } ] autorelease];
}

@end
