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

@property (copy, nonatomic)     NSString    *Name;
@property (copy, nonatomic)     NSString    *Website;
@property (retain, nonatomic)   NSNumber    *AlbumCount;
@property (retain, nonatomic)   NSNumber    *Rating;
@property (retain, nonatomic)   NSNumber    *Genre;
@property (retain, nonatomic)   NSString    *IsFavorite;

@end
