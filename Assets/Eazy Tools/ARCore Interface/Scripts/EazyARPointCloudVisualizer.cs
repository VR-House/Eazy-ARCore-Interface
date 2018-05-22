using GoogleARCore;
using UnityEngine;

namespace VRHouse.ARTools
{
    public class EazyARCorePointCloudVisualizer : MonoBehaviour
    {
        private const int MAX_POINTS_COUNT = 61440;
        private Mesh mesh;
        private Renderer renderer;
        private Vector3[] points = new Vector3[MAX_POINTS_COUNT];

        public void Start()
        {
            renderer = GetComponent<Renderer>();
            mesh = GetComponent<MeshFilter>().mesh;
            mesh.Clear();
        }

        public void Update()
        {
            renderer.enabled = EazyARCoreInterface.VisualizePointCloud;

            // Fill in the data to draw the point cloud.
            if (EazyARCoreInterface.instance.visualizePointCloud && Frame.PointCloud.IsUpdatedThisFrame)
            {
                // Copy the point cloud points for mesh verticies.
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    points[i] = Frame.PointCloud.GetPoint(i);
                }

                // Update the mesh indicies array.
                int[] indices = new int[Frame.PointCloud.PointCount];
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    indices[i] = i;
                }

                mesh.Clear();
                mesh.vertices = points;
                mesh.SetIndices(indices, MeshTopology.Points, 0);
            }
        }
    }
}
