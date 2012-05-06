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

using GoblinXNA.Graphics;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that is used to render a geometric model with different level of
    /// details depending on the distance of the model from the viewer. 
    /// </summary>
    public class LODNode : GeometryNode
    {
        #region Member Fields

        protected int levelOfDetail;
        protected List<IModel> models;
        protected bool autoComputeLevelOfDetail;
        protected List<float> autoComputeDistances;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a LOD (Level Of Detail) node that holds a list of models with different level
        /// of details. 
        /// </summary>
        /// <param name="name">
        /// The name of this node (has to be unique for correct networking behavior)
        /// </param>
        /// <param name="models">
        /// A list of models with different level of details
        /// </param>
        public LODNode(String name, List<IModel> models)
            : base(name)
        {
            levelOfDetail = 0;
            this.models = models;
            if (models != null && models.Count == 0)
                throw new GoblinException("'models' should contain more than one models");
            base.Model = models[0];
            autoComputeLevelOfDetail = false;
            autoComputeDistances = new List<float>();
        }

        /// <summary>
        /// Creates a LOD (Level Of Detail) node that holds a list of models with different level
        /// of details. 
        /// </summary>
        /// <param name="models">A list of models with different level of details</param>
        public LODNode(List<IModel> models) : this("", models) { }

        public LODNode() : this(null) { }

        #endregion

        #region Override Properties
        public override IModel Model
        {
            get
            {
                return base.Model;
            }
            set
            {
                throw new GoblinException("You are not allowed to directly set the Model property " +
                    "for LODNode. Use LevelOfDetail property to set which model to use");
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a set of models with different level of details to use.
        /// </summary>
        public virtual List<IModel> Models
        {
            get { return models; }
            set 
            { 
                models = value;
                base.Model = models[0];
            }
        }

        /// <summary>
        /// Gets or sets current level of detail. Default value is 0. The value has to be
        /// between 0 and number of models - 1 (Models.Count - 1). 
        /// </summary>
        /// <remarks>
        /// If AutoComputerLevelOfDetail is set to true, then setting this value would NOT
        /// take effect. 
        /// </remarks>
        /// <exception cref="GoblinException">If level of detail is out of range</exception>
        /// <see cref="AutoComputeLevelOfDetail"/>
        public virtual int LevelOfDetail
        {
            get { return levelOfDetail; }
            set
            {
                if (!autoComputeLevelOfDetail && (levelOfDetail != value))
                {
                    levelOfDetail = value;
                    if (levelOfDetail < 0 || levelOfDetail >= models.Count)
                        throw new GoblinException("LevelOfDetail must be a positive number less than " +
                            "the number of models");
                    base.Model = models[levelOfDetail];
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to automatically compute the appropriate level of details
        /// to use based on the distances set in AutoComputeDistances.
        /// </summary>
        /// <remarks>
        /// You have to also set AutoComputeDistances if you set this value to true.
        /// </remarks>
        /// <see cref="AutoComputeDistances"/>
        public virtual bool AutoComputeLevelOfDetail
        {
            get { return autoComputeLevelOfDetail; }
            set { autoComputeLevelOfDetail = value; }
        }

        /// <summary>
        /// Gets or sets the distances to use to switch among different level of details
        /// automatically when AutoComputeLevelOfDetail is set to true. The number of distances
        /// should be (Models.Count - 1).
        /// </summary>
        /// <remarks>
        /// If there are four models and the AutoComputeDistances is set to {5, 10, 20}, then when
        /// the distance (dist) between the viewer and the model is:
        /// [0 - 5], the first model is used;
        /// [5 - 10], the second model is used;
        /// [10 - 20], the third model is used;
        /// [20 - infinity], the fourth model is used.
        /// </remarks>
        /// <exception cref="GoblinException">If number of distances is incorrect</exception>
        /// <see cref="AutoComputeLevelOfDetail"/>
        public virtual List<float> AutoComputeDistances
        {
            get { return autoComputeDistances; }
            set
            {
                if (value.Count != (models.Count - 1))
                    throw new GoblinException("Number of distances should equal to (number of models - 1)");
                autoComputeDistances = value;
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Updates the level of details using the given camera (viewer) location. 
        /// </summary>
        /// <param name="cameraLocation"></param>
        internal void Update(Vector3 cameraLocation)
        {
            if (autoComputeDistances.Count != (models.Count - 1))
                return;

            int newLevelOfDetail = 0;
            float dist = Vector3.Subtract(worldTransform.Translation, cameraLocation).Length();
            for (int i = 0; i < autoComputeDistances.Count; i++)
                if (dist <= autoComputeDistances[i])
                {
                    newLevelOfDetail = i;
                    break;
                }

            if (autoComputeDistances.Count > 0 && (dist > autoComputeDistances[autoComputeDistances.Count - 1]))
                newLevelOfDetail = autoComputeDistances.Count;

            if (newLevelOfDetail != levelOfDetail)
            {
                levelOfDetail = newLevelOfDetail;
                base.Model = models[levelOfDetail];
            }
        }
        #endregion

        #region Override Methods

        public override Node CloneNode()
        {
            throw new GoblinException("You should not clone LOD node");
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            // NOT FINISHED YET

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            // NOT FINISHED YET
        }
#endif

        #endregion
    }
}
