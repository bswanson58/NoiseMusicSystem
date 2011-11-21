//
//  RemoteNoiseClient.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <RestKit/RestKit.h>

@interface RemoteNoiseClient : NSObject

- (void) initializeClient:(NSString *) serverAddress;

- (void) requestServerVersion;

- (void) requestEvents;
- (void) revokeEvents;

@end
