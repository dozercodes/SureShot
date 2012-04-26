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

namespace GoblinXNA.Helpers.XNATweener
{
    /// <summary>
    /// <para>The Loop class is a static class for easy loop control of the Tweener.</para>
    /// <para>You can loop continuousely FrontToBack or BackAndForth or for a specific number of times.</para>
    /// <para>It can be used either by the static methods on this class or by the corresponding methods on the Tweener classes.</para>
    /// </summary>
    public static class Loop
    {
        #region Static methods
        public static void FrontToBack(ITweener tweener)
        {
            tweener.Ended += tweener.Restart;
        }

        public static void FrontToBack(ITweener tweener, int times)
        {
            TimesLoopingHelper helper = new TimesLoopingHelper(tweener, times);
            tweener.Ended += helper.FrontToBack;
        }

        public static void BackAndForth(ITweener tweener)
        {
            tweener.Ended += delegate { tweener.Reverse(); };
        }

        public static void BackAndForth(ITweener tweener, int times)
        {
            TimesLoopingHelper helper = new TimesLoopingHelper(tweener, times);
            tweener.Ended += helper.BackAndForth;
        }
        #endregion

        #region Internal classes
        private struct TimesLoopingHelper
        {
            public TimesLoopingHelper(ITweener tweener, int times)
            {
                this.tweener = tweener;
                this.times = times;
            }

            private int times;
            private ITweener tweener;

            private bool Stop()
            {
                return --times == 0;
            }

            public void FrontToBack()
            {
                if (Stop())
                {
                    tweener.Ended -= FrontToBack;
                }
                else
                {
                    tweener.Reset();
                }
            }

            public void BackAndForth()
            {
                if (Stop())
                {
                    tweener.Ended -= BackAndForth;
                }
                else
                {
                    tweener.Reverse();
                }
            }
        }
        #endregion
    }
}
