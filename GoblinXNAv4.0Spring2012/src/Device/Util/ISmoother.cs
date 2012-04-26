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
using System.Xml;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Device.Util
{
    /// <summary>
    /// An interface for matrix smoothing/filtering classes.
    /// </summary>
    public interface ISmoother
    {
        /// <summary>
        /// Gets a filtered matrix based on the given position and orientation.
        /// </summary>
        /// <param name="p">The original position</param>
        /// <param name="q">The original rotation</param>
        /// <param name="result">A smoothed/filterd matrix</param>
        void FilterMatrix(ref Vector3 p, ref Quaternion q, out Matrix result);

        /// <summary>
        /// Resets the smoothing history.
        /// </summary>
        void ResetHistory();

#if !WINDOWS_PHONE
        /// <summary>
        /// Saves the information of this smoother to an XML element.
        /// </summary>
        /// <param name="xmlDoc">The XML document to be saved.</param>
        /// <returns></returns>
        XmlElement Save(XmlDocument xmlDoc);

        /// <summary>
        /// Loads the information of this smoother from an XML element.
        /// </summary>
        /// <param name="xmlNode"></param>
        void Load(XmlElement xmlNode);
#endif
    }
}
