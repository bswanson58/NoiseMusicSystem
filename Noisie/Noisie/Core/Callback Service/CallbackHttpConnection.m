//
//  CallbackHttpConnection.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "CallbackHttpConnection.h"
#import "CallbackHttpServer.h"
#import "HTTPDataResponse.h"

@interface CallbackHttpConnection ()

@property (nonatomic, retain)   CallbackHttpServer  *mServer;

@end

@implementation CallbackHttpConnection

@synthesize mServer;

- (id)initWithAsyncSocket:(GCDAsyncSocket *)newSocket configuration:(HTTPConfig *)aConfig {
    self = [super initWithAsyncSocket:newSocket configuration:aConfig];
    if( self ) {
        self.mServer = (CallbackHttpServer *)[aConfig server];
    }
    
    return( self );
}

- (NSObject<HTTPResponse> *)httpResponseForMethod:(NSString *)method URI:(NSString *)path {
//	TTDPRINT(@"httpResponseForURI: method:%@ path:%@", method, path);
	
	//NSData *requestData = [(NSData *)CFHTTPMessageCopySerializedMessage(request) autorelease];
	
	//NSString *requestStr = [[[NSString alloc] initWithData:requestData encoding:NSASCIIStringEncoding] autorelease];
	//TTDPRINT(@"\n=== Request ====================\n%@\n================================", requestStr);
	
    
	id object = [[self.mServer UriDispatcher] dispatch:path];
    
	if ([object isKindOfClass:[NSString class]]) {
		NSData *browseData = [object dataUsingEncoding:NSUTF8StringEncoding];
        
		return [[[HTTPDataResponse alloc] initWithData:browseData] autorelease];
	} else if ([object isKindOfClass:[NSData class]]) {
		return [[[HTTPDataResponse alloc] initWithData:object] autorelease];
	}
	
	return nil;
}

@end
