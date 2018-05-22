using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRHouse.ARTools
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
        private List<Vector3> previousFrameMeshVertices = new List<Vector3>();
        private List<Vector3> meshVertices = new List<Vector3>();
        private List<Color> meshColors = new List<Color>();
        private List<int> meshIndices = new List<int>();
        private Vector3 planeCenter = new Vector3();
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
                detectedPlane.ARcoreDetectedPlane.GetBoundaryPolygon(meshVertices);

                if (AreVerticesListsEqual(previousFrameMeshVertices, meshVertices))
                {
                    return;
                }

                previousFrameMeshVertices.Clear();
                previousFrameMeshVertices.AddRange(meshVertices);

                planeCenter = detectedPlane.CenterPose.position;
                Vector3 planeNormal = detectedPlane.CenterPose.rotation * Vector3.up;
                meshRenderer.material.SetVector("_PlaneNormal", planeNormal);
                int planePolygonCount = meshVertices.Count;

                meshColors.Clear();
                // Fill transparent color to vertices 0 to 3.
                for (int i = 0; i < planePolygonCount; ++i)
                {
                    meshColors.Add(Color.clear);
                }

                // Feather distance 0.2 meters.
                const float featherLength = 0.2f;
                // Feather scale over the distance between plane center and vertices.
                const float featherScale = 0.2f;

                // Add vertex 4 to 7.
                for (int i = 0; i < planePolygonCount; ++i)
                {
                    Vector3 v = meshVertices[i];
                    // Vector from plane center to current point
                    Vector3 d = v - planeCenter;
                    float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
                    meshVertices.Add((scale * d) + planeCenter);

                    meshColors.Add(Color.white);
                }

                meshIndices.Clear();
                int firstOuterVertex = 0;
                int firstInnerVertex = planePolygonCount;

                // Generate triangle (4, 5, 6) and (4, 6, 7).
                for (int i = 0; i < planePolygonCount - 2; ++i)
                {
                    meshIndices.Add(firstInnerVertex);
                    meshIndices.Add(firstInnerVertex + i + 1);
                    meshIndices.Add(firstInnerVertex + i + 2);
                }

                // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
                // (7, 3, 0), (7, 0, 4)
                for (int i = 0; i < planePolygonCount; ++i)
                {
                    int outerVertex1 = firstOuterVertex + i;
                    int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
                    int innerVertex1 = firstInnerVertex + i;
                    int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

                    meshIndices.Add(outerVertex1);
                    meshIndices.Add(outerVertex2);
                    meshIndices.Add(innerVertex1);

                    meshIndices.Add(innerVertex1);
                    meshIndices.Add(outerVertex2);
                    meshIndices.Add(innerVertex2);
                }

                mesh.Clear();
                mesh.SetVertices(meshVertices);
                mesh.SetIndices(meshIndices.ToArray(), MeshTopology.Triangles, 0);
                mesh.SetColors(meshColors);
            }

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;

            transform.rotation = detectedPlane.CenterPose.rotation;
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
