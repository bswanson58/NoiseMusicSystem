//
//  UriHandlers.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface UriHandlers : NSObject

- (NSString *) onQueueEvent:(NSString *) uri;
- (NSString *) onTransportEvent:(NSString *) uri;

@end
