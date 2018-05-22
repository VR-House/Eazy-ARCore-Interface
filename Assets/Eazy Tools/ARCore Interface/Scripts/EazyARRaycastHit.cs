using GoogleARCore;
using System;
using UnityEngine;

namespace VRHouse.ARTools
{
    public class EazyARRaycastHit
    {
        public RaycastHit raycastHit { get; set; }
        public TrackableHit trackableHit { get; set; }
        public Trackable Trackable { get { return EazyARCoreInterface.isSimulated ? null : trackableHit.Trackable; } }
        public Pose Pose { get { return EazyARCoreInterface.isSimulated ? new Pose(raycastHit.point, Quaternion.FromToRotation(raycastHit.transform.up, raycastHit.normal) * raycastHit.transform.rotation) : trackableHit.Pose; } }
        public TrackableHitFlags Flags { get { return EazyARCoreInterface.isSimulated ? TrackableHitFlags.PlaneWithinPolygon : trackableHit.Flags; } }

        public static implicit operator RaycastHit(EazyARRaycastHit ARHit)
        {
            return ARHit.raycastHit;
        }

        public static implicit operator TrackableHit(EazyARRaycastHit ARHit)
        {
            return ARHit.trackableHit;
        }

        public static implicit operator EazyARRaycastHit(RaycastHit hit)
        {
            EazyARRaycastHit ARHit = new EazyARRaycastHit();
            ARHit.raycastHit = hit;
            return ARHit;
        }

        public static implicit operator EazyARRaycastHit(TrackableHit hit)
        {
            EazyARRaycastHit ARHit = new EazyARRaycastHit();
            ARHit.trackableHit = hit;
            return ARHit;
        }
    }
}
