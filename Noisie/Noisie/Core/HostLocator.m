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
@property (nonatomic, retain, readwrite) NSMutableArray         *services;
@property (nonatomic, retain, readwrite) NSNetServiceBrowser    *netServiceBrowser;
@property (nonatomic, retain, readwrite) NSNetService           *currentResolve;

- (void) stopCurrentResolve;

@end

@implementation HostLocator

@synthesize ownEntry;
@synthesize ownName;
@synthesize services;
@synthesize netServiceBrowser;
@synthesize currentResolve;

- (void)dealloc {
    // Cleanup any running resolve and free memory
    [self stopCurrentResolve];
    self.services = nil;
    
    [self.netServiceBrowser stop];
    self.netServiceBrowser = nil;

    self.ownEntry = nil;
    self.ownName = nil;
    
    [super dealloc];
}

// Creates an NSNetServiceBrowser that searches for services of a particular type in a particular domain.
// If a service is currently being resolved, stop resolving it and stop the service browser from
// discovering other services.
- (BOOL)searchForServicesOfType:(NSString *)type inDomain:(NSString *)domain {
    self.services = [[[NSMutableArray alloc] init] autorelease];
    
    [self.netServiceBrowser stop];
    [self.services removeAllObjects];
    
    NSNetServiceBrowser *aNetServiceBrowser = [[NSNetServiceBrowser alloc] init];
    
    if(!aNetServiceBrowser) {
        // The NSNetServiceBrowser couldn't be allocated and initialized.
        return NO;
    }
    
    aNetServiceBrowser.delegate = self;
    self.netServiceBrowser = aNetServiceBrowser;
    [aNetServiceBrowser release];
    [self.netServiceBrowser searchForServicesOfType:type inDomain:domain];
    
    self.ownName = @"Noisie";
    
    return YES;
}

- (void)stopCurrentResolve {
    [self.currentResolve stop];
    self.currentResolve = nil;
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)netServiceBrowser didRemoveService:(NSNetService *)service moreComing:(BOOL)moreComing {
    // If a service went away, stop resolving it if it's currently being resolved and remove it from the list.
    if (self.currentResolve && [service isEqual:self.currentResolve]) {
        [self stopCurrentResolve];
    }
    
    [self.services removeObject:service];
    if (self.ownEntry == service)
        self.ownEntry = nil;
}   

- (void)netServiceBrowser:(NSNetServiceBrowser *)netServiceBrowser didFindService:(NSNetService *)service moreComing:(BOOL)moreComing {
    // If a service came online, add it to the list.
    if ([service.name isEqual:self.ownName]) {
        self.ownEntry = service;
    }
    else {
        NSString    *serviceName = service.name;
        
        [self.services addObject:service];
    
        self.currentResolve = service;
        [self.currentResolve setDelegate:self];
    
        // Attempt to resolve the service. A value of 0.0 sets an unlimited time to resolve it. The user can
        // choose to cancel the resolve by selecting another service in the table view.
        [self.currentResolve resolveWithTimeout:0.0];
    }
}   

// This should never be called, since we resolve with a timeout of 0.0, which means indefinite
- (void)netService:(NSNetService *)sender didNotResolve:(NSDictionary *)errorDict {
    [self stopCurrentResolve];
}

- (void)netServiceDidResolveAddress:(NSNetService *)service {
    assert(service == self.currentResolve);
    
    [service retain];
    [self stopCurrentResolve];
    
    NSString    *hostname = service.hostName;
    NSInteger   port = service.port;
    
    for (NSData* data in [service addresses]) {
        char addressBuffer[100];
        struct sockaddr_in* socketAddress = (struct sockaddr_in*) [data bytes];
        int sockFamily = socketAddress->sin_family;

        if (sockFamily == AF_INET || sockFamily == AF_INET6) {
            const char* addressStr = inet_ntop(sockFamily,
                                               &(socketAddress->sin_addr), addressBuffer,
                                               sizeof(addressBuffer));
            
            int port = ntohs(socketAddress->sin_port);
            if (addressStr && port)
                NSLog(@"Found service at %s:%d", addressStr, port);
        }
    }
    
    [service release];
}

@end
