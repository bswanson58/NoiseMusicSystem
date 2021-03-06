//
//  RemoteDataClient.h
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <RestKit/RestKit.h>

@interface RemoteDataClient : NSObject <RKObjectLoaderDelegate>

- (void) initializeClient:(NSString *) serverAddress;

- (void) requestArtistList;
- (void) requestArtistInfo:(NSNumber *) artistId;
- (void) requestAlbumList:(NSNumber *) forArtist;
- (void) requestAlbumInfo:(NSNumber *) albumId;
- (void) requestTrackList:(NSNumber *) forAlbum;
- (void) requestFavoriteList;

@end
