//
//  AlbumInfoResult.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AlbumInfoResult.h"

@implementation AlbumInfoResult

@synthesize AlbumInfo;

- (void) dealloc {
    self.AlbumInfo = nil;
    
    [super dealloc];
}

@end
