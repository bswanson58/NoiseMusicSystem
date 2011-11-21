//
//  ServerVersion.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ServerVersion : NSObject

@property (nonatomic, retain)   NSNumber    *Major;
@property (nonatomic, retain)   NSNumber    *Minor;
@property (nonatomic, retain)   NSNumber    *Build;
@property (nonatomic, retain)   NSNumber    *Revision;

@end
