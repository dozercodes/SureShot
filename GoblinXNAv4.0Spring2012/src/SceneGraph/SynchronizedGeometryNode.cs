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
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;

using GoblinXNA.Helpers;
using GoblinXNA.Network;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A GeometryNode that is synchronized across multiple machines over network.
    /// NOTE: The original GeometryNode does not implement INetworkObject anymore, so if you
    /// want to synchronize the transform of a GeometryNode across machines, you should use this
    /// node instead.
    /// </summary>
    public class SynchronizedGeometryNode : GeometryNode, INetworkObject
    {
        #region Member Fields

        protected bool readyToSend;
        protected bool hold;
        protected int sendFrequencyInHertz;

        protected bool reliable;
        protected bool ordered;

        protected bool requiresPrecision;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a network object.
        /// </summary>
        /// <param name="name"></param>
        public SynchronizedGeometryNode(String name) :
            base(name)
        {
            readyToSend = false;
            hold = false;
            sendFrequencyInHertz = 0;

            ordered = true;
            reliable = true;

            requiresPrecision = false;
        }

        public SynchronizedGeometryNode() : this("") { }

        #endregion

        #region Properties

        public String Identifier
        {
            get { return (name.Equals("") ? "Node " + id : name); }
            internal set { name = value; }
        }

        public bool ReadyToSend
        {
            get { return readyToSend; }
            set { readyToSend = value; }
        }

        public bool Hold
        {
            get { return hold; }
            set { hold = value; }
        }

        public int SendFrequencyInHertz
        {
            get { return sendFrequencyInHertz; }
            set { sendFrequencyInHertz = value; }
        }

        public bool Reliable
        {
            get { return reliable; }
            set { reliable = value; }
        }

        public bool Ordered
        {
            get { return ordered; }
            set { ordered = value; }
        }

        /// <summary>
        /// Gets or sets whether the transform of this geometry node needs to be transmitted
        /// to the other machines with precise matrix values. If set to true, 
        /// MatrixHelper.ConvertToUnoptimizedBytes (more bytes - 16 floats) will be used instead of
        /// MatrixHelper.ConvertToOptimizedBytes (less bytes - 7 or 10 floats) to convert its transform 
        /// to bytes. Default value is false.
        /// </summary>
        public bool RequiresPrecision
        {
            get { return requiresPrecision; }
            set { requiresPrecision = value; }
        }

        #endregion

        #region Public Methods

        public virtual byte[] GetMessage()
        {
            if (requiresPrecision)
                return MatrixHelper.ConvertToUnoptimizedBytes(worldTransform);
            else
                return MatrixHelper.ConvertToOptimizedBytes(worldTransform);
        }

        public virtual void InterpretMessage(byte[] msg, int startIndex, int length)
        {
            if (requiresPrecision)
                MatrixHelper.ConvertFromUnoptimizedBytes(msg, startIndex, ref worldTransform);
            else
                MatrixHelper.ConvertFromOptimizedBytes(msg, startIndex, length, ref worldTransform);
            physicsProperties.PhysicsWorldTransform = worldTransform;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Saves the information of this physics object to an XML element.
        /// </summary>
        /// <param name="xmlDoc">The XML document to be saved.</param>
        /// <returns></returns>
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            XmlElement networkNode = xmlDoc.CreateElement(System.ComponentModel.TypeDescriptor.GetClassName(this));

            networkNode.SetAttribute("Reliable", reliable.ToString());
            networkNode.SetAttribute("Ordered", ordered.ToString());
            networkNode.SetAttribute("Hold", hold.ToString());
            networkNode.SetAttribute("SendFrequencyInHertz", sendFrequencyInHertz.ToString());
            networkNode.SetAttribute("RequiresPrecision", requiresPrecision.ToString());

            xmlNode.AppendChild(networkNode);

            return xmlNode;
        }

        /// <summary>
        /// Loads the information of this physics object from an XML element.
        /// </summary>
        /// <param name="xmlNode"></param>
        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            XmlElement networkNode = (XmlElement)xmlNode.LastChild;

            if (networkNode.HasAttribute("Reliable"))
                reliable = bool.Parse(networkNode.GetAttribute("Reliable"));
            if (networkNode.HasAttribute("Ordered"))
                ordered = bool.Parse(networkNode.GetAttribute("Ordered"));
            if (networkNode.HasAttribute("Hold"))
                hold = bool.Parse(networkNode.GetAttribute("Hold"));
            if (networkNode.HasAttribute("SendFrequencyInHertz"))
                sendFrequencyInHertz = int.Parse(networkNode.GetAttribute("SendFrequencyInHertz"));
            if (networkNode.HasAttribute("RequiresPrecision"))
                requiresPrecision = bool.Parse(networkNode.GetAttribute("RequiresPrecision"));
        }
#endif

        #endregion
    }
}
