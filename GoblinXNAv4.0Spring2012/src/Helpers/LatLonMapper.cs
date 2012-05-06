/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Author: Sean White (swhite@cs.columbia.edu)
 * 
 *************************************************************************************/ 


using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Helpers
{
    //Static class to handle conversion from longitude,latitude to mercator projection map coordinates
    //In this case, Google maps and Microsoft maps are both mercator projections.
    //Note that some rotation is also required because the site map has been rotated.
    //However, my understanding is that Mercator Projections are conformal mappings which
    //means they preserve angles but can't be scaled.  This could be incorrect.

    //Eventually, this needs to be generalized so that we can take a longitude and latitude and 
    //map it correctly given the GPS coordinates and orientation of a fiducial

    /// <summary>
    /// Maps latitude and longitude to a 3D point.
    /// </summary>
    public class LatLonMapper
    {
        // 0,0 of map is strange (in upper right with x increasing left, y increasing down)
        // Offset from origin of map is x: 1.5 , y: 2.4
        //
        // The following latitude and longitude are approximated from Google Maps
        // upper right of map
        //(40.81893883339474, -73.9561414718628)

        // upper left of map
        //(40.82002884826906, -73.95869493484497)

        // lower right of map
        //(40.815615902850745, -73.95848572254181)

        //lower left of map
        //(40.81676889944122, -73.96104991436005)

        // map origin - Upper right coords using wikipedia mercator projection
        // y value 0.781681508835296
        // x value 0
        // which means the basic vector Vector3(1.5f, 2.4f, 0);

        // upper left lat/lon
        // y value 0.78170664761957
        // x value 0.00255346298217773

        // lower right lat/lon
        // y value 0.781604875334632
        // x value 0.00234425067901611

        // Upper right lat/lon
        // y value 4174455.75
        // x value -6893212
        // upper left lat/lon
        // y value 4174590
        // x value -6893450
        // lower right lat/lon
        // y value 4174046.5
        // x value -6893430.5

        /// <summary>
        /// Converts longitude to x value in meters but for ECEF which isn't a mercator projection
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        static double lon2xECEF(double lon, double lat, double alt)
        {

            // conversions based on http://mathforum.org/library/drmath/view/51832.html
            // a   the equatorial earth radius
            // f   the "flattening" parameter ( = (a-b)/a ,the ratio of the
            // difference between the equatorial and polar radii to a;
            // this is a measure of how "elliptical" a polar cross-section
            // is).

            //For WGS84, 
            double a  = 6378137; // meters
            double f = 1/298.257224;

            //                                    1
            //C =  ---------------------------------------------------
            //     sqrt( cos^2(latitude) + (1-f)^2 * sin^2(latitude) )

            double c = 1 / (Math.Sqrt(Math.Pow(Math.Cos(lat), 2.0) + Math.Pow((1 - f), 2.0) * 
                Math.Pow(Math.Sin(lat), 2.0)));

            // S = (1-f)^2 * C
            double s = Math.Pow((1-f),2)*c;
            

            // Then a point with (geodetic) latitude "lat," longitude "lon," and 
            // altitude h above the reference ellipsoid has ECEF coordinates

            // x = (aC+h)cos(lat)cos(lon)
            // y = (aC+h)cos(lat)sin(lon)
            // z = (aS+h)sin(lat) 

           
            return (lon);
        }

        // not currently using this
        #region Wikipedia Mercator Projection
        

        //Mercator projection conversion
        // based on http://en.wikipedia.org/wiki/Mercator_projection

        //convert longitude to x value 
        // scalefactor and dest could be preset

        static double origin = -73.9561414718628;
        static double scalefactor = 1;

        public static double lon2x(double dest)
        {
            return ((origin - dest) * scalefactor);
        }

        //convert latitude to y value 
        public static double lat2y(double lat)
        {
            double radlat = DegToRad(lat);
            return (Math.Log(Math.Tan(radlat)+(1/Math.Cos(radlat)))*scalefactor);
        }

        // y value 0.781681508835296
        // x value 0

        static double yorigin = 0.781681508835296;
        static double xorigin = 0;
        //cartesian distance from upper right to upper left is 5.5

        static double xscale = 5.5d / 0.00255346298217773d;
        static double yscale = (11.6f-2.4f)/(0.781681508835296 - 0.781604875334632);

        // lower right lat/lon
        // y value 0.781604875334632
        // x value 0.00234425067901611
        public static Vector3 latlon2sitemap(double lat, double lon)
        {
            // first offset to map origin upper right
            Vector3 location = new Vector3(0f, 0f, 0);

            double x = lon2GX(lon);
            double y = lat2GY(lat);

            // shift to map origin
            x = -1*x - 6893212; // note that x decreases in negative values as it moves left in longitude so *-1
            y = y - 12602760;  

            //scale to map location (basically scaling for the 
            x = x * (5.0 / 240.0);
            y = y * (3.0 / 152.0); 

            location.X = location.X + (float)x;
            location.Y = location.Y + (float)y;

            return (location);


        }
        #endregion

        // the following are google specific transformations that should work assuming 
        // a zoom level of 17.
        static double TILE_SIZE = 256;
        static double tiles = Math.Pow(2, 17);
        static double circumference = TILE_SIZE * tiles;
        static double radius = (circumference / (2 * Math.PI));

        public static double lat2GY(double lat)
        {
            double latitude = DegToRad(lat);
            //double y  = (radius)/2.0 * Math.Log( (1.0 + Math.Sin(latitude)) /(1.0 - Math.Sin(latitude)) );
            //double y = ((tiles*TILE_SIZE) / 2.0) - Math.Log((1.0 + Math.Sin(latitude)) / (1.0 - Math.Sin(latitude)));
            double y = tiles * TILE_SIZE / 2 - (radius)/2.0 * Math.Log( (1.0 + Math.Sin(latitude)) /(1.0 - Math.Sin(latitude)) ) ;
            return y;
        }

        public static double lon2GX(double lon)
        {
            double longitude = DegToRad(lon);
            return (radius * longitude);
        }

        // Note that all C# Math functions assume radians so we need some quick conversions
        public static double DegToRad(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        public static double RadToDeg(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        #region alternative mercator projection from Chris

        static public Point getXYfromLatLon(double lat,double lon) {

            int zoom = 1;
            
            int MapTileWidth = 256;
            int MapTileHeight = 256;
            double l_lon = lon +180.0;
            double l_x = l_lon/360.0 *131072;

            int tile_x = (int) l_x >> zoom;

            double l_lat = lat / 180.0 * Math.PI;
            double l_y = Math.PI - 0.5 * Math.Log((1+Math.Sin(l_lat))/(1-Math.Sin(l_lat)));
            l_y = l_y / 2 / Math.PI*131072;
            int tile_y = (int) l_y >> zoom;
            
            int dx = ( ( tile_x + 1 ) << zoom ) - ( tile_x << zoom );
            double tx = l_x / Math.Pow( 2.0, zoom );
            tx -= (int)tx;
            ////int dy = ( ( tile_y + 1 ) << zoom ) - ( tile_y << zoom );
            double ty = l_y / Math.Pow( 2.0, zoom );
            ty -= (int)ty;

            return (new Point((int) (tx*MapTileWidth),(int) (ty*MapTileHeight)));

        }




        #endregion

    }
}
