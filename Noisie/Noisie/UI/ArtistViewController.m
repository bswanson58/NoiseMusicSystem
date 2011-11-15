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
#import "RoAlbum.h"
#import "Events.h"

@interface ArtistViewController ()

@property (nonatomic, retain)   RoArtist        *mArtist;
@property (nonatomic, retain)   NSMutableArray  *mAlbumList;

- (void) onAlbumList:(NSNotification *) notification;

@end

@implementation ArtistViewController
@synthesize uiAlbumList;

@synthesize mArtist;
@synthesize mAlbumList;

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
    self.mAlbumList = nil;
    self.uiAlbumList = nil;
    
    [super dealloc];
}

- (void) displayArtist:(RoArtist *)artist {
    self.mArtist = artist;
    [self setTitle:[NSString stringWithFormat:@"Artist - %@", artist.Name]];

    [[NSNotificationCenter defaultCenter] postNotificationName:EventAlbumListRequest object:self.mArtist];
}

- (void) onAlbumList:(NSNotification *)notification {
    AlbumListResult *result = [notification object];
    
    [self.mAlbumList removeAllObjects];
    [self.mAlbumList addObjectsFromArray:result.Albums];
    
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
    static NSString *CellIdentifier = @"Cell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (cell == nil) {
        cell = [[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier] autorelease];
    }
    
    // Configure the cell...
    RoAlbum    *album = [self.mAlbumList objectAtIndex:[indexPath row]];
    
    [cell setText:album.Name];
    
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
}

- (void)viewDidUnload {
    [self setUiAlbumList:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
