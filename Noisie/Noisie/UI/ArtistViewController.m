//
//  ArtistViewController.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistViewController.h"
#import "AlbumListResult.h"
#import "RoArtist.h"
#import "RoArtistInfo.h"
#import "RoAlbum.h"
#import "Events.h"
#import "Base64.h"

@interface ArtistViewController ()

@property (nonatomic, retain)   RoArtist        *mArtist;
@property (nonatomic, retain)   RoArtistInfo    *mArtistInfo;
@property (nonatomic, retain)   NSMutableArray  *mAlbumList;

- (void) onArtistInfo:(NSNotification *) notification;
- (void) onAlbumList:(NSNotification *) notification;

@end

@implementation ArtistViewController

@synthesize uiAlbumList;
@synthesize uiArtistImage;
@synthesize mArtist;
@synthesize mArtistInfo;
@synthesize mAlbumList;
@synthesize uiAlbumCell;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mArtist = nil;
    self.mArtistInfo = nil;
    self.mAlbumList = nil;
    self.uiAlbumList = nil;
    self.uiAlbumCell = nil;
    
    [uiArtistImage release];
    [super dealloc];
}

- (void) displayArtist:(RoArtist *)artist {
    self.mArtist = artist;
    [self setTitle:[NSString stringWithFormat:@"Artist - %@", artist.Name]];
    
    [self.mAlbumList removeAllObjects];
    [self.uiAlbumList reloadData];
    
    self.uiArtistImage.image = nil;

    [[NSNotificationCenter defaultCenter] postNotificationName:EventAlbumListRequest object:self.mArtist];
    [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistInfoRequest object:self.mArtist.DbId];
}

- (void) onArtistInfo:(NSNotification *)notification {
    RoArtistInfo    *info = [notification object];
    
    if([info.ArtistId isEqualToNumber:self.mArtist.DbId]) {
        self.mArtistInfo = info;
    
        if( self.mArtistInfo.ArtistImage != nil ) {
            self.uiArtistImage.image = [UIImage imageWithData:[Base64 decodeBase64WithString:self.mArtistInfo.ArtistImage]];
        }
    }
}

- (void) onAlbumList:(NSNotification *)notification {
    AlbumListResult *result = [notification object];
    
    [self.mAlbumList removeAllObjects];
    [self.mAlbumList addObjectsFromArray:result.Albums];
    
    NSSortDescriptor *orderDescriptor = [[[NSSortDescriptor alloc] initWithKey:@"Name" ascending:YES] autorelease];
    [self.mAlbumList sortUsingDescriptors:[NSArray arrayWithObject:orderDescriptor]];
    
    [self.uiAlbumList reloadData];
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mAlbumList count]);
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"AlbumListCell";
    
    AlbumListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.uiAlbumCell;
        self.uiAlbumCell = nil;
    }
    
    // Configure the cell...
    RoAlbum    *album = [self.mAlbumList objectAtIndex:[indexPath row]];
    
    cell.Album = album;
    [cell.uiAlbumName setText:album.Name];
    [cell.uiTrackCount setText:[NSString stringWithFormat:@"%d", album.TrackCount]];
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    RoAlbum    *album = [self.mAlbumList objectAtIndex:[indexPath row]];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventAlbumSelected object:album];
}


- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.mAlbumList = [[[NSMutableArray alloc] init] autorelease];

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumList:) name:EventAlbumListUpdate object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistInfo:) name:EventArtistInfoUpdate object:nil];
}

- (void)viewDidUnload {
    [self setUiAlbumList:nil];
    [self setUiArtistImage:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
