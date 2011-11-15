//
//  RoBase.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoBase.h"

@implementation RoBase

@synthesize DbId;

- (void) dealloc {
    self.DbId = nil;
    
    [super dealloc];
}

@end
