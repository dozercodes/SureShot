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
using Microsoft.Xna.Framework.Input;

namespace GoblinXNA.Device.Generic
{
    #region Keyboard Delegates

    /// <summary>
    /// A delegate/callback function that defines what to do when a key is pressed.
    /// </summary>
    /// <param name="key">The key pressed</param>
    /// <param name="modifier">A struct that indicates whether any of the modifier keys 
    /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
    public delegate void HandleKeyPress(Keys key, KeyModifier modifier);

    /// <summary>
    /// A delegate/callback function that defines what to do when a key is released.
    /// </summary>
    /// <param name="key">The key released</param>
    /// <param name="modifier">A struct that indicates whether any of the modifier keys 
    /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
    public delegate void HandleKeyRelease(Keys key, KeyModifier modifier);

    /// <summary>
    /// A delegate/callback function that defines what to do when a key is typed.
    /// </summary>
    /// <param name="key">The key typed</param>
    /// <param name="modifier">A struct that indicates whether any of the modifier keys 
    /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
    public delegate void HandleKeyType(Keys key, KeyModifier modifier);

    #endregion

    #region Keyboard Structs
    /// <summary>
    /// A struct that defines whether any of the modifier keys (e.g., Shift, Alt, or Ctrl) 
    /// are held down.
    /// </summary>
    public struct KeyModifier
    {
        /// <summary>
        /// Shift key is held down
        /// </summary>
        public bool ShiftKeyPressed;
        /// <summary>
        /// Alt key is held down
        /// </summary>
        public bool AltKeyPressed;
        /// <summary>
        /// Ctrl key is held down
        /// </summary>
        public bool CtrlKeyPressed;
    }

    #endregion

    #region Keyboard Enums
    /// <summary>
    /// An enum that defines the keyboard event type.
    /// </summary>
    public enum KeyboardEventType : byte{
        Press,
        Release,
        Type
    }

    #endregion

    /// <summary>
    /// A helper class for handling the keyboard input. This class wraps the functionalities provided
    /// by XNA's KeyboardState class. Keyboard events are handled based on interrupt method (callback
    /// functions), rather than the polling method (XNA's KeyboardState), so that the developer doesn't need to 
    /// poll the status of the keyboard state every frame by herself/himself.
    /// </summary>
    /// <example>
    /// An example of adding a keyboard type event handler:
    /// 
    /// KeyboardInput.Instance.KeyTypeEvent += new HandleKeyType(KeyTypeHandler);
    ///
    /// private void KeyTypeHandler(Microsoft.Xna.Framework.Input.Keys key, KeyModifier modifier)
    /// {
    ///    //Insert your key type handling code here
    ///    if(key == Keys.A)
    ///    {
    ///        ....
    ///    }
    /// }
    /// </example>
    /// <remarks>
    /// KeyboardInput is a singleton class, so you should access this class through Instance property.
    /// </remarks>
    public class KeyboardInput : InputDevice
    {
        #region Member Fields

        private bool isAvailable;
        /// <summary>
        /// Keyboard state, set every frame in the Update method.
        /// </summary>
        private KeyboardState keyboardState;

        /// <summary>
        /// Keys pressed last frame, for comparison if a key was just pressed.
        /// </summary>
        private List<Keys> keysPressedLastFrame;
        private List<Keys> keysBeingPressed;
        private List<KeyJustPressed> currentPressedKeys;
        private List<KeyJustPressed> releasedKeys;
        private List<Keys> releasedKeyList;

        private bool onlyTrackWhenFocused;

        private int initialRepetitionWait;
        private int repetitionWait;
        private int repetitionTime;

        private static KeyboardInput input;

        #endregion

        #region Events
        /// <summary>
        /// An event to add or remove key press delegate/callback functions
        /// </summary>
        public event HandleKeyPress KeyPressEvent;

        /// <summary>
        /// An event to add or remove key release delegate/callback functions
        /// </summary>
        public event HandleKeyRelease KeyReleaseEvent;

        /// <summary>
        /// An event to add or remove key type delegate/callback functions
        /// </summary>
        public event HandleKeyType KeyTypeEvent;
        #endregion

        #region Private Constructor
        /// <summary>
        /// A private constructor.
        /// </summary>
        /// <remarks>
        /// Don't instantiate this constructor.
        /// </remarks>
        private KeyboardInput()
        {
            keysPressedLastFrame = new List<Keys>();
            currentPressedKeys = new List<KeyJustPressed>();
            releasedKeys = new List<KeyJustPressed>();
            releasedKeyList = new List<Keys>();
            keysBeingPressed = new List<Keys>();
            keysPressedLastFrame = new List<Keys>();

            onlyTrackWhenFocused = true;

            initialRepetitionWait = 800;
            repetitionWait = 100;
            repetitionTime = 0;

            isAvailable = true;
        }

