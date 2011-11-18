//
//  ArtistInfoResult.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistInfoResult.h"

@implementation ArtistInfoResult

@synthesize ArtistInfo;

- (void) dealloc {
    self.ArtistInfo = nil;
    
    [super dealloc];
}

@end
