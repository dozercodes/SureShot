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
 * Authors: John Waugh 
 *          Mark Eaddy 
 *          Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using GoblinXNA;
using GoblinXNA.Helpers;

namespace GoblinXNA.Device.InterSense
{
	/// <summary>
	/// Helper class for accessing the Poll() method in UpdClient
	/// </summary>
	//[Concern("Network")]
	public class MyUdpClient : UdpClient
	{
		public MyUdpClient(string hostname, int port)
			: base(hostname, port)
		{ }

		public bool PollForData(int secs)
		{
			return Client.Poll(secs * 1000000, SelectMode.SelectRead);
		}
	}

	/// <summary>
	/// Summary description for InterSenseSocket.
	/// </summary>
	//[Concern("Input.Network")]
	public class InterSenseSocket : IDisposable
	{
		public InterSenseSocket()
		{
			udp = null;
			sourcePoint = null;
		}

		public void Dispose()
		{
			Close();
		}

		//Connect to the data source on the given socket
		public bool Connect(string server_hostname, int server_port)
		{
			hostname = server_hostname;

			//[Concern("Logging")]
			/*MyTrace.Log(TraceLevel.Info, "Connecting to {0}:{1}",
				server_hostname, server_port);*/

			//[Concern("EH")]
			try
			{
				udp = new MyUdpClient(server_hostname, server_port);

                IPHostEntry host = Dns.GetHostEntry(server_hostname);
				//[Concern("EC")]
				if(host.AddressList.Length <= 0)
				{
					//[Concern("Logging")]
					/*MyTrace.Log(TraceLevel.Warning,
						"Could not find IP address of server '{0}'.",
						server_hostname);*/
					Close();
					return false;
				}

				sourcePoint = new System.Net.IPEndPoint(host.AddressList[0],server_port);
				//[Concern("Logging")]
				Log.Write("SourcePoint: "+sourcePoint.ToString());

				// NOTE: We should send a test message here so that we know right away if
				// ISenseServer is running (reduces delay).

				return true;
			}
			catch(SocketException ex)
			{
				// The help docs say that ex.ErrorCode will be the socket error code
				// but its wrong. The value should be WSAECONNRESET but instead its E_FAIL.
				//[Concern("Logging")]
				if (ex.Message.IndexOf("forcibly") > -1)
				{
					// ISenseServer died (CTRL^C)
					/*MyTrace.Log(TraceLevel.Warning,
						"InterSenseSocket::Connect() - ISenseServer '{0}' appears to have died.",
						server_hostname);*/
				}
				else
				{
					/*MyTrace.Log(TraceLevel.Info, "InterSenseSocket::Connect() - Error creating udp Connection to {0}:{1} : {2}",
						server_hostname,
						server_port,
						ex.ToString());*/
				}

				Close();
				return false;
			}
			catch(Exception)
			{
				//[Concern("Logging")]
				/*MyTrace.Log(TraceLevel.Info, "InterSenseSocket::Connect() - Error creating udp Connection to {0}:{1} : {2}",
					server_hostname,
					server_port,
					ex.ToString());*/
				Close();
				return false;
			}
		}

		public bool IsConnected()
		{
			return udp != null;
		}

		public void Close()
		{
			if(udp!=null)
			{
				udp.Close();
				udp = null;
			}
		}

