//
//  ServerVersion.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ServerVersion.h"

@implementation ServerVersion

@synthesize Major;
@synthesize Minor;
@synthesize Build;
@synthesize Revision;

- (void) dealloc {
    self.Major = nil;
    self.Minor = nil;
    self.Build = nil;
    self.Revision = nil;
    
    [super dealloc];
}

@end
