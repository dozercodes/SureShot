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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAModel = Microsoft.Xna.Framework.Graphics.Model;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// Default implementation of model loader that can load DirectX model files including .x and .fbx
    /// </summary>
    public class ModelLoader : IModelLoader
    {
        public IModel Load(String path, String modelAssetName)
        {
            path = (path.Equals("")) ? State.GetSettingVariable("ModelDirectory") : path;
            String filePath = Path.Combine(path, modelAssetName);
            XNAModel xnaModel = State.Content.Load<XNAModel>(@"" + filePath);

            // Get matrix transformations of the model
            if (xnaModel != null)
            {
                Matrix[] transforms = new Matrix[xnaModel.Bones.Count];
                xnaModel.CopyAbsoluteBoneTransformsTo(transforms);

                IModel model = new Model(transforms, xnaModel.Meshes);
                return model;
            }
            else
            {
                Log.Write("Model " + filePath + " does not exist ");
                return null;
            }
        }
    }
}
