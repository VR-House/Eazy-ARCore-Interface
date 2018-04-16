using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyTools.ARCoreInterface
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
        [Tooltip("Whether to visualize the tracked planes")]
        public bool visualizeTrackedPlanes = true;
        [Tooltip("Whether the tracked planes cast shadows")]
        public bool trackedPlanesCastShadows = false;
        [Tooltip("Whether the tracked planes receive shadows")]
        public bool trackedPlanesReceiveShadows = true;
        [Tooltip("Whether to allow other physics objects to collide with the tracked planes")]
        public bool allowTrackedPlanesCollisions = false;
        [Tooltip("The material to use to visualize the tracked planes")]
        public Material trackedPlanesMaterial;

        [Header("Point Cloud")]
        [Tooltip("Whether to visualize the point cloud")]
        public bool visualizePointCloud = true;
        [Tooltip("The material to use to visualize the point cloud")]
        public Material pointCloudMaterial;

        /// <summary>
        /// The layer of the tracked planes.
        /// </summary>
        public static LayerMask TrackedPlaneLayer { get; private set; }

        /// <summary>
        /// The tag of the tracked planes.
        /// </summary>
        public static string TrackedPlaneTag { get; private set; }

        /// <summary>
        /// The AR camera
        /// </summary>
        public static Camera ARCamera { get { return instance.arCamera; } set { instance.arCamera = value; } }

        /// <summary>
        /// Whether this is a simulation of ARcore in the editor, or playing on the actual devide
        /// </summary>
        public static bool isSimulated { get; private set; }

        /// <summary>
        /// Whether the tracked planes are visualized
        /// </summary>
        public static bool VisualizeTrackedPlanes { get { return instance.visualizeTrackedPlanes; } set { instance.visualizePointCloud = value; } }

        /// <summary>
        /// Whether the tracked planes cast shadows
        /// </summary>
        public static bool TrackedPlanesCastShadows { get { return instance.trackedPlanesCastShadows; } set { instance.visualizePointCloud = value; } }

        /// <summary>
        /// Whether the tracked planes receive shadows
        /// </summary>
        public static bool TrackedPlanesReceiveShadows { get { return instance.trackedPlanesReceiveShadows; } set { instance.visualizePointCloud = value; } }

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

            // Check if trackables layers are assigned in Layermask
            TrackedPlaneLayer = LayerMask.NameToLayer("EazyARTrackedPlane");
            if (TrackedPlaneLayer == -1)
            {
                throw new UnassignedReferenceException("Layer EazyARTrackedPlane cannot be found. Please assign it in the Layer Manager.");
            }

            TrackedPlaneTag = "EazyARTrackedPlane";

            // Disable collisions between trackables layer and other layers
            if (!allowTrackedPlanesCollisions)
            {
                for (int i = 0; i <= 31; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (layerName.Length > 0)
                    {
                        Physics.IgnoreLayerCollision(TrackedPlaneLayer, i);
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
        /// Creates the mesh of a tracked plane in the environment
        /// </summary>
        /// <param name="trackedPlane"></param>
        /// <returns></returns>
        public static EazyARTrackedPlaneMesh CreateTrackedPlane(EazyARTrackedPlane trackedPlane)
        {
            //GameObject planeObject = Instantiate(instance.trackedPlanePrefab, Vector3.zero, Quaternion.identity, instance.transform);
            GameObject planeObject = new GameObject("TrackedPlane");
            planeObject.transform.position = Vector3.zero;
            planeObject.transform.rotation = Quaternion.identity;
            planeObject.transform.SetParent(instance.transform);

            EazyARTrackedPlaneMesh planeMesh = planeObject.AddComponent<EazyARTrackedPlaneMesh>();
            planeMesh.Initialize(trackedPlane);
            return planeMesh;
        }

        /// <summary>
        /// Does an AR raycast on the tracked planes
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
                raycast = Physics.Raycast(ray, out rayHit, Mathf.Infinity, (1 << TrackedPlaneLayer));
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
        /// <param name="trackable">The trackable (e.g tracked plane) that the anchor is going to be attached to.</param>
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