		public bool GetData(InterSense.StationArray stationArray, out ISDllBridge.ISD_TRACKER_DATA_TYPE dataISense)
		{
			dataISense = new ISDllBridge.ISD_TRACKER_DATA_TYPE();

			for(int i=0;i<ISDllBridge.ISD_MAX_STATIONS;i++)
			{
				ISDllBridge.ISD_STATION_STATE_TYPE isdStation = new ISDllBridge.ISD_STATION_STATE_TYPE();
				isdStation.Position = new float[3];
				isdStation.Orientation = new float[4];
				dataISense.Station[i] = isdStation;
			}

			//[Concern("EC")]
			if(udp==null)
				return false;

			string str = "ISD_GetData -station ";
			//first, send our request
			int activeCount = 0;	//how many stations are active
			for(int i=0;i<ISDllBridge.ISD_MAX_STATIONS;i++)
			{
				if(!stationArray.isActive(i))
					continue;
				activeCount++;
				if (activeCount > 1)
					str += ",";
				str += (i+1).ToString();
			}
			if(activeCount<1)
				return true;

			//should have our proper request string now
			//need to turn it into bytes (there must be a better way to do this in C#)
			byte[] bytes = new byte[str.Length];
			for(int i=0;i<str.Length;i++)
				bytes[i] = Convert.ToByte(str[i]);

			//send the command to get 6DOF information from the server (the server actually communicates with the device)
			//[Concern("EH")]
			try
			{
				udp.Send(bytes,bytes.Length);
			}
			catch(Exception ex)
			{
				HandleException(ex);
				return false;
			}

			//now read it back until we fill up everything
			//get the bytes
			//[Concern("EH")]
			try
			{
				const int timeout = 1; // secs
				//[Concern("EC")]
				if (!udp.PollForData(timeout))
				{
					//[Concern("Logging")]
					/*MyTrace.Log(TraceLevel.Warning,
						"Timed out waiting for station data. (Timeout: {0} secs).",
						timeout);*/
					return false;
				}

				bytes = udp.Receive(ref sourcePoint);
			}
			catch(Exception ex)
			{
				HandleException(ex);
				return false;
			}

			//parse it
			str = Encoding.ASCII.GetString(bytes);

			string[] stationMsgs = str.Split(splitSemicolon);

			//[Concern("Logging")]
			if (stationMsgs.Length != activeCount)
			{
				/*MyTrace.Log(TraceLevel.Warning,
					"Did not receive data for all stations. Requested {0} Received {1}",
					activeCount, stationMsgs.Length);*/
			}

			foreach(string stationMsg in stationMsgs)
			{
				string[] toks = stationMsg.Split(splitSpace);
			
				int station = Int32.Parse(toks[0].Split(splitEquals)[1])-1;

				dataISense.Station[station].Position[0] = (float)Double.Parse(toks[1].Split(splitEquals)[1]);
				dataISense.Station[station].Position[1] = (float)Double.Parse(toks[2].Split(splitEquals)[1]);
				dataISense.Station[station].Position[2] = (float)Double.Parse(toks[3].Split(splitEquals)[1]);
				
				dataISense.Station[station].Orientation[0] = (float)Double.Parse(toks[4].Split(splitEquals)[1]);
				dataISense.Station[station].Orientation[1] = (float)Double.Parse(toks[5].Split(splitEquals)[1]);
				dataISense.Station[station].Orientation[2] = (float)Double.Parse(toks[6].Split(splitEquals)[1]);
			}

			hasGottenData = true;

			return true;
		}

		//[Concern("EH")]
		void HandleException(Exception ex)
		{
			//[Concern("Logging")]
			if (ex is SocketException)
			{
				SocketException sex = (SocketException) ex;

				// The help docs say that ex.ErrorCode will be the socket error code
				// but its wrong. The value should be WSAECONNRESET but instead its E_FAIL.
				if (sex.Message.IndexOf("forcibly") > -1)
				{
					if (hasGottenData)
					{
						// ISenseServer died (CTRL^C)
						/*MyTrace.Log(TraceLevel.Warning, 
							"InterSenseSocket::GetData() - ISenseServer '{0}' appears to have died.",
							hostname);*/
					}
					else
					{
						// ISenseServer died (CTRL^C)
						/*MyTrace.Log(TraceLevel.Warning, 
							"InterSenseSocket::GetData() - ISenseServer not running on '{0}'.",
							hostname);*/
					}
				}
				else
				{
					/*MyTrace.Log(TraceLevel.Info, "InterSenseSocket::GetData() - Socket for '(0}' was closed: {1}",
						hostname,
						sex.ToString());*/
				}
			}
			else        
			{
				/*MyTrace.Log(TraceLevel.Error, "InterSenseSocket::GetData() - Socket error for host '{0}': {1}",
					hostname,
					ex.ToString());*/
			}

			Close();
		}

		/*
		 * member data
		 * */
		protected	MyUdpClient	udp;			//this is what we use to send and receive data (has destination data in it)
		protected	IPEndPoint	sourcePoint;	//this identifies where we get data from
		protected	string		hostname;		//Keep track of original hostname for error msgs
		protected	bool		hasGottenData = false;
		//[Concern("Opt")]
		char[]		splitEquals = "=".ToCharArray();
		//[Concern("Opt")]
		char[]		splitSpace = " ".ToCharArray();
		//[Concern("Opt")]
		char[]		splitSemicolon = ";".ToCharArray();
	}
}
