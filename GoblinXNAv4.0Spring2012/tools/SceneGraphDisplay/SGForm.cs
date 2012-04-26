/************************************************************************************ 
 * Copyright (c) 2008, Columbia University
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
 * Author: Nikhil H Ramesh (nf2241@columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Device.Generic;
using GoblinXNA.Graphics.Geometry;
using System.Reflection;
using GoblinXNA.Physics;
using System.Threading;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SceneGraphDisplay
{

    public partial class SGForm : Form
    {
        #region Properties

        // To store the scene graph Tree structure and the individual node properties
        TreeNode ViewRootNode;
        Hashtable PropertyValueTable;
        Hashtable ViewNodeSGNodeMap;

        // Variables to store the current application's SceneGraph properties.
        public Scene CurrentScene;

        // Currently selected tree node in the TreeView Structure
        TreeNode CurrentSelectedTreeNode;

        // 
        Object[] CurrentDisplayedObjects = new object[20];
        string[] CurrentPropertyStrings = new string[20];
        int CurrentDisplayedCounter = -1;
 
        // Currently picked geometry node object by clicking on the application screen
        GeometryNode CurrentPickedGeometryNode;

        // Lock variable for accessing the TreeView structure. 
        private ReaderWriterLock TreeLock = new ReaderWriterLock();

        // Variable for flagging any updates done on the treeview
        bool IsSceneGraphChanged;

        // Logging object
        public SGDLogger myLogger;

        // Variable used to store the name of the property on the grid table on which the mouse is hovered.
        string CurrentPropertyString = null;


        // The different type of Nodes - two arrays for the node names and the node coloring scheme in the Graphical Display
        System.Drawing.Color[] SGNodeColors = { System.Drawing.Color.DimGray, System.Drawing.Color.LightBlue, System.Drawing.Color.Gold, System.Drawing.Color.DarkMagenta, System.Drawing.Color.Green, System.Drawing.Color.DarkOrange, System.Drawing.Color.Violet, System.Drawing.Color.DarkKhaki };
        string[] SGNodeNames = { "BranchNode", "TransformNode", "GeometryNode", "LightNode", "CameraNode", "MarkerNode", "ParticleNode", "SoundNode" };
        System.Drawing.Color SGNodeDefaultColor = System.Drawing.Color.Chocolate;

        //System.Drawing.Color[] SGNamespaceColors = { };
        //string[] SGNamespaces = { "GoblinXNA.Graphics", "GoblinXNA.Shaders", "GoblinXNA.Physics", "GoblinXNA.Network", "GoblinXNA.Sounds" };

        // variables for determining the size of the nodes in the graphical display
        int counter = 0;

        float NodeHeight = 8.0f; // 10.0f;
        float NodeWidth = 8.0f; // 10.0f;
        float HeightBetweenNodes = 12.0f; // 15.0f;
        float WidthBetweenNodes = 12.0f; //15.0f;

        float XOffset = 4.0f; // 5.0f;
        float YOffset = 4.0f; //5.0f;

        float MagnificationFactor = 5.0f;

        // The variable used to store the zoom ratio. By default it is set to 1.0
        float ZoomFactor = 1.0f;

        #endregion

        #region Constructors

        public SGForm()
        {
            PropertyValueTable = new Hashtable();
            IsSceneGraphChanged = false;
            myLogger = new SGDLogger();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponent();
            this.dataGridView1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            //this.dataGridView1.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
            this.panel3.Cursor = (System.Windows.Forms.Cursor)System.Windows.Forms.Cursors.Hand;
        }

        public SGForm(Scene sc)
        {
            CurrentScene = sc;
            PropertyValueTable = new Hashtable();
            ViewNodeSGNodeMap = new Hashtable();
            IsSceneGraphChanged = false;
            myLogger = new SGDLogger();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            this.dataGridView1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            //this.dataGridView1.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
            this.panel3.Cursor = (System.Windows.Forms.Cursor)System.Windows.Forms.Cursors.Hand;
        }

        public void setSceneGraph(Scene sc)
        {
            CurrentScene = sc;
        }
        #endregion

        #region Running the tool and threads

        // Main function to be called on the SGForm object. 
        // This function spawns a thread (StartAndLoop function) 
        public void RunTool()
        {
            Delegate_StartAndLoop dStartTool = new Delegate_StartAndLoop(StartAndLoop);
            dStartTool.BeginInvoke(null, null);
        }

        /// <summary>
        /// This function will execute a continuous loop of scenegraph traversal and treeview update operations.
        /// </summary>
        delegate void Delegate_StartAndLoop();
        private void StartAndLoop()
        {
            Delegate_UpdateUIAfterPopulate dUpdateUIAfterPopulate = new Delegate_UpdateUIAfterPopulate(UpdateUIAfterPopulate);
            Delegate_UpdateUIFirstTime dUpdateUIFirstTime = new Delegate_UpdateUIFirstTime(UpdateUIFirstTime);

            try
            {
                // Drawing the treeview for the first time.
                this.Invoke(dUpdateUIFirstTime, CurrentScene.RootNode);
                while (true)
                {
                    // Sleep time of 100 ms is used between consecutive updates 
                    System.Threading.Thread.Sleep(100);
                    // Drawing the treeview and updating the graphical display if there are any changes in the scenegraph
                    this.Invoke(dUpdateUIAfterPopulate, CurrentScene.RootNode);
                }
            }
            catch (Exception e)
            {
                myLogger.Log("Exception in RunTool: " + e.Message);
                return;
            }

        }

        // Function called to Build the TreeView from the applications SceneGraph for the first time.
        private void BuildViewFromSceneGraph(Node GraphRootNode)
        {
            while ((ViewRootNode = UpdateViewNodeFromSceneGraphNode(null, GraphRootNode, true)) == null) ;
        }

        delegate void Delegate_UpdateUIFirstTime(object graphObject);
        public void UpdateUIFirstTime(object GraphRootObject)
        {
            BuildViewFromSceneGraph((Node)GraphRootObject);
            TreeLock.AcquireWriterLock(Timeout.Infinite);
            treeSceneGraph.Nodes.Clear();
            treeSceneGraph.Nodes.Add(ViewRootNode);
            treeSceneGraph.Nodes[0].ExpandAll();
            treeSceneGraph.TopNode = ViewRootNode;
            treeSceneGraph.AfterSelect += new TreeViewEventHandler(treeSceneGraph_AfterSelect);
            TreeLock.ReleaseWriterLock();
        }

        delegate void Delegate_UpdateUIAfterPopulate(object SceneGraphNode);
        public void UpdateUIAfterPopulate(object GraphRootObject)
        {
            TreeLock.AcquireWriterLock(Timeout.Infinite);
            IsSceneGraphChanged = false;
            DateTime Start = DateTime.Now;
            UpdateViewNodeFromSceneGraphNode(ViewRootNode, GraphRootObject, false);
            if (CurrentSelectedTreeNode != null)
            {
                PopulateDataGrid(GetCurrentDisplayedObjectProperties());
                //PopulateDataGrid(pvlist);
            }
            DateTime Stop = DateTime.Now;
            TimeSpan Duration = Stop - Start;
            /// Uncomment the next two lines if the total duration of the update is to be added to the name of the RootNode of the tree.
            //ViewRootNode.Text += " " + Duration.TotalMilliseconds;
            //ViewRootNode.Text += "Count: " + ViewNodeSGNodeMap.Count;
            TreeLock.ReleaseWriterLock();
            if (IsSceneGraphChanged)
            {
                panel1.Invalidate();
                panel3.Invalidate();
            }
        }

        #endregion

        # region TreeView Related

        // Function called to update the TreeView from the application's SceneGraph every consecutive call.
        private TreeNode UpdateViewNodeFromSceneGraphNode(TreeNode ViewNode, object SceneGraphNode, bool FirstUpdate)
        {
            if (SceneGraphNode == null)
            {
                ViewNode = null;
                return null;
            }

            try
            {
                bool IsStereo = CurrentScene.CameraNode.Stereo;
                DateTime start = DateTime.Now;
                List<PropertyValue> PropertyValueList = new List<PropertyValue>();

                // View node is created to correspond to the SceneGraphNode
                Type nodeType = SceneGraphNode.GetType();
                if (FirstUpdate)
                {
                    ViewNode = new TreeNode(nodeType.Name);
                }
                else
                {
                    // Make updates to the TreeNode Properties only when necessary. Else performance is effected.
                    if (ViewNode.Text != nodeType.Name)
                    {
                        ViewNode.Text = nodeType.Name;
                    }
                }

                // All the properties of the SceneGraphNode are extracted into this array of PropertyInfo.
                PropertyInfo[] nodeProperties = nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                int counter = 0;
                foreach (PropertyInfo nodeProperty in nodeProperties)
                {
                    // If the property is generic, then it indicates it is a collection of child nodes. 
                    if (nodeProperty.PropertyType.IsGenericType)
                    {

                        // Based on the type of the current node, it can have different children nodes.
                        Type nodePropertyType = nodeProperty.ReflectedType;

                        // BranchNode, TransformNode, GeometryNode have collection of SceneGraph.Node as children. 
                        IList<GoblinXNA.SceneGraph.Node> NodeCollection;

                        // ***********************************************************
                        // The children of BranchNode, TransformNode & GeometryNode are other types of nodes. Hence include those here to add as child nodes.
                        // ***********************************************************                    
                        if (nodePropertyType == typeof(BranchNode) || nodePropertyType == typeof(TransformNode) || nodePropertyType == typeof(GeometryNode) || nodePropertyType == typeof(MarkerNode))
                        {
                            NodeCollection = (IList<GoblinXNA.SceneGraph.Node>)nodeProperty.GetValue(SceneGraphNode, null);
                            foreach (object ChildGraphNode in NodeCollection)
                            {
                                // If the treeview is being built for the first time, then the subtree corresponding to the child graph node is added 
                                if (FirstUpdate || counter >= ViewNode.Nodes.Count)
                                {
                                    ViewNode.Nodes.Add(UpdateViewNodeFromSceneGraphNode(null, ChildGraphNode, true));
                                    IsSceneGraphChanged = true;
                                }
                                // else, the subtree corresponding to the child graph node is just updated. 
                                else
                                {
                                    UpdateViewNodeFromSceneGraphNode(ViewNode.Nodes[counter], ChildGraphNode, FirstUpdate);
                                }
                                counter++;
                            }
                        }

                    }

                    try
                    {
                        if (!IsStereo && (nodeProperty.Name.StartsWith("Left") || nodeProperty.Name.StartsWith("Right")))
                            continue;
                        object PropertyValue = nodeProperty.GetValue(SceneGraphNode, null);
                        if (PropertyValue == null)
                            continue;
                        Type valtype = PropertyValue.GetType();

                        /// Uncomment the next few lines of code to include Camera object in the TreeView. This can be also be extended to other complex objects. 

                        // Special casing "Camera" object to be included in the TreeView along with all the other nodes
                        //if (!nodeProperty.PropertyType.IsGenericType && (valtype.IsClass && (valtype == typeof(GoblinXNA.SceneGraph.Camera))))
                        //{
                        //    if (FirstUpdate || counter >= ViewNode.Nodes.Count)
                        //    {
                        //        ViewNode.Nodes.Add(UpdateViewNodeFromSceneGraphNode(null, PropertyValue, true));
                        //        IsSceneGraphChanged = true;
                        //    }
                        //    else
                        //        UpdateViewNodeFromSceneGraphNode(ViewNode.Nodes[counter], PropertyValue, FirstUpdate);
                        //    counter++;
                        //}

                        // If the checkbox for "Include Objects in the View" is checked, then include all the special classes (under the mentioned namespaces) are added as tree nodes
                        //**********************************************************************
                        // This has to be extended to include all the special classes in the GoblinXNA Framework. 
                        //***********************************************************************
                        if (rbObjectInclusion.Checked && (valtype.IsClass && valtype != typeof(GoblinXNA.SceneGraph.Camera) &&
                                    (valtype.Namespace == "GoblinXNA.Graphics.Geometry"
                                    || valtype.Namespace == "GoblinXNA.Shaders"
                                    || valtype.Namespace == "GoblinXNA.Graphics"
                                    || valtype.Namespace == "GoblinXNA.Physics"
                                    || valtype.Namespace == "GoblinXNA.Network"
                                    || valtype.Namespace == "GoblinXNA.Sounds"
                                    )))
                        {
                            if (FirstUpdate || counter >= ViewNode.Nodes.Count)
                            {
                                ViewNode.Nodes.Add(UpdateViewNodeFromSceneGraphNode(null, PropertyValue, true));
                                IsSceneGraphChanged = true;
                            }
                            else
                                UpdateViewNodeFromSceneGraphNode(ViewNode.Nodes[counter], PropertyValue, FirstUpdate);
                            counter++;
                        }
                        else
                            PropertyValueList.Add(new PropertyValue(nodeProperty.Name, PropertyValue.ToString()));

                    }
                    catch (Exception e)
                    {
                        myLogger.Log(nodeProperty.Name + " message: " + e.Message + " error: " + e.ToString());
                    }

                }

                // during update, if any graph nodes have been removed, then correspondingly the treeview nodes should also be removed
                while (counter < ViewNode.Nodes.Count)
                {
                    IsSceneGraphChanged = true;
                    ViewNode.Nodes.RemoveAt(counter);
                    counter++;
                }

                // Additional properties to be set if the node is of a particular type. 
                if (nodeType == typeof(GeometryNode))
                {
                    GeometryNode currentGeometryNode = (GeometryNode)(SceneGraphNode);
                    BoundingSphere bs = currentGeometryNode.BoundingVolume;
                    Matrix transmat = currentGeometryNode.WorldTransformation;

                    // workaround for storing the X,Y coordinates of the particular geometry node in the property value table
                    Vector3 point = State.Device.Viewport.Project(bs.Center, CurrentScene.CameraNode.Camera.Projection, CurrentScene.CameraNode.Camera.View, Matrix.Identity);
                    PropertyValueList.Add(new PropertyValue("[X]", "" + point.X));
                    PropertyValueList.Add(new PropertyValue("[Y]", "" + point.Y));

                    // If the geometry node is not within the cameras bounding frustum, then mark the node as invisible (by changing the background color to red)
                    bool doesIntersect = CurrentScene.CameraNode.BoundingFrustum.Intersects(bs);
                    if (!doesIntersect && ViewNode.BackColor != System.Drawing.Color.Red)
                    {
                        ViewNode.BackColor = System.Drawing.Color.Red;
                        IsSceneGraphChanged = true;
                    }

                    if (doesIntersect && ViewNode.BackColor == System.Drawing.Color.Red)
                    {
                        ViewNode.BackColor = System.Drawing.Color.Empty;
                        IsSceneGraphChanged = true;
                    }

                    // If the geometry node is the currently picked object (using mouse click on the screen), then mark the node (by changing the background color to blue)
                    if (CurrentPickedGeometryNode == currentGeometryNode && ViewNode.BackColor != System.Drawing.Color.Blue)
                    {
                        ViewNode.TreeView.SelectedNode = ViewNode;
                        ViewNode.BackColor = System.Drawing.Color.Blue;
                        IsSceneGraphChanged = true;
                    }

                    if (CurrentPickedGeometryNode != currentGeometryNode && ViewNode.BackColor == System.Drawing.Color.Blue)
                    {
                        ViewNode.BackColor = System.Drawing.Color.Empty;
                        IsSceneGraphChanged = true;
                    }
                }

                // This key is used to store all the properties of this view node in the hash table of PropertyValue
                int key = ViewNode.GetHashCode();

                // The property value table contains all the properties of the current node stored into a hash map
                if (PropertyValueTable.Contains(key))
                    PropertyValueTable[key] = PropertyValueList;
                else
                {
                    PropertyValueTable.Add(key, PropertyValueList);
                }

                // The Scene node is itself mapped to the tree node using this hash map 
                if (ViewNodeSGNodeMap.Contains(key))
                    ViewNodeSGNodeMap[key] = SceneGraphNode;
                else
                {
                    ViewNodeSGNodeMap.Add(key, PropertyValueList);
                }

                /// Uncomment the next three lines to get a log of all the nodes being created / updated and the time duration for each of the operations
                //DateTime stop = DateTime.Now;
                //TimeSpan duration = stop - start;
                //myLogger.Log("node:" + ViewNode.Text + ", time:" + duration.TotalMilliseconds);
                return ViewNode;
            }
            catch (Exception e)
            {
                myLogger.Log("Error message: " + e.Message);
                return null;
            }
        }

        // Simple class for storing the property and value pair (properties of class objects)
        public class PropertyValue
        {
            public string Property;
            public string Value;
            public PropertyValue(string strProperty, string strValue)
            {
                Property = strProperty;
                Value = strValue;
            }
        }

        // Handler function for selecting a new node in the treeview structure
        private void treeSceneGraph_AfterSelect(object sender, TreeViewEventArgs e)
        {
            int nodenumber = treeSceneGraph.SelectedNode.GetHashCode();
            CurrentSelectedTreeNode = treeSceneGraph.SelectedNode;
            label1.Text = treeSceneGraph.SelectedNode.Text;
            bool HighlightOnScreen = false;
            if (treeSceneGraph.SelectedNode.Text == "GeometryNode")
            {
                HighlightOnScreen = true;
            }
            dataGridView1.Rows.Clear();
            // check if the propertyvalue list corresponding to selected node is present in the hash table
            if (PropertyValueTable.Contains(nodenumber) && ViewNodeSGNodeMap.Contains(nodenumber))
            {
                CurrentDisplayedCounter=0;
                CurrentDisplayedObjects[CurrentDisplayedCounter] = ViewNodeSGNodeMap[nodenumber];
                List<PropertyValue> PropertyValueList = (List<PropertyValue>)PropertyValueTable[nodenumber];
                int i = 0;
                if (HighlightOnScreen)
                {
                    CurrentPickedGeometryNode = (GeometryNode)ViewNodeSGNodeMap[nodenumber];
                }
                else
                {
                    CurrentPickedGeometryNode = null;
                }
                foreach (PropertyValue pv in PropertyValueList)
                {
                    if (pv.Property == "[X]" || pv.Property == "[Y]")
                        continue;
                    dataGridView1.Rows.Add();
                    dataGridView1["colProperty", i].Value = pv.Property;
                    dataGridView1["colValue", i].Value = pv.Value;
                    i++;
                }
            }
            else
            {
                dataGridView1.Rows.Add();
                dataGridView1["colProperty", 0].Value = "none";
            }
        }

        #endregion

        public class SGDLogger
        {
            // Temporary logging mechanism
            StreamWriter filestream;
            string Logfile = "mylog.txt";

            public SGDLogger()
            {
                filestream = new StreamWriter(Logfile);
            }

            public SGDLogger(string LogFileName)
            {
                Logfile = LogFileName;
                filestream = new StreamWriter(Logfile);
            }

            public void Log(string LogText)
            {
                filestream.WriteLine(LogText);
            }

        }

        # region GraphicalDisplay Related
        /// <summary>
        /// Class for storing the Start and End coordinates of a subtree. 
        /// </summary>
        public class Pair
        {
            public int start;
            public int end;
            public Pair()
            { }

            public Pair(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        /// <summary>
        /// Recursive Function for drawing the SceneGraph tree structure.
        /// </summary>
        private Pair EvenInorderTraverseTree(TreeNode Head, int depth, int[] index, Graphics formgraphics, Pen mypen, SolidBrush myBrush, float NodeWidth, float NodeHeight, float WidthBetweenNodes, float HeightBetweenNodes, float XOffset, float YOffset, bool textEnabled)
        {
            int max;
            // Determing the x-coordinate (index[depth]) at the current depth by examining the x-coordinate values at [depth-1] and [depth+1]
            if (depth > 0)
            {
                max = index[depth - 1] - 1;
            }
            else
            {
                max = index[depth];
            }
            max = (max < index[depth]) ? index[depth] : max;
            max = (max < index[depth + 1]) ? index[depth + 1] : max;
            index[depth] = max + 1;


            // High and low variables are used to store the extremities of the subtree starting from the current node.
            int high, low;
            if (Head.Nodes.Count > 0)
            {
                low = 10000; high = -1;
            }
            else
            {
                low = index[depth]; high = index[depth];
            }
            Pair[] retval = new Pair[Head.Nodes.Count];
            int counter = 0;
            foreach (TreeNode child in Head.Nodes)
            {
                retval[counter] = EvenInorderTraverseTree(child, depth + 1, index, formgraphics, mypen, myBrush, NodeWidth, NodeHeight, WidthBetweenNodes, HeightBetweenNodes, XOffset, YOffset, textEnabled);
                if (retval[counter].end > high)
                    high = retval[counter].end;
                if (retval[counter].start < low)
                    low = retval[counter].start;
                counter++;
            }

            // update the index[depth] (current x-coordinate)
            index[depth] = (high + low) / 2;
            mypen.Color = System.Drawing.Color.Black;

            for (int i = 0; i < counter; i++)
                formgraphics.DrawLine(mypen, new PointF(((high + low) / 2 * WidthBetweenNodes) + XOffset + NodeWidth / 2, (depth * HeightBetweenNodes) + YOffset + NodeHeight), new PointF(((retval[i].start + retval[i].end) / 2 * WidthBetweenNodes) + XOffset + NodeWidth / 2, ((depth + 1) * HeightBetweenNodes) + YOffset));

            // highlighting the node, if it is the currentselected node
            if (Head == CurrentSelectedTreeNode)
            {
                myBrush.Color = System.Drawing.Color.Black;
                formgraphics.FillRectangle(myBrush, ((high + low) / 2 * WidthBetweenNodes) + XOffset, (depth * HeightBetweenNodes) + YOffset, NodeWidth, NodeHeight);
            }

            // Coloring the nodes based on the node types
            myBrush.Color = SGNodeDefaultColor;
            for (int i = 0; i < SGNodeNames.Length; i++)
            {
                if (Head.Text.StartsWith(SGNodeNames[i]))
                {
                    myBrush.Color = SGNodeColors[i];
                    break;
                }
            }

            // if the node is either invisible (red) or currently selected node (blue) then use similar coloring of the geometry nodes even for the corresponding graphical display nodes
            if (Head.BackColor == System.Drawing.Color.Red || Head.BackColor == System.Drawing.Color.Blue)
            {
                myBrush.Color = Head.BackColor;
            }

            formgraphics.FillEllipse(myBrush, ((high + low) / 2 * WidthBetweenNodes) + XOffset, (depth * HeightBetweenNodes) + YOffset, NodeWidth, NodeHeight);

            // writing the string  "NodeName(NodeID)" on the graphical display node
            if (textEnabled)
            {
                string nodeString = "";
                int key = Head.GetHashCode();
                string nodeName = "", nodeID = null;
                List<PropertyValue> pvlist = (List<PropertyValue>)PropertyValueTable[key];
                foreach (PropertyValue pv in pvlist)
                {
                    if (pv.Property == "Name")
                    {
                        nodeName = pv.Value;
                    }
                    if (pv.Property == "ID")
                    {
                        nodeID = pv.Value;
                    }
                }
                nodeString = nodeName;
                if (nodeID != null)
                    nodeString += "(" + nodeID + ")";
                if (nodeString != "")
                {
                    float FontSize = 8.0f;
                    Font stringFont = new Font(FontFamily.GenericSansSerif, FontSize);
                    SizeF stringSize = formgraphics.MeasureString(nodeString, stringFont);
                    while (stringSize.Width > WidthBetweenNodes / 2 + NodeWidth / 2)
                    {
                        FontSize -= 0.5f;
                        stringFont = new Font(FontFamily.GenericSansSerif, FontSize);
                        stringSize = formgraphics.MeasureString(nodeString, stringFont);
                    }
                    float XPosition = ((high + low) / 2 * WidthBetweenNodes) + XOffset + (NodeWidth / 2.0f) - (stringSize.Width / 2.0f);
                    float YPosition = (depth * HeightBetweenNodes) + YOffset + (NodeHeight / 2.0f) - (stringSize.Height / 2.0f);
                    myBrush.Color = System.Drawing.Color.Black;
                    formgraphics.FillRectangle(myBrush, new RectangleF(XPosition - 1, YPosition - 1, stringSize.Width + 2, stringSize.Height + 2));
                    myBrush.Color = System.Drawing.Color.White;
                    formgraphics.DrawString(nodeString, stringFont, myBrush, new PointF(XPosition, YPosition));
                }
            }
            else
            {
                // ******** workaround *********
                // store the x and y coordinates of a graphical display node in the corresponding TreeNode name property
                // this information is used when a mouse click event is triggered on the panel displaying the structure, the entire TreeView is searched using this stored info and it is matched against the clicked mouse coordinates.
                Head.Name = "" + (((high + low) / 2 * WidthBetweenNodes) + XOffset) + "," + ((depth * HeightBetweenNodes) + YOffset);
            }
            return new Pair(low, high);
        }

        /// <summary>
        /// Function for drawing the bounding box around the currently picked geometry node
        /// This function should explicitly be called in the Game.Draw function of the main application
        /// </summary>
        public void UpdatePickedObjectDrawing()
        {
            if (CurrentPickedGeometryNode != null)
            {
                Vector3[] corners = CurrentPickedGeometryNode.Model.MinimumBoundingBox.GetCorners();
                Matrix renderMatrix = CurrentPickedGeometryNode.MarkerTransform *
                    CurrentPickedGeometryNode.WorldTransformation;
                for (int i = 0; i < corners.Length; i++)
                    Vector3.Transform(ref corners[i], ref renderMatrix, out corners[i]);

                DebugShapeRenderer.AddBoundingBox(corners, Microsoft.Xna.Framework.Color.Red, 0);
            }
        }

        /// <summary>
        /// This function is used to recursively traverse the TreeView (corresponding to the scenegraph) and update the currentselectednode variable appropriately
        /// </summary>
        private bool FindTreeNode(TreeNode Head, int x, int y, float NodeHeight, float NodeWidth, bool IsSmallPanel)
        {
            try
            {
                float CurrentX, CurrentY;

                char[] separators = { ',' };
                if (Head.Name == null)
                    return false;
                string[] XYValues = Head.Name.Split(separators);
                CurrentX = float.Parse(XYValues[0]);
                CurrentY = float.Parse(XYValues[1]);
                if (!IsSmallPanel)
                {
                    CurrentX *= MagnificationFactor;
                    CurrentX += WindowOffsetX;
                    CurrentY *= MagnificationFactor;
                    CurrentY += WindowOffsetY;
                }
                if (y < CurrentY + (NodeHeight / 2) && y > CurrentY - (NodeHeight / 2) && x < CurrentX + (NodeWidth / 2) && x > CurrentX - (NodeWidth / 2))
                {
                    CurrentSelectedTreeNode = Head;
                    CurrentSelectedTreeNode.TreeView.SelectedNode = CurrentSelectedTreeNode;
                    return true;
                }
                if (y < CurrentY + NodeHeight / 2)
                {
                    return false;
                }
                foreach (TreeNode child in Head.Nodes)
                {
                    if (FindTreeNode(child, x, y, NodeHeight, NodeWidth, IsSmallPanel))
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                myLogger.Log("Error at findtreenode at node(" + Head.Text + ") - " + Head.Name);
                return false;
            }

        }

        #endregion

        #region Object Picking Related

        /// <summary>
        /// Simple class for storing the nearest picked geometry node and the corresponding distance of the ray (from start point to point of intersection)
        /// </summary>
        public class PickedObj
        {
            public GeometryNode PickedNode;
            public float DistanceOfRay;
            public PickedObj(GeometryNode node, float distance)
            {
                PickedNode = node;
                DistanceOfRay = distance;
            }

        }

        /// <summary>
        /// Ray-Sphere(Bounding volume of a geometry node) intersection test function
        /// </summary>
        private float RaySphereIntersectionTest(GeometryNode node, Vector3 nearPoint, Vector3 farPoint)
        {
            Vector3 Direction = (farPoint - nearPoint);

            BoundingSphere sphere = node.BoundingVolume;

            float A = Vector3.Dot(Direction, Direction);
            float B = 2.0f * Vector3.Dot(Direction, (nearPoint - sphere.Center));
            float C = Vector3.Dot((nearPoint - sphere.Center), (nearPoint - sphere.Center)) - (sphere.Radius * sphere.Radius);

            if ((B * B) < (4 * A * C))
                return -1.0f;
            if ((B * B) == (4 * A * C))
            {
                float singleroot = (-B / (2 * A));
                if (singleroot > 0.0 && singleroot < 1.0)
                    return singleroot;
                return -1.0f;
            }
            else
            {
                float firstroot = ((-B - (float)System.Math.Sqrt((B * B) - (4 * A * C))) / (2 * A));
                if (firstroot > 0.0 && firstroot < 1.0)
                    return firstroot;
                float secondroot = ((-B + (float)System.Math.Sqrt((B * B) - (4 * A * C))) / (2 * A));
                if (secondroot > 0.0 && secondroot < 1.0)
                    return secondroot;
                return -1.0f;
            }
        }

        /// <summary>
        /// Recursive function used to pick the object in the scene based on the projected near point and far point of the ray corresponding to a mouse click on the screen
        /// </summary>
        private PickedObj PickRayCast(object SceneGraphNode, Vector3 nearPoint, Vector3 farPoint)
        {
            if (SceneGraphNode == null)
            {
                return null;
            }

            Vector3 Direction = (farPoint - nearPoint);
            float MinDistance = -1.0f;
            float DistanceOfIntersection;
            GeometryNode ClosestGeometryNode = null;
            if (SceneGraphNode.GetType() == typeof(GeometryNode))
            {
                if ((DistanceOfIntersection = RaySphereIntersectionTest((GeometryNode)SceneGraphNode, nearPoint, farPoint)) > 0.0)
                {
                    if (((GeometryNode)SceneGraphNode).BoundingVolume.Center.Length() > 0.0)
                    {
                        MinDistance = DistanceOfIntersection;
                        ClosestGeometryNode = (GeometryNode)SceneGraphNode;
                    }
                }
            }


            try
            {
                Type nodeType = SceneGraphNode.GetType();
                PropertyInfo[] nodeProperties = nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo nodeProperty in nodeProperties)
                {
                    if (nodeProperty.PropertyType.IsGenericType)
                    {
                        Type nodePropertyType = nodeProperty.ReflectedType;
                        IList<GoblinXNA.SceneGraph.Node> NodeCollection;

                        if (nodePropertyType == typeof(BranchNode) || nodePropertyType == typeof(TransformNode) || nodePropertyType == typeof(GeometryNode) || nodePropertyType == typeof(MarkerNode))
                        {
                            NodeCollection = (IList<GoblinXNA.SceneGraph.Node>)nodeProperty.GetValue(SceneGraphNode, null);
                            PickedObj ChildPickedObj;
                            foreach (object ChildGraphNode in NodeCollection)
                            {
                                ChildPickedObj = PickRayCast(ChildGraphNode, nearPoint, farPoint);
                                if (ChildPickedObj != null && (MinDistance < 0.0 || ChildPickedObj.DistanceOfRay < MinDistance))
                                {
                                    MinDistance = ChildPickedObj.DistanceOfRay;
                                    ClosestGeometryNode = ChildPickedObj.PickedNode;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (MinDistance > 0.0)
            {
                PickedObj returnObj = new PickedObj(ClosestGeometryNode, MinDistance);
                return returnObj;
            }
            return null;
        }


        /// <summary>
        /// Function used to update the background color of the selected node in the treeview
        /// </summary>
        public bool HighlightPickedGeometryNode(TreeNode ViewNode, GeometryNode node)
        {
            if (ViewNode == null)
                return false;

            if (ViewNode.Text == "GeometryNode")
            {
                int key = ViewNode.GetHashCode();
                List<PropertyValue> pvlist = (List<PropertyValue>)PropertyValueTable[key];
                foreach (PropertyValue pv in pvlist)
                {
                    if (pv.Property == "ID" && pv.Value == node.ID.ToString())
                    {
                        ViewNode.BackColor = System.Drawing.Color.Blue;
                        return true;
                    }
                }
            }

            foreach (TreeNode ChildNode in ViewNode.Nodes)
            {
                if (HighlightPickedGeometryNode(ChildNode, node))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Mouse click handler function for the screen. 
        /// Calculate the ray corresponding to the X-Y coordinates in the screen space
        /// Then call PickRayCast which will pick the nearest geometry node corresponding to the click. 
        /// </summary>
        public void SG_MouseClickHandler(int button, Microsoft.Xna.Framework.Point mouseLocation)
        {
            /// TODO: Add a support for modifier key (i.e. mouse click along with a modifier key is used for picking the nearest object on the screen)
            GoblinXNA.Device.Generic.KeyModifier km;

            if (button == MouseInput.LeftButton)
            {
                // 0 means on the near clipping plane, and 1 means on the far clipping plane
                Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                // Now convert the near and far source to actual near and far 3D points based on our eye location and view frustum
                Vector3 nearPoint = State.Device.Viewport.Unproject(nearSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                Vector3 farPoint = State.Device.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                Node SceneGraphNode = CurrentScene.RootNode;
                PickedObj NearestPickedObject = PickRayCast(SceneGraphNode, nearPoint, farPoint);
                if (NearestPickedObject != null)
                    CurrentPickedGeometryNode = NearestPickedObject.PickedNode;
                else
                    CurrentPickedGeometryNode = null;

                UpdatePickedObjectDrawing();
            }
        }

        #endregion

        #region Handler functions for the Panels
        // Handler function for painting on the small panel
        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {
            counter++;
            base.OnPaint(e);
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);
            System.Drawing.Graphics formGraphics;
            formGraphics = e.Graphics;
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);

            int[] index = new int[50];
            for (int i = 0; i < 50; i++)
                index[i] = 0;

            // Call the EvenInorderTraverseTree function with the appropriate parameters related to the sizes of the nodes
            Pair pa = EvenInorderTraverseTree(ViewRootNode, 0, index, formGraphics, myPen, myBrush, ZoomFactor * NodeWidth, ZoomFactor * NodeHeight, ZoomFactor * WidthBetweenNodes, ZoomFactor * HeightBetweenNodes, ZoomFactor * XOffset, ZoomFactor * YOffset, false);

            int depth;
            for (depth = 0; depth < 50; depth++)
                if (index[depth] == 0)
                    break;

            // Vary the height and width of the panel based on the current structure
            panel1.Height = (int)(depth * ZoomFactor * HeightBetweenNodes + YOffset * ZoomFactor * 4);
            panel1.Width = (int)(pa.end * ZoomFactor * WidthBetweenNodes + XOffset * ZoomFactor * 4);

            myPen.Dispose();
            myBrush.Dispose();
        }

        // Handler function for mouse click event on the small panel
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            TreeLock.AcquireReaderLock(Timeout.Infinite);
            FindTreeNode(treeSceneGraph.Nodes[0], e.X, e.Y, NodeHeight, NodeWidth, true);
            panel1.Invalidate();
            panel3.Invalidate();
            TreeLock.ReleaseReaderLock();
        }

        int WindowOffsetX = 0;
        int WindowOffsetY = 0;
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            System.Drawing.Graphics formGraphics;
            formGraphics = e.Graphics;

            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            int[] index = new int[50];
            for (int i = 0; i < 50; i++)
                index[i] = 0;
            /// call eveninordertraversetree with the correct parameters. zoomfactor is multiplied for all the height , width and position parameters
            Pair pa = EvenInorderTraverseTree(ViewRootNode, 0, index, formGraphics, myPen, myBrush, NodeWidth * MagnificationFactor * ZoomFactor, NodeHeight * MagnificationFactor * ZoomFactor, WidthBetweenNodes * MagnificationFactor * ZoomFactor, HeightBetweenNodes * MagnificationFactor * ZoomFactor, WindowOffsetX, WindowOffsetY, true);
            myPen.Dispose();
            myBrush.Dispose();
        }

        private void panel2_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            // Vary the windowoffsets according to the scroll position in the small panel.
            // The large panel painting will depend on these window offset values
            WindowOffsetX = (int)(panel2.AutoScrollPosition.X * MagnificationFactor);
            WindowOffsetY = (int)(panel2.AutoScrollPosition.Y * MagnificationFactor);
            panel3.Invalidate();
        }

        int Panel3CurrentX = -1;
        int Panel3CurrentY = -1;

        /// <summary>
        /// Functions MouseUp and MouseDown have been used to handle the mouse drag event in the large panel
        /// </summary>
        void panel3_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Calculate the delta in x and y for the mouse drag
                int DeltaX = (int)((float)(Panel3CurrentX - e.X) / MagnificationFactor);
                int DeltaY = (int)((float)(Panel3CurrentY - e.Y) / MagnificationFactor);
                Panel3CurrentX = e.X;
                Panel3CurrentY = e.Y;
                int newPanelPositionX = (panel2.HorizontalScroll.Value + DeltaX);
                if (newPanelPositionX < 0 || newPanelPositionX > panel2.HorizontalScroll.Maximum)
                    newPanelPositionX = panel2.AutoScrollPosition.X;
                int newPanelPositionY = (panel2.VerticalScroll.Value + DeltaY);
                if (newPanelPositionY < 0 || newPanelPositionY > panel2.VerticalScroll.Maximum)
                    newPanelPositionY = panel2.AutoScrollPosition.Y;

                panel2.AutoScrollPosition = new System.Drawing.Point(newPanelPositionX, newPanelPositionY);
                // Invalidate the panel so as to force paint the new panel 
                panel2.Invalidate();

                WindowOffsetX = (int)(panel2.AutoScrollPosition.X * MagnificationFactor);
                WindowOffsetY = (int)(panel2.AutoScrollPosition.Y * MagnificationFactor);
                // Invalidate the panel so as to force paint the new panel 
                panel3.Invalidate();
                Panel3CurrentX = -1;
                Panel3CurrentY = -1;
            }
        }

        void panel3_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            /// just record the current mouse coordinates on the panel.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Panel3CurrentX = e.X;
                Panel3CurrentY = e.Y;
            }
        }


        // Handler function for mouse click event on the large panel
        private void panel3_MouseClick(object sender, MouseEventArgs e)
        {
            TreeLock.AcquireReaderLock(Timeout.Infinite);
            FindTreeNode(treeSceneGraph.Nodes[0], e.X, e.Y, NodeHeight * MagnificationFactor, NodeWidth * MagnificationFactor, false);
            panel1.Invalidate();
            panel3.Invalidate();
            TreeLock.ReleaseReaderLock();
        }

        // function for drawing the SGNode legend (Node-Color mapping) on the small panel in the corner of the tab 
        private void panel5_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            SolidBrush myBrush = new SolidBrush(System.Drawing.Color.Black);
            // vary the following values to determine the size of the legend table.
            float diameter = 10.0f;
            float YDiff = 12.5f;
            float XOffset = 5.0f;
            int i;
            float FontSize = 8.0f;
            Font stringFont = new Font(FontFamily.GenericSansSerif, FontSize);

            for (i = 0; i < SGNodeColors.Length; i++)
            {
                myBrush.Color = SGNodeColors[i];
                e.Graphics.FillEllipse(myBrush, new RectangleF(XOffset, (i + 0.5f) * YDiff, diameter, diameter));
                myBrush.Color = System.Drawing.Color.Black;
                e.Graphics.DrawString(SGNodeNames[i], stringFont, myBrush, new PointF(XOffset * 2 + diameter, (i + 0.5f) * YDiff));
            }
            myBrush.Color = SGNodeDefaultColor;
            e.Graphics.FillEllipse(myBrush, new RectangleF(XOffset, (i + 0.5f) * YDiff, diameter, diameter));
            myBrush.Color = System.Drawing.Color.Black;
            e.Graphics.DrawString("Other Objects", stringFont, myBrush, new PointF(XOffset * 2 + diameter, (i + 0.5f) * YDiff));
            i++;

        }

        private void trackBar1_ValueChanged(object sender, System.EventArgs e)
        {
            ZoomFactor = ((trackBar1.Value - (float)(trackBar1.Maximum + trackBar1.Minimum) / 2) + (float)(trackBar1.Maximum - trackBar1.Minimum)) / (float)(trackBar1.Maximum - trackBar1.Minimum);
        }


        #endregion

        #region Handler functions for datagridview
        void dataGridView1_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            dataGridView1.ContextMenuStrip.Items.Clear();
            dataGridView1.ContextMenuStrip.Hide();
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count && dataGridView1["colProperty", e.RowIndex].Value != null)
            {
                CurrentPropertyString = dataGridView1["colProperty", e.RowIndex].Value.ToString();
                List<PropertyValue> PropertyValueList = GetObjectProperties(CurrentPropertyString);
                if (PropertyValueList != null && PropertyValueList.Count > 0)
                {
                    dataGridView1.ContextMenuStrip.Items.Add("Click here for detailed information", null, new EventHandler(DetailedInfoClickHandler));
                }
            }
            else
            {
                CurrentPropertyString = null;
            }
        }

        public void DetailedInfoClickHandler(Object sender, EventArgs e)
        {
            dataGridView1.ContextMenuStrip.Hide();
            if (CurrentPropertyString != null)
            {
                label1.Text += " ---> " + CurrentPropertyString;
                button1.Enabled = true;
                List<PropertyValue> PropertyValueList = GetObjectProperties(CurrentPropertyString);

                if (PropertyValueList != null && PropertyValueList.Count > 0)
                {
                    Type nodeType = CurrentDisplayedObjects[CurrentDisplayedCounter].GetType();
                    PropertyInfo ObjectPropertyInfo = nodeType.GetProperty(CurrentPropertyString);
                    CurrentDisplayedObjects[CurrentDisplayedCounter + 1] = ObjectPropertyInfo.GetValue(CurrentDisplayedObjects[CurrentDisplayedCounter], null);
                    CurrentPropertyStrings[CurrentDisplayedCounter + 1] = CurrentPropertyString;
                    CurrentDisplayedCounter++;
                    PopulateDataGrid(PropertyValueList);
                }
            }

        }

        private void PopulateDataGrid(List<PropertyValue> PropertyValueList)
        {
            //int DataGridCount = dataGridView1.Rows.Count;
            //int PropertyCount = PropertyValueList.Count;
            //if (DataGridCount > PropertyCount+1)
            //{
            //    for (int j = PropertyCount+1; j < DataGridCount; j++)
            //        dataGridView1.Rows.RemoveAt(j);
            //}
            //else if (DataGridCount < PropertyCount+1)
            //{
            //    dataGridView1.Rows.Add(PropertyCount+1 - DataGridCount);
            //}

            int i = 0;
            dataGridView1.Rows.Clear();
            foreach (PropertyValue pv in PropertyValueList)
            {

                if (pv.Property == "[X]" || pv.Property == "[Y]")
                    continue;
                dataGridView1.Rows.Add();
                dataGridView1["colProperty", i].Value = pv.Property;
                dataGridView1["colValue", i].Value = pv.Value;
                i++;
            }
        }

        private List<PropertyValue> GetCurrentDisplayedObjectProperties()
        {
            try
            {
                if (CurrentDisplayedCounter < 0 || CurrentDisplayedObjects[CurrentDisplayedCounter] == null)
                    return null;
                List<PropertyValue> PropertyValueList = new List<PropertyValue>();
                for (int i = 1; i <= CurrentDisplayedCounter; i++)
                {
                    object SceneObject = (object)CurrentDisplayedObjects[i-1];
                    CurrentDisplayedObjects[i] = GetPropertyObject(SceneObject, CurrentPropertyStrings[i]);
                }
                PropertyValueList = GetAllProperties(CurrentDisplayedObjects[CurrentDisplayedCounter]);
                return PropertyValueList;
            }
            catch (Exception)
            {
                return null;
            }
 
        }

        private List<PropertyValue> GetObjectProperties(string ObjectTypeName)
        {
            try
            {
                if (CurrentDisplayedCounter < 0 || CurrentDisplayedObjects[CurrentDisplayedCounter] == null)
                    return null;
                object SceneObject = GetPropertyObject((object)CurrentDisplayedObjects[CurrentDisplayedCounter], ObjectTypeName);
                return GetAllProperties(SceneObject);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private object GetPropertyObject(object SceneGraphNode, string ObjectTypeName)
        {
            try
            {
                Type nodeType = SceneGraphNode.GetType();
                PropertyInfo ObjectPropertyInfo = nodeType.GetProperty(ObjectTypeName);
                object SceneObject = ObjectPropertyInfo.GetValue(SceneGraphNode, null);
                return SceneObject;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private List<PropertyValue> GetAllProperties(object SceneObject)
        {
            try
            {
                Type objectType = SceneObject.GetType();

                List<PropertyValue> PropertyValueList = new List<PropertyValue>();

                // All the properties of the SceneGraphNode are extracted into this array of PropertyInfo.
                PropertyInfo[] objectProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo objectProperty in objectProperties)
                {
                    object PropertyValue = objectProperty.GetValue(SceneObject, null);
                    if (PropertyValue == null)
                        continue;
                    Type valtype = PropertyValue.GetType();
                    PropertyValueList.Add(new PropertyValue(objectProperty.Name, PropertyValue.ToString()));
                }

                return PropertyValueList;
            }
            catch (Exception)
            {
                return null;
            }
 
        }

        private List<PropertyValue> GetCurrentProperties()
        {
            try
            {
                Type objectType = CurrentDisplayedObjects[CurrentDisplayedCounter].GetType();

                List<PropertyValue> PropertyValueList = new List<PropertyValue>();

                // All the properties of the SceneGraphNode are extracted into this array of PropertyInfo.
                PropertyInfo[] objectProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo objectProperty in objectProperties)
                {
                    object PropertyValue = objectProperty.GetValue(CurrentDisplayedObjects[CurrentDisplayedCounter], null);
                    if (PropertyValue == null)
                        continue;
                    Type valtype = PropertyValue.GetType();
                    PropertyValueList.Add(new PropertyValue(objectProperty.Name, PropertyValue.ToString()));
                }

                return PropertyValueList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            if (CurrentDisplayedCounter < 2)
            {
                button1.Enabled = false;
                TreeNode selectednode = treeSceneGraph.SelectedNode;
                treeSceneGraph.SelectedNode = null;
                treeSceneGraph.SelectedNode = selectednode;
            }
            else
            {
                CurrentDisplayedCounter--;
                List<PropertyValue> PropertyValueList = GetCurrentProperties();
                if (PropertyValueList != null && PropertyValueList.Count > 0)
                {
                    dataGridView1.Rows.Clear();
                    int i = 0;
                    foreach (PropertyValue pv in PropertyValueList)
                    {
                        if (pv.Property == "[X]" || pv.Property == "[Y]")
                            continue;
                        dataGridView1.Rows.Add();
                        dataGridView1["colProperty", i].Value = pv.Property;
                        dataGridView1["colValue", i].Value = pv.Value;
                        i++;
                    }
                }
                if(label1.Text.Contains(" ---> "))
                {
                    label1.Text = label1.Text.Remove(label1.Text.LastIndexOf(" ---> "));
                }
            }
            
        }
    }

}

