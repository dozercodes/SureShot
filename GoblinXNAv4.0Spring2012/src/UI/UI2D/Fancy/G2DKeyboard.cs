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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Device.Generic;

namespace GoblinXNA.UI.UI2D.Fancy
{
    /// <summary>
    /// NOT FINISHED YET
    /// </summary>
    internal class G2DKeyboard : G2DComponent
    {
        #region Enums

        public enum KeyboardLayout { QWERTY, Dvorak, SingleDvorakRight, SingleDvorakLeft };

        #endregion

        #region Member Fields

        protected int keySize;
        protected Texture2D keyTexture;
        protected KeyboardLayout layout;
        protected G2DButton[] keys;

        protected String[] keyLowerStrings;
        protected String[] keyUpperStrings;

        protected bool shiftPressed;

        #endregion

        #region Const Key Strings

        protected String[] QWERTYLower = {
            "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Back",
            "Ctrl", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\",
            "Alt", "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'", "Enter",
            "Shift", "z", "x", "c", "v", "b", "n", "m", ",", ".", "/", "Space"
            };

        protected String[] QWERTYUpper = {
            "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "Back",
            "Ctrl", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "O", "{", "}", "|",
            "Alt", "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"", "Enter",
            "Shift", "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?", "Space"
            };

        protected String[] DvorakLower = {
            "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "[", "]", "Back",
            "Ctrl", "'", ",", ".", "p", "y", "f", "g", "c", "r", "l", "/", "=", "\\",
            "Alt", "a", "o", "e", "u", "i", "d", "h", "t", "n", "s", "-", "Enter",
            "Shift", ";", "q", "j", "k", "x", "b", "m", "w", "v", "z", "Space"
            };

        protected String[] DvorakUpper = {
            "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "{", "}", "Back",
            "Ctrl", "\"", "<", ">", "P", "Y", "F", "G", "C", "R", "L", "?", "+", "|",
            "Alt", "A", "O", "E", "U", "I", "D", "H", "T", "N", "S", "_", "Enter",
            "Shift", ":", "Q", "J", "K", "X", "B", "M", "W", "V", "Z", "Space"
            };

        protected String[] SingleDvorakRightUpper = null;
        protected String[] SingleDvorakRightLower = null;
        protected String[] SingleDvorakLeftUpper = null;
        protected String[] SingleDvorakLeftLower = null;

        #endregion

        #region Constructors

        public G2DKeyboard(int keySize) : this(keySize, KeyboardLayout.QWERTY) { }

        public G2DKeyboard(int keySize, KeyboardLayout layout)
            : base()
        {
            this.keySize = keySize;
            this.layout = layout;
            keyTexture = null;

            keys = new G2DButton[53];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = new G2DButton();

            shiftPressed = false;

            PrepareKeyStrings();
            MapStringsToKeyButtons();
        }

        #endregion

        #region Properties

        public int KeySize
        {
            get { return keySize; }
        }

        public virtual KeyboardLayout Layout
        {
            get { return layout; }
            set
            {
                if (layout != value)
                {
                    layout = value;

                    PrepareKeyStrings();
                    MapStringsToKeyButtons();
                }
            }
        }

        public virtual Texture2D KeyTexture
        {
            get { return keyTexture; }
            set
            {
                keyTexture = value;
                foreach (G2DButton key in keys)
                    key.Texture = keyTexture;
            }
        }

        #endregion

        #region Override Properties

        public override SpriteFont TextFont
        {
            get
            {
                return base.TextFont;
            }
            set
            {
                base.TextFont = value;

                foreach (G2DButton key in keys)
                    key.TextFont = value;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void PrepareKeyStrings()
        {
            switch (layout)
            {
                case KeyboardLayout.QWERTY:
                    keyLowerStrings = QWERTYLower;
                    keyUpperStrings = QWERTYUpper;
                    break;
                case KeyboardLayout.Dvorak:
                    keyLowerStrings = DvorakLower;
                    keyUpperStrings = DvorakUpper;
                    break;
                case KeyboardLayout.SingleDvorakLeft:
                    keyLowerStrings = SingleDvorakRightLower;
                    keyUpperStrings = SingleDvorakRightUpper;
                    break;
                case KeyboardLayout.SingleDvorakRight:
                    keyLowerStrings = SingleDvorakLeftLower;
                    keyUpperStrings = SingleDvorakLeftUpper;
                    break;
            }
        }

        protected virtual void MapStringsToKeyButtons()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (shiftPressed)
                    keys[i].Text = keyLowerStrings[i];
                else
                    keys[i].Text = keyUpperStrings[i];
            }
        }

        #endregion

        #region Override Methods

        internal override void RegisterMouseInput()
        {
            base.RegisterMouseInput();

            foreach (G2DButton key in keys)
                key.RegisterMouseInput();
        }

        internal override void RemoveMouseInput()
        {
            base.RemoveMouseInput();

            foreach (G2DButton key in keys)
                key.RemoveMouseInput();
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            foreach (G2DButton key in keys)
                key.RenderWidget();
        }

        #endregion
    }
}
