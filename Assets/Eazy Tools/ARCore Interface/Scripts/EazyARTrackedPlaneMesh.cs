using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyTools.ARCoreInterface
{
    public class EazyARDetectedPlaneMesh : MonoBehaviour
    {
        /// <summary>
        /// The trakced planed this mesh represents
        /// </summary>
        public EazyARDetectedPlane detectedPlane { get; private set; }

        /// <summary>
        /// Whether collisions with other physics objects are allowed on this object
        /// </summary>
        public bool AllowCollisions { get; private set; }

        private MeshCollider meshCollider;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private List<Vector3> boundaryPoints = new List<Vector3>();
        private List<Vector3> previousFrameBoundaryPoints = new List<Vector3>();
        private Mesh mesh;
        private bool createdSimulatedMesh = false;
        private bool initialized = false;

        void Awake()
        {
            // Initialize components
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // Set renderer settings
            meshRenderer.shadowCastingMode = EazyARCoreInterface.DetectedPlanesCastShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = EazyARCoreInterface.DetectedPlanesReceiveShadows;

            // Create empty mesh
            mesh = new Mesh();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

            // Set layer & tag
            gameObject.layer = EazyARCoreInterface.DetectedPlaneLayer;
            gameObject.tag = EazyARCoreInterface.DetectedPlaneTag;
            
        }

        /// <summary>
        /// Initialized and created the mesh
        /// </summary>
        /// <param name="plane"></param>
        public void Initialize(EazyARDetectedPlane plane)
        {
            detectedPlane = plane;
            meshRenderer.material = detectedPlane.Direction == EazyARDetectedPlane.PlaneDirection.Horizontal ? EazyARCoreInterface.instance.detectedHorizontalPlanesMaterial : EazyARCoreInterface.instance.detectedVerticalPlanesMaterial;
            initialized = true;
            Update();
        }

        void Update()
        {
            if (!initialized)
            {
                throw new System.InvalidOperationException("ARDetectedPlaneMesh is not initialized. Call Initialize with appropriate configuration as soon as component is created");
            }

            if (detectedPlane == null)
            {
                return;
            }
            else if (detectedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (detectedPlane.TrackingState != TrackingState.Tracking)
            {
                meshRenderer.enabled = false;
                meshCollider.enabled = false;
                return;
            }

            meshRenderer.enabled = meshRenderer.material == null || !EazyARCoreInterface.instance.visualizeDetectedPlanes ? false : true;
            meshCollider.enabled = true;

            UpdateMeshIfNeeded();
        }

        private void UpdateMeshIfNeeded()
        {
            if (EazyARCoreInterface.isSimulated)
            {
                if (createdSimulatedMesh)
                {
                    return;
                }

                // Create a blueprint plane to copy its mesh that simulates an AR detected plane
                GameObject blueprintPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);

                // Copy the plane mesh
                Mesh meshToCopy = blueprintPlane.GetComponent<MeshFilter>().sharedMesh;
                mesh.Clear();
                mesh.vertices = meshToCopy.vertices;
                mesh.triangles = meshToCopy.triangles;
                mesh.uv = meshToCopy.uv;
                mesh.normals = meshToCopy.normals;
                mesh.colors = meshToCopy.colors;
                mesh.tangents = meshToCopy.tangents;
                mesh.RecalculateBounds();

                createdSimulatedMesh = true;

                Destroy(blueprintPlane.gameObject);
            }
            else
            {
                detectedPlane.ARcoreDetectedPlane.GetBoundaryPolygon(boundaryPoints);

                if (AreVerticesListsEqual(previousFrameBoundaryPoints, boundaryPoints))
                {
                    return;
                }

                int[] indices = TriangulatorXZ.Triangulate(boundaryPoints);

                mesh.Clear();
                mesh.SetVertices(boundaryPoints);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateBounds();
            }

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        private bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
        {
            if (firstList.Count != secondList.Count)
            {
                return false;
            }

            for (int i = 0; i < firstList.Count; i++)
            {
                if (firstList[i] != secondList[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