        #endregion

        #region Public Properties

        public String Identifier
        {
            get { return "Keyboard"; }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        /// <summary>
        /// Gets or sets whether to only handle keyboard events when the application window is focused.
        /// </summary>
        /// <remarks>Default value is true</remarks>
        public bool OnlyHandleWhenFocused
        {
            get { return onlyTrackWhenFocused; }
            set { onlyTrackWhenFocused = value; }
        }

        /// <summary>
        /// Gets or sets how long it should wait initially beforing repeating the same key type
        /// when a key is held down
        /// </summary>
        /// <remarks>The wait time is in milliseconds. Default value is 500 ms.</remarks>
        public int InitialRepetitionWait
        {
            get { return initialRepetitionWait; }
            set { initialRepetitionWait = value; }
        }

        /// <summary>
        /// Gets or sets how long it should wait after the initial wait time before repeating 
        /// the same key type when a key is held down
        /// </summary>
        /// <remarks>The wait time is in milliseconds. Default value is 100 ms.</remarks>
        public int RepetitionWait
        {
            get { return repetitionTime; }
            set { repetitionTime = value; }
        }

        /// <summary>
        /// Gets the instantiation of KeyboardInput class.
        /// </summary>
        public static KeyboardInput Instance
        {
            get
            {
                if (input == null)
                {
                    input = new KeyboardInput();
                }

                return input;
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Checks whether 'key' is a special key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys. 
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) || (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemSemicolon || // ;:
                key == Keys.OemQuotes || // '"
                key == Keys.OemComma || // ,<
                key == Keys.OemPeriod || // .>
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus) // =+
                return false;

            // Else is is a special key
            return true;
        }

        /// <summary>
        /// Checks whether 'key' is a modifier key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool IsKeyModifier(Keys key)
        {
            return (key == Keys.LeftShift) || (key == Keys.RightShift) ||
                (key == Keys.LeftAlt) || (key == Keys.RightAlt) ||
                (key == Keys.LeftControl) || (key == Keys.RightControl);
        }

        #endregion

        #region Public Helpers

        public static KeyModifier GetKeyModifier(List<Keys> keys)
        {
            KeyModifier modifier = new KeyModifier();

            if (keys.Contains(Keys.RightShift) || keys.Contains(Keys.LeftShift))
                modifier.ShiftKeyPressed = true;
            if (keys.Contains(Keys.RightAlt) || keys.Contains(Keys.LeftAlt))
                modifier.AltKeyPressed = true;
            if (keys.Contains(Keys.RightControl) || keys.Contains(Keys.LeftControl))
                modifier.CtrlKeyPressed = true;

            return modifier;
        }

        /// <summary>
        /// Convers a 'key' to a char.
        /// </summary>
        /// <remarks>
        /// If the keys are mapped other than on a default QWERTY keyboard, this method will not 
        /// work properly. Most keyboards will return the same for A-Z and 0-9, but the special 
        /// keys might be different. 
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="shiftPressed"></param>
        /// <returns></returns>
        public static char KeyToChar(Keys key, bool shiftPressed)
        {
            // If key will not be found, just return space
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            }
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 && shiftPressed == false)
                ret = (char)((int)'0' + (keyNum - Keys.D0));
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : '.';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : ',';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            return ret;
        }

        #endregion

        #region Update

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            // If the window is not on focus (only for the case of windowed version)
            // then we don't need to process the keyboard events
            if (onlyTrackWhenFocused && !deviceActive)
                return;

            // Update the current keyboard state
            keyboardState = Keyboard.GetState();
            keysBeingPressed.Clear();
            keysBeingPressed.AddRange(keyboardState.GetPressedKeys());
            // First remove weird keys being pressed without doing anything
            keysBeingPressed.Remove(Keys.Attn);
            keysBeingPressed.Remove(Keys.Zoom);
            KeyModifier modifier = GetKeyModifier(keysBeingPressed);

