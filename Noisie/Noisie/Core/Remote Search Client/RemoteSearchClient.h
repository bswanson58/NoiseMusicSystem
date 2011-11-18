//
//  RemoteSearchClient.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <RestKit/RestKit.h>

@interface RemoteSearchClient : NSObject <RKObjectLoaderDelegate>

- (void) initializeClient:(NSString *) serverAddress;

- (void) requestSearch:(NSString *)searchText;

@end
