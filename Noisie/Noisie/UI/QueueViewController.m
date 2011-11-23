//
//  QueueViewController.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "QueueViewController.h"
#import "PlayQueueListCell.h"
#import "RoPlayQueueTrack.h"
#import "Events.h"
#import "TransportCommands.h"

@interface QueueViewController ()

@property (retain, nonatomic)   NSMutableArray *mPlayQueueList;

- (void) onServerConnected:(NSNotification *)notification;
- (void) onPlayQueueList:(NSNotification *)notification;
- (void) onPlayQueueChanged:(NSNotification *) notification;
- (void) requestCommand:(int) command;

@end

@implementation QueueViewController

@synthesize uiQueueList;
@synthesize uiListCell;
@synthesize uiPlayButton;
@synthesize uiPauseButton;
@synthesize uiStopButton;
@synthesize uiPlayPreviousButton;
@synthesize uiPlayNextButton;
@synthesize uiRepeatButton;
@synthesize mPlayQueueList;

- (id)initForTabController {
    self = [super initWithNibName:nil bundle:nil];
    if (self) {
        self.tabBarItem = [[[UITabBarItem alloc] initWithTitle:@"Now Playing" image:nil tag:2] autorelease];
    }
    return self;
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.uiListCell = nil;
    self.uiQueueList = nil;

    [uiPlayButton release];
    [uiPauseButton release];
    [uiStopButton release];
    [uiPlayPreviousButton release];
    [uiPlayNextButton release];
    [uiRepeatButton release];
    [super dealloc];
}

- (void) onServerConnected:(NSNotification *)notification {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventPlayQueueListRequest object:nil];
}

- (void) onPlayQueueList:(NSNotification *)notification {
    NSArray  *trackList = [notification object];
    
    [self.mPlayQueueList removeAllObjects];
    [self.mPlayQueueList addObjectsFromArray:trackList];
    
    [self.uiQueueList reloadData];
}

- (void) onPlayQueueChanged:(NSNotification *)notification {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventPlayQueueListRequest object:nil];
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    // Return the number of rows in the section.
    return([self.mPlayQueueList count]);
}


- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *cellIdentifier = @"PlayQueueListCell";
    
    PlayQueueListCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    if (cell == nil) {
        [[NSBundle mainBundle] loadNibNamed:cellIdentifier owner:self options:nil];
        
        cell = self.uiListCell;
        self.uiListCell = nil;
    }
    
    // Configure the cell...
    [cell setTrack:[self.mPlayQueueList objectAtIndex:[indexPath row]]];
    
    return cell;
}

#pragma mark - Command handlers

- (IBAction)cmdPlay:(id)sender {
    [self requestCommand:eTransportCommand_Play];
}

- (IBAction)cmdPause:(id)sender {
    [self requestCommand:eTransportCommand_Pause];
}

- (IBAction)cmdStop:(id)sender {
    [self requestCommand:eTransportCommand_Stop];
}

- (IBAction)cmdPlayPrevious:(id)sender {
    [self requestCommand:eTransportCommand_PlayPrevious];
}

- (IBAction)cmdPlayNext:(id)sender {
    [self requestCommand:eTransportCommand_PlayNext];
}

- (IBAction)cmdRepeat:(id)sender {
}

- (void) requestCommand:(int)command {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventTransportCommand object:[NSNumber numberWithInt:command]];
}

#pragma mark - View lifecycle

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

- (void)viewDidLoad {
    [super viewDidLoad];

    self.navigationItem.title = @"Now Playing";
    self.mPlayQueueList = [[[NSMutableArray alloc] init] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onPlayQueueList:) name:EventPlayQueueListUpdate object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onPlayQueueChanged:) name:EventPlayQueueChanged object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onServerConnected:) name:EventServerConnected object:nil];
    [[NSNotificationCenter defaultCenter] postNotificationName:EventPlayQueueListRequest object:nil];
}

- (void)viewDidUnload {
    [self setUiQueueList:nil];
    [self setUiPlayButton:nil];
    [self setUiPauseButton:nil];
    [self setUiStopButton:nil];
    [self setUiPlayPreviousButton:nil];
    [self setUiPlayNextButton:nil];
    [self setUiRepeatButton:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
