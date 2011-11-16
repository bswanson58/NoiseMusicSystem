//
//  AlbumViewController.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>

@class RoAlbum;

@interface AlbumViewController : UIViewController <UITableViewDelegate, UITableViewDataSource>

@property (retain, nonatomic) IBOutlet UITableView *uiTrackList;

- (void) displayAlbum:(RoAlbum *) album;

@end
