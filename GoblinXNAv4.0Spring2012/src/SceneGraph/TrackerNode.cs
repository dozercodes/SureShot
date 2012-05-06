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
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;

using GoblinXNA.Device;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Util;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that handles 6DOF tracking devices.
    /// </summary>
    public class TrackerNode : BranchNode
    {
        #region Member Fields
        private Matrix worldTransform;
        private ISmoother smoother;
        private IPredictor predictor;
        private String deviceIdentifier;
        protected bool smooth;
        protected bool predict;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a tracker node with the given 6DOF device identifier (see InputMapper class
        /// for the identifier strings).
        /// </summary>
        /// <param name="name">The name of this node</param>
        /// <param name="deviceIdentifier">The 6DOF device identifier (see InputMapper class)</param>
        /// <exception cref="GoblinException">If the given device identifier is not a 6DOF device</exception>
        public TrackerNode(String name, String deviceIdentifier) :
            base(name)
        {
            worldTransform = Matrix.Identity;
            this.deviceIdentifier = deviceIdentifier;
            if (!InputMapper.Instance.Contains6DOFInputDevice(deviceIdentifier))
                throw new GoblinException(deviceIdentifier + " is not recognized. Only 6DOF devices " +
                    "are allowed to be used with TrackerNode.");

            smoother = null;
            predictor = null;
            smooth = false;
            predict = false;
        }

        /// <summary>
        /// Creates a tracker node with the given 6DOF device identifier (see InputMapper class
        /// for the identifier strings).
        /// </summary>
        /// <param name="deviceIdentifier">The 6DOF device identifier (see InputMapper class)</param>
        public TrackerNode(String deviceIdentifier) : this("", deviceIdentifier) { }

#if WINDOWS
        public TrackerNode() : this(GenericInput.Instance.Identifier) { }
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the 6DOF device identifier (see InputMapper class for the identifier strings)
        /// </summary>
        /// <exception cref="GoblinException">If the given device identifier is not a 6DOF device</exception>
        public String DeviceIdentifier
        {
            get { return deviceIdentifier; }
            set
            {
                if (!InputMapper.Instance.Contains6DOFInputDevice(value))
                    throw new GoblinException(value + " is not recognized. Only 6DOF devices " +
                        "are allowed to be used with TrackerNode.");
                deviceIdentifier = value;
            }
        }

        /// <summary>
        /// Gets or sets the smoothing filter to apply to the transformation returned from the
        /// 6DOF device.
        /// </summary>
        public ISmoother Smoother
        {
            get { return smoother; }
            set
            {
                smoother = value;
                smooth = (smoother != null);
            }
        }

        /// <summary>
        /// Gets or sets the prediction filter to apply to the transformation returned from the
        /// 6DOF device.
        /// </summary>
        public IPredictor Predictor
        {
            get { return predictor; }
            set
            {
                predictor = value;
                predict = (predictor != null);
            }
        }

        /// <summary>
        /// Gets the tranformation of the 6DOF tracker.
        /// </summary>
        public Matrix WorldTransformation
        {
            get 
            {
                Vector3 p = new Vector3();
                Quaternion q = Quaternion.Identity;

                if (smooth || predict)
                {
                    Vector3 scale;
                    InputMapper.Instance.GetWorldTransformation(deviceIdentifier).Decompose(out scale, out q, out p);
                }

                if (smooth)
                    smoother.FilterMatrix(ref p, ref q, out worldTransform);
                else
                    worldTransform = InputMapper.Instance.GetWorldTransformation(deviceIdentifier);

                if (predict)
                    predictor.UpdatePredictor(ref p, ref q);

                return worldTransform;
            }
        }
        #endregion

        #region Override Methods

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("DeviceIdentifier", deviceIdentifier);
            if (smooth)
                xmlNode.AppendChild(smoother.Save(xmlDoc));
            if (predict)
                xmlNode.AppendChild(predictor.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("DeviceIdentifier"))
                deviceIdentifier = xmlNode.GetAttribute("DeviceIdentifier");

            if (!InputMapper.Instance.Contains6DOFInputDevice(deviceIdentifier))
                throw new GoblinException(deviceIdentifier + " is not recognized. Only 6DOF devices " +
                    "are allowed to be used with TrackerNode.");

            foreach (XmlElement xmlChild in xmlNode.ChildNodes)
            {
                Type classType = Type.GetType(xmlChild.Name);
                try
                {
                    Smoother = (ISmoother)Activator.CreateInstance(classType);
                    smoother.Load(xmlChild);
                }
                catch (Exception)
                {
                    try
                    {
                        Predictor = (IPredictor)Activator.CreateInstance(classType);
                        predictor.Load(xmlChild);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
#endif

        #endregion
    }
}
