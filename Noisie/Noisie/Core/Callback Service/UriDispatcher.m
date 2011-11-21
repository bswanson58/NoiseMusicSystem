//
//  UriDispatcher.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "UriDispatcher.h"

@interface UriDispatcher ()

@property (nonatomic, retain)   NSMutableArray  *mSchemeMap;
@end

@implementation UriDispatcher

@synthesize mSchemeMap;

-(id)init {
	if ([super init]!=nil) {
        self.mSchemeMap = [[[NSMutableArray alloc] init] autorelease];
	}
    
	return self;
}

- (void) dealloc {
    self.mSchemeMap = nil;
    
    [super dealloc];
}

-(void)registerScheme:(NSString *)regex 
		 toController:(id)controller 
		  andSelector:(NSString *)selector {
	[self.mSchemeMap addObject:[NSArray arrayWithObjects:regex,controller,selector,nil]];
}

-(id)dispatch:(NSString *)relativeURL {
	id controller =nil;
	SEL selector;
	
	for( NSArray *a in self.mSchemeMap ) {
		NSString *scheme = [a objectAtIndex:0];
        
		if([[relativeURL lowercaseString] hasPrefix:[scheme lowercaseString]]) {
			controller = [a objectAtIndex:1];
			selector =	 NSSelectorFromString([a objectAtIndex:2]);

			break;
		}
		
	}
	
    if( controller != nil ) {
        if([controller respondsToSelector:selector]) {
            return [controller performSelector:selector withObject:relativeURL];
        }
	}
    
	return nil;
}


@end
