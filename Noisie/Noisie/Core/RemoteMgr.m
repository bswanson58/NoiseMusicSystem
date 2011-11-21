//
//  RemoteMgr.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteMgr.h"
#import "RemoteNoiseClient.h"
#import "RemoteDataClient.h"
#import "RemoteQueueClient.h"
#import "RemoteSearchClient.h"
#import "CallbackServer.h"
#import "HostLocator.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "RoTrack.h"
#import "Events.h"

@interface RemoteMgr ()

@property (nonatomic, retain)   RemoteNoiseClient   *mNoiseClient;
@property (nonatomic, retain)   RemoteDataClient    *mDataClient;
@property (nonatomic, retain)   RemoteQueueClient   *mQueueClient;
@property (nonatomic, retain)   RemoteSearchClient  *mSearchClient;
@property (nonatomic, retain)   CallbackServer      *mCallbackServer;
@property (nonatomic, retain)   HostLocator         *mHostLocator;

- (void) onArtistListRequest:(NSNotification *) notification;
- (void) onArtistInfoRequest:(NSNotification *) notification;
- (void) onAlbumListRequest:(NSNotification *) notification;
- (void) onAlbumInfoRequest:(NSNotification *) notification;
- (void) onTrackListRequest:(NSNotification *) notification;
- (void) onAlbumQueueRequest:(NSNotification *)notification;
- (void) onTrackQueueRequest:(NSNotification *)notification;
- (void) onFavoriteListRequest:(NSNotification *)notification;
- (void) onPlayQueueListRequest:(NSNotification *)notification;
- (void) onSearchRequest:(NSNotification *)notification;

@end

@implementation RemoteMgr

@synthesize mNoiseClient;
@synthesize mDataClient;
@synthesize mQueueClient;
@synthesize mSearchClient;
@synthesize mCallbackServer;
@synthesize mHostLocator;

- (id) init {
    self = [super init];
    if( self ) {
        self.mNoiseClient = [[[RemoteNoiseClient alloc] init] autorelease];
        self.mDataClient = [[[RemoteDataClient alloc] init] autorelease];
        self.mQueueClient = [[[RemoteQueueClient alloc] init] autorelease];
        self.mSearchClient = [[[RemoteSearchClient alloc] init] autorelease];
        self.mCallbackServer = [[[CallbackServer alloc] init] autorelease];
        self.mHostLocator = [[[HostLocator alloc] init] autorelease];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistListRequest:) name:EventArtistListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistInfoRequest:) name:EventArtistInfoRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumListRequest:) name:EventAlbumListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumInfoRequest:) name:EventAlbumInfoRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackListRequest:) name:EventTrackListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumQueueRequest:) name:EventQueueAlbumRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackQueueRequest:) name:EventQueueTrackRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onFavoriteListRequest:) name:EventFavoritesListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onPlayQueueListRequest:) name:EventPlayQueueListRequest object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onSearchRequest:) name:EventSearchRequest object:nil];
        
        [self.mHostLocator searchForServicesOfType:@"_Noise._tcp." inDomain:@"local."];
    }
    
    return( self );
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mNoiseClient = nil;
    self.mDataClient = nil;
    self.mQueueClient = nil;
    self.mSearchClient = nil;
    self.mCallbackServer = nil;
    self.mHostLocator = nil;
    
    [super dealloc];
}

- (void) initialize:(NSString *)serverAddress {
    [self.mNoiseClient initializeClient:[NSString stringWithFormat:@"%@/Noise", serverAddress]];
    [self.mDataClient initializeClient:[NSString stringWithFormat:@"%@/Noise/Data", serverAddress]];
    [self.mQueueClient initializeClient:[NSString stringWithFormat:@"%@/Noise/Queue", serverAddress]];
    [self.mSearchClient initializeClient:[NSString stringWithFormat:@"%@/Noise/Search", serverAddress]];
    [self.mCallbackServer initializeServer];
    
    [self.mNoiseClient requestServerVersion];
}

- (void) onArtistListRequest:(NSNotification *)notification {
    [self.mDataClient requestArtistList];
}

- (void) onArtistInfoRequest:(NSNotification *)notification {
    NSNumber    *artistId = [notification object];
    
    [self.mDataClient requestArtistInfo:artistId];
}

- (void) onAlbumListRequest:(NSNotification *)notification {
    RoArtist    *forArtist = [notification object];
    
    [self.mDataClient requestAlbumList:forArtist.DbId];
}

- (void) onAlbumInfoRequest:(NSNotification *)notification {
    NSNumber    *albumId = [notification object];
    
    [self.mDataClient requestAlbumInfo:albumId];
}

- (void) onTrackListRequest:(NSNotification *)notification {
    RoAlbum     *forAlbum = [notification object];
    
    [self.mDataClient requestTrackList:forAlbum.DbId];
}

- (void) onFavoriteListRequest:(NSNotification *)notification {
    [self.mDataClient requestFavoriteList];
}

- (void) onAlbumQueueRequest:(NSNotification *)notification {
    if([[notification object] isKindOfClass:[RoAlbum class]]) {
        RoAlbum     *album = [notification object];
    
        [self.mQueueClient enqueueAlbum:album.DbId];
    }
    else if([[notification object] isKindOfClass:[NSNumber class]]) {
        NSNumber    *albumId = [notification object];
        
        [self.mQueueClient enqueueAlbum:albumId];
    }
}

- (void) onTrackQueueRequest:(NSNotification *)notification {
    if([[notification object] isKindOfClass:[RoTrack class]]) {
        RoTrack     *track = [notification object];
    
        [self.mQueueClient enqueueTrack:track.DbId];
    }
    else if([[notification object]isKindOfClass:[NSNumber class]]) {
        NSNumber    *trackId = [notification object];
        
        [self.mQueueClient enqueueTrack:trackId];
    }
}

- (void) onPlayQueueListRequest:(NSNotification *)notification {
    [self.mQueueClient requestPlayQueueList];
}

- (void) onSearchRequest:(NSNotification *)notification {
    NSString    *searchText = [notification object];
    
    [self.mSearchClient requestSearch:searchText];
}

@end
