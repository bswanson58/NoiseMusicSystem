//
//  AlbumViewController.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "TrackListCell.h"

@class RoAlbum;

@interface AlbumViewController : UIViewController <UITableViewDelegate, UITableViewDataSource>

@property (retain, nonatomic) IBOutlet UIImageView   *uiAlbumImage;
@property (retain, nonatomic) IBOutlet UILabel       *uiReleaseYear;
@property (retain, nonatomic) IBOutlet UITableView   *uiTrackList;
@property (retain, nonatomic) IBOutlet TrackListCell *uiTrackCell;

- (void) displayAlbum:(RoAlbum *) album;

@end
