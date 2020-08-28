using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Subjects;
using LightPipe.Dto;
using LightPipe.Interfaces;
using LightPipe.Utility;
using MoreLinq;

namespace LightPipe.Models {
    internal class ProcessContext {
        public  int         ImageWidth { get; }
        public  int         ImageHeight { get; }

        public ProcessContext( Bitmap image ) {
            ImageHeight = image.Height;
            ImageWidth = image.Width;
        }
    }

    public class ImageProcessor : IImageProcessor {
        private const int       cZoneSummaryLength = 10;
        private const int       cWhitenessLimit = 230;
        private const int       cBlacknessLimit = 25;
        private const int       cMinimumBinCount = 4;
        private const byte      cBinMask = 0b11100000;
        private const int       cSourceSampleFrequency = 8;

        private readonly List<ZoneDefinition>   mZones;
        private readonly Dictionary<string, List<ZoneSummary>>  mZoneSummaries;
        private readonly Subject<ZoneSummary>   mZoneUpdated;

        public  IObservable<ZoneSummary>        ZoneUpdate => mZoneUpdated;

        public  long                            ElapsedTime { get; private set; }

        public ImageProcessor() {
//            mZones = new List<ZoneDefinition> {
//                new ZoneDefinition( "Top Left", new RectangleF( 20, 20, 20, 20 )),
//                new ZoneDefinition( "Bottom Left", new RectangleF( 20, 65, 20, 20 )),
//                new ZoneDefinition( "Top Right", new RectangleF( 70, 20, 20, 20 )),
//                new ZoneDefinition( "Bottom Right", new RectangleF( 70, 70, 20, 20 ))
//            };
//            mZones = new List<ZoneDefinition>{ new ZoneDefinition( "Center", new RectangleF( 20, 20, 60, 60 )) };
//            mZones = new List<ZoneDefinition> {
//                new ZoneDefinition( "Corner", new RectangleF( 1, 1, 20, 20 )),
//                new ZoneDefinition( "Middle", new RectangleF( 40, 40, 20, 20 )),
//                new ZoneDefinition( "Right", new RectangleF( 75, 25, 20, 50 ))
//            };
//            mZones = new List<ZoneDefinition> {
//                new ZoneDefinition( "Left", new RectangleF( 5, 20, 20, 50 )),
//                new ZoneDefinition( "Center", new RectangleF( 35, 35, 30, 30 )),
//                new ZoneDefinition( "Right", new RectangleF( 75, 5, 20, 50 )),
//                new ZoneDefinition( "Bottom", new RectangleF( 20, 80, 60, 15 ))
//            };

            mZoneSummaries = new Dictionary<string, List<ZoneSummary>>();

            mZoneUpdated = new Subject<ZoneSummary>();
        }

        public void ProcessImage( Bitmap image ) {
            var stopWatch = Stopwatch.StartNew();
            var context = new ProcessContext( image );
            var zoneData = new Dictionary<string, IEnumerable<PixelData>>();

            mZones.ForEach( zone => {
                var zoneArea = ZoneToContext( zone, context );

                using( var bitmapAccess = new DirectAccessBitmap( image, zoneArea.X, zoneArea.Y, zoneArea.Width, zoneArea.Height )) {
                    zoneData.Add( zone.ZoneName, bitmapAccess.SamplePixels( cSourceSampleFrequency ));
                }
            });

            zoneData.ForEach( zonePair => {
                var zoneList = 
                    from pixel in zonePair.Value
                    where IsUsableColor( pixel ) 
                    select AssignZoneAndBin( pixel, zonePair.Key );

                UpdateZoneQueue( ProcessZone( zonePair.Key, zoneList ));
            });

            ElapsedTime = stopWatch.ElapsedMilliseconds;
            stopWatch.Stop();
        }

        private ZoneSummary ProcessZone( string zoneId, IEnumerable<PixelData> zoneData ) {
            var colorBins = 
                from pixel in zoneData.AsParallel()
                group pixel by pixel.ColorBin into g
                where g.Count() > cMinimumBinCount
                orderby g.Count() descending
                select new ColorBin( g.Key, g.Count());

            return new ZoneSummary( zoneId, colorBins );
        }

        private void UpdateZoneQueue( ZoneSummary summary ) {
            if( summary.Colors.Any()) {
                if( mZoneSummaries.ContainsKey( summary.ZoneId )) {
                    var summaryList = mZoneSummaries[summary.ZoneId];

                    summaryList.Add( summary );
                    while( summaryList.Count > cZoneSummaryLength ) {
                        summaryList.RemoveAt( 0 );
                    }
                }
                else {
                    mZoneSummaries.Add( summary.ZoneId, new List<ZoneSummary> { summary });
                }

                mZoneUpdated.OnNext( summary );
            }
        }

        private bool IsUsableColor( PixelData colorData ) {
            return (( colorData.Red > cBlacknessLimit && colorData.Red < cWhitenessLimit ) ||
                    ( colorData.Green > cBlacknessLimit && colorData.Green < cWhitenessLimit ) ||
                    ( colorData.Blue > cBlacknessLimit && colorData.Blue < cWhitenessLimit ));
        }

        private PixelData AssignZoneAndBin( PixelData pixelData, string zoneId ) {
            pixelData.SetZone( zoneId );
            pixelData.SetBin( CreateBinnedColor( pixelData ));

            return pixelData;
        }

        private Rectangle ZoneToContext( ZoneDefinition zone, ProcessContext context ) {
            return new Rectangle( (int)(( zone.ZoneArea.X / 100.0F ) * context.ImageWidth ), (int)(( zone.ZoneArea.Y / 100.0F ) * context.ImageHeight ),
                                  (int)(( zone.ZoneArea.Width / 100.0F ) * context.ImageWidth ), (int)(( zone.ZoneArea.Height / 100.0F ) * context.ImageHeight ));
        }

        private System.Windows.Media.Color CreateBinnedColor( PixelData colorData ) {
            return System.Windows.Media.Color.FromArgb( colorData.Alpha, (byte)( colorData.Red & cBinMask ), (byte)( colorData.Green & cBinMask ), (byte)( colorData.Blue & cBinMask ));
        }
    }
}
