//
//  RoAlbumInfo.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoAlbumInfo.h"

@implementation RoAlbumInfo

@synthesize AlbumId;
@synthesize AlbumCover;

- (void) dealloc {
    self.AlbumId = nil;
    self.AlbumCover = nil;
    
    [super dealloc];
}

@end
