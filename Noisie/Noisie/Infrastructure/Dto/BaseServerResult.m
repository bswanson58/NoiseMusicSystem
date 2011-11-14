//
//  BaseServerResult.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"

@implementation BaseServerResult

@synthesize Success;
@synthesize ErrorMessage;

- (void) dealloc {
    self.Success = nil;
    self.ErrorMessage = nil;
    
    [super dealloc];
}

@end
