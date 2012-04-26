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
 * Authors: Hrvoje Benko 
 *          Mark Eaddy
 * 
 *************************************************************************************/ 

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Note: We must use "Goblin.Engine.Devices" instead of "Goblin.Engine.Device"
// so we don't collide with the DirectX "Device" class.
namespace GoblinXNA.Device.InterSense 
{
	/// <summary>
	/// A driver class for accessing the functions defined in isense.dll.
	/// </summary>
	public class ISDllBridge
	{
		#region InterSense constants 

		// tracking system type
		public enum ISD_SYSTEM_TYPE
		{
			ISD_NONE = 0,           // none found, or can't identify 
			ISD_PRECISION_SERIES,   // InertiaCube2, IS-300, IS-600, IS-900 and IS-1200 
			ISD_INTERTRAX_SERIES    // InterTrax 
		}

		// tracking system model 
		public enum ISD_SYSTEM_MODEL
		{
			ISD_UNKNOWN = 0,          
			ISD_IS300,          // 3DOF system 
			ISD_IS600,          // 6DOF system 
			ISD_IS900,          // 6DOF system   
			ISD_INTERTRAX,      // InterTrax (Serial) 
			ISD_INTERTRAX_2,    // InterTrax (USB) 
			ISD_INTERTRAX_LS,   // InterTraxLS, verification required 
			ISD_INTERTRAX_LC,   // InterTraxLC 
			ISD_ICUBE2,         // InertiaCube2 
			ISD_ICUBE2_PRO,     // InertiaCube2 Pro 
			ISD_IS1200,         // 6DOF system   
			ISD_ICUBE3          // InertiaCube3 
		}

		public enum ISD_INTERFACE_TYPE
		{
			ISD_INTERFACE_UNKNOWN = 0,
			ISD_INTERFACE_SERIAL,
			ISD_INTERFACE_USB,
			ISD_INTERFACE_ETHERNET_UDP,
			ISD_INTERFACE_ETHERNET_TCP,
			ISD_INTERFACE_IOCARD,
			ISD_INTERFACE_PCMCIA
		}

		public const int ISD_MAX_STATIONS	= 8;	// for now limited to 8

		public const int ISD_MAX_TRACKERS	= 8;

		// orientation format 
		public const int ISD_EULER			= 1;
		public const int ISD_QUATERNION		= 2;

		// Coordinate frame in that position and orientation data is reported 
		public const int ISD_DEFAULT_FRAME	= 1;    // InterSense default 
		public const int ISD_VSET_FRAME		= 2;    // Virtual set frame, use for camera tracker only 

		// number of supported stylus buttons 
		public const int ISD_MAX_BUTTONS	= 8;

		// hardware is limited to 10 analog/digital input channels per station 
		public const int ISD_MAX_CHANNELS	= 10;

		// maximum supported number of bytes for auxiliary input data
		public const int ISD_MAX_AUX_INPUTS = 4;

		// maximum supported number of bytes for auxiliary output data
		public const int ISD_MAX_AUX_OUTPUTS = 4;

		#endregion

		#region Intersense custom structs

		///////////////////////////////////////////////////////////////////////////////
		//define these structs for accessing the dll structs
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_TRACKER_INFO_TYPE
		{
			public float  LibVersion;     // InterSense Library version 

			public /*ulong*/ uint  TrackerType;    // IS Precision series or InterTrax. 
			// TrackerType can be: 
			// ISD_PRECISION_SERIES for IS-300, IS-600, IS-900 and IS-1200 model trackers, 
			// ISD_INTERTRAX_SERIES for InterTrax, or 
			// ISD_NONE if tracker is not initialized 

			public /*ulong*/ uint TrackerModel;   // ISD_UNKNOWN, ISD_IS300, ISD_IS600, ISD_IS900, ISD_INTERTRAX 

			public /*ulong*/ uint  Port;           // Number of the rs232 port. Starts with 1. 

			// Communications statistics. For information only. 

			public /*ulong*/ uint  RecordsPerSec;
			public float  KBitsPerSec;    

			// Following items are used to configure the tracker and can be set in
			// the isenseX.ini file 

			public /*ulong*/ uint  SyncState;   // 4 states: 0 - OFF, system is in free run 
			//           1 - ON, hardware genlock frequency is automatically determined
			//           2 - ON, hardware genlock frequency is specified by the user
			//           3 - ON, no hardware signal, lock to the user specified frequency  

			public float  SyncRate;    // Sync frequency - number of hardware sync signals per second, 
			// or, if SyncState is 3 - data record output frequency 

			public /*ulong*/ uint  SyncPhase;   // 0 to 100 per cent    

			public /*ulong*/ uint  Interface;   // hardware interface, read-only 

			public /*ulong*/ uint  UltTimeout;  // IS-900 only, ultrasonic timeout (sampling rate)
			public /*ulong*/ uint  UltVolume;   // IS-900 only, ultrasonic speaker volume
			public /*ulong*/ uint  dwReserved4;

			public float  FirmwareRev; // Firmware revision 
			public float  fReserved2;
			public float  fReserved3;
			public float  fReserved4;

			public /*bool*/ int   LedEnable;   // IS-900 only, blue led on the SoniDiscs enable flag
			public /*bool*/ int   bReserved2;
			public /*bool*/ int   bReserved3;
			public /*bool*/ int   bReserved4;	
		}
		
