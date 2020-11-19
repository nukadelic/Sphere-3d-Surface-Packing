
using UnityEngine;
//using Parabox.CSG;

namespace SurfacePacking
{
    public class PackingDemoScript : MonoBehaviour
    {
        // Reference vars 

        [Header("Game Object References")]
        public MeshFilter slicer;
        public MeshFilter target;
        public Transform[] erasers;
        public PackingDemoDraw draw;

        // Spheres Sizes 

        [Tooltip("Note, size will be auto sorted when extraced in the `Extract()` method ")]
        public float[] sizes = new float[] { 0.1f, 0.05f };
        
        // Settings vars 

        [Header("PackingConfig")]
        public bool logThreadTimes = true;
        public uint randomSeed = 23487;
        public float sphereVsSphereCollisionRatio = 0.75f;
        public int solverCount = 3;
        public int maxTimeLoadInMS = 20;
        public float sphereCullingValue = 0.5f;
        public float autoDensityMultiplier = 1.5f;
        public bool ignorePartialTriangles = false;

        // Private vars 

        PackingThreadData data;
        PackingProcess packing;

        //public bool Paused = true;

        //void Update( )
        //{
        //    if( packing != null ) packing.IsPaused = Paused;
        //    Paused = true;
        //}

        void Start( )
        {
            // Set config 

            PackingConfig.logThreadTimes = logThreadTimes;
            PackingConfig.randomSeed = randomSeed;
            PackingConfig.sphereVsSphereCollisionRatio = sphereVsSphereCollisionRatio;
            PackingConfig.solverCount = solverCount;
            PackingConfig.maxTimeLoadInMS = maxTimeLoadInMS;
            PackingConfig.sphereCullingValue = sphereCullingValue;
            PackingConfig.autoDensityMultiplier = autoDensityMultiplier;
            PackingConfig.ignorePartialTriangles = ignorePartialTriangles;
            
            // Create data
            
            data = PackingThreadData.Extract( target, slicer, sizes );

            for( var e = 0; e < erasers.Length; ++e )
            {
                data.AddSubstractingCube( erasers[ e ] );
            }
            
            if( draw != null ) draw.data = data;

            // Create process instance

            packing = new PackingProcess( );

            // Record time

            var t = Time.realtimeSinceStartup;

            // Listen to events 

            packing.OnComplete = ( ) => 
            { 
                t = ( Time.realtimeSinceStartup - t ) * 1000;
                Debug.Log( "PackingProcess: " + t.ToString("N2") + "ms" );
                Debug.Log( $"Result in { data.spheres.Count } spheres");

                CreateMesh();
            };
            
            // Start 

            StartCoroutine( packing.Execute( data ) );
        }

        void CreateMesh()
        {
            //CSG_Model result = Boolean.Intersect( target.gameObject, slicer.gameObject );
            //var composite = new GameObject( "[ Created Mesh ]" );
            //composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
            //composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
            //composite.transform.position = new Vector3( 1, 0, 0 );
        }

        private void OnDestroy( )
        {
            if( packing != null )
            {
                packing.Abort();
                packing = null;
            }
        }

        private void OnGUI( )
        {
            if( packing == null ) return;

            GUILayout.Label("Is Active = " + packing.IsActive );

            var current_progress = packing.CurrentStepProgress;
            
            GUILayout.Label("Current Progress = " + current_progress.ToString("N2") );
            
            GUILayout.Label( $"Step {packing.CurrentStep} / {packing.TotalStepsCount} " );

            var total_progress = ( current_progress + packing.CurrentStep ) / packing.TotalStepsCount;
            
            GUILayout.Label("Total Progress = " + total_progress.ToString("N2") );
        }
    }
}