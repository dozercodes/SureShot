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
    /// A default implementation of ListSelectionModel interface.
    /// </summary>
    public class DefaultListSelectionModel : ListSelectionModel
    {
        #region Member Fields

        protected List<int> selectedIndices;

        protected int anchorIndex;
        protected int leadIndex;
        protected int maxIndex;
        protected int minIndex;

        protected SelectionMode mode;
        protected bool isAdjusting;

        #endregion

        #region Events

        public event ValueChanged ValueChangedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a default implementation of ListSelectionModel interface.
        /// </summary>
        public DefaultListSelectionModel()
        {
            selectedIndices = new List<int>();

            anchorIndex = -1;
            leadIndex = -1;
            maxIndex = -1;
            minIndex = -1;

            mode = SelectionMode.Single;
            isAdjusting = false;
        }

        #endregion

        #region Properties

        public int AnchorSelectionIndex
        {
            get { return anchorIndex; }
            set { anchorIndex = value; }
        }

        public int LeadSelectionIndex
        {
            get { return leadIndex; }
            set { leadIndex = value; }
        }

        public int MaxSelectionIndex
        {
            get { return maxIndex; }
        }

        public int MinSelectionIndex
        {
            get { return minIndex; }
        }

        public SelectionMode SelectionMode
        {
            get { return mode; }
            set { mode = value; }
        }

        public bool ValueIsAdjusting
        {
            get { return isAdjusting; }
            set { isAdjusting = value; }
        }

        public List<int> SelectedIndices
        {
            get { return selectedIndices; }
        }

        #endregion

        #region Public Methods

        public void AddSelectionInterval(int startIndex, int endIndex)
        {
            anchorIndex = startIndex;
            leadIndex = endIndex;

            if (startIndex < minIndex)
                minIndex = startIndex;
            if (endIndex > maxIndex)
                maxIndex = endIndex;

            for (int i = startIndex; i <= endIndex; i++)
                if(!selectedIndices.Contains(i))
                    selectedIndices.Add(i);

            InvokeValueChangedEvent(this, SelectionType.Selection, startIndex, endIndex, isAdjusting);
        }

        public void ClearSelection()
        {
            selectedIndices.Clear();

            maxIndex = -1;
            minIndex = -1;
        }

        public void RemoveSelectionInterval(int startIndex, int endIndex)
        {
            anchorIndex = startIndex;
            leadIndex = endIndex;

            for (int i = startIndex; i <= endIndex; i++)
                selectedIndices.Remove(i);

            selectedIndices.Sort();

            if (selectedIndices.Count > 0)
            {
                minIndex = selectedIndices[0];
                maxIndex = selectedIndices[selectedIndices.Count - 1];
            }
            else
            {
                minIndex = -1;
                maxIndex = -1;
            }

            InvokeValueChangedEvent(this, SelectionType.Deselection, startIndex, endIndex, isAdjusting);
        }

        public void SetSelectionInterval(int startIndex, int endIndex)
        {
            anchorIndex = startIndex;
            leadIndex = endIndex;

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (selectedIndices.Contains(i))
                    selectedIndices.Remove(i);
                else
                    selectedIndices.Add(i);
            }

            selectedIndices.Sort();

            if (selectedIndices.Count > 0)
            {
                minIndex = selectedIndices[0];
                maxIndex = selectedIndices[selectedIndices.Count - 1];
            }
            else
            {
                minIndex = -1;
                maxIndex = -1;
            }

            InvokeValueChangedEvent(this, SelectionType.Mixed, startIndex, endIndex, isAdjusting);
        }

        public void InvokeValueChangedEvent(object source, SelectionType type, int index0, int index1, 
            bool isAdjusting)
        {
            if (ValueChangedEvent != null)
                ValueChangedEvent(source, type, index0, index1, isAdjusting);
        }

        #endregion
    }
}
