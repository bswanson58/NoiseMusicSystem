//
//  RoFavorite.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoBase.h"

@interface RoFavorite : RoBase

@property (retain, nonatomic)   NSNumber    *ArtistId;
@property (retain, nonatomic)   NSNumber    *AlbumId;
@property (retain, nonatomic)   NSNumber    *TrackId;
@property (copy, nonatomic)     NSString    *Artist;
@property (copy, nonatomic)     NSString    *Album;
@property (copy, nonatomic)     NSString    *Track;

- (NSString *) formattedName;

@end
