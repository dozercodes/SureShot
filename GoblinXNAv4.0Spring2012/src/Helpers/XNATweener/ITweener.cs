/************************************************************************************ 
 * Copyright (c) 2008, Thomas Graavard
 * All rights reserved.
 *
 * Used under license by the terms and conditions specified by the Microsoft Public License (Ms-PL)
 * 
 * For more information, please go to http://xnatweener.codeplex.com/license
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
 * Author: Thomas Graavard
 * Incorporated into GoblinXNA by Nicolas Dedual (dedual@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GoblinXNA.Helpers.XNATweener
{
    public delegate void PositionChangedHandler<T>(T newPosition);
    public delegate void EndHandler();

    public interface ITweener
    {
        [Obsolete("Use Playing property instead")]
        bool Running { get;}
        bool Playing { get;}
        event EndHandler Ended;

        void Update(GameTime gameTime);
        [Obsolete("Use Play method instead")]
        void Start();
        [Obsolete("Use Pause method instead")]
        void Stop();
        void Play();
        void Pause();
        void Reset();
        void Restart();
        void Reverse();
    }

    public interface ITweener<T> : ITweener
    {
        T Position { get;}
        event PositionChangedHandler<T> PositionChanged;

        void Reset(T to);
        void Reset(T to, TimeSpan duration);
        void Reset(T to, float speed);
        void Reset(T from, T to, TimeSpan duration);
        void Reset(T from, T to, float speed);
    }
}
