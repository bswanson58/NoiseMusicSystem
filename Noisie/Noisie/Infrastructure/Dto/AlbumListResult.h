//
//  AlbumListResult.h
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "RoAlbum.h"

@interface AlbumListResult : NSObject

@property (nonatomic, assign)   long    ArtistId;
@property (nonatomic, retain)   NSArray *Albums;
@end
