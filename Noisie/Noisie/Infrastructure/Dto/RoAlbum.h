//
//  RoAlbum.h
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "RoBase.h"

@interface RoAlbum : RoBase {
    NSString    *Name;
}

@property (nonatomic, copy)     NSString    *Name;
@property (nonatomic, assign)   int         TrackCount;
@property (nonatomic, assign)   int         Rating;
@property (nonatomic, assign)   int         PublishedYear;
@property (nonatomic, assign)   long        Genre;
@property (nonatomic, assign)   NSString    *IsFavorite;

@end
