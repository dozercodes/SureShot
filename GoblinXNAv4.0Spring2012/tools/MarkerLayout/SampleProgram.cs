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
 * Authors: Ohan Oda (ohan@cs.columbia.edu) 
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace MarkerLayout
{
    /// <summary>
    /// This program demonstrates how to use MarkerLayout class to generate marker array
    /// images and configuration files automatically.
    /// </summary>
    public class SampleProgram
    {
        static void Main(string[] args)
        {
            GenerateALVARLayout();
            //GenerateNyARToolkitLayout();
            //GenerateFromXML();
        }

        /// <summary>
        /// Generates a marker layout image and configuration file to be used with ALVAR
        /// tracking library.
        /// </summary>
        public static void GenerateALVARLayout()
        {
            // Create a layout manager with size 400x400 pixels, and actual marker size of 9 inches
            LayoutManager layout = new LayoutManager(400, 400, 9);

            // Create arrays of marker IDs we want to layout
            // NOTE: Please use the SampleMarkerCreator project that comes with the ALVAR
            // package to generate the raw marker images
            int[] array1 = { 0, 1 };
            int[] array2 = { 2, 3 };

            int[][] marker_arrays = new int[2][];
            marker_arrays[0] = array1;
            marker_arrays[1] = array2;

            // Layout the markers
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                    layout.AddMarker(marker_arrays[j][i], new Point(60 + j * 172, 60 + i * 172),
                        "raw_markers/ALVAR/MarkerData_" + marker_arrays[j][i] + ".png");
            }

            // Set the (0, 0) point in the configuration file to be at (60, 60) in the layout image
            // In this case, it is at the left-upper corner of marker ID 0. 
            layout.ConfigCenter = new Point(60, 60);

            // Compile the layout
            layout.Compile();

            // Output the layout image in gif format
            layout.OutputImage("ALVARArray.gif", ImageFormat.Gif);

            // Output the configuration file
            layout.OutputConfig("ALVARConfig.xml", LayoutManager.ConfigType.ALVAR);

            // Disposes the layout
            layout.Dispose();
        }

        /// <summary>
        /// Generates a marker layout image and configuration file to be used with NyARToolkit
        /// tracking library.
        /// </summary>
        public static void GenerateNyARToolkitLayout()
        {
            // Create a layout manager with size 400x400 pixels, and actual marker size of 9 inches
            LayoutManager layout = new LayoutManager(400, 400, 9);

            List<KeyValuePair<string, string>> generalMarkerInfo = new List<KeyValuePair<string,string>>();
            generalMarkerInfo.Add(new KeyValuePair<string,string>("patternWidth", "16"));
            generalMarkerInfo.Add(new KeyValuePair<string,string>("patternHeight", "16"));
            generalMarkerInfo.Add(new KeyValuePair<string,string>("confidence", "0.7"));

            int[] array1 = { 0, 1 };
            int[] array2 = { 2, 3 };

            int[][] marker_arrays = new int[2][];
            marker_arrays[0] = array1;
            marker_arrays[1] = array2;

            // Layout the markers
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    int id = marker_arrays[j][i];

                    List<KeyValuePair<string, string>> markerInfo = new List<KeyValuePair<string, string>>();
                    markerInfo.Add(new KeyValuePair<string, string>("patternName", "marker" + id + ".patt"));
                    markerInfo.AddRange(generalMarkerInfo);

                    layout.AddMarker(id, new Point(60 + j * 172, 60 + i * 172),
                        "raw_markers/ALVAR/MarkerData_" + id + ".png", markerInfo);
                }
            }
            
            // Set the (0, 0) point in the configuration file to be at (60, 60) in the layout image
            // In this case, it is at the left-upper corner of marker ID 0. 
            layout.ConfigCenter = new Point(60, 60);

            // Compile the layout
            layout.Compile();

            // Output the layout image in gif format
            layout.OutputImage("NyARToolkitArray.gif", ImageFormat.Gif);

            // Output the configuration file
            layout.OutputConfig("NyARToolkitConfig.xml", LayoutManager.ConfigType.NyARToolkit);

            // Disposes the layout
            layout.Dispose();
        }

        /// <summary>
        /// Generates the same marker layout in GenerateALVARLayout using an XML file.
        /// </summary>
        public static void GenerateFromXML()
        {
            // Create a layout manager from an XML file
            //LayoutManager layout = new LayoutManager("SampleALVARLayout.xml");
            LayoutManager layout = new LayoutManager("SampleNyARToolkitLayout.xml");

            // Disposes the layout
            layout.Dispose();
        }
    }
}
