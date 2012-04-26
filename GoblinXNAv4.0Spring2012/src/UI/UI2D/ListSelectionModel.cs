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
    #region Enum

    /// <summary>
    /// An enum that specifies the selection mode of the items in a list.
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// A mode of being able to select one or more contiguous ranges of indices at a time.
        /// </summary>
        MultipleInterval,

        /// <summary>
        /// A mode of being able to select one contiguous range of indices at a time.
        /// </summary>
        SingleInterval,

        /// <summary>
        /// A mode of being able to select one list index at a time.
        /// </summary>
        Single
    }

    #endregion

    /// <summary>
    /// An interface that defines the selection model of a list component such as G2DList and G2DComboBox.
    /// </summary>
    public interface ListSelectionModel
    {
        #region Events

        /// <summary>
        /// An event triggered whenever the selection status changes.
        /// </summary>
        event ValueChanged ValueChangedEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the first index argument from the most recent call to setSelectionInterval(), 
        /// addSelectionInterval() or removeSelectionInterval().
        /// </summary>
        int AnchorSelectionIndex { get; set; }

        /// <summary>
        /// Gets the second index argument from the most recent call to setSelectionInterval(), 
        /// addSelectionInterval() or removeSelectionInterval().
        /// </summary>
        int LeadSelectionIndex { get; set; }

        /// <summary>
        /// Gets the last selected index or -1 if the selection is empty.
        /// </summary>
        int MaxSelectionIndex { get; }

        /// <summary>
        /// Gets  the first selected index or -1 if the selection is empty.
        /// </summary>
        int MinSelectionIndex { get; }

        /// <summary>
        /// Gets or sets the current selection mode.
        /// </summary>
        SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Indicates whether the value is undergoing a series of changes.
        /// </summary>
        bool ValueIsAdjusting { get; set; }

        /// <summary>
        /// Gets a list of indecies in the list that are selected.
        /// </summary>
        List<int> SelectedIndices { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the selection to be the set union of the current selection and the indices between
        /// startIndex and endIndex inclusive.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        void AddSelectionInterval(int startIndex, int endIndex);

        /// <summary>
        /// Changes the selection to the empty set.
        /// </summary>
        void ClearSelection();

        /// <summary>
        /// Changes the selection to be the set difference of the current selection and the indices between 
        /// startIndex and endIndex inclusive.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        void RemoveSelectionInterval(int startIndex, int endIndex);

        /// <summary>
        /// Changes the selection to be between startIndex and endIndex inclusive.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        void SetSelectionInterval(int startIndex, int endIndex);

        /// <summary>
        /// Invokes the value changed event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        /// <param name="type">The type of selection.</param>
        /// <param name="index0">The starting index in the list where the selection status changed.</param>
        /// <param name="index1">The ending index in the list where the selection status changed.</param>
        /// <param name="isAdjusting">Whether upcoming changes to the value of the model should 
        /// be considered a single event.</param>
        void InvokeValueChangedEvent(object source, SelectionType type, int index0, int index1, bool isAdjusting);

        #endregion
    }
}
