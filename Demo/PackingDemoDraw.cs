using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// DEBUG ONLY SCRIPT // 

namespace SurfacePacking
{
    public class PackingDemoDraw : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField, HideInInspector] public Camera SVC;
        #endif
        
        [HideInInspector] public PackingThreadData data;
        
        void OnDrawGizmos( )
        {
            if( data == null) return;

            if( data.spheres == null || data.spheres.Count < 1 ) return;

            if( draw_spheres ) DrawSpheres();

            if( draw_triangles ) DrawTriangles();

            DrawTiangleIndex();

            if( draw_verticies_in_bounds ) DrawVerticiesInBounds();
        }

        public bool draw_spheres = true;

        void DrawSpheres()
        {
            var CM = Matrix4x4.identity;
            if( SVC != null ) CM = SVC.cameraToWorldMatrix;
            for( var i = 0; i < data.spheres.Count; ++i )
            {
                var PD = data.spheres[ i ];
                var R = PD.radius;
                var P = PD.position;
                
                //var L = PD.collsion > 0 ? 1 - PD.collsion : PD.collsion / PackingConfig.sphereCullingValue;
                //var C = Color.Lerp( Color.green, Color.blue, L );
                var C = PD.group > 0 ? Color.red : Color.blue;

                #if UNITY_EDITOR

                Handles.color = C;
                Handles.DrawWireDisc( P, CM * Vector3.forward, R );
                Handles.color = Color.green;
                Handles.DrawLine( P, P + PD.normal * R );

                #endif

                Gizmos.color = C;
                Gizmos.DrawWireSphere( P, R );
            }
        }

        public bool draw_verticies_in_bounds = false;

        void DrawVerticiesInBounds()
        {
            for( var j = 0; j < data.verticesInBounds.Count; ++j )
            {
                var idx = data.verticesInBounds[ j ];

                var V = data.verticesRaw[ idx ];
                    
                var P = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * V );
                    
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere( P, 0.01f );
            }
        }

        public bool draw_triangles = false;
        public bool draw_triangle_all = false;
        public int draw_triangle_index = -1;

        void DrawTriangles()
        {
            foreach( var t in data.triangles )
            {
                for( int i = 0; i < ( draw_triangle_all ? 3 : 1 ) ; ++i )
                {
                    var v = ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ t.GetValueAt( i ) ] );

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere( data.positionTarget + v, 0.01f );

                    if( t.partial )     
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere( t.GetCollisionPointAt( i ), 0.01f );
                    }
                }
            }
        }

        void DrawTiangleIndex()
        {
            if( draw_triangle_index > -1 && draw_triangle_index < data.triangles.Count )
            {
                var T = data.triangles[ draw_triangle_index ];
                
                var Px = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.x ] );
                var Py = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.y ] );
                var Pz = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.z ] );
                
                #if UNITY_EDITOR

                Handles.color = Color.white;
                Handles.DrawAAPolyLine( 2f, Px, Py, Pz, Px );
                Handles.Label( Px, "area:" + T.area.ToString("N3")
                    + "\nA2:" + ( T.area * data.spaceTargetToWorld.lossyScale.magnitude * PackingConfig.autoDensityMultiplier ).ToString("N3") 
                );
                
                #endif
                
                if( T.partial )     
                {
                    Gizmos.color = Color.blue;
                    if( ! T.xInRange ) Gizmos.DrawWireSphere( T.GetCollisionPointAt( 0 ), 0.01f );
                    if( ! T.yInRange ) Gizmos.DrawWireSphere( T.GetCollisionPointAt( 1 ), 0.01f );
                    if( ! T.zInRange ) Gizmos.DrawWireSphere( T.GetCollisionPointAt( 2 ), 0.01f );
                }
            }
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(PackingDemoDraw))]
    public class ViewSpheresEditor : Editor
    {
        void OnEnable( ) => SceneView.duringSceneGui += OnSceneGUI;
        void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;
        void OnSceneGUI( SceneView view ) => ( (PackingDemoDraw) target ).SVC = view.camera;
    }
    #endif
}
