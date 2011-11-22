//
//  ArtistViewController.h
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "AlbumListCell.h"

@class RoArtist;

@interface ArtistViewController : UIViewController <UITableViewDelegate, UITableViewDataSource>

@property (retain, nonatomic) IBOutlet UITableView      *uiAlbumList;
@property (retain, nonatomic) IBOutlet UIImageView      *uiArtistImage;
@property (retain, nonatomic) IBOutlet UIWebView        *uiArtistBiography;
@property (retain, nonatomic) IBOutlet  AlbumListCell   *uiAlbumCell;

- (void) displayArtist:(RoArtist *) artist;

@end
