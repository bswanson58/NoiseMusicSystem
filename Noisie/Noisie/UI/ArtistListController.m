//
//  ArtistListController.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistListController.h"
#import "ArtistListResult.h"
#import "RoArtist.h"
#import "Events.h"

@interface ArtistListController ()

@property (nonatomic, retain)   NSMutableArray  *mArtistList;

- (void) onArtistListUpdate:(NSNotification *) notification;

@end

@implementation ArtistListController

@synthesize uiArtistList;
@synthesize mArtistList;
@synthesize artistListCell;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mArtistList = nil;
    self.uiArtistList = nil;
    self.artistListCell = nil;

    [super dealloc];
}

- (void) onArtistListUpdate:(NSNotification *)notification {
    ArtistListResult    *result = [notification object];
    
    [self.mArtistList removeAllObjects];
    [self.mArtistList addObjectsFromArray:result.Artists];

    NSSortDescriptor *orderDescriptor = [[[NSSortDescriptor alloc] initWithKey:@"Name" ascending:YES] autorelease];
    [self.mArtistList sortUsingDescriptors:[NSArray arrayWithObject:orderDescriptor]];

    [self.uiArtistList reloadData];
}

#pragma mark - View lifecycle

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.navigationItem.title = @"Library - Artists";
    self.mArtistList = [[[NSMutableArray alloc] init] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistListUpdate:) name:EventArtistListUpdate object:nil];

    [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistListRequest object:nil];
    
    // Uncomment the following line to preserve selection between presentations.
    // self.clearsSelectionOnViewWillAppear = NO;
}

- (void)viewDidUnload {
    [self setUiArtistList:nil];
    
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];
}

- (void)viewDidAppear:(BOOL)animated {
    [super viewDidAppear:animated];
}

- (void)viewWillDisappear:(BOOL)animated {
    [super viewWillDisappear:animated];
}

- (void)viewDidDisappear:(BOOL)animated {
    [super viewDidDisappear:animated];
}

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mArtistList count]);
}


- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"ArtistListCell";
    
    ArtistListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.artistListCell;
        self.artistListCell = nil;
    }
    
    // Configure the cell...
    [cell setArtist:[self.mArtistList objectAtIndex:[indexPath row]]];
  
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    RoArtist    *artist = [self.mArtistList objectAtIndex:[indexPath row]];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistSelected object:artist];
}

/*
 // Override to support rearranging the table view.
 - (void)tableView:(UITableView *)tableView moveRowAtIndexPath:(NSIndexPath *)fromIndexPath toIndexPath:(NSIndexPath *)toIndexPath
 {
 }
 */

/*
 // Override to support conditional rearranging of the table view.
 - (BOOL)tableView:(UITableView *)tableView canMoveRowAtIndexPath:(NSIndexPath *)indexPath
 {
 // Return NO if you do not want the item to be re-orderable.
 return YES;
 }
 */

@end
