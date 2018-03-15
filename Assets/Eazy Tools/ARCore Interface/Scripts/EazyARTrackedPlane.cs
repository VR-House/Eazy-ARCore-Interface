using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyTools.ARCoreInterface
{
    /// <summary>
    /// A planar surface in the real world detected and tracked by ARCore.
    /// </summary>
    public class EazyARTrackedPlane
    {
        /// <summary>
        /// Reference to the actual tracked plane from ARCore. If simulated, it is always null
        /// </summary>
        public TrackedPlane ARcoreTrackedPlane { get; private set; }

        /// <summary>
        /// Gets the position and orientation of the plane's center.
        /// </summary>
        public Pose CenterPose { get { return EazyARCoreInterface.isSimulated ? new Pose(Vector3.zero, Quaternion.identity) : ARcoreTrackedPlane.CenterPose; } }

        /// <summary>
        /// Gets the extent of the plane in the X dimension, centered on the plane position.
        /// </summary>
        public float ExtentX { get { return EazyARCoreInterface.isSimulated ? 0 : ARcoreTrackedPlane.ExtentX; } }

        /// <summary>
        /// Gets the extent of the plane in the Z dimension, centered on the plane position.
        /// </summary>
        public float ExtentZ { get { return EazyARCoreInterface.isSimulated ? 0 : ARcoreTrackedPlane.ExtentZ; } }

        /// <summary>
        /// Gets the tracking state of for the Trackable in the current frame. If simulated, it always returns Tracking.
        /// </summary>
        public TrackingState TrackingState
        {
            get
            {
                if (EazyARCoreInterface.isSimulated)
                {
                    return TrackingState.Tracking;
                }
                else
                {
                    return ARcoreTrackedPlane.TrackingState;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any. If not null, only the subsuming plane should be
        /// considered valid for rendering. If simulated, it always returns null.
        /// </summary>
        public TrackedPlane SubsumedBy
        {
            get
            {
                if (EazyARCoreInterface.isSimulated)
                {
                    return null;
                }
                else
                {
                    return ARcoreTrackedPlane.SubsumedBy;
                }
            }
        }

        public EazyARTrackedPlane()
        {
            this.ARcoreTrackedPlane = null;
        }

        public EazyARTrackedPlane(TrackedPlane trackedPlane)
        {
            this.ARcoreTrackedPlane = trackedPlane;
        }

        public Anchor CreateAnchor(Pose pose)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return null;
            }
            else
            {
                return ARcoreTrackedPlane.CreateAnchor(pose);
            }
        }

        public void GetBoundaryPolygon(List<Vector3> boundaryPolygonPoints)
        {
            //m_NativeSession.PlaneApi.GetPolygon(m_TrackableNativeHandle, boundaryPolygonPoints);
        }
    }
}
