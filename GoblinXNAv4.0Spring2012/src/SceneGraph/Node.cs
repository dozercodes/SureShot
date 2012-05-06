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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;

using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// This class defines the properties and methods required for a scene graph node.
    /// </summary>
    public class Node
    {
        #region Fields

        protected int id;
        protected int groupID;
        protected String name;
        protected bool enabled;

        protected Scene scene;
        protected Node parent;
        protected Object userData;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a scene graph node with the specified node name.
        /// </summary>
        /// <param name="name">The name of this node</param>
        public Node(String name)
        {
            this.name = name;
            id = State.GetNextNodeID();
            groupID = 0;
            enabled = true;

            parent = null;

            scene = null;
            userData = null;
        }

        /// <summary>
        /// Creates a scene graph node with no node name.
        /// </summary>
        public Node() : this("") { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of this scene node. Used for both retrieval and debugging.
        /// </summary>
        /// <remarks>This ID is automatically assigned when a node created, and it is 
        /// guaranteed to be unique</remarks>
        public virtual int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets or sets the group ID of this scene node for group rendering.
        /// </summary>
        /// <remarks>The default value is 0 for all nodes</remarks>
        public virtual int GroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }

        /// <summary>
        /// Gets or sets the name of this scene node. Used for name-based node retrieval <br>
        /// and debugging (Also used as a networking identifier)
        /// </summary>
        public virtual String Name
        {
            get { return name; }
            set 
            {
                if (value.Length > 100)
                    throw new GoblinException("Name length should not exceed 100");
                name = value; 
            }
        }

        /// <summary>
        /// Gets or sets if this node should be used and/or rendered
        /// </summary>
        /// <remarks>The default value is true</remarks>
        public virtual bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets the scene to which this node belongs
        /// </summary>
        internal virtual Scene SceneGraph
        {
            get { return scene; }
            set { scene = value; }
        }

        /// <summary>
        /// Gets or sets the parent node in the scene graph.
        /// </summary>
        public virtual Node Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets or sets an user defined data associated with this node. 
        /// </summary>
        public virtual Object UserData
        {
            get { return userData; }
            set { userData = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clones this scene node without its children
        /// </summary>
        /// <returns>A cloned node</returns>
        public virtual Node CloneNode() 
        {
            Node node = new Node();
            node.Name = this.Name + node.ID;
            node.GroupID = this.GroupID;
            node.Enabled = this.Enabled;

            return node;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Saves the information of this node to an XML document.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            if (name.Length > 0)
                xmlNode.SetAttribute("Name", name);
            xmlNode.SetAttribute("Enabled", enabled.ToString());

            return xmlNode;
        }

        /// <summary>
        /// Loads the information of this node from an XML element.
        /// </summary>
        /// <param name="xmlNode"></param>
        public virtual void Load(XmlElement xmlNode)
        {
            if(xmlNode.HasAttribute("Name"))
                name = xmlNode.GetAttribute("Name");
            if(xmlNode.HasAttribute("Enabled"))
                enabled = bool.Parse(xmlNode.GetAttribute("Enabled"));
        }
#endif

        /// <summary>
        /// Disposes this node.
        /// </summary>
        public virtual void Dispose()
        {
            parent = null;
            scene = null;
        }

        #endregion
    }
}
