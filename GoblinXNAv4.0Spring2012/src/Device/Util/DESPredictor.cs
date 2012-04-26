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
using System.Xml;
using System.Text;
using System.ComponentModel;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Device.Util
{
    /// <summary>
    /// A predictor that implements La Viola's Double Exponentional Smoothing Prediction
    /// algorithm.
    /// </summary>
    public class DESPredictor : IPredictor
    {
        #region Member Fields

        protected const int RESTART_THRESHOLD = 5;
        protected Vector3 transSp, transSp2;
        protected Quaternion rotSq, rotSq2;

        protected int delta;

        protected Vector3 prevRawP;
        protected Quaternion prevRawQ;

        protected float transAlpha;
        protected float rotAlpha;

        protected float transThreshold;
        protected float rotThreshold;

        protected bool initialized;
        protected int restartCount;

        #region Temporary Variables

        protected Matrix tmpMat1;
        protected Matrix tmpMat2;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a double-exponential-smoothing predictor with the specified alpha values,
        /// delta time increment, and thresholds.
        /// </summary>
        /// <param name="transAlpha">An alpha value used for translational smoothing.</param>
        /// <param name="rotAlpha">An alpha value used for rotational smoothing.</param>
        /// <param name="delta">The time delta between each prediction time</param>
        /// <param name="transThreshold">A translational threshold. If set to larger than 0, then
        /// if the distance between the previous transformation and the current transformation is larger
        /// than this threshold, then the current transformation will be dropped from the smoothing
        /// calculation.</param>
        /// <param name="rotThreshold">A rotational threshold. If set to largen than 0, then
        /// if the angle (in radians) between the previous transformation and the current transformation
        /// is larger than this threshold, then the current transformation will be dropped from the
        /// smoothing calculation.</param>
        public DESPredictor(float transAlpha, float rotAlpha, int delta, float transThreshold, 
            float rotThreshold)
        {
            if (transAlpha <= 0 || transAlpha >= 1 || rotAlpha <= 0 || rotAlpha >= 1)
                throw new ArgumentException("alpha value has to be between 0.0f and 1.0f excluding 0 and 1");

            this.transAlpha = transAlpha;
            this.rotAlpha = rotAlpha;
            this.transThreshold = transThreshold;
            this.rotThreshold = rotThreshold;
            this.delta = delta;

            prevRawP = new Vector3();
            prevRawQ = Quaternion.Identity;

            initialized = false;
            restartCount = 0;
        }

        /// <summary>
        /// Creates a double-exponential-smoothing predictor with the specified alpha values,
        /// and delta time increment with no thresholds.
        /// </summary>
        /// <param name="transAlpha">An alpha value used for translational smoothing.</param>
        /// <param name="rotAlpha">An alpha value used for rotational smoothing.</param>
        /// <param name="delta">The time delta between each prediction time.</param>
        public DESPredictor(float transAlpha, float rotAlpha, int delta)
            : this(transAlpha, rotAlpha, delta, -1, -1)
        {
        }

        /// <summary>
        /// Creates a double-exponential-smoothing predictor with the specified alpha value,
        /// and delta time increment with no thresholds.
        /// </summary>
        /// <param name="alpha">An alpha value used for both translational and rotational 
        /// smoothing.</param>
        /// <param name="delta">The time delta between each prediction time.</param>
        public DESPredictor(float alpha, int delta)
            : this(alpha, alpha, delta)
        {
        }

        public DESPredictor() : this(0.5f, 50){}

        #endregion

        #region Public Methods

        public void UpdatePredictor(ref Vector3 p, ref Quaternion q)
        {
            if (initialized)
            {
                bool update = true;

                // If translation threshold is set to larger than 0, then check whether
                // the current transformation and previous transformation has larger translation
                // difference than the threhold. If it is, then we ignore the currently passed
                // transformation, and return the previously smoothed transformation.
                if (transThreshold > 0)
                {
                    float dist = Vector3.Distance(p, prevRawP);
                    if (dist > transThreshold)
                        update = false;
                }

                // If rotational threshold is set to larger than 0, then check whether
                // the current transformation and previous transformation has larger angle
                // difference than the threhold. If it is, then we ignore the currently passed
                // transformation, and return the previously smoothed transformation.
                if (rotThreshold > 0)
                {
                    q.Normalize();
                    prevRawQ.Normalize();
                    float dotProduct = Quaternion.Dot(q, prevRawQ);
                    if (dotProduct > 1)
                        dotProduct = 1;
                    else if (dotProduct < -1)
                        dotProduct = -1;
                    float angleDiff = (float)Math.Acos(dotProduct);
                    if (angleDiff > rotThreshold)
                        update = false;
                }

                if (update)
                {
                    transSp = transAlpha * p + (1 - transAlpha) * transSp;
                    transSp2 = transAlpha * transSp + (1 - transAlpha) * transSp2;

                    prevRawP = p;
                    prevRawQ = q;

                    restartCount = 0;
                }
                else
                {
                    restartCount++;
                    if (restartCount > RESTART_THRESHOLD)
                    {
                        prevRawP = p;
                        prevRawQ = q;

                        restartCount = 0;
                    }
                }
            }
            else
            {
                transSp = transSp2 = p;
                rotSq = rotSq2 = q;

                prevRawP = p;
                prevRawQ = q;

                initialized = true;
            }
        }

        public void GetPrediction(float t, out Matrix result)
        {
            int tau = Math.Max((int)(t / delta), 1);

            float tmp = transAlpha * tau / (1 - transAlpha);
            Vector3 Pt = Vector3.Subtract(Vector3.Multiply(transSp, (2 + tmp)), 
                Vector3.Multiply(transSp2, (1 + tmp)));

            tmp = rotAlpha * tau / (1 - rotAlpha);
            Quaternion Qt = Quaternion.Multiply(rotSq, (2 + tmp)) - Quaternion.Multiply(rotSq2, (1 + tmp));
            Qt.Normalize();

            Matrix.CreateFromQuaternion(ref Qt, out tmpMat1);
            Matrix.CreateTranslation(ref Pt, out tmpMat2);
            Matrix.Multiply(ref tmpMat1, ref tmpMat2, out result);
        }

#if !WINDOWS_PHONE
        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("TransAlpha", transAlpha.ToString());
            xmlNode.SetAttribute("RotAlpha", rotAlpha.ToString());
            xmlNode.SetAttribute("Delta", delta.ToString());

            if (transThreshold != -1)
                xmlNode.SetAttribute("TransThreshold", transThreshold.ToString());
            if (rotThreshold != -1)
                xmlNode.SetAttribute("RotThreshold", rotThreshold.ToString());

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
            if (xmlNode.HasAttribute("TransAlpha"))
                transAlpha = float.Parse(xmlNode.GetAttribute("TransAlpha"));
            if (xmlNode.HasAttribute("RotAlpha"))
                rotAlpha = float.Parse(xmlNode.GetAttribute("RotAlpha"));
            if (xmlNode.HasAttribute("Delta"))
                delta = int.Parse(xmlNode.GetAttribute("Delta"));

            if (transAlpha <= 0 || transAlpha >= 1 || rotAlpha <= 0 || rotAlpha >= 1)
                throw new GoblinException("Alpha value has to be between 0.0f and 1.0f excluding 0 and 1");

            if (xmlNode.HasAttribute("TransThreshold"))
                transThreshold = float.Parse(xmlNode.GetAttribute("TransThreshold"));
            if (xmlNode.HasAttribute("RotThreshold"))
                rotThreshold = float.Parse(xmlNode.GetAttribute("RotThreshold"));
        }
#endif
        #endregion
    }
}
