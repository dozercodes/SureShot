/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
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
using System.IO.Ports;
using System.Timers;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using GoblinXNA.Helpers;

namespace GoblinXNA.Device.GPS
{
    #region Delegates
    public delegate void GPSListener(string latitude, string longitude, string elevation);
    #endregion

    #region Enums
    /// <summary>
    /// The status of the GPS receiver.
    /// </summary>
    public enum GPSState
    {
        Connected,
        NoGPS,
        NoFix,
        Fix
    }
    #endregion

    /// <summary>
    /// This is a GPS class for reading data from a serial port and parsing the data
    /// into useful coordinates.
    /// 
    /// The GPS is assumed to be on a serial port. Code has been tested on USB and Bluetooth
    /// based GPS devices.  To use, create a new instance of the GPS and add a GPSListener
    /// delegate or poll the object.
    /// </summary>
    public class GPS : InputDevice
    {
        # region Member Fields

        private String gpsPort;  
        private SerialPort sp;
        private int baudRate;
        private string latitude;
        private string longitude;
        private string elevation;
        private String timeStamp;
        private GPSState state;
        private bool keepAlive;  // if set, try to find GPS device if lost
        private GPSListener dataListener;

        private System.Timers.Timer heartbeat;
        private DateTime lastMessage;

        private static GPS gps;

        # endregion

        # region Constructors

        /// <summary>
        /// A private constructor.
        /// </summary>
        private GPS() {
            
            longitude = "no data";
            elevation = "no data";
            timeStamp = "no Data";
            state = GPSState.NoGPS;
            keepAlive = false;
        }

        # endregion

        # region Properties
        /// <summary>
        /// Gets the current latitude.
        /// </summary>
        public string Latitude
        {
            get { return latitude; }
        }

        /// <summary>
        /// Gets the current longitude.
        /// </summary>
        public String Longitude
        {
            get { return longitude; }
        }

        /// <summary>
        /// Gets the current elevation.
        /// </summary>
        public String Elevation
        {
            get { return elevation; }
        }

        public String Identifier
        {
            get { return "GPS"; }
        }

        public bool IsAvailable
        {
            get { return (state == GPSState.Connected); }
        }

        public static GPS Instance
        {
            get
            {
                if (gps == null)
                    gps = new GPS();

                return gps;
            }
        }

        /// <summary>
        /// Gets the serial port associated with this GPS receiver.
        /// </summary>
        public String SerialPort
        {
            get { return gpsPort; }
        }

        /// <summary>
        /// Gets the baud rate for GPS serial port communication.
        /// </summary>
        public int BaudRate
        {
            get { return baudRate; }
        }

        /// <summary>
        /// Gets or sets whether to try to automatically find GPS if lost.
        /// </summary>
        public bool KeepAlive
        {
            get { return keepAlive; }
            set
            {
                if (keepAlive == value || state != GPSState.Connected)
                    return;

                keepAlive = value;
                if (keepAlive)
                {
                    // start timer to create a heartbeat every 2 seconds
                    heartbeat = new System.Timers.Timer(2000);
                    heartbeat.Elapsed += new ElapsedEventHandler(heartbeat_tick);
                    heartbeat.Start();
                }
                else
                    heartbeat.Stop();
            }
        }

        # endregion

        #region Methods

        /// <summary>
        /// Initializes a GPS receiver with baud rate of 38400 through the specified 'port'.
        /// </summary>
        /// <param name="port">Serial port associated with GPS. ex: COM6:</param>
        public void Initialize(String port)
        {
            Initialize(port, 38400);
        }

        /// <summary>
        /// Initializes a GPS receiver with the specified 'baud' rate through the specified 'port'.
        /// </summary>
        /// <param name="port">Serial port associated with GPS. ex: COM6</param>
        /// <param name="baud">Baud rate for GPS serial port communication</param>
        public void Initialize(String port, int baud)
        {
            // create a new serial port using the specified port and baud rate
            // I currently don't catch exceptions in creating the GPS so the user
            // needs to do this.  
            sp = new SerialPort(port, baud);
            baudRate = baud;
            gpsPort = port;

            // create an event handler to listen to data as it comes in
            sp.DataReceived += new SerialDataReceivedEventHandler(AsynchronousReader);

            // Try to open the serial port.  If it works and keepalive is true,
            // create a heartbeat timer.  This provides robustness in the case that
            // the GPS disappears for a moment from the serial port but isn't 
            // necessary
            try
            {
                sp.Open();
            }
            catch (Exception e)
            {
                Log.Write(e.Message, Log.LogLevel.Warning);
            }
        }

        /// <summary>
        /// Adds a listener to the GPS object so we can report coordinates as they come in
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(GPSListener listener)
        {
            if (listener != null) dataListener = listener;
        }

        /// <summary>
        /// This is the listener to handle data as it comes in from the serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsynchronousReader(object sender, SerialDataReceivedEventArgs e)
        {
            // read a line of data from the serial buffer
            string inbuffer = sp.ReadLine();

            // note the time of the most recent data output
            lastMessage = DateTime.Now;

            // parse the line of data
            ParseNMEAString(inbuffer);

            // if there are any listeners associated with the GPS object, 
            // send them a message with the parsed data
            WriteLatLon();
        }

        /// <summary>
        /// Sends the parsed data (latitude and longitude) to any listeners.
        /// </summary>
        private void WriteLatLon()
        {
            // This should be fixed to handle multiple listeners.
            if (dataListener != null) dataListener(latitude, longitude, elevation);
        }

