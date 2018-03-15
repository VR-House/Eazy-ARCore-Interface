﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

namespace EazyTools.ARCoreInterface
{

    public class ARTrackedPlaneVisualizer : MonoBehaviour
    {
        public Material material;

        public EazyARTrackedPlane TrackedPlane { get; private set; }

        MeshCollider m_meshCollider;
        MeshFilter m_meshFilter;
        MeshRenderer m_meshRenderer;
        List<Vector3> m_points = new List<Vector3>();
        List<Vector3> m_previousFramePoints = new List<Vector3>();
        Mesh m_mesh;
        bool createdSimulatedMesh = false;

        void Awake()
        {
            m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_meshFilter = gameObject.AddComponent<MeshFilter>();
            m_meshRenderer = gameObject.AddComponent<MeshRenderer>();

            m_mesh = new Mesh();
            m_meshFilter.mesh = m_mesh;
            m_meshCollider.sharedMesh = m_mesh;

            // Set layer
            gameObject.layer = EazyARCoreInterface.TrackedPlaneLayer;
        }

        public void Initialize(EazyARTrackedPlane plane)
        {
            TrackedPlane = plane;
            m_meshRenderer.material = material;
            Update();
        }

        void Update()
        {
            if (TrackedPlane == null)
            {
                return;
            }
            else if (TrackedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (TrackedPlane.TrackingState != TrackingState.Tracking)
            {
                m_meshRenderer.enabled = false;
                m_meshCollider.enabled = false;
                return;
            }

            m_meshRenderer.enabled = true;
            m_meshCollider.enabled = true;

            UpdateMeshIfNeeded();
        }

        void UpdateMeshIfNeeded()
        {
            if (EazyARCoreInterface.isSimulated)
            {
                if (createdSimulatedMesh)
                {
                    return;
                }

                //Mesh meshToCopy = ARCoreInterface.instance.simulatedTrackedPanel.GetComponent<MeshFilter>().sharedMesh;
                //m_mesh.Clear();
                //m_mesh.vertices = meshToCopy.vertices;
                //m_mesh.triangles = meshToCopy.triangles;
                //m_mesh.uv = meshToCopy.uv;
                //m_mesh.normals = meshToCopy.normals;
                //m_mesh.colors = meshToCopy.colors;
                //m_mesh.tangents = meshToCopy.tangents;
                //m_mesh.RecalculateBounds();

                //createdSimulatedMesh = true;
            }
            else
            {
                TrackedPlane.ARcoreTrackedPlane.GetBoundaryPolygon(m_points);

                if (AreVerticesListsEqual(m_previousFramePoints, m_points))
                {
                    return;
                }

                int[] indices = TriangulatorXZ.Triangulate(m_points);

                m_mesh.Clear();
                m_mesh.SetVertices(m_points);
                m_mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                m_mesh.RecalculateBounds();
            }

            m_meshCollider.sharedMesh = null;
            m_meshCollider.sharedMesh = m_mesh;
        }

        bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
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