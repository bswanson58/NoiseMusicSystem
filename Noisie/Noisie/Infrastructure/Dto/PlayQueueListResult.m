//
//  PlayQueueListResult.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "PlayQueueListResult.h"

@implementation PlayQueueListResult

@synthesize Tracks;

- (void) dealloc {
    self.Tracks = nil;
    
    [super dealloc];
}

@end
