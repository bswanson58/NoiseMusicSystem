//
//  UriDispatcher.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface UriDispatcher : NSObject

-(void)registerScheme:(NSString *)scheme
		 toController:(id)controller 
		  andSelector:(NSString *)selector;

-(id)dispatch:(NSString *)relativeURL;

@end