            // First handle repetition press that is handled as key type action
            if (currentPressedKeys.Count > 0)
            {
                KeyJustPressed keyPressed = currentPressedKeys[currentPressedKeys.Count - 1];

                if (keysPressedLastFrame.Contains(keyPressed.Key))
                {
                    repetitionTime += (int)elapsedTime.TotalMilliseconds;
                    bool processTypeEvent = false;

                    if (keyPressed.JustPressed)
                    {
                        if (repetitionTime >= initialRepetitionWait)
                        {
                            processTypeEvent = true;
                            repetitionTime = 0;
                            keyPressed.JustPressed = false;
                        }
                    }
                    else
                    {
                        if (repetitionTime >= repetitionWait)
                        {
                            processTypeEvent = true;
                            repetitionTime = 0;
                        }
                    }

                    if (processTypeEvent && (KeyTypeEvent != null))
                        KeyTypeEvent(keyPressed.Key, modifier);
                }
            }

            // HANDLE KEY RELEASE AND TYPE
            foreach (KeyJustPressed key in currentPressedKeys)
            {
                if (keysPressedLastFrame.Contains(key.Key) &&
                    !keysBeingPressed.Contains(key.Key))
                {
                    if(KeyReleaseEvent != null)
                        KeyReleaseEvent(key.Key, modifier);

                    if(KeyTypeEvent != null)
                        KeyTypeEvent(key.Key, modifier);

                    releasedKeys.Add(key);
                    releasedKeyList.Add(key.Key);
                }
            }

            foreach (KeyJustPressed key in releasedKeys)
                currentPressedKeys.Remove(key);
            releasedKeys.Clear();

            // HANDLE KEY PRESS
            foreach (Keys key in keysBeingPressed)
            {
                if (!releasedKeyList.Contains(key) && !keysPressedLastFrame.Contains(key) 
                    && !IsKeyModifier(key))
                {
                    currentPressedKeys.Add(new KeyJustPressed(key));

                    if(KeyPressEvent != null)
                        KeyPressEvent(key, modifier);
                }
            }

            releasedKeyList.Clear();

            keysPressedLastFrame.Clear();
            keysPressedLastFrame.AddRange(keysBeingPressed);
        }

        #endregion

        #region Private Class

        private class KeyJustPressed
        {
            public Keys Key;
            public bool JustPressed;

            public KeyJustPressed(Keys key)
            {
                this.Key = key;
                JustPressed = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Triggers the key event callback functions programatically with the given byte data
        /// array. Use the GetNetworkData(KeyboardEventType, Keys, KeyModifier) function to convert 
        /// each of the keyboard events and the necessary information (e.g., key) to a byte array.
        /// </summary>
        /// <see cref="GetNetworkData(KeyboardEventType, Keys, KeyModifier)"/>
        /// <param name="data">An array of bytes containing specific data used to trigger
        /// the key event callback functions</param>
        public void TriggerDelegates(byte[] data)
        {
            byte type = data[0];
            Keys key = (Keys)data[1];
            KeyModifier modifier = new KeyModifier();
            modifier.AltKeyPressed = BitConverter.ToBoolean(data, 2);
            modifier.CtrlKeyPressed = BitConverter.ToBoolean(data, 3);
            modifier.ShiftKeyPressed = BitConverter.ToBoolean(data, 4);

            switch (type)
            {
                case (byte)KeyboardEventType.Press:
                    if(KeyPressEvent != null)
                        KeyPressEvent(key, modifier);
                    break;
                case (byte)KeyboardEventType.Release:
                    if(KeyReleaseEvent != null)
                        KeyReleaseEvent(key, modifier);
                    break;
                case (byte)KeyboardEventType.Type:
                    if(KeyTypeEvent != null)
                        KeyTypeEvent(key, modifier);
                    break;
            }
        }

        /// <summary>
        /// Converts the keyboard event type, the key, and the modifier keys' information 
        /// to an array of bytes so that it can be sent over the network.
        /// </summary>
        /// <param name="type">Press, Release, or Type</param>
        /// <param name="key">The key</param>
        /// <param name="modifier">A struct that indicates whether any of the modifier 
        /// keys are held down</param>
        /// <returns></returns>
        public byte[] GetNetworkData(KeyboardEventType type, Keys key, KeyModifier modifier)
        {
            // 1 byte for type, 1 byte for key, and 1 (bool) * 3 bytes for modifier
            byte[] data = new byte[5];

            data[0] = (byte)type;
            data[1] = (byte)key;
            data[2] = BitConverter.GetBytes(modifier.AltKeyPressed)[0];
            data[3] = BitConverter.GetBytes(modifier.CtrlKeyPressed)[0];
            data[4] = BitConverter.GetBytes(modifier.ShiftKeyPressed)[0];

            return data;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        #endregion
    }
}
