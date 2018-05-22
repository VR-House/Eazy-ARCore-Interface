namespace VRHouse.ARTools.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloEazyARController : MonoBehaviour
    {
        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject robotPrefab;

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject searchingForPlaneUI;

        /// <summary>
        /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<EazyARDetectedPlane> m_NewPlanes = new List<EazyARDetectedPlane>();

        /// <summary>
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<EazyARDetectedPlane> m_AllPlanes = new List<EazyARDetectedPlane>();

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            //_QuitOnConnectionErrors();

            // Check that motion tracking is tracking.
            if (EazyARSession.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                if (!m_IsQuitting && EazyARSession.Status.IsARValid())
                {
                    searchingForPlaneUI.SetActive(true);
                }

                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            m_NewPlanes = EazyARSession.GetTrackablePlanes(TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                EazyARCoreInterface.CreateDetectedPlane(m_NewPlanes[i]);
            }

            // Disable the snackbar UI when no planes are valid.
            m_AllPlanes = EazyARSession.GetTrackablePlanes();
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            searchingForPlaneUI.SetActive(showSearchingUI);

            // If the player has not touched the screen, we are done with this update.
            Vector3 inputPos = Vector3.zero;
            #if UNITY_EDITOR
                if (!Input.GetMouseButtonDown(0))
                {
                    return;
                }
                else
                {
                    inputPos = Input.mousePosition;
                }
            #elif UNITY_ANDROID
                Touch touch;
                if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                {
                    return;
                }
                inputPos = Input.GetTouch(0).position;
            #endif

            // Raycast against the location the player touched to search for planes.
            EazyARRaycastHit hit;
            if (EazyARCoreInterface.ARRaycast(inputPos.x, inputPos.y, out hit))
            {
                GameObject andyObj = Instantiate(robotPrefab, hit.Pose.position, hit.Pose.rotation);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                Anchor anchor = EazyARCoreInterface.CreateAnchor(hit.Trackable, hit.Pose);

                // Andy should look at the camera but still be flush with the plane.
                if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                {
                    // Get the camera position and match the y-component with the hit position.
                    Vector3 cameraPositionSameY = EazyARCoreInterface.ARCamera.transform.position;
                    cameraPositionSameY.y = hit.Pose.position.y;

                    // Have Andy look toward the camera respecting his "up" perspective, which may be from ceiling.
                    Vector3 direction = EazyARCoreInterface.ARCamera.transform.position - andyObj.transform.position;
                    float dot = Vector3.Dot(direction, Vector3.up);
                    bool orthogonal = ((dot == 1f) || (dot == -1f));
                    if (orthogonal)
                    {
                        return;
                    }

                    Vector3 fwd = Vector3.ProjectOnPlane(direction, Vector3.up);
                    Quaternion fwdRot = Quaternion.LookRotation(fwd, Vector3.up);
                    andyObj.transform.localRotation = andyObj.transform.localRotation * fwdRot;
                }

                // Make Andy model a child of the anchor.
                andyObj.transform.parent = anchor.transform;
            }
        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors()
        {
            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (EazyARSession.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (EazyARSession.Status.IsARError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
