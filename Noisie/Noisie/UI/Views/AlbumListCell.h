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

@property (retain, nonatomic) RoAlbum  *Album;

@property (retain, nonatomic) IBOutlet UILabel *uiAlbumName;
@property (retain, nonatomic) IBOutlet UILabel *uiTrackCount;

- (IBAction)cmdPlay:(id)sender;

@end
