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
#import "RoSearchResultItem.h"
#import "Events.h"

@interface ArtistListController ()

@property (nonatomic, retain)   NSMutableDictionary *mArtistSections;
@property (nonatomic, retain)   NSMutableArray      *mSectionList;

- (void) onServerConnected:(NSNotification *) notification;
- (void) onArtistListUpdate:(NSNotification *) notification;

@end

@implementation ArtistListController

@synthesize uiArtistList;
@synthesize mArtistSections;
@synthesize mSectionList;
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
    
    self.mArtistSections = nil;
    self.mSectionList = nil;
    self.uiArtistList = nil;
    self.artistListCell = nil;

    [super dealloc];
}

- (void) onServerConnected:(NSNotification *)notification {
    if( [self.mSectionList count] > 0 ) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistListRequest object:nil];
    }
}

- (void) onArtistListUpdate:(NSNotification *)notification {
    ArtistListResult    *result = [notification object];
    
    [self.mArtistSections removeAllObjects];
    [self.mSectionList removeAllObjects];
    
    for ( RoArtist *artist in result.Artists ) {
        NSString        *section = [artist.Name substringToIndex:1];
        NSMutableArray  *sectionList = [self.mArtistSections objectForKey:section];
        
        if( sectionList == nil ) {
            sectionList = [[[NSMutableArray alloc] init] autorelease];
            
            [self.mArtistSections setObject:sectionList forKey:section];
            [self.mSectionList addObject:section];
        }
        
        [sectionList addObject:artist];
    }

    NSSortDescriptor *orderDescriptor = [[[NSSortDescriptor alloc] initWithKey:@"Name" ascending:YES] autorelease];
    NSArray          *descriptorArray = [NSArray arrayWithObject:orderDescriptor];
    
    for( NSMutableArray *sectionList in self.mArtistSections.objectEnumerator ) {
        [sectionList sortUsingDescriptors:descriptorArray];
    }
    
    [self.mSectionList sortUsingComparator:^NSComparisonResult(id obj1, id obj2) { return [((NSString *)obj1) compare:((NSString *)obj2) options:NSNumericSearch]; }];

    [self.uiArtistList reloadData];
}

- (void) setSearchFocus:(RoSearchResultItem *)item {
    bool    found = false;
    
    for( NSMutableArray *sectionList in self.mArtistSections.objectEnumerator ) {
        for( RoArtist *artist in sectionList ) {
            if([artist.DbId isEqualToNumber:item.ArtistId]) {
                [[NSNotificationCenter defaultCenter] postNotificationName:EventArtistSelected object:artist];
            
                found = true;
                break;
            }
        }
        
        if( found ) {
            break;
        }
    }
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return([self.mSectionList count]);
}

- (NSArray *)sectionIndexTitlesForTableView:(UITableView *)tableView {
    return( self.mSectionList );
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    NSString        *sectionKey = [self.mSectionList objectAtIndex:section];
    NSMutableArray  *sectionList = [self.mArtistSections objectForKey:sectionKey];
    
    return([sectionList count]);
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
    NSString        *sectionKey = [self.mSectionList objectAtIndex:indexPath.section];
    NSMutableArray  *sectionList = [self.mArtistSections objectForKey:sectionKey];
    
    [cell setArtist:[sectionList objectAtIndex:[indexPath row]]];
  
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    NSString        *sectionKey = [self.mSectionList objectAtIndex:indexPath.section];
    NSMutableArray  *sectionList = [self.mArtistSections objectForKey:sectionKey];
    RoArtist        *artist = [sectionList objectAtIndex:[indexPath row]];
    
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

#pragma mark - View lifecycle

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.navigationItem.title = @"Library - Artists";
    self.mArtistSections = [[[NSMutableDictionary alloc] init] autorelease];
    self.mSectionList = [[[NSMutableArray alloc] init] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onServerConnected:) name:EventServerConnected object:nil];
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

@end
