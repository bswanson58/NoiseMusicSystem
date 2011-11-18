//
//  SearchResult.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "SearchResult.h"

@implementation SearchResult

@synthesize Items;

- (void) dealloc {
    self.Items = nil;
    
    [super dealloc];
}

@end