        /// <summary>
        /// Parses the data string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool ParseNMEAString(string s)
        {
            // Remove any extraneous chars at front of string
            try
            {
                while (s[0] != '$')
                {
                    s = s.Remove(0, 1);
                }
            }
            catch (Exception e)
            {
				Log.Write("Couldn't read:" + e.Message, Log.LogLevel.Warning);
            }

            Debug.WriteLine(s);
            // Depending on the type of message, parse correctly
            switch (s.Split(',')[0])
            {
                case "$GPRMC":
                    return ParseGPRMC(s);
                case "$GPGGA":
                    return ParseGPGGA(s);
                case "$GPGSA":		// GPS DOP and active satellites
                case "$GPGSV":		// GPS satellites in view
                    return true;  // throw these away for now
                default:
                    return false;  //Not a string we handle yet
            }

        }

        /// <summary>
        /// Parses $GPGGA - Global Positioning System Fix Data
        ///
        /// </summary>
        /// <remarks>
        /// Referenced from http://home.mira.net/~gnb/gps/nmea.html#gpgga
        /// </remarks>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool ParseGPGGA(string s)
        {
            string[] words = s.Split(',');
            if (words[2] != "")
            {
                //words[2] = words[2].Remove(4, 1);
                //words[2] = words[2].Insert(2, ".");
                latitude = NMEADegreesToDecimal(words[2]) + " " + words[3];
                //words[4] = words[4].Remove(5, 1);
                //words[4] = words[4].Insert(3, ".");
                longitude = NMEADegreesToDecimal(words[4]) + " " + words[5];

                timeStamp = words[1];
                elevation = words[9];
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Parses $GPRMC - Recommended Minimum Specific GPS/TRANSIT Data
        ///
        /// </summary>
        /// <remarks>
        /// Referenced from http://home.mira.net/~gnb/gps/nmea.html#gprmc
        /// 
        /// $GPRMC,hhmmss.ss,A,llll.ll,a,yyyyy.yy,a,x.x,x.x,ddmmyy,x.x,a,m*hh
        /// Field #
        /// 1    = UTC time of fix
        /// 2    = Data status (A=Valid position, V=navigation receiver warning)
        /// 3    = Latitude of fix
        /// 4    = N or S of longitude
        /// 5    = Longitude of fix
        /// 6    = E or W of longitude
        /// 7    = Speed over ground in knots
        /// 8    = Track made good in degrees True
        /// 9    = UTC date of fix
        /// 10   = Magnetic variation degrees (Easterly var. subtracts from true course)
        /// 11   = E or W of magnetic variation
        /// 12   = Mode indicator, (A=Autonomous, D=Differential, E=Estimated, N=Data not valid)
        /// 13   = Checksum
        /// </remarks>
        /// <param name="s">String to parse</param>
        /// <returns></returns>
        private bool ParseGPRMC(string s)
        {
            string[] words = s.Split(',');

            if (words[3] != "")
            {
                //words[3] = words[3].Remove(4, 1);
                //words[3] = words[3].Insert(2, ".");
                latitude =  NMEADegreesToDecimal(words[3]) + " " + words[4];
                //words[5] = words[5].Remove(5, 1);
                //words[5] = words[5].Insert(3, ".");
                longitude = NMEADegreesToDecimal(words[5]) + " " + words[6];
                timeStamp = words[1];
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Converts NMEA degrees decimal-minutes to decimal-degrees.
        /// </summary>
        /// <param name="nmeaString">NMEA string</param>
        /// <returns>decimal-degrees string</returns>
        public String NMEADegreesToDecimal(String nmeaString)
        {
            // Most GPS system provide their results in degrees decimal-minutes but we need it in decimal-degrees
            // For example: 4312.4516 N is actually 43 degrees + 12.4516/60 minutes plus a heading

            float nmea = (float) Convert.ToDouble(nmeaString);
            float degrees = (float) Math.Truncate(nmea / 100.0);
            float minutes = nmea - (100 * degrees);
            float dd = degrees + minutes / 60;

            return dd.ToString();
        }

        /// <summary>
        /// Heartbeat for robustness
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void heartbeat_tick(object sender, ElapsedEventArgs e)
        {
            long delta = DateTime.Now.Ticks - lastMessage.Ticks;

            if (delta > 40000000)
            {
                state = GPSState.NoGPS;
            }
            else
            {
                if (latitude == "00.000000 N")
                    state = GPSState.NoFix;
                else
                    state = GPSState.Fix;
            }

            //Console.WriteLine("State: " + state);

            if (state == GPSState.NoGPS) 
                RestartGPS();

        }

        /// <summary>
        /// Restarts the GPS
        /// </summary>
        private void RestartGPS()
        {
            try
            {
                Log.Write("Restarting GPS", Log.LogLevel.Log);
                sp.Close();
                sp.Open();
            }
            catch (Exception e)
            {
                Log.Write(e.Message, Log.LogLevel.Warning);
            }
        }

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            // Nothing to update
        }

        public void TriggerDelegates(byte[] data)
        {
            throw new GoblinException("TriggerDelegates for GPS not implemented yet.");
        }

        public void Dispose()
        {
            if (heartbeat != null) 
                heartbeat.Dispose();
            sp.Close();
            sp = null;
        }

        # endregion
    }
}
