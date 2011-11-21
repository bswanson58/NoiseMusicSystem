//
//  CallbackHttpServer.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "HTTPServer.h"
#import "UriDispatcher.h"

@interface CallbackHttpServer : HTTPServer

@property (nonatomic, retain)   UriDispatcher   *UriDispatcher;

@end
