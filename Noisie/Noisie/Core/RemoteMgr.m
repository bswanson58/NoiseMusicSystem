//
//  RemoteMgr.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteMgr.h"
#import "RemoteDataClient.h"

@interface RemoteMgr ()

@property (nonatomic, retain)   RemoteDataClient    *mDataClient;

@end

@implementation RemoteMgr

@synthesize mDataClient;

- (id) init {
    self = [super init];
    if( self ) {
        self.mDataClient = [[[RemoteDataClient alloc] init] autorelease];
    }
    
    return( self );
}

- (void) dealloc {
    self.mDataClient = nil;
    
    [super dealloc];
}

- (void) initialize:(NSString *)severAddress {
    [self.mDataClient initializeClient:[NSString stringWithFormat:@"%@/Data", severAddress]];
    
    [self.mDataClient requestArtistList];
}

@end
