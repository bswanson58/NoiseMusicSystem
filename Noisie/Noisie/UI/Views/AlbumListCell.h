//
//  AlbumListCell.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoAlbum.h"

@interface AlbumListCell : UITableViewCell

@property (retain, nonatomic) IBOutlet UILabel *uiAlbumName;
@property (retain, nonatomic) IBOutlet UILabel *uiTrackCount;
@property (retain, nonatomic) IBOutlet UILabel *uiPublishedYear;

- (IBAction)cmdPlay:(id)sender;

- (void) setAlbum:(RoAlbum *) album;

@end
