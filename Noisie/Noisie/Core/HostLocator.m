//
//  HostLocator.m
//  Noisie
//
//  Created by William Swanson on 11/20/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "HostLocator.h"
#include <arpa/inet.h>

// A category on NSNetService that's used to sort NSNetService objects by their name.
@interface NSNetService (HostLocatorAdditions)

- (NSComparisonResult) localizedCaseInsensitiveCompareByName:(NSNetService *)aService;

@end

@implementation NSNetService (HostLocatorAdditions)

- (NSComparisonResult) localizedCaseInsensitiveCompareByName:(NSNetService *)aService {
    return [[self name] localizedCaseInsensitiveCompare:[aService name]];
}

@end@interface HostLocator () <NSNetServiceDelegate, NSNetServiceBrowserDelegate>

@property (nonatomic, retain, readwrite) NSNetService           *ownEntry;
@property (nonatomic, copy, readwrite)   NSString               *ownName;
@property (nonatomic, retain, readwrite) NSMutableArray         *mServices;
@property (nonatomic, retain, readwrite) NSNetServiceBrowser    *mServiceBrowser;
@property (nonatomic, retain, readwrite) NSNetService           *mCurrentResolve;

- (void) stopCurrentResolve;

@end

@implementation HostLocator

@synthesize ownEntry;
@synthesize ownName;
@synthesize mServices;
@synthesize mServiceBrowser;
@synthesize mCurrentResolve;
@synthesize delegate;

- (id) init {
    self = [super init];
    if( self ) {
        self.mServices = [[[NSMutableArray alloc] init] autorelease];
        
        NSNetServiceBrowser *aNetServiceBrowser = [[NSNetServiceBrowser alloc] init];
        if( aNetServiceBrowser ) {
            aNetServiceBrowser.delegate = self;
            self.mServiceBrowser = aNetServiceBrowser;
            [aNetServiceBrowser release];
        }
    }
    
    return( self );
}

- (void)dealloc {
    // Cleanup any running resolve and free memory
    [self stopCurrentResolve];
    self.mServices = nil;

    self.mServiceBrowser = nil;
    self.ownEntry = nil;
    self.ownName = nil;
    
    self.delegate = nil;
    
    [super dealloc];
}

// Creates an NSNetServiceBrowser that searches for services of a particular type in a particular domain.
// If a service is currently being resolved, stop resolving it and stop the service browser from
// discovering other services.
- (BOOL)searchForServicesOfType:(NSString *)type inDomain:(NSString *)domain {
    BOOL    retValue = NO;
    
    if( self.mServiceBrowser != nil ) {
        [self.mServiceBrowser stop];
        [self.mServices removeAllObjects];
    
        [self.mServiceBrowser searchForServicesOfType:type inDomain:domain];
    
        self.ownName = @"Noisie";
    
        retValue = YES;
    }
    
    return( retValue );
}

- (void)stopCurrentResolve {
    if( self.mCurrentResolve != nil ) {
        [self.mCurrentResolve stop];
        self.mCurrentResolve = nil;
    }
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)netServiceBrowser didRemoveService:(NSNetService *)service moreComing:(BOOL)moreComing {
    // If a service went away, stop resolving it if it's currently being resolved and remove it from the list.
    if( self.mCurrentResolve && [service isEqual:self.mCurrentResolve]) {
        [self stopCurrentResolve];
    }
    
    [self.mServices removeObject:service];
    if (self.ownEntry == service) {
        self.ownEntry = nil;
    }
}   

- (void)netServiceBrowser:(NSNetServiceBrowser *)netServiceBrowser didFindService:(NSNetService *)service moreComing:(BOOL)moreComing {
    // If a service came online, add it to the list.
    if ([service.name isEqual:self.ownName]) {
        self.ownEntry = service;
    }
    else {
        [self.mServices addObject:service];
    
        self.mCurrentResolve = service;
        [self.mCurrentResolve setDelegate:self];
    
        // Attempt to resolve the service. A value of 0.0 sets an unlimited time to resolve it. The user can
        // choose to cancel the resolve by selecting another service in the table view.
        [self.mCurrentResolve resolveWithTimeout:0.0];
    }
}   

// This should never be called, since we resolve with a timeout of 0.0, which means indefinite
- (void)netService:(NSNetService *)sender didNotResolve:(NSDictionary *)errorDict {
    [self stopCurrentResolve];
}

- (void)netServiceDidResolveAddress:(NSNetService *)service {
    assert(service == self.mCurrentResolve);
    
    [service retain];
    [self stopCurrentResolve];
    
    NSString    *hostname = service.hostName;
    NSRange     rangeOfDomain = [hostname rangeOfString:@".local."];
    if( rangeOfDomain.location != NSNotFound ) {
        hostname = [hostname substringToIndex:rangeOfDomain.location];
    }
    
//    if( delegate != nil ) {
//        [delegate onHostLocated:hostname port:service.port serverName:service.name];
//    }
    
    for (NSData* data in [service addresses]) {
        char addressBuffer[100];
        struct sockaddr_in* socketAddress = (struct sockaddr_in*) [data bytes];
        int sockFamily = socketAddress->sin_family;

        if (sockFamily == AF_INET ) { //|| sockFamily == AF_INET6) {
            const char* addressStr = inet_ntop(sockFamily,
                                               &(socketAddress->sin_addr), addressBuffer,
                                               sizeof(addressBuffer));
            
            int port = ntohs(socketAddress->sin_port);
            if( addressStr && port ) {
                NSLog(@"Found service at %s:%d", addressStr, port);
                
                if( delegate != nil ) {
                    [delegate onHostLocated:[NSString stringWithFormat:@"%s", addressStr] port:port serverName:service.name];
                }
            }
        }
    }
    
    [service release];
}

@end
