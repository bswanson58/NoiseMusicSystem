//
//  BaseServerResult.h
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoBase.h"

@interface BaseServerResult : NSObject

@property (nonatomic, copy) NSString    *Success;
@property (nonatomic, copy) NSString    *ErrorMessage;

@end
