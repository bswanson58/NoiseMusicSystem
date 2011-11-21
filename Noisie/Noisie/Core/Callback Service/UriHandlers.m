//
//  UriHandlers.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "UriHandlers.h"
#import "Events.h"

@implementation UriHandlers

- (NSString *) onQueueEvent:(NSData *)data {
    NSLog( @"%@", @"Queue event occurred." );
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventPlayQueueChanged object:nil];
    
    return( @"" );
}

- (NSString *) onTransportEvent:(NSData *)data {
    NSLog( @"%@", @"Transport event occurred." );
    
    return( @"" );
}

@end
