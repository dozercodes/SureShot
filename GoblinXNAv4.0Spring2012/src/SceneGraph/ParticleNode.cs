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
using GoblinXNA.Graphics;
#if WINDOWS_PHONE
using GoblinXNA.Graphics.ParticleEffects2D;
#else
using GoblinXNA.Graphics.ParticleEffects;
#endif

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A delegate function that defines how a list of particle effects should be updated. The update
    /// is mainly for adding new particles to each of the effects.
    /// </summary>
    /// <param name="worldTransform">The transformation of the particle effects</param>
    /// <param name="particleEffects">A list of particle effects added in a ParticleNode</param>
    public delegate void ParticleUpdateHandler(Matrix worldTransform, List<ParticleEffect> particleEffects);

    /// <summary>
    /// A scene graph node that defines a collection of particle effects.
    /// </summary>
    public class ParticleNode : Node
    {
        #region Member Fields

        protected List<ParticleEffect> particleEffects;
        protected Matrix worldTransformation;
        protected Matrix markerTransform;
        protected bool isRendered;
        protected bool shouldRender;

        protected Matrix tmpMat;

        #endregion

        #region Events

        public event ParticleUpdateHandler UpdateHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a scene graph node that holds a collection of particle effects.
        /// </summary>
        /// <param name="name">The name of this particle node</param>
        public ParticleNode(String name)
            : base(name)
        {
            particleEffects = new List<ParticleEffect>();
            worldTransformation = Matrix.Identity;
            markerTransform = Matrix.Identity;
            UpdateHandler = null;
            isRendered = false;
            shouldRender = false;
            tmpMat = Matrix.Identity;
        }

        public ParticleNode() : this("") { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a collection of particle effects associated with this node.
        /// </summary>
        public List<ParticleEffect> ParticleEffects
        {
            get { return particleEffects; }
            set
            {
                particleEffects = value;
                particleEffects.Sort();
            }
        }

        /// <summary>
        /// Gets or sets the world transformation of the associated particle effects.
        /// </summary>
        /// <remarks>
        /// If there is a TransformNode in its ancestor nodes, then you usually shouldn't set
        /// this matrix.
        /// </remarks>
        public Matrix WorldTransformation
        {
            get { return worldTransformation; }
            set { worldTransformation = value; }
        }

        /// <summary>
        /// Gets or sets whether the particle systems in this node should be rendered.
        /// The default value is true.
        /// </summary>
        public bool IsRendered
        {
            get { return isRendered; }
            set { isRendered = value; }
        }

        internal bool ShouldRender
        {
            get { return shouldRender; }
            set { shouldRender = value; }
        }

        public Matrix MarkerTransformation
        {
            get { return markerTransform; }
            internal set { markerTransform = value; }
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Updates the associated particle effects.
        /// </summary>
        /// <param name="elapsedTime"></param>
        internal void Update(TimeSpan elapsedTime)
        {
            if (UpdateHandler != null)
                UpdateHandler(worldTransformation, particleEffects);

            foreach (ParticleEffect effect in particleEffects)
                if(effect.Enabled)
                    effect.Update(elapsedTime);
        }

        /// <summary>
        /// Renders the associated particle effects.
        /// </summary>
        internal void Render()
        {
            foreach (ParticleEffect effect in particleEffects)
                if (effect.Enabled)
                {
#if !WINDOWS_PHONE
                    effect.Shader.SetParameters(effect);
#endif
                    if(markerTransform.M44 == 1)
                        effect.Render(markerTransform);
                }
        }
        #endregion

        #region Override Methods

        public override Node CloneNode()
        {
            ParticleNode node = (ParticleNode) base.CloneNode();
            node.ParticleEffects.AddRange(particleEffects);
            node.WorldTransformation = worldTransformation;
            node.UpdateHandler = UpdateHandler;

            return node;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            foreach (ParticleEffect effect in particleEffects)
                xmlNode.AppendChild(effect.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            foreach (XmlElement element in xmlNode.ChildNodes)
            {
                ParticleEffect effect = (ParticleEffect)Activator.CreateInstance(Type.GetType(element.Name));
                effect.Load(element);
                particleEffects.Add(effect);
            }
        }
#endif

        public override void Dispose()
        {
            foreach (ParticleEffect effect in particleEffects)
                effect.Dispose();
        }

        #endregion
    }
}
