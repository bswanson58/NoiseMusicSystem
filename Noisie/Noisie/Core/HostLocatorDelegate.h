//
//  HostLocatorDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol HostLocatorDelegate <NSObject>

- (void) onHostLocated:(NSString *) address 
                  port:(NSInteger)port
            serverName:(NSString *) servername;

@end
