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
 *         Steve Henderson (henderso@cs.columbia.edu)
 *         
 * Notes:  The shadow mapping part is not working correctly due to skin mesh transformation
 *         that is computed in the shader
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.Shaders;
using GoblinXNA.Helpers;
using GoblinXNA.Physics;

using SkinnedModelWindows;
using System.ComponentModel;

namespace Tutorial14___Skinned_Model_Animation
{
    public class AnimatedModel : GoblinXNA.Graphics.Model
    {
        #region Fields

        /// <summary>
        /// The speed and direction of the animation.  +fwd, -reverse
        /// </summary>
        private float animationSpeedDirection = 1.0f;
        
        /// <summary>
        /// A holder for the last SpeedDir when animation is stopped.
        /// </summary>
        private float lastSpeedDir;

        private AnimationPlayer animationPlayer;
        private SkinningData skinningData;
        private Microsoft.Xna.Framework.Graphics.Model skinnedModel;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a model with both information loaded from a model file and mesh defined
        /// using VertexBuffer and IndexBuffer
        /// </summary>
        /// <param name="transforms">Transforms applied to each model meshes</param>
        /// <param name="mesh">A collection of model meshes</param>
        /// <param name="model"></param>
        public AnimatedModel(Microsoft.Xna.Framework.Graphics.Model aModel)
            : base(null, aModel.Meshes)
        {  
            this.skinnedModel = aModel;

            // Look up our custom skinning information.
            skinningData = aModel.Tag as SkinningData;

            if (skinningData == null)
                throw new GoblinException("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            Matrix[] newBones = animationPlayer.GetBoneTransforms();
            int k = animationPlayer.GetBoneTransforms().Length;

            this.transforms = new Matrix[k];

            for (int i = 0; i < k; i++)
                this.transforms[i] = newBones[i];

            CalculateMinimumBoundingSphere();
            
            //The text you pass in here needs to match the .fx shader file 
            shader = new SkinnedModelShader();

            resourceName = "";
#if WINDOWS
            shaderName = TypeDescriptor.GetClassName(shader);
#endif
            modelLoaderName = "AnimatedModelLoader";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the skinned model.
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.Model SkinnedModel
        {
            get { return skinnedModel; }
        }

        public AnimationPlayer AnimationPlayer 
        {
            get { return animationPlayer; }
        }

        /// <summary>
        /// Sets the animation speed and direction.  +fwd, -rev
        /// </summary>
        public float AnimationSpeedDirection
        {
            get { return animationSpeedDirection; }
            set
            {
                //Are we stopped?
                if (animationSpeedDirection == 0)
                {
                    lastSpeedDir = value;
                }
                else
                {
                    animationSpeedDirection = value;
                    lastSpeedDir = value;
                }
            }
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Copies only the geometry (Mesh,  
        /// MinimumBoundingBox, MinimumBoundingSphere, TriangleCount and Transforms)
        /// </summary>
        /// <param name="model">A source model from which to copy</param>
        public override void CopyGeometry(IModel model)
        {
            if (!(model is AnimatedModel))
                return;

            AnimatedModel srcModel = (AnimatedModel)model;
            skinnedModel = srcModel.SkinnedModel;

            vertices.AddRange(((IPhysicsMeshProvider)model).Vertices);
            indices.AddRange(((IPhysicsMeshProvider)model).Indices);

            // Look up our custom skinning information.
            skinningData = srcModel.skinnedModel.Tag as SkinningData;

            if (skinningData == null)
                throw new GoblinException("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            triangleCount = srcModel.TriangleCount;
            boundingBox = srcModel.MinimumBoundingBox;
            boundingSphere = srcModel.MinimumBoundingSphere;
            UseInternalMaterials = srcModel.UseInternalMaterials;
        }

        /// <summary>
        /// Renders the model itself as well as the minimum bounding box if showBoundingBox
        /// is true. By default, SimpleEffectShader is used to render the model.
        /// </summary>
        /// <remarks>
        /// This function is called automatically to render the model, so do not call this method
        /// </remarks>
        /// <param name="material">Material properties of this model</param>
        /// <param name="renderMatrix">Transform of this model</param>
        public override void Render(ref Matrix renderMatrix, Material material)
        {
            if (!UseInternalMaterials)
            {
                material.InternalEffect = null;
                if ((shader.CurrentMaterial != material) || material.HasChanged)
                {
                    shader.SetParameters(material);

                    foreach (IShader afterEffect in afterEffectShaders)
                        afterEffect.SetParameters(material);

                    material.HasChanged = false;
                }
            }

            // Render the actual model
            foreach (ModelMesh modelMesh in this.mesh)
            {
                Matrix.Multiply(ref transforms[modelMesh.ParentBone.Index], ref renderMatrix, out tmpMat1);

                if (UseInternalMaterials && (shader is IAlphaBlendable))
                    ((IAlphaBlendable)shader).SetOriginalAlphas(modelMesh.Effects);

                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    if (UseInternalMaterials)
                    {
                        material.InternalEffect = part.Effect;
                        shader.SetParameters(material);
                        ((SkinnedModelShader)shader).UpdateBones(this.transforms);
                    }

                    shader.Render(
                        ref tmpMat1,
                        (UseInternalMaterials) ? part.Effect.CurrentTechnique.Name : technique,
                        delegate
                        {
                            State.Device.SetVertexBuffer(part.VertexBuffer);
                            State.Device.Indices = part.IndexBuffer;
                            State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);

                        });

                    foreach (IShader afterEffect in afterEffectShaders)
                    {
                        if (UseInternalMaterials)
                            afterEffect.SetParameters(material);

                        afterEffect.Render(
                            ref tmpMat1,
                            "",
                            delegate
                            {
                                State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                    part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);

                            });
                    }
                }

            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stop (pause) the animation.
        /// </summary>
        public void Stop()
        {
            if (animationSpeedDirection != 0)
            {
                lastSpeedDir = animationSpeedDirection;
                animationSpeedDirection = 0;
            }
        }

        /// <summary>
        /// Start (play) the animation
        /// </summary>
        public void Start()
        {
            animationSpeedDirection = lastSpeedDir;
        }

        /// <summary>
        /// Helper class to get all the animation clip names in the model.
        /// </summary>
        /// <returns></returns>
        public string DumpClipNames()
        {
            string result = "";

            foreach (KeyValuePair<string, AnimationClip> kvp in skinningData.AnimationClips)
            {
                result = result + kvp.Key + ":";
            }
            return result;
        }

        /// <summary>
        /// Load the specified clip and play it.  Started by default
        /// to facilitate debugging.
        /// 
        /// If you don't want to play it after it's loaded,
        /// call Stop() immedietly.
        /// </summary>
        /// <param name="clipName">The name of the clip (e.g. "Take 001" in dude.fbx)</param>
        public void LoadAnimationClip(string clipName)
        {
            if (skinningData.AnimationClips.ContainsKey(clipName))
            {
                AnimationClip clip = skinningData.AnimationClips[clipName];
                animationPlayer.StartClip(clip);
                animationSpeedDirection = 1.0f;
                this.lastSpeedDir = animationSpeedDirection;
            }
            else
            {
                throw new GoblinException("Clip name does not exist in model.  Use DumpClipNames for a list..");
            }
        }

        /// <summary>
        /// Update the animation
        /// </summary>
        /// <param name="gt"></param>
        public void Update(GameTime gt)
        {
            long etTicks = gt.ElapsedGameTime.Ticks;
            long scaledTicks = (long)(etTicks * animationSpeedDirection);
            TimeSpan adjustedElapsedTime = new TimeSpan(scaledTicks);
            animationPlayer.Update(adjustedElapsedTime, true, Matrix.Identity);

            //Copy the bones
            Matrix[] newBones = animationPlayer.GetSkinTransforms();
            for (int i = 0; i < newBones.Length; i++)
                this.transforms[i] = newBones[i];
        }

        #endregion
    }
}
