//
//  FavoritesViewController.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "FavoritesViewController.h"
#import "FavoriteListResult.h"
#import "RoFavorite.h"
#import "Events.h"

@interface FavoritesViewController ()

@property (retain, nonatomic)   NSMutableArray     *mFavorites;

- (void) onFavoritesList:(NSNotification *)notification;

@end

@implementation FavoritesViewController
@synthesize uiFavoritesList;
@synthesize uiFavoriteCell;

@synthesize mFavorites;

- (id)initForTabController {
    self = [super initWithNibName:nil bundle:nil];
    if (self) {
        self.tabBarItem = [[[UITabBarItem alloc] initWithTitle:@"Favorites" image:nil tag:3] autorelease];
    }
    return self;
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mFavorites = nil;
    self.uiFavoritesList = nil;

    [uiFavoriteCell release];
    [super dealloc];
}

- (void) onFavoritesList:(NSNotification *)notification {
    FavoriteListResult  *result = [notification object];
    
    [self.mFavorites removeAllObjects];
    [self.mFavorites addObjectsFromArray:result.Favorites];
    
    [self.uiFavoritesList reloadData];
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mFavorites count]);
}


- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"FavoriteListCell";
    
    FavoriteListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.uiFavoriteCell;
        self.uiFavoriteCell = nil;
    }
    
    // Configure the cell...
    RoFavorite  *favorite = [self.mFavorites objectAtIndex:[indexPath row]];
    
    cell.Favorite = favorite;
    [cell.uiFavoriteName setText:[favorite formattedName]];
    
    return cell;
}

#pragma mark - View lifecycle

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)viewDidLoad {
    [super viewDidLoad];

    self.mFavorites = [[[NSMutableArray alloc] init] autorelease];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventFavoritesListRequest object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onFavoritesList:) name:EventFavoritesListUpdate object:nil];
}

- (void)viewDidUnload {
    [self setUiFavoritesList:nil];
    [self setUiFavoriteCell:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
