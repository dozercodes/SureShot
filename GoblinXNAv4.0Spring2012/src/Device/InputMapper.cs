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

using GoblinXNA.Network;

namespace GoblinXNA.Device
{
    /// <summary>
    /// A class that maps all of the available input devices to a set of unified functions.
    /// </summary>
    /// <remarks>
    /// InputMapper is a singleton class, so you should access this class through Instance property.
    /// </remarks>
    public class InputMapper : IDisposable
    {
        private DeviceEnumerator enumerator;

        private static InputMapper mapper;

        /// <summary>
        /// A private constructor.
        /// </summary>
        /// <remarks>
        /// Don't instantiate this.
        /// </remarks>
        private InputMapper()
        {
            enumerator = new DeviceEnumerator();
        }

        /// <summary>
        /// Gets the instantiation of InputMapper class.
        /// </summary>
        public static InputMapper Instance
        {
            get
            {
                if (mapper == null)
                    mapper = new InputMapper();

                return mapper;
            }
        }

        /// <summary>
        /// Gets the world transformation of a 6DOF input device with the given string identifier.
        /// </summary>
        /// <param name="identifier">A string identifier for a 6DOF input device</param>
        /// <returns></returns>
        public Matrix GetWorldTransformation(String identifier)
        {
            if (!enumerator.Available6DOFDevices.ContainsKey(identifier))
                return Matrix.Identity;
            else
                return enumerator.Available6DOFDevices[identifier].WorldTransformation;
        }

        /// <summary>
        /// Triggers a delegate/callback function defined in a non-6DOF input device with the given
        /// string identifier by passing an array of bytes that contains data in certain format.
        /// For the specific data format, please see each of the TriggerDelegates(byte[]) 
        /// functions implemented in each class that implements InputDevice interface
        /// (e.g., MouseInput).
        /// </summary>
        /// <param name="identifier">A string identifier for a non-6DOF input device</param>
        /// <param name="data"></param>
        public void TriggerInputDeviceDelegates(String identifier, byte[] data)
        {
            if (enumerator.AvailableDevices.ContainsKey(identifier))
                enumerator.AvailableDevices[identifier].TriggerDelegates(data);
        }

        /// <summary>
        /// Indicates whether a non-6DOF input device with the given string identifier is available.
        /// </summary>
        /// <param name="identifier">A string identifier for a non-6DOF input device</param>
        /// <returns></returns>
        public bool ContainsInputDevice(String identifier)
        {
            return enumerator.AvailableDevices.ContainsKey(identifier);
        }

        /// <summary>
        /// Indicates whether a 6DOF input device with the given string identifier is available.
        /// </summary>
        /// <param name="identifier">A string identifier for a 6DOF input device</param>
        /// <returns></returns>
        public bool Contains6DOFInputDevice(String identifier)
        {
            return enumerator.Available6DOFDevices.ContainsKey(identifier);
        }

        /// <summary>
        /// Adds an input device to be enumerated.
        /// </summary>
        /// <param name="device">An input device to be added</param>
        /// <exception cref="GoblinException">If duplicate device identifier exists</exception>
        public void AddInputDevice(InputDevice device)
        {
            if (!enumerator.AdditionalDevices.ContainsKey(device.Identifier))
                enumerator.AdditionalDevices.Add(device.Identifier, device);
            else
                throw new GoblinException("The identifier: " + device.Identifier + " is already used");
        }

        /// <summary>
        /// Adds a 6DOF input device to be enumerated.
        /// </summary>
        /// <param name="device">A 6DOF input device to be added</param>
        /// <exception cref="GoblinException">If duplicate device identifier exists</exception>
        public void Add6DOFInputDevice(InputDevice_6DOF device)
        {
            if (!enumerator.Additional6DOFDevices.ContainsKey(device.Identifier))
                enumerator.Additional6DOFDevices.Add(device.Identifier, device);
            else
                throw new GoblinException("The identifier: " + device.Identifier + " is already used");
        }

        /// <summary>
        /// Reenumerates all of the available input devices. You should call this function after you
        /// add your own input devices.
        /// </summary>
        public void Reenumerate()
        {
            enumerator.Reenumerate();
        }

        /// <summary>
        /// Updates all of the status of the available 6DOF and non-6DOF input devices.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="deviceActive"></param>
        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            enumerator.Update(elapsedTime, deviceActive);
        }

        /// <summary>
        /// Disposes all of the enumerated 6DOF and non-6DOF input devices.
        /// </summary>
        public void Dispose()
        {
            foreach (InputDevice device in enumerator.AvailableDevices.Values)
                device.Dispose();

            foreach (InputDevice_6DOF device in enumerator.Available6DOFDevices.Values)
                device.Dispose();

            enumerator.Dispose();
        }
    }
}
