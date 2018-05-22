using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

namespace VRHouse.ARTools
{
    /// <summary>
    /// Represents an ARCore session, which is an attachment point from the app
    /// to the ARCore service. Holds information about the global state for
    /// ARCore, manages tracking of Anchors and Planes, and performs hit tests
    /// against objects ARCore is tracking in the world.
    /// </summary>
    public static class EazyARSession
    {
        /// <summary>
        /// Gets current session status. If simulated, it returnes whatever is set in EazyARCoreInterface.
        /// </summary>
        public static SessionStatus Status
        {
            get
            {
                if (EazyARCoreInterface.isSimulated)
                {
                    return status;
                }
                else
                {
                    return Session.Status;
                }
            }
        }

        private static bool simulatedPlaneDetected = false;
        private static SessionStatus status = SessionStatus.Tracking;
        private static ApkAvailabilityStatus apkAvailabilityStatus = ApkAvailabilityStatus.SupportedInstalled;

        /// <summary>
        /// Sets the simulated session status.
        /// </summary>
        /// <param name="status">The new session status</param>
        public static void SetStatus(SessionStatus status)
        {
            if (!EazyARCoreInterface.isSimulated)
            {
                return;
            }

            EazyARSession.status = status;
        }

        /// <summary>
        /// Sets the simulated ARCore apk availability status
        /// </summary>
        /// <param name="apkAvailabilityStatus">The new apk availability status</param>
        public static void SetARCoreAvailabilityStatus(ApkAvailabilityStatus apkAvailabilityStatus)
        {
            EazyARSession.apkAvailabilityStatus = apkAvailabilityStatus;
        }

        /// <summary>
        /// Gets Trackables ARCore has detected.
        /// </summary>
        /// <param name="trackables">A reference to a list of detected planes that will be filled by the method call.</param>
        /// <param name="filter">A filter on the type of data to return.</param>
        public static List<EazyARDetectedPlane> GetTrackablePlanes(TrackableQueryFilter filter = TrackableQueryFilter.All)
        {
            List<EazyARDetectedPlane> planes = new List<EazyARDetectedPlane>();

            if (EazyARCoreInterface.isSimulated && (!simulatedPlaneDetected || filter == TrackableQueryFilter.All))
            {
                // simulated horizontal plane
                planes.Add(new EazyARDetectedPlane());

                // simulated vertical plane
                Quaternion rot = new Quaternion();
                rot.eulerAngles = new Vector3(0, 90, -90);
                Pose pose = new Pose(Vector3.zero, rot);
                planes.Add(new EazyARDetectedPlane(pose));

                simulatedPlaneDetected = true;
            }
            else if (!EazyARCoreInterface.isSimulated)
            {
                List<DetectedPlane> trackedPlanes = new List<DetectedPlane>();
                Session.GetTrackables<DetectedPlane>(trackedPlanes, filter);

                foreach (DetectedPlane plane in trackedPlanes)
                {
                    planes.Add(new EazyARDetectedPlane(plane));
                }
            }

            return planes;
        }

        /// <summary>
        /// Checks the availability of the ARCore APK on the device. If simualted, it always returns SupportedInstalled
        /// </summary>
        /// <returns>An AsyncTask that completes with an ApkAvailabilityStatus when the availability is known.</returns>
        public static AsyncTask<ApkAvailabilityStatus> CheckApkAvailability()
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return new AsyncTask<ApkAvailabilityStatus>(apkAvailabilityStatus);
            }
            else
            {
                return Session.CheckApkAvailability();
            }
        }

        /// <summary>
        /// Requests an installation of the ARCore APK on the device. If simulated, it always returns null.
        /// </summary>
        /// <param name="userRequested">Whether the installation was requested explicity by a user action.</param>
        /// <returns>An AsyncTask that completes with an ApkInstallationStatus when the installation
        /// status is resolved.</returns>
        public static AsyncTask<ApkInstallationStatus> RequestApkInstallation(bool userRequested)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return null;
            }
            else
            {
                return Session.RequestApkInstallation(userRequested);
            }
        }
    }
}
