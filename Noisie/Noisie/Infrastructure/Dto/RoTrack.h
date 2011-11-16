//
//  RoTrack.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "RoBase.h"

@interface RoTrack : RoBase {
    NSString    *Name;
}

@property (nonatomic, copy)     NSString        *Name;
@property (nonatomic, retain)   NSNumber        *ArtistId;
@property (nonatomic, retain)   NSNumber        *AlbumId;
@property (nonatomic, assign)   unsigned long    DurationMilliseconds;
@property (nonatomic, assign)   int              Rating;
@property (nonatomic, assign)   unsigned int     TrackNumber;
@property (nonatomic, copy)     NSString        *VolumeName;
@property (nonatomic, copy)     NSString        *IsFavorite;

@end
