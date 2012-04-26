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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// 
    /// </summary>
    public interface ListModel
    {
        #region Properties

        /// <summary>
        /// Gets a list of elements in this model.
        /// </summary>
        List<object> Elements { get; }

        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever the content of this model changes.
        /// </summary>
        event ContentsChanged ContentsChangedEvent;

        /// <summary>
        /// An event triggered whenever a list of items is added to this model.
        /// </summary>
        event IntervalAdded IntervalAddedEvent;

        /// <summary>
        /// An event triggered whenever a list of items is removed from this model.
        /// </summary>
        event IntervalRemoved IntervalRemovedEvent;

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified element to the end of this list.
        /// </summary>
        /// <param name="element">The element to be added to the end of this list.</param>
        void Add(object element);

        /// <summary>
        /// Inserts the specified element at the specified position in this list.
        /// </summary>
        /// <param name="index">The position in this list to insert.</param>
        /// <param name="element">The element to be inserted.</param>
        void Insert(int index, object element);

        /// <summary>
        /// Inserts a list of elements to this list from the specified start position.
        /// </summary>
        /// <param name="eles">The list of elements to be inserted.</param>
        /// <param name="startIndex">The start position in this list to insert a list of elements.</param>
        void InsertRange(object[] eles, int startIndex);

        /// <summary>
        /// Removes the first (lowest-indexed) occurrence of the element from this list.
        /// </summary>
        /// <param name="element">The element to be removed from this list.</param>
        /// <returns>Whether it successfully removed this element from this list.</returns>
        bool Remove(object element);

        /// <summary>
        /// Removes the element at the specified position in this list.
        /// </summary>
        /// <param name="index">The position to remove an element.</param>
        /// <returns>Whether it successfully removed this element from this list.</returns>
        bool RemoveAt(int index);

        /// <summary>
        /// Removes all elements from this list.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Removes the elements at the specified range of indices.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        void RemoveRange(int startIndex, int length);

        /// <summary>
        /// Replaces the element at the specified position in this list with the specified element.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="element"></param>
        void Replace(int index, object element);

        /// <summary>
        /// Replaces the elements at the specified range of indices with the specified elements.
        /// </summary>
        /// <param name="eles"></param>
        /// <param name="startIndex"></param>
        void ReplaceRange(object[] eles, int startIndex);

        /// <summary>
        /// Invokes the content changed event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        /// <param name="index0">The starting index in the list where content is changed.</param>
        /// <param name="index1">The ending index in the list where content is changed.</param>
        void InvokeContentsChangedEvent(object source, int index0, int index1);

        /// <summary>
        /// Invokes the interval added event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        /// <param name="index0">The starting index in the list where a list of items is added.</param>
        /// <param name="index1">The ending index in the list where a list items is added.</param>
        void InvokeIntervalAddedEvent(object source, int index0, int index1);

        /// <summary>
        /// Invokes the interval removed event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        /// <param name="index0">The starting index in the list where a list of items is removed.</param>
        /// <param name="index1">The ending index in the list where a list of items is removed.</param>
        void InvokeIntervalRemovedEvent(object source, int index0, int index1);

        #endregion
    }
}
