using System;
using MilkBottle.Interfaces;

namespace MilkBottle.Models.Sunset {
    // from: http://wiki.crowe.co.nz/Calculate%20Sunrise%2fSunset.ashx

    public class CelestialData {
        public DateTime SunRise;
        public DateTime SunSet;
        public DateTime SolarNoon;

        public DateTime CivilTwilightStart;
        public DateTime CivilTwilightEnd;

        public DateTime NauticalTwilightStart;
        public DateTime NauticalTwilightEnd;

        public DateTime AstronomicalTwilightStart;
        public DateTime AstronomicalTwilightEnd;

        public DateTime MoonRise;
        public DateTime MoonSet;
    }

    public class CelestialCalculator : ICelestialCalculator {
        public CelestialData CalculateData( double latitude, double longitude ) {
            // Determine the # of hours that the local time is different from UTC/GMT
            TimeSpan ts = DateTime.Now - DateTime.UtcNow;

            // Get the number of hours difference
            double timeZoneDifferenceFromUtc = ts.TotalHours;

            // Get back the data
            return CalculateData( latitude, longitude, DateTime.Now, timeZoneDifferenceFromUtc );
        }

        public CelestialData CalculateData( double latitude, double longitude, DateTime forTime, double timeZoneDifferenceFromUtc ) {
            var data = new CelestialData();

            FindSunAndTwilightDataForDate( data, forTime, timeZoneDifferenceFromUtc, longitude, latitude );
            FindMoonRiseAndSet( data, forTime, timeZoneDifferenceFromUtc, longitude, latitude );

            var ts = data.SunSet - data.SunRise;
            data.SolarNoon = data.SunRise.AddMilliseconds( ts.TotalMilliseconds / 2 );

            return data;
        }

        private string hrsmin( double hours ) {
            //
            //	takes decimal hours and returns a string in hhmm format
            //
            double hrs, h, m;
            double dum;
            hrs = Math.Floor( hours * 60 + 0.5 ) / 60.0;
            h = Math.Floor( hrs );
            m = Math.Floor( 60 * ( hrs - h ) + 0.5 );
            dum = ( h * 100 + m );

            return dum.ToString( "0000" );
        }


        private int ipart( double x ) {
            //
            //	returns the integer part - like int() in basic
            //
            double a;
            if( x > 0 ) {
                a = Math.Floor( x );
            }
            else {
                a = Math.Ceiling( x );
            }
            return Convert.ToInt32( a );
        }


        private double frac( double x ) {
            //
            //	returns the fractional part of x as used in minimoon and minisun
            //
            double a;
            a = x - Math.Floor( x );
            if( a < 0 )
                a += 1;
            return a;
        }

        //
        // round rounds the number num to dp decimal places
        // the second line is some C like jiggery pokery I
        // found in an OReilly book which means if dp is null
        // you get 2 decimal places.
        //
        private double round( double num, double dp ) {
            //   dp = (!dp ? 2: dp);
            return Math.Round( num * Math.Pow( 10, dp ) ) / Math.Pow( 10, dp );
        }


        private double range( double x ) {
            //
            //	returns an angle in degrees in the range 0 to 360
            //
            double a;
            double b;
            b = x / 360;
            a = 360 * ( b - ipart( b ) );
            if( a < 0 ) {
                a = a + 360;
            }
            return a;
        }


        private double GetJulianDate( DateTime dt, int hour ) {
            //
            //	Takes the day, month, year and hours in the day and returns the
            //  modified julian day number defined as mjd = jd - 2400000.5
            //  checked OK for Greg era dates - 26th Dec 02
            //
            int Day, Month, Year;

            Day = dt.Day;
            Month = dt.Month;
            Year = dt.Year;

            double a, b;
            if( Month <= 2 ) {
                Month = Month + 12;
                Year = Year - 1;
            }
            a = 10000.0 * Year + 100.0 * Month + Day;
            if( a <= 15821004.1 ) {
                b = -2 * Math.Floor( (double)( Year + 4716 ) / 4 ) - 1179;
            }
            else {
                b = Math.Floor( (double)Year / 400 ) - Math.Floor( (double)Year / 100 ) + Math.Floor( (double)Year / 4 );
            }
            a = 365.0 * Year - 679004.0;
            return ( a + b + Math.Floor( 30.6001 * ( Month + 1 ) ) + Day + hour / 24.0 );
        }

