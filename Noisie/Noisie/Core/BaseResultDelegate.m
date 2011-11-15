//
//  BaseResultDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"

@implementation BaseResultDelegate

- (void)objectLoader:(RKObjectLoader *)objectLoader didLoadObjects:(NSArray *)objects {
    NSLog( @"BaseLoaderDelegate didLoadObject was called!" );
}

- (void)objectLoaderDidLoadUnexpectedResponse:(RKObjectLoader *)objectLoader {
//    ErrorInfo   *info = [[[ErrorInfo alloc] initWithMessage:@"An unexpected response was received from the server." location:@"BaseLoaderDelegate:didLoadUnexpectedResponse"] autorelease];
    
//    [[NSNotificationCenter defaultCenter] postNotificationName:EventErrorOccurred object:info];
    
    NSLog( @"%@", @"An unexpected response was received from the server." );
}

- (void)objectLoader:(RKObjectLoader *)objectLoader didFailWithError:(NSError *)error {  
//    ErrorInfo   *info = [[[ErrorInfo alloc] initWithError:error location:@"BaseLoaderDelegate:didFailWithError"] autorelease];
    
//    [[NSNotificationCenter defaultCenter] postNotificationName:EventErrorOccurred object:info];
    
    NSLog( @"BaseResultDelegate:didFailWithError: %@", error );
}  

@end
