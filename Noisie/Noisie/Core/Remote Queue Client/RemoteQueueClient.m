//
//  RemoteQueueClient.m
//  Noisie
//
//  Created by William Swanson on 11/16/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteQueueClient.h"
#import "BaseServerResult.h"

@interface RemoteQueueClient ()

@property (nonatomic, retain)   RKObjectManager     *mClient;

- (void) initObjectMappings;

@end

@implementation RemoteQueueClient

@synthesize mClient;

- (void) dealloc {
    self.mClient = nil;
    
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

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {
    NSLog( @"%@", @"RemoteDataClient:didLoadObjects called!" );
}  

- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteDataClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[BaseServerResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
}

@end
