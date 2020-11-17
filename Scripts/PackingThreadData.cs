
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace SurfacePacking
{
    public class PackingThreadData
    { 
        public static PackingThreadData Extract( MeshFilter target, MeshFilter slicer, float[] sizes )
        {
            Collider colliderSlicer = slicer.GetComponent<Collider>();

            if( colliderSlicer == null )
            {
                var MC = slicer.gameObject.AddComponent<MeshCollider>();
                MC.sharedMesh = slicer.sharedMesh;
                colliderSlicer = MC;
            }

            Collider colliderTarget = target.GetComponent<Collider>();

            if( colliderTarget == null )
            {
                var MC = target.gameObject.AddComponent<MeshCollider>();
                MC.sharedMesh = target.sharedMesh;
                colliderTarget = MC;
            }

            var mesh = target.mesh;

            var data = new PackingThreadData();

            var SZ = sizes.ToList(); SZ.Sort(); SZ.Reverse();

            data.sphereSizes = SZ.ToArray();

            data.colliderSlicer = colliderSlicer;
            data.colliderTarget = colliderTarget;

            data.random = new Unity.Mathematics.Random( PackingConfig.randomSeed );
            
            data.trianglesRaw = mesh.triangles;
            data.verticesRaw = mesh.vertices;
            data.normalsRaw = mesh.normals;

            data.positionTarget = target.transform.position;
            data.positionSlicer = slicer.transform.position;
            
            data.spaceSlicerToWorld = slicer.transform.localToWorldMatrix;
            data.spaceWorldToSlicer = slicer.transform.worldToLocalMatrix;
            data.spaceTargetToWorld = target.transform.localToWorldMatrix;
            data.spaceWorldToTarget = target.transform.worldToLocalMatrix;

            return data;
        }

        
        public Vector3 positionTarget;
        public Vector3 positionSlicer;

        public Collider colliderSlicer;
        public Collider colliderTarget;

        public Matrix4x4 spaceTargetToWorld;

        public Matrix4x4 spaceWorldToTarget;
        public Matrix4x4 spaceSlicerToWorld;
        public Matrix4x4 spaceWorldToSlicer;

        public Vector3[] verticesRaw;
        public int[] trianglesRaw;
        public Vector3[] normalsRaw;

        public Random random;
        
        public float[ ] sphereSizes = new float[] { 0.05f };

        public List<SpheresData> spheres = new List<SpheresData>();

        /// <summary> The total surface area occupied by all of the triangles </summary>
        public float triangleSurfaceArea = 0f;

        /// <summary> Stores the procentage of the area occupied by the triangle at index [i] relative to the total surface area </summary>
        public List<float> triangleSurfaceAreaDistribution = new List<float>();    

        /// <summary> Processed triangles </summary>
        public List<Triangle> triangles = new List<Triangle>();
        /// <summary> Processed verticy indexes </summary>
        public List<int> verticesInBounds = new List<int>();

    }
}
