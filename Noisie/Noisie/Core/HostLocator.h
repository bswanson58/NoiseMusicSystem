//
//  HostLocator.h
//  Noisie
//
//  Created by William Swanson on 11/20/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface HostLocator : NSObject

- (BOOL)searchForServicesOfType:(NSString *)type inDomain:(NSString *)domain;

@end
