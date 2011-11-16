//
//  RemoteMgr.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteMgr.h"
#import "RemoteDataClient.h"
#import "RemoteQueueClient.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "RoTrack.h"
#import "Events.h"

@interface RemoteMgr ()

@property (nonatomic, retain)   RemoteDataClient    *mDataClient;
@property (nonatomic, retain)   RemoteQueueClient   *mQueueClient;

- (void) onArtistListRequest:(NSNotification *) notification;
- (void) onAlbumListRequest:(NSNotification *) notification;
- (void) onTrackListRequest:(NSNotification *) notification;
- (void) onAlbumQueueRequest:(NSNotification *)notification;
- (void) onTrackQueueRequest:(NSNotification *)notification;

@end

@implementation RemoteMgr

@synthesize mDataClient;
@synthesize mQueueClient;

- (id) init {
    self = [super init];
    if( self ) {
        self.mDataClient = [[[RemoteDataClient alloc] init] autorelease];
        self.mQueueClient = [[[RemoteQueueClient alloc] init] autorelease];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistListRequest:) name:EventArtistListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumListRequest:) name:EventAlbumListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackListRequest:) name:EventTrackListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumQueueRequest:) name:EventQueueAlbumRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackQueueRequest:) name:EventQueueTrackRequest object:nil];
    }
    
    return( self );
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mDataClient = nil;
    self.mQueueClient = nil;
    
    [super dealloc];
}

- (void) initialize:(NSString *)serverAddress {
    [self.mDataClient initializeClient:[NSString stringWithFormat:@"%@/Data", serverAddress]];
    [self.mQueueClient initializeClient:[NSString stringWithFormat:@"%@/Queue", serverAddress]];
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

- (void) onAlbumQueueRequest:(NSNotification *)notification {
    RoAlbum     *album = [notification object];
    
    [self.mQueueClient enqueueAlbum:album.DbId];
}

- (void) onTrackQueueRequest:(NSNotification *)notification {
    RoTrack     *track = [notification object];
    
    [self.mQueueClient enqueueTrack:track.DbId];
}

@end
