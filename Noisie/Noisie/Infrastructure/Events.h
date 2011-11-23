//
//  Events.h
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

extern NSString * const EventServerVersion;
extern NSString * const EventServerConnected;

extern NSString * const EventArtistListRequest;
extern NSString * const EventArtistListUpdate;
extern NSString * const EventArtistSelected;
extern NSString * const EventArtistInfoRequest;
extern NSString * const EventArtistInfoUpdate;

extern NSString * const EventAlbumListRequest;
extern NSString * const EventAlbumListUpdate;
extern NSString * const EventAlbumSelected;
extern NSString * const EventAlbumInfoRequest;
extern NSString * const EventAlbumInfoUpdate;

extern NSString * const EventTrackListRequest;
extern NSString * const EventTrackListUpdate;

extern NSString * const EventFavoritesListRequest;
extern NSString * const EventFavoritesListUpdate;

extern NSString * const EventQueueTrackRequest;
extern NSString * const EventQueueAlbumRequest;

extern NSString * const EventPlayQueueListRequest;
extern NSString * const EventPlayQueueListUpdate;

extern NSString * const EventSearchRequest;
extern NSString * const EventSearchUpdate;

extern NSString * const EventPlayQueueChanged;

extern NSString * const EventTransportCommand;
