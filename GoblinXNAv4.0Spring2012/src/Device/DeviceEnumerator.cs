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

using GoblinXNA.Device.Generic;

namespace GoblinXNA.Device
{
    /// <summary>
    /// An enumerator for all of the available input devices including 2D input devices
    /// (e.g., keyboard, mouse) and 6DOF input devices (e.g., InterSense tracker).
    /// </summary>
    /// <remarks>
    /// It's possible to access each device class directly, but this class provides 
    /// simple APIs to access the available devices
    /// </remarks>
    internal class DeviceEnumerator : IDisposable
    {
        protected Dictionary<String, InputDevice> availableDevices;
        protected Dictionary<String, InputDevice_6DOF> available6DOFDevices;

        protected Dictionary<String, InputDevice> additionalDevices;
        protected Dictionary<String, InputDevice_6DOF> additional6DOFDevices;

        protected bool updating;
        protected bool enumerateLater;

        /// <summary>
        /// Creates a device enumerator and enumerates all of the available input devices
        /// </summary>
        /// <remarks>
        /// This constructor calls Reenumerate method, so you don't need to call it in your
        /// code for initial enumeration.
        /// </remarks>
        /// <seealso cref="Reenumerate"/>
        public DeviceEnumerator()
        {
            availableDevices = new Dictionary<String, InputDevice>();
            available6DOFDevices = new Dictionary<String, InputDevice_6DOF>();
            additionalDevices = new Dictionary<string, InputDevice>();
            additional6DOFDevices = new Dictionary<string, InputDevice_6DOF>();
            updating = false;
            enumerateLater = false;
            Reenumerate();
        }

        /// <summary>
        /// Gets all of the available non-6DOF input devices (e.g., mouse, keyboard)
        /// </summary>
        public Dictionary<String, InputDevice> AvailableDevices
        {
            get { return availableDevices; }
        }

        /// <summary>
        /// Gets all of the available 6DOF input devices (e.g., InterSense tracker)
        /// </summary>
        public Dictionary<String, InputDevice_6DOF> Available6DOFDevices
        {
            get { return available6DOFDevices; }
        }

        public Dictionary<String, InputDevice> AdditionalDevices
        {
            get { return additionalDevices; }
        }

        public Dictionary<String, InputDevice_6DOF> Additional6DOFDevices
        {
            get { return additional6DOFDevices; }
        }

        /// <summary>
        /// Reenumerates all of the available input devices.
        /// </summary>
        /// <remarks>
        /// The constructor calls this method, so you should only call this method if you plugged/unplugged
        /// any input devices after calling the constructor
        /// </remarks>
        public void Reenumerate()
        {
            if (!updating)
            {
                availableDevices.Clear();
                available6DOFDevices.Clear();

#if !XBOX
                // Add all of the non-6DOF input devices if available
                MouseInput mouseInput = MouseInput.Instance;
                if (mouseInput.IsAvailable)
                    availableDevices.Add(mouseInput.Identifier, mouseInput);

                KeyboardInput keyboardInput = KeyboardInput.Instance;
                if (keyboardInput.IsAvailable)
                    availableDevices.Add(keyboardInput.Identifier, keyboardInput);

                GenericInput genericInput = GenericInput.Instance;
                if (genericInput.IsAvailable)
                    available6DOFDevices.Add(genericInput.Identifier, genericInput);

#endif

                foreach (InputDevice device in additionalDevices.Values)
                    if (device.IsAvailable)
                        availableDevices.Add(device.Identifier, device);

                foreach (InputDevice_6DOF device in additional6DOFDevices.Values)
                    if (device.IsAvailable)
                        available6DOFDevices.Add(device.Identifier, device);
            }
            else
                enumerateLater = true;
        }

        /// <summary>
        /// Updates all of the input devices' status
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="deviceActive"></param>
        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            updating = true;

            foreach (InputDevice inputDevice in availableDevices.Values)
                inputDevice.Update(elapsedTime, deviceActive);

            foreach (InputDevice_6DOF inputDevice6DOF in available6DOFDevices.Values)
                inputDevice6DOF.Update(elapsedTime, deviceActive);

            updating = false;

            if (enumerateLater)
            {
                Reenumerate();
                enumerateLater = false;
            }
        }

        public void Dispose()
        {
            availableDevices.Clear();
            available6DOFDevices.Clear();
            additionalDevices.Clear();
            additional6DOFDevices.Clear();
        }
    }
}
