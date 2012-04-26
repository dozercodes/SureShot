using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using GoblinXNA.SceneGraph;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// A default comparer for sorting the drawing order of transparent geometries.
    /// </summary>
    public class DefaultTransparencyComparer : IComparer<GeometryNode>
    {
        #region IComparer<GeometryNode> Members

        public int Compare(GeometryNode x, GeometryNode y)
        {
            if ((x == null) || (y == null))
                return 0;

            double thisDist = Vector3.Distance(x.BoundingVolume.Center,
                State.CameraTransform.Translation);
            double otherDist = Vector3.Distance(y.BoundingVolume.Center,
                State.CameraTransform.Translation);

            if (thisDist > otherDist)
                return -1;
            else if (thisDist == otherDist)
                return 0;
            else
                return 1;
        }

        #endregion
    }
}
