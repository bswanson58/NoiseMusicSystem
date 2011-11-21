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
#import "ServerResultDelegate.h"
#import "Events.h"
#include <arpa/inet.h>
#include <ifaddrs.h>

@interface RemoteNoiseClient () <RKObjectLoaderDelegate>

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, copy)     NSString                *mClientIpAddress;
@property (nonatomic, assign)   NSNumber                *mClientPort;
@property (nonatomic, retain)   NSString                *mCallbackAddress;
@property (nonatomic, retain)   ServerVersionDelegate   *mServerVersionDelegate;
@property (nonatomic, retain)   ServerResultDelegate    *mServerResultDelegate;

- (void) initObjectMappings;
- (NSString *)getIPAddress;

- (void) onServerVersion:(ServerVersion *) version;
- (void) onServerResult:(BaseServerResult *) result;

@end

@implementation RemoteNoiseClient

@synthesize mClient;
@synthesize mClientIpAddress;
@synthesize mClientPort;
@synthesize mCallbackAddress;
@synthesize mServerVersionDelegate;
@synthesize mServerResultDelegate;

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[[RKObjectManager alloc] initWithBaseURL:serverAddress] autorelease];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];

    self.mClientIpAddress = [self getIPAddress];
    self.mClientPort = [NSNumber numberWithInt:6502];
    self.mCallbackAddress = [NSString stringWithFormat:@"http://%@:%@/", self.mClientIpAddress, self.mClientPort];
}

- (void) dealloc {
    self.mClient = nil;
    self.mClientIpAddress = nil;
    self.mClientPort = nil;
    self.mCallbackAddress = nil;
    self.mServerVersionDelegate = nil;
    self.mServerResultDelegate = nil;
    
    [super dealloc];
}

- (void) requestServerVersion {
    [self.mClient loadObjectsAtResourcePath:@"serverVersion" 
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[ServerVersion class]]
                                   delegate:self.mServerVersionDelegate];
}

- (void) onServerVersion:(ServerVersion *)version {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventServerConnected object:version];
}

- (void) requestEvents {
    NSMutableString *path = [NSMutableString stringWithString:@"requestEvents"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:self.mCallbackAddress, @"address", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self.mServerResultDelegate];
}

- (void) revokeEvents {
    NSMutableString *path = [NSMutableString stringWithString:@"revokeEvents"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:self.mCallbackAddress, @"address", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[BaseServerResult class]]
                                   delegate:self.mServerResultDelegate];
}

- (void) onServerResult:(BaseServerResult *)result {
    if(![result.Success isEqualToString:@"true"]) {
        NSLog( @"Error from server in RemoteNoiseClient: %@", result.ErrorMessage );
    }
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
    
    mapping = [RKObjectMapping mappingForClass:[BaseServerResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    self.mServerVersionDelegate = [[[ServerVersionDelegate alloc] initWithServerVersionBlock:^(ServerVersion *version) { [self onServerVersion:version]; } ] autorelease];
    self.mServerResultDelegate = [[[ServerResultDelegate alloc] initWithServerResultBlock:^(BaseServerResult *result) { [self onServerResult:result]; } ] autorelease];
}

- (NSString *)getIPAddress {
    NSString *address = @"error";
    struct ifaddrs *interfaces = NULL;
    struct ifaddrs *temp_addr = NULL; 
    int success = 0;
    
    // retrieve the current interfaces - returns 0 on success
    success = getifaddrs(&interfaces);
    if (success == 0) {
        // Loop through linked list of interfaces
        temp_addr = interfaces;
        while(temp_addr != NULL) {
            if(temp_addr->ifa_addr->sa_family == AF_INET) {
                // Check if interface is en0 which is the wifi connection on the iPhone
                if([[NSString stringWithUTF8String:temp_addr->ifa_name] isEqualToString:@"en0"]) {
                    // Get NSString from C String
                    address = [NSString stringWithUTF8String:inet_ntoa(((struct sockaddr_in *)temp_addr->ifa_addr)->sin_addr)];
                    
                    NSLog( @"My IP address is:%@", address );
                }
            }
            
            temp_addr = temp_addr->ifa_next; 
        }
    }
    
    // Free memory
    freeifaddrs(interfaces);
    return address;
}

@end