        private string caldat( double mjd ) {
            //
            //	Takes mjd and returns the civil calendar date in Gregorian calendar
            //  as a string in format yyyymmdd.hhhh
            //  looks OK for Greg era dates  - not good for earlier - 26th Dec 02
            //
            double calout;
            double b, d, f, jd, jd0, c, e, day, month, year, hour;
            jd = mjd + 2400000.5;
            jd0 = Math.Floor( jd + 0.5 );
            if( jd0 < 2299161.0 ) {
                c = jd0 + 1524.0;
            }
            else {
                b = Math.Floor( ( jd0 - 1867216.25 ) / 36524.25 );
                c = jd0 + ( b - Math.Floor( b / 4 ) ) + 1525.0;
            }
            d = Math.Floor( ( c - 122.1 ) / 365.25 );
            e = 365.0 * d + Math.Floor( d / 4 );
            f = Math.Floor( ( c - e ) / 30.6001 );
            day = Math.Floor( c - e + 0.5 ) - Math.Floor( 30.6001 * f );
            month = f - 1 - 12 * Math.Floor( f / 14 );
            year = d - 4715 - Math.Floor( ( 7 + month ) / 10 );
            hour = 24.0 * ( jd + 0.5 - jd0 );
            //  hour = hrsmin(hour);
            calout = round( year * 10000.0 + month * 100.0 + day + hour / 10000, 4 );
            return calout + ""; //making sure calout is a string
        }


        private double[] quad( double ym, double yz, double yp ) {
            //
            //	finds the parabola through the three points (-1,ym), (0,yz), (1, yp)
            //  and returns the coordinates of the max/min (if any) xe, ye
            //  the values of x where the parabola crosses zero (roots of the quadratic)
            //  and the number of roots (0, 1 or 2) within the interval [-1, 1]
            //
            //	well, this routine is producing sensible answers
            //
            //  results passed as array [nz, z1, z2, xe, ye]
            //
            double nz, a, b, c, dis, dx, xe, ye, z1 = 0, z2 = 0;
            double[] quadout = new double[5];

            nz = 0;
            a = 0.5 * ( ym + yp ) - yz;
            b = 0.5 * ( yp - ym );
            c = yz;
            xe = -b / ( 2 * a );
            ye = ( a * xe + b ) * xe + c;
            dis = b * b - 4.0 * a * c;
            if( dis > 0 ) {
                dx = 0.5 * Math.Sqrt( dis ) / Math.Abs( a );
                z1 = xe - dx;
                z2 = xe + dx;
                if( Math.Abs( z1 ) <= 1.0 )
                    nz += 1;
                if( Math.Abs( z2 ) <= 1.0 )
                    nz += 1;
                if( z1 < -1.0 )
                    z1 = z2;
            }
            quadout[0] = nz;
            quadout[1] = z1;
            quadout[2] = z2;
            quadout[3] = xe;
            quadout[4] = ye;
            return quadout;
        }


        private double lmst( double mjd, double glong ) {
            //
            //	Takes the mjd and the longitude (west negative) and then returns
            //  the local sidereal time in hours. Im using Meeus formula 11.4
            //  instead of messing about with UTo and so on
            //
            double lst, t, d;
            d = mjd - 51544.5;
            t = d / 36525.0;
            lst = range( 280.46061837 + 360.98564736629 * d + 0.000387933 * t * t - t * t * t / 38710000 );
            return ( lst / 15.0 + glong / 15 );
        }


        private double[] minisun( double t ) {
            //
            //	returns the ra and dec of the Sun in an array called suneq[]
            //  in decimal hours, degrees referred to the equinox of date and using
            //  obliquity of the ecliptic at J2000.0 (small error for +- 100 yrs)
            //	takes t centuries since J2000.0. Claimed good to 1 arcmin
            //
            double p2 = 6.283185307, coseps = 0.91748, sineps = 0.39778;
            double L, M, DL, SL, X, Y, Z, RHO, ra, dec;
            double[] suneq = new double[2];

            M = p2 * frac( 0.993133 + 99.997361 * t );
            DL = 6893.0 * Math.Sin( M ) + 72.0 * Math.Sin( 2 * M );
            L = p2 * frac( 0.7859453 + M / p2 + ( 6191.2 * t + DL ) / 1296000 );
            SL = Math.Sin( L );
            X = Math.Cos( L );
            Y = coseps * SL;
            Z = sineps * SL;
            RHO = Math.Sqrt( 1 - Z * Z );
            dec = ( 360.0 / p2 ) * Math.Atan( Z / RHO );
            ra = ( 48.0 / p2 ) * Math.Atan( Y / ( X + RHO ) );
            if( ra < 0 )
                ra += 24;
            suneq[0] = dec;
            suneq[1] = ra;
            return suneq;
        }


