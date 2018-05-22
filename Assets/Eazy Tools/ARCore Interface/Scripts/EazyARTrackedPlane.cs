using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRHouse.ARTools
{
    /// <summary>
    /// A planar surface in the real world detected and tracked by ARCore.
    /// </summary>
    public class EazyARDetectedPlane
    {
        /// <summary>
        /// Reference to the actual tracked plane from ARCore. If simulated, it is always null
        /// </summary>
        public DetectedPlane ARcoreDetectedPlane { get; private set; }

        /// <summary>
        /// Gets the position and orientation of the plane's center.
        /// </summary>
        public Pose CenterPose
        {
            get { return EazyARCoreInterface.isSimulated ? simulatedCenterPose : ARcoreDetectedPlane.CenterPose; }
            private set { simulatedCenterPose = value; }
        }

        /// <summary>
        /// Gets the extent of the plane in the X dimension, centered on the plane position.
        /// </summary>
        public float ExtentX { get { return EazyARCoreInterface.isSimulated ? 0 : ARcoreDetectedPlane.ExtentX; } }

        /// <summary>
        /// Gets the extent of the plane in the Z dimension, centered on the plane position.
        /// </summary>
        public float ExtentZ { get { return EazyARCoreInterface.isSimulated ? 0 : ARcoreDetectedPlane.ExtentZ; } }

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
                    return ARcoreDetectedPlane.TrackingState;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the plane subsuming this plane, if any. If not null, only the subsuming plane should be
        /// considered valid for rendering. If simulated, it always returns null.
        /// </summary>
        public DetectedPlane SubsumedBy
        {
            get
            {
                if (EazyARCoreInterface.isSimulated)
                {
                    return null;
                }
                else
                {
                    return ARcoreDetectedPlane.SubsumedBy;
                }
            }
        }

        public PlaneDirection Direction
        {
            get
            {
                float angleX = Mathf.Abs((CenterPose.rotation.x > 180) ? CenterPose.rotation.x - 360 : CenterPose.rotation.x);
                float angleZ = Mathf.Abs((CenterPose.rotation.z > 180) ? CenterPose.rotation.z - 360 : CenterPose.rotation.z);

                if (angleX > 45 || angleZ > 45)
                {
                    return PlaneDirection.Vertical;
                }
                else
                {
                    return PlaneDirection.Horizontal;
                }
            }
        }

        public enum PlaneDirection
        {
            Horizontal,
            Vertical
        }

        private Pose simulatedCenterPose;

        public EazyARDetectedPlane()
        {
            this.ARcoreDetectedPlane = null;
            this.CenterPose = new Pose(Vector3.zero, Quaternion.identity);
        }

        public EazyARDetectedPlane(Pose pose)
        {
            this.ARcoreDetectedPlane = null;
            this.CenterPose = pose;
        }

        public EazyARDetectedPlane(DetectedPlane detectedPlane)
        {
            this.ARcoreDetectedPlane = detectedPlane;
        }

        public Anchor CreateAnchor(Pose pose)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return null;
            }
            else
            {
                return ARcoreDetectedPlane.CreateAnchor(pose);
            }
        }

        public void GetBoundaryPolygon(List<Vector3> boundaryPolygonPoints)
        {
            //m_NativeSession.PlaneApi.GetPolygon(m_TrackableNativeHandle, boundaryPolygonPoints);
        }
    }
}
