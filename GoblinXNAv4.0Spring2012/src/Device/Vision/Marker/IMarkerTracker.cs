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

using Microsoft.Xna.Framework;

using GoblinXNA.Device.Capture;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A marker tracker interface. Any marker tracker class should implement this interface.
    /// </summary>
    public interface IMarkerTracker
    {
        /// <summary>
        /// Gets whether the tracker has been initialized.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Gets the camera projection matrix used for this marker tracker.
        /// </summary>
        Matrix CameraProjection { get; }

        /// <summary>
        /// Gets or sets the near clipping plane used to compute CameraProjection.
        /// </summary>
        float ZNearPlane { get; set; }

        /// <summary>
        /// Gets or sets the far clipping plane used to compute CameraProjection.
        /// </summary>
        float ZFarPlane { get; set; }

        /// <summary>
        /// Gets or sets whether to perform marker tracking. For optimization purpose, if you don't need
        /// marker tracking while in certain state, it's best to set this to false, and when you need it
        /// you can set back to true. The default value is true.
        /// </summary>
        bool EnableTracking { get; set; }

        /// <summary>
        /// Initilizes the marker tracker with a set of configuration parameters.
        /// </summary>
        /// <param name="configs">A set of configuration parameters</param>
        void InitTracker(params Object[] configs);

        /// <summary>
        /// Associates a marker with an identifier so that the identifier can be used to find this
        /// marker after processing the image. 
        /// </summary>
        /// <param name="markerConfigs">A set of parameters that identifies a maker. (e.g., for
        /// ARTag, this parameter would be the marker array name or marker ID)</param>
        /// <returns>An identifier for this marker object</returns>
        Object AssociateMarker(params Object[] markerConfigs);

#if WINDOWS
        /// <summary>
        /// Processes the video image captured from an initialized video capture device. 
        /// </summary>
        /// <param name="captureDevice">An initialized video capture device</param>
        /// <param name="imagePtr">A pointer that contains an image to be processed</param>
        void ProcessImage(IVideoCapture captureDevice, IntPtr imagePtr);
#else
        /// <summary>
        /// Processes the video image captured from an initialized video capture device. 
        /// </summary>
        /// <param name="captureDevice">An initialized video capture device</param>
        /// <param name="imagePtr"></param>
        void ProcessImage(IVideoCapture captureDevice, byte[] imagePtr);
#endif

        /// <summary>
        /// Checks whether a marker identified by 'markerID' is found in the processed image
        /// after calling ProcessImage(...) method.
        /// </summary>
        /// <param name="markerID">An ID associated with a marker returned from AssociateMarker(...)
        /// method.</param>
        /// <returns>A boolean value representing whether a marker was found</returns>
        bool FindMarker(Object markerID);

        /// <summary>
        /// Gets the pose transformation of the found marker after calling the FindMarker(...) method.
        /// </summary>
        /// <remarks>
        /// This method should be called if and only if FindMarker(...) method returned true for
        /// the marker you're looking for. 
        /// </remarks>
        /// <returns>The pose transformation of a found marker</returns>
        Matrix GetMarkerTransform();

        /// <summary>
        /// Disposes this marker tracker.
        /// </summary>
        void Dispose();
    }
}