        private double[] minimoon( double t ) {
            //
            // takes t and returns the geocentric ra and dec in an array mooneq
            // claimed good to 5' (angle) in ra and 1' in dec
            // tallies with another approximate method and with ICE for a couple of dates
            //
            double p2 = 6.283185307, arc = 206264.8062, coseps = 0.91748, sineps = 0.39778;
            double L0, L, LS, F, D, H, S, N, DL, CB, L_moon, B_moon, V, W, X, Y, Z, RHO, dec, ra;
            double[] mooneq = new double[2];

            L0 = frac( 0.606433 + 1336.855225 * t );    // mean longitude of moon
            L = p2 * frac( 0.374897 + 1325.552410 * t ); //mean anomaly of Moon
            LS = p2 * frac( 0.993133 + 99.997361 * t ); //mean anomaly of Sun
            D = p2 * frac( 0.827361 + 1236.853086 * t ); //difference in longitude of moon and sun
            F = p2 * frac( 0.259086 + 1342.227825 * t ); //mean argument of latitude

            // corrections to mean longitude in arcsec
            DL = 22640 * Math.Sin( L );
            DL += -4586 * Math.Sin( L - 2 * D );
            DL += +2370 * Math.Sin( 2 * D );
            DL += +769 * Math.Sin( 2 * L );
            DL += -668 * Math.Sin( LS );
            DL += -412 * Math.Sin( 2 * F );
            DL += -212 * Math.Sin( 2 * L - 2 * D );
            DL += -206 * Math.Sin( L + LS - 2 * D );
            DL += +192 * Math.Sin( L + 2 * D );
            DL += -165 * Math.Sin( LS - 2 * D );
            DL += -125 * Math.Sin( D );
            DL += -110 * Math.Sin( L + LS );
            DL += +148 * Math.Sin( L - LS );
            DL += -55 * Math.Sin( 2 * F - 2 * D );

            // simplified form of the latitude terms
            S = F + ( DL + 412 * Math.Sin( 2 * F ) + 541 * Math.Sin( LS ) ) / arc;
            H = F - 2 * D;
            N = -526 * Math.Sin( H );
            N += +44 * Math.Sin( L + H );
            N += -31 * Math.Sin( -L + H );
            N += -23 * Math.Sin( LS + H );
            N += +11 * Math.Sin( -LS + H );
            N += -25 * Math.Sin( -2 * L + F );
            N += +21 * Math.Sin( -L + F );

            // ecliptic long and lat of Moon in rads
            L_moon = p2 * frac( L0 + DL / 1296000 );
            B_moon = ( 18520.0 * Math.Sin( S ) + N ) / arc;

            // equatorial coord conversion - note fixed obliquity
            CB = Math.Cos( B_moon );
            X = CB * Math.Cos( L_moon );
            V = CB * Math.Sin( L_moon );
            W = Math.Sin( B_moon );
            Y = coseps * V - sineps * W;
            Z = sineps * V + coseps * W;
            RHO = Math.Sqrt( 1.0 - Z * Z );
            dec = ( 360.0 / p2 ) * Math.Atan( Z / RHO );
            ra = ( 48.0 / p2 ) * Math.Atan( Y / ( X + RHO ) );
            if( ra < 0 )
                ra += 24;
            mooneq[0] = dec;
            mooneq[1] = ra;
            return mooneq;
        }


        private double sin_alt( double iobj, double mjd0, double hour, double glong, double cglat, double sglat ) {
            //
            //	this rather mickey mouse function takes a lot of
            //  arguments and then returns the sine of the altitude of
            //  the object labelled by iobj. iobj = 1 is moon, iobj = 2 is sun
            //
            double mjd, t, ra, dec, tau, salt, rads = 0.0174532925;
            double[] objpos = new double[2];
            mjd = mjd0 + hour / 24.0;
            t = ( mjd - 51544.5 ) / 36525.0;
            if( iobj == 1 ) {
                objpos = minimoon( t );
            }
            else {
                objpos = minisun( t );
            }
            ra = objpos[1];
            dec = objpos[0];
            // hour angle of object
            tau = 15.0 * ( lmst( mjd, glong ) - ra );
            // sin(alt) of object using the conversion formulas
            salt = sglat * Math.Sin( rads * dec ) + cglat * Math.Cos( rads * dec ) * Math.Cos( rads * tau );
            return salt;
        }


