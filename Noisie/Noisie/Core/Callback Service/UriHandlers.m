//
//  UriHandlers.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "UriHandlers.h"

@implementation UriHandlers

- (NSString *) onQueueEvent:(NSData *)data {
    NSLog( @"%@", @"Queue event occurred." );
    
    return( @"Queue event occurred." );
}

- (NSString *) onTransportEvent:(NSData *)data {
    NSLog( @"%@", @"Transport event occurred." );
    
    return( @"Transport event occurred." );
}

@end
