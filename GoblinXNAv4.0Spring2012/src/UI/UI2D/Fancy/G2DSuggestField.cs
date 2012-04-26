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

namespace GoblinXNA.UI.UI2D.Fancy
{
    #region Event Delegates

    /// <summary>
    /// Invoked when an item in the suggestion list is selected. 
    /// </summary>
    /// <param name="suggestBox">The G2DList class that invoked this delegate function.</param>
    public delegate void SuggestBoxStateChanged(G2DList suggestBox);

    #endregion

    /// <summary>
    /// A suggestion field is a text field that provides a list of possible text inputs such as search terms 
    /// in a drop down box every time the user modifies a text in the field. If the user selects one of 
    /// the texts in the suggestion list, the text in the text field will be replaced with the selected one.
    /// The suggestion list disappears either if a selection is made or the user presses 'Enter' key. 
    /// </summary>
    public class G2DSuggestField : G2DTextField
    {
        #region Enum

        /// <summary>
        /// An enum that specifies a text matching criteria.
        /// </summary>
        public enum MatchCriteria 
        { 
            StartsWith, 
            Contains, 
            Custom 
        }

        #endregion

        #region Delegate

        /// <summary>
        /// A delegate function that tests whether a text 'b' is part of text 'a'.
        /// </summary>
        /// <param name="textToMatch"></param>
        /// <param name="textToBeMatched"></param>
        /// <returns></returns>
        public delegate bool IsPartOf(String a, String b);

        #endregion

        #region Member Fields

        protected G2DList suggestList;
        protected List<string> data;
        protected int maxSuggestions;
        protected List<string> matches;

        protected MatchCriteria criteria;
        protected IsPartOf isPartOf;
        protected bool keepSuggestionListOpaque;

        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever a new item is selected in the suggestion list.
        /// </summary>
        public event SuggestBoxStateChanged SuggestBoxStateChangedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a suggestion field without a list of possible text inputs.
        /// </summary>
        public G2DSuggestField()
            : this(new List<string>()) { }

        /// <summary>
        /// Creates a suggestion field with a list of possible text inputs and maximum of eight
        /// suggested texts to display in the drop down box.
        /// </summary>
        /// <param name="suggestionList">A list of possible text inputs.</param>
        public G2DSuggestField(String[] suggestionList)
            : this(new List<string>(suggestionList), 8) { }

        /// <summary>
        /// Creates a suggestion field with a list of possible text inputs and maximum of eight
        /// suggested texts to display in the drop down box.
        /// </summary>
        /// <param name="suggestionList">A list of possible text inputs.</param>
        public G2DSuggestField(List<String> suggestionList)
            : this(suggestionList, 8) { }

        /// <summary>
        /// Creates a suggestion field with a list of possible text inputs and the maximum number of 
        /// suggested texts to display in the drop down box.
        /// </summary>
        /// <param name="suggestionList">A list of possible text inputs.</param>
        /// <param name="maxSuggestions">The maximum number of suggested texts to display in
        /// the drop down box.</param>
        public G2DSuggestField(List<String> suggestionList, int maxSuggestions) 
            : base()
        {
            this.data = suggestionList;
            this.maxSuggestions = maxSuggestions;

            suggestList = new G2DList();
            suggestList.Visible = false;

            matches = new List<string>();
            criteria = MatchCriteria.Contains;
            isPartOf = null;
            keepSuggestionListOpaque = true;

            suggestList.SelectionModel.ValueChangedEvent += new ValueChanged(ItemSelected);

            name = "G2DSuggestField";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list of possible text inputs.
        /// </summary>
        public List<string> SuggestionList
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of suggested texts to display.
        /// </summary>
        public virtual int MaxSuggestions
        {
            get { return maxSuggestions; }
            set { maxSuggestions = value; }
        }

        /// <summary>
        /// Gets or sets the criteria for matching the entered text with the list of possible text inputs
        /// in order to suggest a possible input. The default value is MatchCriteria.Contains.
        /// </summary>
        public virtual MatchCriteria Criteria
        {
            get { return criteria; }
            set { criteria = value; }
        }

        /// <summary>
        /// Gets or sets a matching function used to identify which texts from the list of possible text
        /// inputs should be displayed based on the current text typed in the text field. This matching
        /// function is used only if Criteria property is set to MatchCriteria.Custom.
        /// </summary>
        public virtual IsPartOf MatchFunction
        {
            set { isPartOf = value; }
        }

        /// <summary>
        /// Gets or sets whether to keep the suggestion box to render in opaque background. If set to false,
        /// then the suggestion box will have the same transparency value as this component. If true, the
        /// suggestion box will remain opaque even if this component's transparency value is modified. The 
        /// default value is true.
        /// </summary>
        /// <remarks>
        /// If there are other UI components positioned below this component, then it's better to set this 
        /// property to true.
        /// </remarks>
        public virtual bool KeepSuggestionBoxOpaque
        {
            get { return keepSuggestionListOpaque; }
            set 
            { 
                keepSuggestionListOpaque = value;

                if (value)
                {
                    suggestList.Transparency = 1;
                    suggestList.TextTransparency = 1;
                }
                else
                {
                    suggestList.Transparency = this.Transparency;
                    suggestList.TextTransparency = this.TextTransparency;
                }
            }
        }

        #endregion

        #region Override Properties

        public override float Transparency
        {
            get
            {
                return base.Transparency;
            }
            set
            {
                base.Transparency = value;
                if(!keepSuggestionListOpaque)
                    suggestList.Transparency = value;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }
            set
            {
                base.BackgroundColor = value;
                suggestList.BackgroundColor = value;
            }
        }

        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
                suggestList.BorderColor = value;
            }
        }