        private string FindSunAndTwilightDataForDate( CelestialData data, DateTime dt, double tz, double glong, double glat ) {
            //
            //	this is my attempt to encapsulate most of the program in a function
            //	then this function can be generalised to find all the Sun events.
            //
            //
            double mjd = GetJulianDate(dt, 0);
            double sglat, cglat, date, ym, yz, utrise = 0, utset = 0;
            double yp, nz, hour, xe, ye, z1, z2, rads = 0.0174532925;

            bool rise, sett, above;
            double[] quadout = new double[4];
            double[] sinho = new double[4];
            string outstring = "";
            //
            //	Set up the array with the 4 values of sinho needed for the 4
            //      kinds of sun event
            //
            sinho[0] = Math.Sin( rads * -0.833 );       //sunset upper limb simple refraction
            sinho[1] = Math.Sin( rads * -6.0 );     //civil twi
            sinho[2] = Math.Sin( rads * -12.0 );        //nautical twi
            sinho[3] = Math.Sin( rads * -18.0 );        //astro twi
            sglat = Math.Sin( rads * glat );
            cglat = Math.Cos( rads * glat );
            date = mjd - tz / 24;
            //
            //	main loop takes each value of sinho in turn and finds the rise/set
            //      events associated with that altitude of the Sun
            //
            for( int j = 0; j < 4; j++ ) {
                rise = false;
                sett = false;
                above = false;
                hour = 1.0;
                ym = sin_alt( 2, date, hour - 1.0, glong, cglat, sglat ) - sinho[j];
                if( ym > 0.0 )
                    above = true;
                //
                // the while loop finds the sin(alt) for sets of three consecutive
                // hours, and then tests for a single zero crossing in the interval
                // or for two zero crossings in an interval or for a grazing event
                // The flags rise and sett are set accordingly
                //
                while( hour < 25 && ( sett == false || rise == false ) ) {
                    yz = sin_alt( 2, date, hour, glong, cglat, sglat ) - sinho[j];
                    yp = sin_alt( 2, date, hour + 1.0, glong, cglat, sglat ) - sinho[j];
                    quadout = quad( ym, yz, yp );
                    nz = quadout[0];
                    z1 = quadout[1];
                    z2 = quadout[2];
                    xe = quadout[3];
                    ye = quadout[4];

                    // case when one event is found in the interval
                    if( nz == 1 ) {
                        if( ym < 0.0 ) {
                            utrise = hour + z1;
                            rise = true;
                        }
                        else {
                            utset = hour + z1;
                            sett = true;
                        }
                    } // end of nz = 1 case

                    // case where two events are found in this interval
                    // (rare but whole reason we are not using simple iteration)
                    if( nz == 2 ) {
                        if( ye < 0.0 ) {
                            utrise = hour + z2;
                            utset = hour + z1;
                        }
                        else {
                            utrise = hour + z1;
                            utset = hour + z2;
                        }
                    } // end of nz = 2 case

                    // set up the next search interval
                    ym = yp;
                    hour += 2.0;

                } // end of while loop
                  //
                  // now search has completed, we compile the string to pass back
                  // to the main loop. The string depends on several combinations
                  // of the above flag (always above or always below) and the rise
                  // and sett flags
                  //

                switch( j ) {
                    case 0: // SunRise/SunSet
                            //data.XXX = z2.ToString();
                        data.SunRise = ( rise || sett ) ? rise ? ConvertToDateTime( dt, utrise ) : DateTime.MinValue : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        data.SunSet = ( rise || sett ) ? sett ? ConvertToDateTime( dt, utset ) : DateTime.MinValue.AddDays( 1 ) : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        break;
                    case 1: // Civil Twilight
                        data.CivilTwilightStart = ( rise || sett ) ? rise ? ConvertToDateTime( dt, utrise ) : DateTime.MinValue : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        data.CivilTwilightEnd = ( rise || sett ) ? sett ? ConvertToDateTime( dt, utset ) : DateTime.MinValue.AddDays( 1 ) : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        break;
                    case 2: // nautical Twilight
                        data.NauticalTwilightStart = ( rise || sett ) ? rise ? ConvertToDateTime( dt, utrise ) : DateTime.MinValue : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        data.NauticalTwilightEnd = ( rise || sett ) ? sett ? ConvertToDateTime( dt, utset ) : DateTime.MinValue.AddDays( 1 ) : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        break;
                    case 3: // Astronomical Twilight
                        data.AstronomicalTwilightStart = ( rise || sett ) ? rise ? ConvertToDateTime( dt, utrise ) : DateTime.MinValue : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        data.AstronomicalTwilightEnd = ( rise || sett ) ? sett ? ConvertToDateTime( dt, utset ) : DateTime.MinValue.AddDays( 1 ) : ( above ) ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
                        break;
                }

                if( rise || sett ) {
                    if( rise )
                        outstring += " " + hrsmin( utrise );
                    else
                        outstring += " No Rise";
                    if( sett )
                        outstring += " " + hrsmin( utset );
                    else
                        outstring += " No Set";
                }
                else {
                    if( above )
                        outstring += "above";
                    else
                        outstring += "below";
                }
            } // end of for loop - next condition

            return outstring;
        }

