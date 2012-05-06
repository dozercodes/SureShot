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
using Microsoft.Xna.Framework.Audio;

using GoblinXNA.Sounds;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that represents a 3D sound source. For example, this node can be 
    /// added under a GeometryNode to create a sound source that follows a geometry model.
    /// </summary>
    public class SoundNode : Node, IAudioEmitter
    {
        #region Member Fields

        protected Matrix worldTransformation;
        protected Vector3 velocity;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a scene graph node that holds a 3D sound source with a specified node name.
        /// </summary>
        /// <param name="name">The name of this sound node</param>
        public SoundNode(String name)
            : base(name)
        {
            worldTransformation = Matrix.Identity;
            velocity = new Vector3();
        }

        /// <summary>
        /// Creates a scene graph node that holds a 3D sound source.
        /// </summary>
        public SoundNode() : this(""){}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the world transformation (position and orientation) of the 3D sound source.
        /// 
        /// If there is a TransformNode in its ancestor nodes, then you usually should not manually
        /// set this matrix.
        /// </summary>
        public Matrix WorldTransformation
        {
            get { return worldTransformation; }
            set 
            {
                velocity = (value.Translation - worldTransformation.Translation) * scene.FPS;
                worldTransformation = value;
            }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
        }

        public Vector3 Position
        {
            get { return worldTransformation.Translation; }
        }

        public Vector3 Forward
        {
            get { return worldTransformation.Forward; }
        }

        public Vector3 Up
        {
            get { return worldTransformation.Up; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays the sound effect in 3D.
        /// </summary>
        /// <remarks>
        /// Before calling this method, you should make sure that you initialized the Sound class by calling
        /// the Sound.Initialize(...) method. Otherwise, an exception will be thrown.
        /// </remarks>
        /// <param name="soundEffect"></param>
        public void Play(SoundEffect soundEffect)
        {
            Sound.Instance.PlaySoundEffect3D(soundEffect, this);
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Plays the 3D sound specified by its cue name.
        /// </summary>
        /// <remarks>
        /// Before calling this method, you should make sure that you initialized the Sound class by calling
        /// the Sound.Initialize(...) method. Otherwise, an exception will be thrown.
        /// </remarks>
        /// <param name="cueName"></param>
        public void Play(String cueName)
        {
            Sound.Instance.Play3D(cueName, this);
        }
#endif

        #endregion

        #region Override Methods

        public override Node CloneNode()
        {
            SoundNode node = (SoundNode)base.CloneNode();
            node.WorldTransformation = worldTransformation;

            return node;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);
           
            // nothing to add to the XML info

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            // nothing to load
        }
#endif

        #endregion
    }
}
