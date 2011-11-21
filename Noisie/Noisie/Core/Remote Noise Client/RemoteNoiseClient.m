//
//  RemoteNoiseClient.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteNoiseClient.h"
#import "ServerVersion.h"
#import "ServerVersionDelegate.h"

@interface RemoteNoiseClient () <RKObjectLoaderDelegate>

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   ServerVersionDelegate   *mServerVersionDelegate;

- (void) initObjectMappings;

- (void) onServerVersion:(ServerVersion *) version;

@end

@implementation RemoteNoiseClient

@synthesize mClient;
@synthesize mServerVersionDelegate;

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[[RKObjectManager alloc] initWithBaseURL:serverAddress] autorelease];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];
}

- (void) dealloc {
    self.mClient = nil;
    
    [super dealloc];
}

- (void) requestServerVersion {
    [self.mClient loadObjectsAtResourcePath:@"serverVersion" 
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[ServerVersion class]]
                                   delegate:self.mServerVersionDelegate];
}

- (void) onServerVersion:(ServerVersion *)version {
    NSLog( @"Connected to server version: %@.%@.%@", version.Major, version.Minor, version.Build );
}

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {
    NSLog( @"%@", @"RemoteNoiseClient:didLoadObjects called!" );
}  

- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteNoiseClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[ServerVersion class]];
    [mapping mapAttributes:@"Major", @"Minor", @"Build", @"Revision", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    self.mServerVersionDelegate = [[[ServerVersionDelegate alloc] initWithServerVersionBlock:^(ServerVersion *version) { [self onServerVersion:version]; } ] autorelease];
}
@end
