//
//  RoArtist.h
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoBase.h"

@interface RoArtist : RoBase {
    NSString    *Name;
}

@property (nonatomic, copy)     NSString        *Name;
@property (nonatomic, assign)   int              AlbumCount;
@property (nonatomic, assign)   int              Rating;
@property (nonatomic, retain)   NSString        *Genre;
@property (nonatomic, copy)     NSString        *IsFavorite;

@end
