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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that defines a bundle of optically tracked fiducial marker arrays.
    /// The supporting marker nodes can supplement the tracking of the base marker array
    /// if the base marker array is not visible.
    /// 
    /// Any nodes added below a MarkerBundleNode with WorldTranformation properties will be affected by the
    /// transformation returned by the marker tracker including GeometryNode, ParticleNode, SoundNode,
    /// CameraNode, and LightNode.
    /// </summary>
    /// <remarks>
    /// Do not add any of the supporting marker nodes to the scene since they will be updated
    /// automatically by this node.
    /// </remarks>
    public class MarkerBundleNode : MarkerNode
    {
        #region Member Fields

        private List<RelativeMarker> supportingMarkers;
        private bool autoReconfigure;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a marker bundle node.
        /// </summary>
        /// <param name="name">Name of this marker bundle node</param>
        /// <param name="tracker">A marker tracker used to track this fiducial marker</param>
        /// <param name="supportingMarkerNodes">A list of marker nodes that will supplement
        /// the base marker array</param>
        /// <param name="markerConfigs">A list of configs that specify the fiducial marker 
        /// (can be either an array or a single marker) to look for</param>
        public MarkerBundleNode(String name, IMarkerTracker tracker, 
            List<MarkerNode> supportingMarkerNodes, params object[] markerConfigs)
            : base(name, tracker, markerConfigs)
        {
            supportingMarkers = new List<RelativeMarker>();
            foreach (MarkerNode markerNode in supportingMarkerNodes)
            {
                RelativeMarker marker = new RelativeMarker(markerNode);
                supportingMarkers.Add(marker);
            }
            autoReconfigure = false;
        }

        /// <summary>
        /// Creates a marker bundle node.
        /// </summary>
        /// <param name="tracker">A marker tracker used to track this fiducial marker</param>
        /// <param name="supportingMarkerNodes">A list of marker nodes that will supplement
        /// the base marker array</param>
        /// <param name="markerConfigs">A list of configs that specify the fiducial marker 
        /// (can be either an array or a single marker) to look for</param>
        public MarkerBundleNode(IMarkerTracker tracker, List<MarkerNode> supportingMarkerNodes,
            params object[] markerConfigs)
            : this("", tracker, supportingMarkerNodes, markerConfigs)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether to auto reconfigure the transform of each supporting marker nodes
        /// relative to the base marker array. Default value is false.
        /// </summary>
        public bool AutoReconfigure
        {
            get { return autoReconfigure; }
            set { autoReconfigure = value; }
        }

        public override int MaxDropouts
        {
            get
            {
                return base.MaxDropouts;
            }
            set
            {
                base.MaxDropouts = value;
                foreach (RelativeMarker marker in supportingMarkers)
                    marker.Node.MaxDropouts = value;
            }
        }

        #endregion

        #region Overridden Methods

        public override void Update(float elapsedTime)
        {
            foreach (RelativeMarker marker in supportingMarkers)
                marker.Node.Update(elapsedTime);

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

                foreach (RelativeMarker marker in supportingMarkers)
                {
                    if (marker.Node.MarkerFound && (!marker.Initialized || autoReconfigure))
                    {
                        marker.RelativeTransform = rawMat *
                            Matrix.Invert(marker.Node.WorldTransformation);
                        marker.Initialized = true;
                    }
                }

                rawMat = GetAverage(rawMat);

                if (smooth || predict)
                {
                    Vector3 scale;
                    rawMat.Decompose(out scale, out q, out p);
                }

                if (smooth)
                    smoother.FilterMatrix(ref p, ref q, out worldTransformation);
                else
                    worldTransformation = rawMat;

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
                Matrix mat = GetAverage();
                if (mat.M44 != 0)
                {
                    prevMatrix = mat;
                    worldTransformation = mat;
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
        }

        #endregion

        #region Private Methods

        private Matrix GetAverage(Matrix original)
        {
            List<Matrix> transforms = new List<Matrix>();
            foreach (RelativeMarker marker in supportingMarkers)
            {
                if (marker.Node.MarkerFound && marker.Initialized)
                {
                    transforms.Add(Matrix.Multiply(marker.RelativeTransform,
                        marker.Node.WorldTransformation));
                }
            }

            if (transforms.Count > 0)
            {
                Vector3 origPos, temp;
                Quaternion origRot;
                original.Decompose(out temp, out origRot, out origPos);

                Vector3 pos;
                Quaternion rot;
                foreach (Matrix mat in transforms)
                {
                    mat.Decompose(out temp, out rot, out pos);
                    origPos += pos;
                    Quaternion.Lerp(ref origRot, ref rot, 1.0f / (1 + transforms.Count), out origRot);
                }

                origRot.Normalize();
                origPos *= 1.0f / (1 + transforms.Count);

                Matrix result = Matrix.CreateFromQuaternion(origRot);
                result.Translation = origPos;

                return result;
            }
            else
                return original;
        }

        private Matrix GetAverage()
        {
            List<Matrix> transforms = new List<Matrix>();
            foreach (RelativeMarker marker in supportingMarkers)
            {
                if (marker.Node.MarkerFound && marker.Initialized)
                {
                    transforms.Add(Matrix.Multiply(marker.RelativeTransform,
                        marker.Node.WorldTransformation));
                }
            }

            if (transforms.Count > 0)
            {
                return transforms[0];
            }
            else
                return MatrixHelper.Empty;
        }

        #endregion

        #region Private Class

        private class RelativeMarker
        {
            public MarkerNode Node;
            public Matrix RelativeTransform;
            public bool Initialized;

            public RelativeMarker(MarkerNode node)
            {
                Node = node;
                RelativeTransform = Matrix.Identity;
                Initialized = false;
            }
        }

        #endregion
    }
}
