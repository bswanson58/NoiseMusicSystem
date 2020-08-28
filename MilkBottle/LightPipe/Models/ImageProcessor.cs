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

        private readonly IZoneManager           mZoneManager;
        private readonly Dictionary<string, List<ZoneSummary>>  mZoneSummaries;
        private readonly Subject<ZoneSummary>   mZoneUpdated;
        private readonly ZoneGroup              mZoneGroup;

        public  IObservable<ZoneSummary>        ZoneUpdate => mZoneUpdated;

        public  long                            ElapsedTime { get; private set; }

        public ImageProcessor( IZoneManager zoneManager ) {
            mZoneManager = zoneManager;
            mZoneSummaries = new Dictionary<string, List<ZoneSummary>>();

            mZoneGroup = mZoneManager.GetCurrentGroup();

            mZoneUpdated = new Subject<ZoneSummary>();
        }

        public void ProcessImage( Bitmap image ) {
            if( mZoneGroup != null ) {
                var stopWatch = Stopwatch.StartNew();
                var context = new ProcessContext( image );
                var zoneData = new Dictionary<string, IEnumerable<PixelData>>();

                mZoneGroup.Zones.ForEach( zone => {
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