		///////////////////////////////////////////////////////////////////////////////
		// ISD_STATION_INFO_TYPE can only be used with IS Precision Series tracking devices.
		// If passed to ISD_SetStationConfig or ISD_GetStationConfig with InterTrax, FALSE is returned. 
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_STATION_INFO_TYPE
		{
			public /*ulong*/ uint   ID;             // unique number identifying a station. It is the same as that 
			// passed to the ISD_SetStationConfig and ISD_GetStationConfig   
			// functions and can be 1 to ISD_MAX_STATIONS 

			public /*bool*/ int    State;          // TRUE if ON, FALSE if OFF 

			public /*bool*/ int   Compass;        // 0, 1 or 2 for OFF, PARTIAL and FULL. Older versions of tracker
			// firmware supported only 0 and 1, that stood for ON or OFF. Please
			// use the new notation. This API will correctly interpret the settings.
			// Compass setting is ignored if station is configured for 
			// Fusion Mode operation. 

			public /*long*/ int    InertiaCube;    // InertiaCube associated with this station. If no InertiaCube is
			// assigned, this number is -1. Otherwise, it is a positive number
			// 1 to 4 

			public /*ulong*/ uint   Enhancement;    // levels 0, 1, or 2 
			public /*ulong*/ uint   Sensitivity;    // levels 1 to 4 
			public /*ulong*/ uint   Prediction;     // 0 to 50 ms 
			public /*ulong*/ uint   AngleFormat;    // ISD_EULER or ISD_QUATERNION 
		
			public /*bool*/ int TimeStamped;    // TRUE if time stamp is requested 
			public /*bool*/ int GetInputs;      // TRUE if button and joystick data is requested 
			public /*bool*/ int GetEncoderData; // TRUE if raw encoder data is requested 
			public /*bool*/ int bReserved1;     

			public /*ulong*/ uint   CoordFrame;     // coord frame in that position and orientation data is reported  

			// AccelSensitivity is used for 3-DOF tracking with InertiaCube2 only. It controls how fast 
			// tilt correction, using accelerometers, is applied. Valid values are 1 to 4, with 2 as default. 
			// Default is best for head tracking in static environment, with user sited. 
			// Level 1 reduces the amount of tilt correction during movement. While it will prevent any effect  
			// linear accelerations may have on pitch and roll, it will also reduce stability and dynamic accuracy. 
			// It should only be used in situations when sensor is not expected to experience a lot of movement.
			// Level 3 allows for more aggressive tilt compensation, appropriate when sensor is moved a lot, 
			// for example, when user is walking for long durations of time. 
			// Level 4 allows for even greater tilt corrections. It will reduce orientation accuracy by 
			// allowing linear accelerations to effect orientation, but increase stability. This level 
			// is appropriate for when user is running, or in other situations when sensor experiences 
			// a great deal of movement. 
			// AccelSensitivity is an advanced tuning parameter and is not used in the configuration files. 
			// The only way to set it is through the API, otherwise it will remain at default.

			public /*ulong*/ uint  AccelSensitivity; 

			public /*ulong*/ int   dwReserved3;    
			public /*ulong*/ int   dwReserved4;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
			public float[]         TipOffset;   // coordinates in station frame of the point being tracked
			
			public float		   fReserved4;

			public /*bool*/ int    GetCameraData;  // TRUE to get computed FOV, aperture, etc  
			public /*bool*/ int    GetAuxInputs;     
			public /*bool*/ int    bReserved3;
			public /*bool*/ int    bReserved4;
		}
			
