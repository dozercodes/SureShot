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

namespace GoblinXNA.Helpers.XNATweener
{
    public static class Bounce
    {
        public static float EaseOut(float t, float b, float c, float d)
        {
            if ((t /= d) < (1 / 2.75))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
            }
            else if (t < (2.5 / 2.75))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }
        }

        public static float EaseIn(float t, float b, float c, float d)
        {
            return c - EaseOut(d - t, 0, c, d) + b;
        }

        public static float EaseInOut(float t, float b, float c, float d)
        {
            if (t < d / 2) return EaseIn(t * 2, 0, c, d) * 0.5f + b;
            else return EaseOut(t * 2 - d, 0, c, d) * .5f + c * 0.5f + b;
        }
    }
}