        public override Color DisabledColor
        {
            get
            {
                return base.DisabledColor;
            }
            set
            {
                base.DisabledColor = value;
                suggestList.DisabledColor = value;
            }
        }

        public override bool DrawBackground
        {
            get
            {
                return base.DrawBackground;
            }
            set
            {
                base.DrawBackground = value;
                suggestList.DrawBackground = value;
            }
        }

        public override bool DrawBorder
        {
            get
            {
                return base.DrawBorder;
            }
            set
            {
                base.DrawBorder = value;
                suggestList.DrawBorder = value;
            }
        }

        public override GoblinEnums.HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return base.HorizontalAlignment;
            }
            set
            {
                base.HorizontalAlignment = value;
                suggestList.HorizontalAlignment = value;
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;

                suggestList.Bounds = new Rectangle(paintBounds.X, paintBounds.Y + paintBounds.Height + 2,
                    paintBounds.Width, 0); 
            }
        }

        public override Component Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;

                suggestList.Bounds = new Rectangle(paintBounds.X, paintBounds.Y + paintBounds.Height + 2,
                    paintBounds.Width, 0);
            }
        }

        public override SpriteFont TextFont
        {
            get
            {
                return base.TextFont;
            }
            set
            {
                base.TextFont = value;
                suggestList.TextFont = value;
            }
        }

        public override Color TextColor
        {
            get
            {
                return base.TextColor;
            }
            set
            {
                base.TextColor = value;
                suggestList.TextColor = value;
            }
        }

        public override float TextTransparency
        {
            get
            {
                return base.TextTransparency;
            }
            set
            {
                base.TextTransparency = value;
                if(!keepSuggestionListOpaque)
                    suggestList.TextTransparency = value;
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;

                if(textFont == null || data.Count == 0)
                    return;

                suggestList.Model.RemoveAll();

                bool prevVisibility = suggestList.Visible;

                if (label.Length > 0)
                {
                    matches.Clear();

                    foreach (String str in data)
                    {
                        switch (criteria)
                        {
                            case MatchCriteria.StartsWith:
                                if (str.StartsWith(label))
                                    matches.Add(str);
                                break;
                            case MatchCriteria.Contains:
                                if (str.Contains(label))
                                    matches.Add(str);
                                break;
                            case MatchCriteria.Custom:
                                if (isPartOf != null && isPartOf(str, label))
                                    matches.Add(str);
                                break;
                        }

                        if (matches.Count >= maxSuggestions)
                            break;
                    }

                    if (matches.Count > 0)
                    {
                        matches.Sort();

                        int longest = 0;
                        String longestStr = "";

                        foreach (String str in matches)
                        {
                            if (str.Length > longest)
                            {
                                longest = str.Length;
                                longestStr = str;
                            }
                        }

                        int longestStrLength = (int)(textFont.MeasureString(longestStr).X) + 12;

                        int width = Math.Max(longestStrLength, this.Bounds.Width);
                        int height = matches.Count * (int)(textFont.MeasureString("A").Y + 2) + 4;

                        suggestList.Bounds = new Rectangle(suggestList.Bounds.X, suggestList.Bounds.Y, width, height);

                        suggestList.Model.InsertRange(matches.ToArray(), 0);

                        suggestList.Visible = true;
                    }
                    else
                        suggestList.Visible = false;
                }
                else
                    suggestList.Visible = false;

                if (prevVisibility != suggestList.Visible)
                    InvokeSuggestBoxStateChangedEvent();
            }
        }

        #endregion

        #region Override Methods

        internal override void RegisterKeyInput()
        {
            base.RegisterKeyInput();

            suggestList.RegisterKeyInput();
        }

        internal override void RemoveKeyInput()
        {
            base.RemoveKeyInput();

            suggestList.RemoveKeyInput();
        }

        internal override void RegisterMouseInput()
        {
            base.RegisterMouseInput();

            suggestList.RegisterMouseInput();
        }

        internal override void RemoveMouseInput()
        {
            base.RemoveMouseInput();

            suggestList.RemoveMouseInput();
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            suggestList.RenderWidget();
        }

        #endregion

        #region Protected Methods

        protected void ItemSelected(object src, SelectionType type, int firstIndex, int lastIndex, bool isAdjusting)
        {
            if (type == SelectionType.Selection)
            {
                base.Text = (String)suggestList.Model.Elements[firstIndex];

                UpdateCaretPosition();

                suggestList.Visible = false;
                suggestList.SelectionModel.ClearSelection();

                InvokeSuggestBoxStateChangedEvent();
            }
        }

        protected void InvokeSuggestBoxStateChangedEvent()
        {
            if (SuggestBoxStateChangedEvent != null)
                SuggestBoxStateChangedEvent(suggestList);
        }

        #endregion
    }
}