		///////////////////////////////////////////////////////////////////////////////
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_STATION_STATE_TYPE
		{
			public byte    TrackingStatus;   // tracking status byte 
			public byte    NewData;          
			public byte    CommIntegrity;    // Communication integrity of wireless link 
			public byte    bReserved3;       // pack to 4 byte boundary

			[MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
			public float[] Orientation;		// Supports both Euler and Quaternion formats 
			
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
			public float[] Position;		// Always in meters 

			public float   TimeStamp;       // Seconds, reported only if requested 
		
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
			public bool[] ButtonState;		// Only if requested

			// Current hardware is limited to 10 channels, only 2 are used. 
			// The only device using this is the IS-900 wand that has a built-in
			// analog joystick. Channel 1 is x-axis rotation, channel 2 is y-axis
			// rotation 
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
			public short[] AnalogData;		// only if requested 

			[MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
			public byte[] AuxInputs;

			public /*long*/ int    lReserved2;
			public /*long*/ int    lReserved3;
			public /*long*/ int    lReserved4;

			public /*ulong*/ uint   dwReserved1;
			public /*ulong*/ uint   dwReserved2;
			public /*ulong*/ uint   dwReserved3;
			public /*ulong*/ uint   dwReserved4;

			public float   StillTime;      // IC2 and PC-Tracker only
			public float   fReserved2;
			public float   fReserved3;
			public float   fReserved4;
		}	

		///////////////////////////////////////////////////////////////////////////////
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_CAMERA_ENCODER_DATA_TYPE
		{
			public byte    TrackingStatus;     // tracking status byte 
			public byte    bReserved1;         // pack to 4 byte boundary 
			public byte    bReserved2;
			public byte    bReserved3;

			public uint   Timecode;           // timecode, not implemented yet 
			public int    ApertureEncoder;    // Aperture encoder counts, relative to last reset of power up 
			public int    FocusEncoder;       // Focus encoder counts 
			public int    ZoomEncoder;        // Zoom encoded counts 
			public uint   TimecodeUserBits;   // Time code user bits, not implemented yet 

			public float   Aperture;           // Computed Aperture value 
			public float   Focus;              // Computed focus value (mm), not implemented yet 
			public float   FOV;                // Computed vertical FOV value (degrees) 
			public float   NodalPoint;         // Nodal point offset due to zoom and focus (mm) 

			public int    lReserved1;
			public int    lReserved2;
			public int    lReserved3;
			public int    lReserved4;

			public uint   dwReserved1;
			public uint   dwReserved2;
			public uint   dwReserved3;
			public uint   dwReserved4;

			public float   fReserved1;
			public float   fReserved2;
			public float   fReserved3;
			public float   fReserved4;
		}
		///////////////////////////////////////////////////////////////////////////////
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_TRACKER_DATA_TYPE
		{
			// Marshalling a struct with a member that is an array of structs
			// is not supported by .NET Marshaller v1 according to a web post
			// we read!
			//[MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
			//public ISD_STATION_STATE_TYPE [] Station = new ISD_STATION_STATE_TYPE[ISD_MAX_STATIONS];

			// We use an inner class so that you can access the data the same
			// way its done in Java and C++, e.g., data.Station[0].Position[0]
			public STATION_ARRAY Station;

			// Inner class
			[StructLayout(LayoutKind.Sequential)]
			public struct STATION_ARRAY
			{
				// Do not change the order of these fields!  They have to
				// be laid out in memory the same way as the C structs.
				private ISD_STATION_STATE_TYPE Station0;
				private ISD_STATION_STATE_TYPE Station1;
				private ISD_STATION_STATE_TYPE Station2;
				private ISD_STATION_STATE_TYPE Station3;
				private ISD_STATION_STATE_TYPE Station4;
				private ISD_STATION_STATE_TYPE Station5;
				private ISD_STATION_STATE_TYPE Station6;
				private ISD_STATION_STATE_TYPE Station7;

				// Create a nice array indexer so we can say "data.Station[0]"
				public ISD_STATION_STATE_TYPE this[long index]
				{
					get 
					{
						switch(index)
						{
							case 0: return Station0;
							case 1: return Station1;
							case 2: return Station2;
							case 3: return Station3;
							case 4: return Station4;
							case 5: return Station5;
							case 6: return Station6;
							case 7: return Station7;
							default: Debug.Assert(false, "invalid index"); return Station0;
						}
					}

					set
					{
						switch(index)
						{
							case 0: Station0 = value; break;
							case 1: Station1 = value; break;
							case 2: Station2 = value; break;
							case 3: Station3 = value; break;
							case 4: Station4 = value; break;
							case 5: Station5 = value; break;
							case 6: Station6 = value; break;
							case 7: Station7 = value; break;
							default: Debug.Assert(false, "invalid index"); break;
						}
					}
				}
			}
		}	

		///////////////////////////////////////////////////////////////////////////////
		[StructLayout(LayoutKind.Sequential)]
		public struct ISD_CAMERA_DATA_TYPE
		{
			//public ISD_CAMERA_ENCODER_DATA_TYPE [] Station0 = new ISD_CAMERA_ENCODER_DATA_TYPE[ISD_MAX_STATIONS];
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera0;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera1;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera2;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera3;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera4;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera5;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera6;
			public ISD_CAMERA_ENCODER_DATA_TYPE Camera7;
		}	

		///////////////////////////////////////////////////////////////////////////////

		#endregion
		
		#region DLL Function Marshaling
		
		// Returns -1 on failure. To detect tracker automatically specify 0 for commPort.
		// hParent parameter to ISD_OpenTracker is optional and should only be used if 
		// information screen or tracker configuration tools are to be used when available 
		// in the future releases. If you would like a tracker initialization window to be 
		// displayed, specify TRUE value for the infoScreen parameter (not implemented in
		// this release).
		// returns ISD_TRACKER_HANDLE (IntPtr) that is a handle for that particular tracker
		[DllImport ("isense.dll")]
		public static extern IntPtr ISD_OpenTracker(
			IntPtr hParent,
			ulong commPort,
			bool infoScreen,	// might have to be int instead of bool
			bool verbose);		// might have to be int instead of bool

		[DllImport ("isense.dll")]
		public static extern uint ISD_OpenAllTrackers(
			int hParent,
			out IntPtr handle,	// handle is ISD_TRACKER_HANDLE==IntPtr
			bool infoScreen,	// might have to be int instead of bool
			bool verbose);		// might have to be int instead of bool

		// This function call deinitializes the tracker, closes communications port and 
		// frees the resources associated with this tracker. If 0 is passed, all currently
		// open trackers are closed. When last tracker is closed, program frees the DLL. 
		[DllImport ("isense.dll")]
		public static extern bool ISD_CloseTracker(
			IntPtr handle);		// handle is ISD_TRACKER_HANDLE==IntPtr

		// When used with IS Precision Series (IS-300, IS-600, IS-900, IS-1200) tracking devices 
		// this function call will set genlock synchronization  parameters, all other fields 
		// in the ISD_TRACKER_INFO_TYPE structure are for information purposes only 
		[DllImport ("isense.dll")]
		public static extern bool ISD_SetTrackerConfig(
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out ISD_TRACKER_INFO_TYPE Tracker, 
			bool verbose);		// might have to be int instead of bool

		// Get RecordsPerSec and KBitsPerSec without requesting genlock settings from the tracker.
		// Use this instead of ISD_GetTrackerConfig to prevent your program from stalling while
		// waiting for the tracker response. 
		[DllImport ("isense.dll")]
		public static extern bool ISD_GetCommInfo( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out ISD_TRACKER_INFO_TYPE Tracker);

		// Configure station as specified in the ISD_STATION_INFO_TYPE structure. Before 
		// this function is called, all elements of the structure must be assigned a value. 
		// stationID is a number from 1 to ISD_MAX_STATIONS. Should only be used with
		// IS Precision Series tracking devices, not valid for InterTrax.  
		[DllImport ("isense.dll")]
		public static extern bool ISD_SetStationConfig( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			ref ISD_STATION_INFO_TYPE Station, 
			short stationID,	// originally WORD
			bool verbose);		// might have to be int instead of bool

		// Fills the ISD_STATION_INFO_TYPE structure with current settings. Function
		// requests configuration records from the tracker and waits for the response.
		// If communications are interrupted, it will stall for several seconds while 
		// attempting to recover the settings. Should only be used with IS Precision Series 
		// tracking devices, not valid for InterTrax.
		// stationID is a number from 1 to ISD_MAX_STATIONS 
		[DllImport ("isense.dll")]
		public static extern bool ISD_GetStationConfig(
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out ISD_STATION_INFO_TYPE Station,
			short stationID,	// originally WORD
			bool verbose);		// might have to be int instead of bool

		// Not supported on UNIX in this release
		// When a tracker is first opened, library automatically looks for a configuration
		// file in current directory of the application. File name convention is
		// isenseX.ini where X is a number, starting at 1, identifying one tracking 
		// system in the order of initialization. This function provides for a way to
		// manually configure the tracker using a different configuration file.
		[DllImport ("isense.dll")]
		public static extern bool ISD_ConfigureFromFile(
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out string path,	// originally (char *) probably won't work in current translation
			bool verbose);		// might have to be int instead of bool

		// Get data from all configured stations. Data is places in the ISD_TRACKER_DATA_TYPE
		// structure. Orientation array may contain Euler angles or Quaternions, depending
		// on the settings of the AngleFormat field of the ISD_STATION_INFO_TYPE structure.
		// TimeStamp is only available if requested by setting TimeStamped field to TRUE.
		[DllImport ("isense.dll")]
		public static extern bool ISD_GetData(
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out ISD_TRACKER_DATA_TYPE data);

		// Get camera encode and other data for all configured stations. Data is places in 
		// the ISD_CAMERA_DATA_TYPE structure. This function does not service serial port, so
		// ISD_GetData must be called prior to this.
		[DllImport ("isense.dll")]
		public static extern bool ISD_GetCameraData( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntTr
			out ISD_CAMERA_DATA_TYPE Data);

		// Reset heading to zero 
		[DllImport ("isense.dll")]
		public static extern bool ISD_ResetHeading( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			short stationID);	// originally WORD

		// Works with all IS-X00 series products and InertiaCube2, and InterTraxLC.
		// For InterTrax30 and InterTrax2 behaves like ISD_ResetHeading.
		// Boresight station using specific reference angles. This is useful when
		// you need to apply a specific offset to system output. For example, if
		// a sensor is mounted at 40 degrees relative to the HMD, you can 
		// enter 0, 40, 0 to get the system to output zero when HMD is horizontal.
		[DllImport ("isense.dll")]
		public static extern bool ISD_BoresightReferenced( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			short stationID,	// originally WORD
			float yaw,
			float pitch, 
			float roll);

		// Works with all IS-X00 series products and InertiaCube2, and InterTraxLC.
		// For InterTrax30 and InterTrax2 behaves like ISD_ResetHeading.
		// Boresight, or unboresight a station. If 'set' is TRUE, all angles
		// are reset to zero. Otherwise, all boresight settings are cleared,
		// including those set by ISD_ResetHeading and ISD_BoresightReferenced
		[DllImport ("isense.dll")]
		public static extern bool ISD_Boresight( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			short stationID,	// originally WORD
			bool setVal);		// might have to be int instead of bool

		// Send a configuration script to the tracker. Script must consist of valid 
		// commands as described in the interface protocol. Commands in the script 
		// should be terminated by the New Line character '\n'. Line Feed character '\r' 
		// is added by the function and is not required. 
		[DllImport ("isense.dll")]
		public static extern bool ISD_SendScript( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			out string script);	// originally (char *) probably won't work in current translation

		// Sends up to 4 output bytes to the auxiliary interface of the station  
		// specified. The number of bytes should match the number the auxiliary outputs
		// interface is set up to expect. If too many are specified, the extra bytes 
		// are ignored. 
		[DllImport ("isense.dll")]
		public static extern bool ISD_AuxOutput( 
			IntPtr handle,		// handle is ISD_TRACKER_HANDLE==IntPtr
			short stationID,	// originally WORD
			out byte AuxOutput, 
			short length);		// originally WORD

		// Platform independent time
		[DllImport ("isense.dll")]
		public static extern float ISD_GetTime();

		#endregion
	}
}
