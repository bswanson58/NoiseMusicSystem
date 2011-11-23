//
//  Events.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "Events.h"

NSString * const    EventServerVersion              = @"event:ServerVersion";
NSString * const    EventServerConnected            = @"event:ServerConnected";

NSString * const    EventArtistListRequest          = @"event:ArtistListRequest";
NSString * const    EventArtistListUpdate           = @"event:ArtistListUpdate";
NSString * const    EventArtistSelected             = @"event:ArtistSelected";
NSString * const    EventArtistInfoRequest          = @"event:ArtistInfoRequest";
NSString * const    EventArtistInfoUpdate           = @"event:ArtistInfoUpdate";

NSString * const    EventAlbumListRequest           = @"event:AlbumListRequest";
NSString * const    EventAlbumListUpdate            = @"event:AlbumListUpdate";
NSString * const    EventAlbumSelected              = @"event:AlbumSelected";
NSString * const    EventAlbumInfoRequest           = @"event:AlbumInfoRequest";
NSString * const    EventAlbumInfoUpdate            = @"event:AlbumInfoUpdate";

NSString * const    EventTrackListRequest           = @"event:TrackListRequest";
NSString * const    EventTrackListUpdate            = @"event:TrackListUpdate";

NSString * const    EventFavoritesListRequest       = @"event:FavoritesListRequest";
NSString * const    EventFavoritesListUpdate        = @"event:FavoritesListUpdate";

NSString * const    EventQueueAlbumRequest          = @"event:QueueAlbumRequest";
NSString * const    EventQueueTrackRequest          = @"event:QueueTrackRequest";

NSString * const    EventPlayQueueListRequest       = @"eventPlayQueueListRequest";
NSString * const    EventPlayQueueListUpdate        = @"eventPlayQueueListUpdate";

NSString * const    EventSearchRequest              = @"event:SearchRequest";
NSString * const    EventSearchUpdate               = @"event:SearchUpdate";
NSString * const    EventSearchFocus                = @"event:SearchFocus";

NSString * const    EventPlayQueueChanged           = @"event:PlayQueueChanged";

NSString * const    EventTransportCommand           = @"event:TransportCommand";
