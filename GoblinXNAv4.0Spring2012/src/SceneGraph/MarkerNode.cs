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
using System.ComponentModel;

using Microsoft.Xna.Framework;

using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that defines an optically tracked fiducial marker.
    /// 
    /// Any nodes added below a MarkerNode with WorldTranformation properties will be affected by the
    /// transformation returned by the marker tracker including GeometryNode, ParticleNode, SoundNode,
    /// CameraNode, and LightNode.
    /// </summary>
    public class MarkerNode : BranchNode
    {
        #region Member Fields

        protected object markerID;
        protected int maxDropouts;
        protected int dropout;
        protected bool found;
        protected bool optimize;
        protected ISmoother smoother;
        protected IPredictor predictor;
        protected IMarkerTracker tracker;
        protected Matrix prevMatrix;
        protected Matrix worldTransformation;
        protected bool smooth;
        protected bool predict;
        protected float predictionTime;

        protected Object[] markerConfigs;

        protected Matrix inverseCameraView;
        protected bool isInverseViewSet;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node that is tracked by fiducial marker (can be either an array or
        /// a single marker) and updated automatically.
        /// </summary>
        /// <param name="name">Name of this marker node</param>
        /// <param name="tracker">A marker tracker used to track this fiducial marker</param>
        /// <param name="markerConfigs">A list of configs that specify the fiducial marker 
        /// (can be either an array or a single marker) to look for</param>
        public MarkerNode(String name, IMarkerTracker tracker, params Object[] markerConfigs)
            : base(name)
        {
            this.tracker = tracker;
            if (tracker != null)
            {
                markerID = tracker.AssociateMarker(markerConfigs);
                this.markerConfigs = markerConfigs;
            }
            found = false;
            maxDropouts = 5;
            prevMatrix = Matrix.Identity;
            dropout = 0;
            optimize = false;

            smoother = null;
            predictor = null;
            smooth = false;
            predict = false;
            predictionTime = 0;

            inverseCameraView = Matrix.Identity;
        }

        /// <summary>
        /// Creates a node that is tracked by fiducial marker (can be either an array or a single
        /// marker) and updated automatically.
        /// </summary>
        /// <param name="tracker">A marker tracker used to track this fiducial marker</param>
        /// <param name="markerConfigs">A list of configs that specify the fiducial marker 
        /// (can be either an array or a single marker) to look for</param>
        public MarkerNode(IMarkerTracker tracker, params Object[] markerConfigs)
            :
            this("", tracker, markerConfigs) { }

        public MarkerNode() : this(null) { }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the marker ID returned by the marker tracker library.
        /// </summary>
        public object MarkerID
        {
            get { return markerID; }
        }
        
        /// <summary>
        /// Gets or sets the smoother used to filter the matrix returned by the optical marker
        /// tracker.
        /// </summary>
        public virtual ISmoother Smoother
        {
            get { return smoother; }
            set 
            { 
                smoother = value; 
                smooth = (smoother != null); 
            }
        }

        /// <summary>
        /// Gets or sets the prediction filter to apply to the transformation returned by the
        /// optical marker tracker.
        /// </summary>
        public virtual IPredictor Predictor
        {
            get { return predictor; }
            set 
            { 
                predictor = value;
                predict = (predictor != null);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of dropouts. If it fails to detect the marker within
        /// 'MaxDropouts' frames, the WorldTransform becomes an empty matrix. Set this value to
        /// -1 number if you want to keep the last detected transformation indefinitely when the
        /// marker is not found.
        /// </summary>
        /// <remarks>
        /// Dropout count is used to make marker tracking more stable. For example, if MaxDropouts
        /// is set to 5, then even if the marker is not detected for 5 frames, it will use the previously
        /// detected transformation.
        /// </remarks>
        /// <seealso cref="WorldTransformation"/>
        public virtual int MaxDropouts
        {
            get { return maxDropouts; }
            set { maxDropouts = value; }
        }

        /// <summary>
        /// Gets whether the marker is detected.
        /// </summary>
        public bool MarkerFound
        {
            get { return found; }
        }

        /// <summary>
        /// Gets or sets the inverse transform of the current camera's view matrix. Set this property
        /// if your camera's view matrix is other than an Identity matrix. Setting this properly will 
        /// ensure that your geometry attached to this marker node will appear on top of the marker.
        /// </summary>
        public Matrix InverseCameraView
        {
            get { return inverseCameraView; }
            set
            {
                inverseCameraView = value;
                isInverseViewSet = true;
            }
        }

        /// <summary>
        /// Gets the transformation of the detected marker. 
        /// </summary>
        /// <remarks>
        /// If no marker is detected after MaxDropouts, then transformation matrix with 
        /// all zero values is returned.
        /// </remarks>
        public Matrix WorldTransformation
        {
            get { return worldTransformation; }
        }

        /// <summary>
        /// Gets or sets whether to optimize the scene graph by not traversing the nodes
        /// added below this node if marker is not found.
        /// </summary>
        public virtual bool Optimize
        {
            get { return optimize; }
            set { optimize = value; }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Marker node does not allow cloning a node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GoblinException">If this method is called</exception>
        public override Node CloneNode()
        {
            throw new GoblinException("You should not clone a Marker node since you should only have one " 
                + "Marker node associated with one marker array");
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("Optimize", optimize.ToString());
            xmlNode.SetAttribute("MaxDropouts", maxDropouts.ToString());
            if (smooth)
                xmlNode.AppendChild(smoother.Save(xmlDoc));
            if (predict)
                xmlNode.AppendChild(predictor.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("Optimize"))
                optimize = bool.Parse(xmlNode.GetAttribute("Optimize"));
            if (xmlNode.HasAttribute("MaxDropouts"))
                maxDropouts = int.Parse(xmlNode.GetAttribute("MaxDropouts"));
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

        #region Update

        /// <summary>
        /// Updates the current matrix of this marker node
        /// </summary>
        /// <param name="elapsedTime">Elapsed time from last update in milliseconds</param>
        public virtual void Update(float elapsedTime)
        {
            if (tracker == null && scene.MarkerTracker != null)
            {
                tracker = scene.MarkerTracker;
                markerID = tracker.AssociateMarker(markerConfigs);
            }

            if (tracker != null && tracker.FindMarker(markerID))
            {
                Vector3 p = new Vector3();
                Quaternion q = Quaternion.Identity;
                Matrix rawMat = tracker.GetMarkerTransform();

                if (smooth || predict)
                {
                    Vector3 scale;
                    rawMat.Decompose(out scale, out q, out p);
                }

                if (smooth)
                    smoother.FilterMatrix(ref p, ref q, out worldTransformation);
                else
                    worldTransformation = rawMat;

                if (isInverseViewSet)
                    Matrix.Multiply(ref worldTransformation, ref inverseCameraView, out worldTransformation);

                if (predict)
                {
                    predictionTime = 0;
                    predictor.UpdatePredictor(ref p, ref q);
                }

                prevMatrix = worldTransformation;
                dropout = 0;
                found = true;
            }
            else
            {
                if (maxDropouts < 0)
                {
                    worldTransformation = prevMatrix;
                    found = false;
                }
                else
                {
                    if (dropout < maxDropouts)
                    {
                        dropout++;
                        if (predict)
                        {
                            predictionTime += elapsedTime;
                            predictor.GetPrediction(predictionTime, out worldTransformation);
                        }
                        else
                            worldTransformation = prevMatrix;
                    }
                    else
                    {
                        found = false;
                        worldTransformation = MatrixHelper.Empty;
                    }
                }

                if (smooth)
                    smoother.ResetHistory();
            }
        }

        #endregion
    }
}
