//
//  RemoteDataClient.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RemoteDataClient.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "ArtistListResult.h"
#import "AlbumListResult.h"
#import "Events.h"

@interface RemoteDataClient ()

@property (nonatomic, retain)   RKObjectManager *mClient;

- (void) initObjectMappings;

@end

@implementation RemoteDataClient

@synthesize mClient;

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mClient = nil;
    
    [super dealloc];
}

- (void) initializeClient:(NSString *)serverAddress {
    self.mClient = [[RKObjectManager alloc] initWithBaseURL:serverAddress];
    [self.mClient setSerializationMIMEType:RKMIMETypeJSON];
    
    [self initObjectMappings];
}

- (void) requestArtistList {
    [self.mClient loadObjectsAtResourcePath:@"artists" 
                              objectMapping:[self.mClient.mappingProvider objectMappingForClass:[ArtistListResult class]]
                                   delegate:self];
}

- (void) requestAlbumList:(long)forArtist {
    
}

- (void)objectLoader:(RKObjectLoader*)objectLoader didLoadObjects:(NSArray*)objects {  
    ArtistListResult    *result = [objects objectAtIndex:0];
    
    if([result.Success isEqualToString:@"true"]) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistListUpdate object:result];
    }
}  
   
- (void)objectLoader:(RKObjectLoader*)objectLoader didFailWithError:(NSError *)error {
    NSLog( @"RemoteDataClient:didFailWithError - %@", error );
}

- (void) initObjectMappings {
    RKObjectMapping *mapping = [RKObjectMapping mappingForClass:[RoArtist class]];
    [mapping mapAttributes:@"DbId", @"Name", @"Website", @"AlbumCount", @"Rating", @"Genre", @"IsFavorite", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[ArtistListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", nil];
    [mapping mapRelationship:@"Artists" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoArtist class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[RoAlbum class]];
    [mapping mapAttributes:@"Name", @"TrackCount", @"Rating", @"PublishedYear", @"Genre", @"IsFavorite", nil];
    [self.mClient.mappingProvider addObjectMapping:mapping];
    
    mapping = [RKObjectMapping mappingForClass:[AlbumListResult class]];
    [mapping mapAttributes:@"Success", @"ErrorMessage", @"ArtistId", nil];
    [mapping mapRelationship:@"Albums" withMapping:[self.mClient.mappingProvider objectMappingForClass:[RoAlbum class]]];
    [self.mClient.mappingProvider addObjectMapping:mapping];
}

@end
