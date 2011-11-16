//
//  MainViewController.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "MainViewController.h"
#import "ArtistListController.h"
#import "ArtistViewController.h"
#import "AlbumViewController.h"
#import "RoArtist.h"
#import "RoAlbum.h"
#import "Events.h"

@interface MainViewController ()

@property (nonatomic, retain)   UINavigationController  *mLibraryNavigationController;
@property (nonatomic, retain)   ArtistListController    *mArtistListController;
@property (nonatomic, retain)   ArtistViewController    *mArtistViewController;
@property (nonatomic, retain)   AlbumViewController     *mAlbumViewController;

- (void) onArtistSelected:(NSNotification *) notification;
- (void) onAlbumSelected:(NSNotification *) notification;

@end

@implementation MainViewController

@synthesize mLibraryNavigationController;
@synthesize mArtistListController;
@synthesize mArtistViewController;
@synthesize mAlbumViewController;

- (void) dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    
    self.mLibraryNavigationController = nil;
    self.mArtistViewController = nil;
    self.mArtistListController = nil;
    self.mAlbumViewController = nil;
    
    [super dealloc];
}

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)didReceiveMemoryWarning {
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

#pragma mark - View lifecycle

- (void)viewDidLoad {
    [super viewDidLoad];

    self.mArtistListController = [[[ArtistListController alloc] initWithNibName:nil bundle:nil] autorelease];
    self.mLibraryNavigationController = [[[UINavigationController alloc] initWithRootViewController:self.mArtistListController] autorelease];
    
    CGRect  rect = CGRectMake(0, 0, self.view.frame.size.width, self.view.frame.size.height - 100);
    [self.mLibraryNavigationController.view setFrame:rect];
    
    [self.mArtistListController setTitle:@"Library - Artists"];
    
    [self.view addSubview:self.mLibraryNavigationController.view];
    [self addChildViewController:self.mLibraryNavigationController];
    [self.mLibraryNavigationController didMoveToParentViewController:self];
    
    self.mArtistViewController = [[[ArtistViewController alloc] initWithNibName:nil bundle:nil] autorelease];
    self.mAlbumViewController = [[[AlbumViewController alloc] initWithNibName:nil bundle:nil] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onArtistSelected:) name:EventArtistSelected object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onAlbumSelected:) name:EventAlbumSelected object:nil];
}

- (void) onArtistSelected:(NSNotification *)notification {
    RoArtist    *artist = [notification object];
    
    [self.mArtistViewController displayArtist:artist];
    [self.mLibraryNavigationController pushViewController:self.mArtistViewController animated:YES];
}

- (void) onAlbumSelected:(NSNotification *)notification {
    RoAlbum     *album = [notification object];
    
    [self.mAlbumViewController displayAlbum:album];
    [self.mLibraryNavigationController pushViewController:self.mAlbumViewController animated:YES];
}

- (void)viewDidUnload {
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
	return YES;
}

@end
