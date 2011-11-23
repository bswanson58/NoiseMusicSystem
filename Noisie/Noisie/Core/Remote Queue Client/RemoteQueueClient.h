//
//  RemoteQueueClient.h
//  Noisie
//
//  Created by William Swanson on 11/16/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <RestKit/RestKit.h>

@interface RemoteQueueClient : NSObject <RKObjectLoaderDelegate>

- (void) initializeClient:(NSString *) serverAddress;

- (void) enqueueTrack:(NSNumber *) trackId;
- (void) enqueueAlbum:(NSNumber *) albumId;

- (void) requestPlayQueueList;

- (void) transportCommand:(NSNumber *) command;

@end
