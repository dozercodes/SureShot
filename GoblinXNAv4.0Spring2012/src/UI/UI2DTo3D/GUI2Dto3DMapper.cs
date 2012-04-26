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
using System.Windows.Forms;

using GoblinXNA.Device.Vision.Marker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace GoblinXNA.UI.UI2DTo3D
{
    /// <summary>
    /// NOT FINISHED YET.
    /// </summary>
    internal class GUI2Dto3DMapper
    {
        #region Structs
        public struct MouseInput
        {
            public Vector3 Pos;
            public ButtonState State;
            public MouseButtons Button;
        };
        #endregion

        #region Variables
        private List<GUIRenderer> renderers;
        private List<IGUIMapper> mappers;
        //private List<CollisionObject> guiColObjs;
        #endregion

        #region Constructors
        public GUI2Dto3DMapper()
        {
            if (!State.Initialized)
                throw new GoblinException("GoblinXNA.GoblinSetting.InitGoblin(...) method needs to be called " +
                    "before you can call this method");

            renderers = new List<GUIRenderer>();
            mappers = new List<IGUIMapper>();
            //guiColObjs = new List<CollisionObject>();
        }
        #endregion

        #region Accessor/Modifiers
        public bool AddMapper(IGUIMapper mapper, Matrix renderMatrix)
        {
            if (!mappers.Contains(mapper))
            {
                mappers.Add(mapper);
                //CollisionObject colObj = new CollisionObject();
                //guiColObjs.Add(colObj);
                //renderers.Add(new GUIRenderer(mapper, renderMatrix, colObj));
                return true;
            }
            else
                return false;
        }

        public void AddMappers(List<IGUIMapper> mappers, List<Matrix> renderMatricies)
        {
            if(mappers.Count != renderMatricies.Count)
                throw new GoblinException("number of mappers has to match number of render matricies");

            this.mappers.AddRange(mappers);
            for (int i = 0; i < mappers.Count; i++)
            {
                //CollisionObject colObj = new CollisionObject();
                //guiColObjs.Add(colObj);
                //renderers.Add(new GUIRenderer(mappers[i], renderMatricies[i], colObj));
            }
        }

        public bool RemoveMapper(IGUIMapper mapper)
        {
            int index = mappers.IndexOf(mapper);
            bool removed = mappers.Remove(mapper);
            if (removed)
                renderers.RemoveAt(index);

            return removed;
        }

        /// <summary>
        /// Removes all of the GUI mappers
        /// </summary>
        public void RemoveAllMappers()
        {
            renderers.Clear();
            mappers.Clear();
        }

        public IGUIMapper GetMapper(int index)
        {
            if (index < 0 || index >= mappers.Count)
                throw new GoblinException("Index out of range for mappers");

            return mappers[index];
        }

        /*public CollisionObject GetGUIObject(IGUIMapper mapper)
        {
            int index = mappers.IndexOf(mapper);
            if (index >= 0)
                return guiColObjs[index];
            else
                return null;
        }*/

        public void UpdateMapperMatrix(IGUIMapper mapper, Matrix renderMatrix)
        {
            int index = mappers.IndexOf(mapper);
            if (index >= 0)
                renderers[index].RenderMatrix = renderMatrix;
        }

        public void UpdateMapperTexture(IGUIMapper mapper, uint[] textureData)
        {
            int index = mappers.IndexOf(mapper);
            if (index >= 0)
                renderers[index].Texture = textureData;
        }

        public List<IGUIMapper> GetAllMappers()
        {
            return mappers;
        }
        #endregion

        #region Event Handlers
        public void ActivateMouseEvent(int mx, int my, ButtonState state, MouseButtons button)
        {
            //foreach (IGUIMapper mapper in mappers)
            //    mapper.MouseEventHandler(mx, my, state, button);
        }

        public void ActivateKeyEvent(Keys key, KeyState state)
        {
            //foreach (IGUIMapper mapper in mappers)
            //    mapper.KeyEventHandler(key, state);
        }
        #endregion

        #region Render
        public void RenderAll()
        {
            foreach (GUIRenderer guiRenderer in renderers)
                guiRenderer.Render();
        }
        #endregion

        #region Update
        public void UpdateInputs()
        {
            foreach (GUIRenderer guiRenderer in renderers)
                guiRenderer.UpdateMouseInput();
        }
        #endregion

        #region GUIRenderer class
        private class GUIRenderer
        {
            #region Variables
            private IGUIMapper mapper;
            private Matrix renderMatrix;
            //private CollisionObject colObj;
            private uint[] textureData;

            private DynamicVertexBuffer vb;
            private DynamicIndexBuffer ib;
            private VertexPositionTexture[] texCoords;
            private Texture2D guiTexture;

            private float width;
            private float height;
            #endregion

            #region Constructors
            public GUIRenderer(IGUIMapper mapper, Matrix renderMatrix/*, CollisionObject colObj*/)
            {
                this.mapper = mapper;
                this.renderMatrix = renderMatrix;
                //this.colObj = colObj;

                guiTexture = new Texture2D(State.Device, mapper.GUIWidth, mapper.GUIHeight,
                    false, mapper.TextureFormat);
                texCoords = new VertexPositionTexture[4];

                // Calculate the GUI positions where it will rendered in the 3D world
                Matrix texPos = Matrix.Identity;

                width = guiTexture.Width * mapper.DrawingScaleFactor.X / 2;
                height = guiTexture.Height * mapper.DrawingScaleFactor.Y / 2;

                texCoords[0].Position = new Vector3(-width, height, 0);
                texCoords[0].TextureCoordinate = new Vector2(0, 1);

                texCoords[1].Position = new Vector3(width, -height, 0);
                texCoords[1].TextureCoordinate = new Vector2(1, 0);

                texCoords[2].Position = new Vector3(-width, -height, 0);
                texCoords[2].TextureCoordinate = new Vector2(0, 0);

                texCoords[3].Position = new Vector3(width, height, 0);
                texCoords[3].TextureCoordinate = new Vector2(1, 1);

                vb = new DynamicVertexBuffer(State.Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
                vb.SetData(texCoords);

                short[] indices = new short[6];

                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                indices[3] = 3;
                indices[4] = 1;
                indices[5] = 0;

                ib = new DynamicIndexBuffer(State.Device, typeof(short), 6, BufferUsage.WriteOnly);
                ib.SetData(indices);

                textureData = mapper.GUITexture;

                //colObj = GoblinSetting.UICollisionWorld.Add2Dto3DGUI(new Vector2(width * 2, height * 2),
                //            renderMatrix, mapper.GetGUIName());
            }
            #endregion

            #region Properties
            public IGUIMapper Mapper
            {
                get { return mapper; }
            }

            public Matrix RenderMatrix
            {
                set
                {
                    renderMatrix = value;

                    /*if(colObj != null)
                        colObj.WorldTransform = renderMatrix;
                    if(!renderMatrix.Equals(Matrix.Identity))
                        colObj = GoblinSetting.UICollisionWorld.Add2Dto3DGUI(new Vector2(width * 2, height * 2),
                            renderMatrix);*/
                }
                get { return renderMatrix; }
            }

            public uint[] Texture
            {
                set { textureData = value; }
            }
            #endregion

            #region Render
            public void Render()
            {
                if (guiTexture.IsDisposed)
                {
                    guiTexture = new Texture2D(State.Device, mapper.GUIWidth, 
                        mapper.GUIHeight, false, mapper.TextureFormat);
                }

                guiTexture.SetData(textureData);

                /*GoblinSetting.Shader.UseTextureTechnique();
                GoblinSetting.Shader.WorldParam.SetValue(renderMatrix);
                GoblinSetting.Shader.TextureParam.SetValue(guiTexture);

                GoblinSetting.Shader.RenderPrimitives(vb, ib, VertexPositionTexture.VertexElements,
                    VertexPositionTexture.SizeInBytes, 4, 2);*/
            }
            #endregion

            #region Update
            public void UpdateMouseInput()
            {
                /*MouseInput input = mapper.GetMappedMouseInput();

                if (float.IsNaN(input.Pos.X))
                    return;

                // Map the 3D mouse coordinate back to 2D position on the GUI
                Matrix tmp = Matrix.CreateTranslation(input.Pos);

                Vector3 mousePos = new Vector3();
                mousePos.X = input.Pos.X / (float)Math.Abs(texCoords[3].Position.X - texCoords[2].Position.X) *
                    mapper.GetGUIWidth();
                mousePos.Y = input.Pos.Y / (float)Math.Abs(texCoords[3].Position.Y - texCoords[2].Position.Y) *
                    mapper.GetGUIHeight();

                Console.WriteLine("Original pos: " + input.Pos);
                Console.WriteLine("Mapped pos: " + mousePos);

                mapper.MouseEventHandler((int)mousePos.X, (int)mousePos.Y, input.State, input.Button);*/
            }
            #endregion
        }
        #endregion
    }
}
