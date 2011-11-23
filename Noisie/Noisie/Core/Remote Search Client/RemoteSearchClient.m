//
//  RemoteSearchClient.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteSearchClient.h"
#import "RoSearchResultItem.h"
#import "SearchResult.h"
#import "SearchResultDelegate.h"
#import "Events.h"

@interface RemoteSearchClient ()

@property (nonatomic, retain)   RKObjectManager         *mClient;
@property (nonatomic, retain)   SearchResultDelegate    *mSearchResultDelegate;

- (void) initObjectMappings;

- (void) onSearchResult:(SearchResult *) result;

@end

@implementation RemoteSearchClient

@synthesize mClient;
@synthesize mSearchResultDelegate;

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[[RKObjectManager alloc] initWithBaseURL:serverAddress] autorelease];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];
}

- (void) dealloc {
    self.mClient = nil;
    self.mSearchResultDelegate = nil;
    
    [super dealloc];
    
}

- (void) requestSearch:(NSString *)searchText {
    NSMutableString *path = [NSMutableString stringWithString:@"search"];
    NSDictionary    *params = [[[NSDictionary alloc] initWithObjectsAndKeys:searchText, @"text", nil] autorelease];
    NSString        *url = [path appendQueryParams:params];
    
    [self.mClient loadObjectsAtResourcePath:url
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[SearchResult class]]
                                   delegate:self.mSearchResultDelegate];
}

- (void) onSearchResult:(SearchResult *) result {
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventSearchUpdate object:result.Items];
    }
}

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {
    NSLog( @"%@", @"RemoteSearchClient:didLoadObjects called!" );
}  

- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteSearchClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[RoSearchResultItem class]];
    [mapping mapAttributes:@"TrackId", @"TrackName", @"AlbumId", @"AlbumName", @"ArtistId", @"ArtistName", @"CanPlay", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[SearchResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Items" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoSearchResultItem class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];

    self.mSearchResultDelegate = [[[SearchResultDelegate alloc] initWithSearchResultBlock:^(SearchResult *result) { [self onSearchResult:result]; } ] autorelease];
}

@end
