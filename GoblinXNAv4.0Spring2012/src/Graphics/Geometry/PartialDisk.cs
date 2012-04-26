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
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GoblinXNA.Graphics.Geometry
{
    /// <summary>
    /// A partial disk geometry primitive constructed with CustomMesh
    /// </summary>
    public class PartialDisk : Disk
    {
        #region Constructors

        /// <summary>
        /// Creates a partial disk on the Y = 0 plane. A partial disk is similar to a 
        /// full disk, except that only the subset of the disk from start through start + sweep is
        /// included (where 0 degrees is along the +Y axis, 90 degrees along the +X axis)
        /// </summary>
        /// <param name="inner">Specifies the inner radius of the partial disk (can be 0).</param>
        /// <param name="outer">Specifies the outer radius of the partial disk. Must be greater
        /// than the 'inner' radius.</param>
        /// <param name="slices">Specifies the number of subdivisions around the Z axis. Must be
        /// greater than 2.</param>
        /// <param name="start">Specifies the starting angle, in radians, of the disk portion.</param>
        /// <param name="sweep">Specifies the sweep angle, in radians, of the disk portion.</param>
        /// /// <param name="twoSided">Specifies whether to render both front and back side</param>
        public PartialDisk(float inner, float outer, int slices, double start, double sweep, bool twoSided)
            : base(inner, outer, slices, start, sweep, twoSided)
        {
            customShapeParameters = inner + ", " + outer + ", " + slices + ", " + start + ", " +
                sweep + ", " + twoSided;
        }

        public PartialDisk(params String[] xmlParams)
            : base(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]), int.Parse(xmlParams[2]),
                double.Parse(xmlParams[3]), double.Parse(xmlParams[4]), bool.Parse(xmlParams[5]))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2] + ", "
                + xmlParams[3] + ", " + xmlParams[4] + ", " + xmlParams[5];
        }

        #endregion
    }
}
