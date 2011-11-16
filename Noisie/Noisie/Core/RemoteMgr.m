//
//  RemoteMgr.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteMgr.h"
#import "RemoteDataClient.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "Events.h"

@interface RemoteMgr ()

@property (nonatomic, retain)   RemoteDataClient    *mDataClient;

- (void) onArtistListRequest:(NSNotification *) notification;
- (void) onAlbumListRequest:(NSNotification *) notification;
- (void) onTrackListRequest:(NSNotification *) notification;

@end

@implementation RemoteMgr

@synthesize mDataClient;

- (id) init {
    self = [super init];
    if( self ) {
        self.mDataClient = [[[RemoteDataClient alloc] init] autorelease];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistListRequest:) name:EventArtistListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumListRequest:) name:EventAlbumListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackListRequest:) name:EventTrackListRequest object:nil];
    }
    
    return( self );
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mDataClient = nil;
    
    [super dealloc];
}

- (void) initialize:(NSString *)severAddress {
    [self.mDataClient initializeClient:[NSString stringWithFormat:@"%@/Data", severAddress]];
}

- (void) onArtistListRequest:(NSNotification *)notification {
    [self.mDataClient requestArtistList];
}

- (void) onAlbumListRequest:(NSNotification *)notification {
    RoArtist    *forArtist = [notification object];
    
    [self.mDataClient requestAlbumList:forArtist.DbId];
}

- (void) onTrackListRequest:(NSNotification *)notification {
    RoAlbum     *forAlbum = [notification object];
    
    [self.mDataClient requestTrackList:forAlbum.DbId];
}

@end
