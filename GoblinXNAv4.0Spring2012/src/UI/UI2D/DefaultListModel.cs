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
    /// A default implementation of ListModel interface.
    /// </summary>
    public class DefaultListModel : ListModel
    {
        #region Member Fields

        protected List<object> elements;

        #endregion

        #region Events

        public event ContentsChanged ContentsChangedEvent;

        public event IntervalAdded IntervalAddedEvent;

        public event IntervalRemoved IntervalRemovedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a default implementation of ListModel interface with the specified object array.
        /// </summary>
        /// <param name="data">An array of object to contain in this list model.</param>
        public DefaultListModel(object[] data)
        {
            elements = new List<object>();
            if (data != null)
                elements.AddRange(data);
        }

        /// <summary>
        /// Creates a default implementation of ListModel interface with no data.
        /// </summary>
        public DefaultListModel() : this(null) { }

        #endregion

        #region Properties

        public List<object> Elements
        {
            get { return elements; }
        }

        #endregion

        #region Public Methods

        public void Add(Object element)
        {
            elements.Add(element);

            InvokeIntervalAddedEvent(this, elements.Count - 1, elements.Count - 1);
        }

        public void Insert(int index, Object element)
        {
            if (index < 0 || index >= elements.Count)
                throw new ArgumentException("Index out of range: " + index);

            elements.Insert(index, element);

            InvokeIntervalAddedEvent(this, index, index);
        }

        public void InsertRange(object[] eles, int startIndex)
        {
            if(startIndex < 0 || startIndex > elements.Count)
                throw new ArgumentException("Index out of range: " + startIndex);

            elements.InsertRange(startIndex, eles);

            InvokeIntervalAddedEvent(this, startIndex, startIndex + eles.Length - 1);
        }

        public bool Remove(Object element)
        {
            int index = elements.IndexOf(element);

            if (index >= 0)
            {
                return RemoveAt(index);
            }
            else
                return false;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= elements.Count)
                return false;

            elements.RemoveAt(index);

            if (IntervalRemovedEvent != null)
                IntervalRemovedEvent(this, index, index);

            return true;
        }

        public void RemoveAll()
        {
            int lastIndex = elements.Count - 1;

            elements.Clear();

            InvokeIntervalRemovedEvent(this, 0, lastIndex);
        }

        public void RemoveRange(int startIndex, int length)
        {
            if (startIndex < 0 || (startIndex + length) >= elements.Count)
                throw new ArgumentException("Index out of range: " + startIndex);

            elements.RemoveRange(startIndex, length);

            InvokeIntervalRemovedEvent(this, startIndex, startIndex + length - 1);
        }

        public void Replace(int index, object element)
        {
            if (index < 0 || index >= elements.Count)
                throw new ArgumentException("Index out of range: " + index);

            elements[index] = element;

            InvokeContentsChangedEvent(this, index, index);
        }

        public void ReplaceRange(object[] eles, int startIndex)
        {
            if (startIndex < 0 || (startIndex + eles.Length) >= elements.Count)
                throw new ArgumentException("Index out of range: " + startIndex);

            int lastIndex = startIndex + eles.Length;

            for (int i = startIndex; i < lastIndex; i++)
                elements[i] = eles[i - startIndex];

            InvokeContentsChangedEvent(this, startIndex, lastIndex - 1);
        }

        public void InvokeContentsChangedEvent(object source, int index0, int index1)
        {
            if (ContentsChangedEvent != null)
                ContentsChangedEvent(source, index0, index1);
        }

        public void InvokeIntervalAddedEvent(object source, int index0, int index1)
        {
            if (IntervalAddedEvent != null)
                IntervalAddedEvent(source, index0, index1);
        }

        public void InvokeIntervalRemovedEvent(object source, int index0, int index1)
        {
            if (IntervalRemovedEvent != null)
                IntervalRemovedEvent(source, index0, index1);
        }

        #endregion
    }
}
