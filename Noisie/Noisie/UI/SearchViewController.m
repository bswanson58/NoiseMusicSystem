//
//  SearchViewController.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "SearchViewController.h"
#import "RoSearchResultItem.h"
#import "Events.h"

@interface SearchViewController ()

@property (retain, nonatomic)   NSMutableArray  *mResultsList;

- (void) onSearchUpdate:(NSNotification *)notification;

@end

@implementation SearchViewController
@synthesize uiSearchText;
@synthesize uiResultsList;
@synthesize uiSearchCell;

@synthesize mResultsList;

- (id)initForTabController {
    self = [super initWithNibName:nil bundle:nil];
    if (self) {
        self.tabBarItem = [[[UITabBarItem alloc] initWithTitle:@"Search" image:nil tag:4] autorelease];
        self.mResultsList = [[[NSMutableArray alloc] init] autorelease];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onSearchUpdate:) name:EventSearchUpdate object:nil];
    }
    return self;
}

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mResultsList = nil;
    
    [uiSearchText release];
    [uiResultsList release];
    [uiSearchCell release];
    [super dealloc];
}

- (void) onSearchUpdate:(NSNotification *)notification {
    NSArray *resultList = [notification object];
    
    [self.mResultsList removeAllObjects];
    [self.mResultsList addObjectsFromArray:resultList];
    
    [self.uiResultsList reloadData];
}

- (IBAction)cmdSearch:(id)sender {
    if([self.uiSearchText.text length] > 0 ) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventSearchRequest object:self.uiSearchText.text];
    }
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mResultsList count]);
}


- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"SearchListCell";
    
    SearchListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.uiSearchCell;
        self.uiSearchCell = nil;
    }
    
    // Configure the cell...
    RoSearchResultItem  *item = [self.mResultsList objectAtIndex:[indexPath row]];
    
    cell.SearchItem = item;
    [cell.uiItemTitle setText:[item formattedTitle]];
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    RoSearchResultItem    *item = [self.mResultsList objectAtIndex:[indexPath row]];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:EventSearchFocus object:item];
}

#pragma mark - View lifecycle

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
}

- (void)viewDidUnload {
    [self setUiSearchText:nil];
    [self setUiResultsList:nil];
    [self setUiSearchCell:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
