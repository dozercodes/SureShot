/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
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
using System.Xml;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that acts as a "switch" among its children. Only one child is
    /// traversed by the scene graph tree at any time.
    /// </summary>
    public class SwitchNode : BranchNode
    {
        #region Member Fields

        protected int switchID;
        protected bool switchChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a scene graph node that acts as a "switch" with a specified node name.
        /// </summary>
        /// <param name="name">The name of this node</param>
        public SwitchNode(String name)
            : base(name)
        {
            switchID = -1;
            switchChanged = false;
        }

        /// <summary>
        /// Creates a scene graph node that acts as a "switch".
        /// </summary>
        public SwitchNode() : this("") { }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the currently traversed child index.
        /// </summary>
        /// <remarks>
        /// This ID is not the actual ID of the child node, but the index based on the order of
        /// addition. The default value is 0, which indicates the 1st child.
        /// </remarks>
        public int SwitchID
        {
            get { return switchID; }
            set
            {
                if (value < 0 || value >= children.Count)
                    return;
                else
                {
                    switchID = value;
                    switchChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the "switch" has been switched.
        /// </summary>
        internal bool SwitchChanged
        {
            get { return switchChanged; }
            set { switchChanged = value; }
        }

        #endregion

        #region Override Methods

        public override void AddChild(Node node)
        {
            base.AddChild(node);

            if (SwitchID < 0)
                SwitchID = 0;
        }

        public override Node CloneNode()
        {
            SwitchNode clone = (SwitchNode)base.CloneNode();
            clone.SwitchID = switchID;

            return clone;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("SwitchID", switchID.ToString());

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("SwitchID"))
                switchID = int.Parse(xmlNode.GetAttribute("SwitchID"));
        }
#endif

        #endregion
    }
}