        private DateTime ConvertToDateTime( DateTime dt, double DecimalHours ) {
            int Hours = Convert.ToInt32(Math.Floor(DecimalHours));
            double tmp = (DecimalHours - Hours) * 60;
            int Minutes = Convert.ToInt32(Math.Floor(tmp));
            tmp = ( tmp - Minutes ) * 60;
            int Seconds = Convert.ToInt32(Math.Floor(tmp));

            return new DateTime( dt.Year, dt.Month, dt.Day, Hours, Minutes, Seconds );
        }

        private string FindMoonRiseAndSet( CelestialData data, DateTime dt, double tz, double glong, double glat ) {
            //
            //	Im using a separate function for moonrise/set to allow for different tabulations
            //  of moonrise and sun events ie weekly for sun and daily for moon. The logic of
            //  the function is identical to find_sun_and_twi_events_for_date()
            //
            //alert(mjd);
            //alert(tz);
            //alert(glong);
            //alert(glat);

            double mjd = GetJulianDate(dt, 0);

            double sglat, sinho, cglat, date, ym, yz, utrise, utset;
            double yp, nz, hour, z1, z2, ye, rads = 0.0174532925;

            bool rise, sett, above;
            double[] quadout = new double[4];
            //var sinho;
            string outstring = "";

            sinho = Math.Sin( rads * 8 / 60 );      //moonrise taken as centre of moon at +8 arcmin
            sglat = Math.Sin( rads * glat );
            cglat = Math.Cos( rads * glat );
            date = mjd - tz / 24;
            rise = false;
            sett = false;
            above = false;
            hour = 1.0;
            utrise = 0;
            utset = 0;
            ym = sin_alt( 1, date, hour - 1.0, glong, cglat, sglat ) - sinho;
            if( ym > 0.0 )
                above = true;
            while( hour < 25 && ( sett == false || rise == false ) ) {
                yz = sin_alt( 1, date, hour, glong, cglat, sglat ) - sinho;
                yp = sin_alt( 1, date, hour + 1.0, glong, cglat, sglat ) - sinho;
                quadout = quad( ym, yz, yp );
                nz = quadout[0];
                z1 = quadout[1];
                z2 = quadout[2];
                ye = quadout[4];

                // case when one event is found in the interval
                if( nz == 1 ) {
                    if( ym < 0.0 ) {
                        utrise = hour + z1;
                        rise = true;
                    }
                    else {
                        utset = hour + z1;
                        sett = true;
                    }
                } // end of nz = 1 case

                // case where two events are found in this interval
                // (rare but whole reason we are not using simple iteration)
                if( nz == 2 ) {
                    if( ye < 0.0 ) {
                        utrise = hour + z2;
                        utset = hour + z1;
                    }
                    else {
                        utrise = hour + z1;
                        utset = hour + z2;
                    }
                }

                // set up the next search interval
                ym = yp;
                hour += 2.0;

            } // end of while loop

            data.MoonRise = ( rise || sett ) ? rise ? ConvertToDateTime( dt, utrise ) : DateTime.MinValue : above ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );
            data.MoonSet = ( rise || sett ) ? sett ? ConvertToDateTime( dt, utset ) : DateTime.MinValue.AddDays( 1 ) : above ? DateTime.MaxValue : DateTime.MaxValue.AddDays( -1 );

            if( rise || sett ) {
                if( rise )
                    outstring += " " + hrsmin( utrise );
                else
                    outstring += " No Rise";
                if( sett )
                    outstring += " " + hrsmin( utset );
                else
                    outstring += " No Set";
            }
            else {
                if( above )
                    outstring += "above";
                else
                    outstring += "below";
            }
            return outstring;
        }
    }
}
