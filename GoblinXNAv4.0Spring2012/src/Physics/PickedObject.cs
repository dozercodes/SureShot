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

using Microsoft.Xna.Framework;

namespace GoblinXNA.Physics
{
    /// <summary>
    /// A class that represents a picked object returned by a 3D object selection method.
    /// </summary>
    public class PickedObject : IComparable<PickedObject>
    {
        #region Member Fields
        private IPhysicsObject pickedObject;
        private float intersectParam;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an instance that holds a the picked physics object with the intersection
        /// parameter.
        /// </summary>
        /// <param name="pickedObject"></param>
        /// <param name="intersectParam"></param>
        public PickedObject(IPhysicsObject pickedObject, float intersectParam)
        {
            this.pickedObject = pickedObject;
            this.intersectParam = intersectParam;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the picked IPhysicsObject.
        /// </summary>
        public IPhysicsObject PickedPhysicsObject
        {
            get { return pickedObject; }
        }

        /// <summary>
        /// Gets the intersection parameter with value ranging from 0.0f (near point) to 1.0f (far point).
        /// </summary>
        /// <remarks>
        /// For example, 0.5f means the intersection point was right in the middle of near point and far point cast ray.
        /// To get the picked point, you can simply do pickedPoint = (nearPoint + (farPoint - nearPoint) * intersectParam)
        /// </remarks>
        public float IntersectParam
        {
            get { return intersectParam; }
        }
        #endregion

        #region IComparable<PickedObject> Members

        /// <summary>
        /// Compares which picked object is closer to the viewer in the 3D scene.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PickedObject other)
        {
            if (intersectParam > other.IntersectParam)
                return 1;
            else if (intersectParam == other.IntersectParam)
                return 0;
            else
                return -1;
        }

        #endregion
    }
}
