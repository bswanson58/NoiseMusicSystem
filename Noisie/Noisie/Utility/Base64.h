//
//  Base64.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//
// from the QSUtilites library on github: https://github.com/mikeho/QSUtilities

#import <Foundation/Foundation.h>

@interface Base64 : NSObject
+ (NSString *)encodeBase64WithString:(NSString *)strData;
+ (NSString *)encodeBase64WithData:(NSData *)objData;

+ (NSData *)decodeBase64WithString:(NSString *)strBase64;
@end
