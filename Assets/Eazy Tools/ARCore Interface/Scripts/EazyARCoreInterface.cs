using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRHouse.ARTools
{
    [AddComponentMenu("Eazy Tools/ ARCore Interface")]
    public class EazyARCoreInterface : MonoBehaviour
    {
        [SerializeField]
        private Camera arCamera;

        [Header("Session")]
        [Tooltip("The current session status when ARCore is simulated (playing in editor)")]
        public SessionStatus simulatedSessionStatus = SessionStatus.Tracking;

        [Header("ARCore APK")]
        [Tooltip("The current availability status of ARCore when it is simulated (playing in editor)")]
        public ApkAvailabilityStatus ARCoreAvailabilityStatus = ApkAvailabilityStatus.SupportedInstalled;

        [Header("Tracked Planes")]
        [Tooltip("Whether to visualize the detected planes")]
        public bool visualizeDetectedPlanes = true;
        [Tooltip("Whether the detected planes cast shadows")]
        public bool detectedPlanesCastShadows = false;
        [Tooltip("Whether the detected planes receive shadows")]
        public bool detectedPlanesReceiveShadows = true;
        [Tooltip("Whether to allow other physics objects to collide with the detected planes")]
        public bool allowDetectedPlanesCollisions = false;
        [Tooltip("The material to use to visualize the detected horizontal planes")]
        public Material detectedHorizontalPlanesMaterial;
        [Tooltip("The material to use to visualize the detected vertical planes")]
        public Material detectedVerticalPlanesMaterial;

        [Header("Point Cloud")]
        [Tooltip("Whether to visualize the point cloud")]
        public bool visualizePointCloud = true;
        [Tooltip("The material to use to visualize the point cloud")]
        public Material pointCloudMaterial;

        /// <summary>
        /// The layer of the detected planes.
        /// </summary>
        public static LayerMask DetectedPlaneLayer { get; private set; }

        /// <summary>
        /// The tag of the dtetected planes.
        /// </summary>
        public static string DetectedPlaneTag { get; private set; }

        /// <summary>
        /// The AR camera
        /// </summary>
        public static Camera ARCamera { get { return instance.arCamera; } set { instance.arCamera = value; } }

        /// <summary>
        /// Whether this is a simulation of ARcore in the editor, or playing on the actual device
        /// </summary>
        public static bool isSimulated { get; private set; }

        /// <summary>
        /// Whether the detected planes are visualized
        /// </summary>
        public static bool VisualizeDetectedPlanes { get { return instance.visualizeDetectedPlanes; } set { instance.visualizePointCloud = value; } }

        /// <summary>
        /// Whether the detected planes cast shadows
        /// </summary>
        public static bool DetectedPlanesCastShadows { get { return instance.detectedPlanesCastShadows; } set { instance.visualizePointCloud = value; } }

        /// <summary>
        /// Whether the detected planes receive shadows
        /// </summary>
        public static bool DetectedPlanesReceiveShadows { get { return instance.detectedPlanesReceiveShadows; } set { instance.visualizePointCloud = value; } }

        /// <summary>
        /// Whether the point cloud is visualized
        /// </summary>
        public static bool VisualizePointCloud { get { return instance.visualizePointCloud; } set { instance.visualizePointCloud = value; } }

        private static EazyARCoreInterface _instance = null;
        public static EazyARCoreInterface instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (EazyARCoreInterface)FindObjectOfType(typeof(EazyARCoreInterface));
                }
                return _instance;
            }
        }

        private void Awake()
        {
            // Determine if this is running on an actual device, or if it simulated.
            #if UNITY_EDITOR
                isSimulated = true;
            #else
                isSimulated = false;
            #endif

            // Initialize ARCore session and camera position
            if (!isSimulated)
            {
                GameObject ARCoreSessionObj = FindObjectOfType<ARCoreSession>().gameObject;

                ARCoreSessionObj.transform.position = Vector3.zero;
                ARCoreSessionObj.transform.rotation = Quaternion.identity;

                ARCamera.transform.position = Vector3.zero;
                ARCamera.transform.rotation = Quaternion.identity;

                ARCamera.transform.SetParent(ARCoreSessionObj.transform);
            }

            // Check if trackables layers are assigned in Layermask
            DetectedPlaneLayer = LayerMask.NameToLayer("EazyARDetectedPlane");
            if (DetectedPlaneLayer == -1)
            {
                throw new UnassignedReferenceException("Layer EazyARDetectedPlane cannot be found. Please assign it in the Layer Manager.");
            }

            DetectedPlaneTag = "EazyARDetectedPlane";

            // Disable collisions between trackables layer and other layers
            if (!allowDetectedPlanesCollisions)
            {
                for (int i = 0; i <= 31; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (layerName.Length > 0)
                    {
                        Physics.IgnoreLayerCollision(DetectedPlaneLayer, i);
                    }
                }
            }

            // Create PointCloud
            GameObject pointCloud = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointCloud.name = "Point Cloud";
            pointCloud.transform.SetParent(transform);
            pointCloud.transform.localPosition = Vector3.zero;
            pointCloud.AddComponent<EazyARCorePointCloudVisualizer>();
            pointCloud.GetComponent<Renderer>().material = pointCloudMaterial;
            Destroy(pointCloud.GetComponent<Collider>());

            // Attach a free look script on the camera if it is simulated
            if (isSimulated)
            {
                arCamera.gameObject.AddComponent<FreeLookCam>();
            }

            // Initialize statuses
            SetSessionStatus(simulatedSessionStatus);
            SetARCoreAvailabilityStatus(ARCoreAvailabilityStatus);
        }

        private void Update()
        {
            SetSessionStatus(simulatedSessionStatus);
            SetARCoreAvailabilityStatus(ARCoreAvailabilityStatus);
        }

        /// <summary>
        /// Sets the simulated session status.
        /// </summary>
        /// <param name="status">The new session status</param>
        public static void SetSessionStatus(SessionStatus status)
        {
            EazyARSession.SetStatus(status);
        }

        /// <summary>
        /// Sets the simulated ARCore apk availability status
        /// </summary>
        /// <param name="apkAvailabilityStatus">The new apk availability status</param>
        public static void SetARCoreAvailabilityStatus(ApkAvailabilityStatus apkAvailabilityStatus)
        {
            EazyARSession.SetARCoreAvailabilityStatus(apkAvailabilityStatus);
        }

        /// <summary>
        /// Creates the mesh of a detected plane in the environment
        /// </summary>
        /// <param name="detectedPlane"></param>
        /// <returns></returns>
        public static EazyARDetectedPlaneMesh CreateDetectedPlane(EazyARDetectedPlane detectedPlane)
        {
            //GameObject planeObject = Instantiate(instance.trackedPlanePrefab, Vector3.zero, Quaternion.identity, instance.transform);
            GameObject planeObject = new GameObject("DetectedPlane");
            planeObject.transform.position = Vector3.zero;
            planeObject.transform.rotation = Quaternion.identity;
            planeObject.transform.SetParent(instance.transform);

            EazyARDetectedPlaneMesh planeMesh = planeObject.AddComponent<EazyARDetectedPlaneMesh>();
            planeMesh.Initialize(detectedPlane);
            return planeMesh;
        }

        /// <summary>
        /// Does an AR raycast on the detected planes
        /// </summary>
        /// <param name="InputPosition"></param>
        /// <param name="hitInfo"></param>
        /// <returns></returns>
        public static bool ARRaycast(float x, float y, out EazyARRaycastHit hitInfo)
        {
            bool raycast = false;

            if (isSimulated)
            {
                RaycastHit rayHit;
                Ray ray = EazyARCoreInterface.ARCamera.ScreenPointToRay(new Vector3(x, y, 0));
                raycast = Physics.Raycast(ray, out rayHit, Mathf.Infinity, (1 << DetectedPlaneLayer));
                hitInfo = rayHit;
            }
            else
            {
                TrackableHit trackableHit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

                raycast = Frame.Raycast(x, y, raycastFilter, out trackableHit);
                hitInfo = trackableHit;
            }

            return raycast;
        }

        /// <summary>
        /// Creates an Anchor at the given <c>Pose</c> that is attached to the given Trackable where semantics of the
        /// attachment relationship are defined by the subcass of Trackable (e.g. TrackedPlane).   Note that the
        /// relative offset between the Pose of multiple Anchors attached to the same Trackable may change
        /// over time as ARCore refines its understanding of the world.
        /// </summary>
        /// <param name="trackable">The trackable (e.g detected plane) that the anchor is going to be attached to.</param>
        /// <param name="pose">The Pose of the location to create the anchor.</param>
        /// <returns>An Anchor attached to the Trackable at <c>Pose</c>. The anchor is disabled automatically when ARCore is simulated in the editor</returns>
        public static Anchor CreateAnchor(Trackable trackable, Pose pose)
        {
            if (isSimulated)
            {
                GameObject anchorObj = new GameObject("Anchor");
                anchorObj.transform.position = pose.position;
                anchorObj.transform.rotation = pose.rotation;
                Anchor anchor = anchorObj.AddComponent<Anchor>();
                anchor.enabled = false;

                return anchor;

            }
            else
            {
                return trackable.CreateAnchor(pose);
            }
        }
    }
}
