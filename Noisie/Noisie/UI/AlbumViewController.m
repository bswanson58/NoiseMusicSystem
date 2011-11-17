//
//  AlbumViewController.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AlbumViewController.h"
#import "TrackListResult.h"
#import "RoAlbum.h"
#import "RoTrack.h"
#import "Events.h"

@interface AlbumViewController ()

@property (nonatomic, retain)   RoAlbum         *mAlbum;
@property (nonatomic, retain)   NSMutableArray  *mTrackList;

- (void) onTrackList:(NSNotification *) notification;

@end

@implementation AlbumViewController

@synthesize uiTrackList;
@synthesize uiTrackCell;
@synthesize mAlbum;
@synthesize mTrackList;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mAlbum = nil;
    self.mTrackList = nil;
    self.uiTrackList = nil;
    
    [uiTrackCell release];
    [super dealloc];
}

- (void) displayAlbum:(RoAlbum *) album {
    self.mAlbum = album;
    [self setTitle:[NSString stringWithFormat:@"Album - %@", self.mAlbum.Name]];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventTrackListRequest object:self.mAlbum];
}

- (void) onTrackList:(NSNotification *)notification {
    TrackListResult *result = [notification object];
    
    [self.mTrackList removeAllObjects];
    [self.mTrackList addObjectsFromArray:result.Tracks];
    
    NSSortDescriptor *trackDescriptor = [[[NSSortDescriptor alloc] initWithKey:@"TrackNumber" ascending:YES] autorelease];
    NSSortDescriptor *volumeDescriptor = [[[NSSortDescriptor alloc] initWithKey:@"VolumeName" ascending:YES] autorelease];
    [self.mTrackList sortUsingDescriptors:[NSArray arrayWithObjects:volumeDescriptor, trackDescriptor, nil]];
    
    [self.uiTrackList reloadData];
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mTrackList count]);
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"TrackListCell";
    
    TrackListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.uiTrackCell;
        self.uiTrackCell = nil;
    }
    
    // Configure the cell...
    RoTrack    *track = [self.mTrackList objectAtIndex:[indexPath row]];
    
    cell.Track = track;
    [cell.uiTrackName setText:track.Name];
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
}

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.mTrackList = [[[NSMutableArray alloc] init] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onTrackList:) name:EventTrackListUpdate object:nil];
}

- (void)viewDidUnload {
    [self setUiTrackList:nil];
    [self setUiTrackCell:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
