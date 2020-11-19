using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SurfacePacking
{
    public class CollisionIndexer : YieldProgress
    {
        bool m_Abort = false;
        float m_Progress = 0f;
        bool m_Complete = false;
        public float GetProgress() => m_Progress;
        public bool IsComplete() => m_Complete;
        public void Abort() => m_Abort = true;
        public void SetData( PackingThreadData data ) => this.data = data;
        public void Start() { }
        public PackingThreadData data;
        //public PackingThreadData data { get => _data; set => _data = value; }

        public IEnumerator WaitFor()
        {
            var start_time = Time.realtimeSinceStartup;

            var cooldown = start_time;
            
            //bool hasNegativeSpace = data.colliderRemover != null;

            //bool hasNegativeSpace = data.subtractingCubes.Count > 0;

            float len = data.trianglesRaw.Length;

            for( var i = 0; i < len; i += 3 )
            {
                if( m_Abort ) break;
                m_Progress = i / len;

                if( i % 666 == 0 )
                {
                    var t = Time.realtimeSinceStartup;
                    
                    if( ( t - cooldown ) * 1000 > PackingConfig.maxTimeLoadInMS  )
                    {
                        cooldown = t;
                        yield return null;
                    }
                }
                
                int in_bounds_count = 0;

                Triangle triangle = new Triangle { xInRange = false,yInRange = false,zInRange = false };

                for( var t = 0; t < 3; ++t )
                {
                    int ti = i + t;

                    var vi = data.trianglesRaw[ ti ];
                    
                    triangle.SetValueAt( t, vi );
                    
                    var vert = data.verticesRaw[ vi ];
                    
                    var P = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * vert );

                    var R = new Ray( P, data.positionSlicer - P );

                    if( data.PointIsInsideNegativeSpace( P ) ) continue;

                    //if( hasNegativeSpace )
                    //{
                    //    bool found = false;

                    //    foreach( var subCube in data.subtractingCubes )
                    //    {
                    //        if( subCube.ContainesWolrdPoint( P ) )
                    //        {
                    //            found=  true;
                    //            break;
                    //        }
                    //    }

                    //    if( found ) continue;

                    //    //if(data.colliderRemover.Raycast( R, out RaycastHit hit_negative, float.MaxValue ))
                    //    //{
                    //    //    triangle.SetCollisionAt( t, hit_negative.point );

                    //    //    continue;
                    //    //}
                    //}

                    bool isHit = data.colliderSlicer.Raycast( R , out RaycastHit hit, float.MaxValue );

                    if( ! isHit )
                    {
                        in_bounds_count ++ ;
                        
                        if ( ! data.verticesInBounds.Contains( vi ) ) 
                            
                            data.verticesInBounds.Add( vi ); 
                    }

                    else triangle.SetCollisionAt( t, hit.point );
                }
                
                if( in_bounds_count < 1 ) continue;
                
                triangle.partial = in_bounds_count < 3;

                if( ! ( PackingConfig.ignorePartialTriangles && triangle.partial ) )

                    data.triangles.Add( triangle );
            }
            
            if( PackingConfig.logThreadTimes )
            {
                var t = ( 1000f * ( Time.realtimeSinceStartup - start_time ) ).ToString( "N2" ) + "ms";
                Debug.Log( $"[PackingCoroutine::{GetType().Name}] complete in " + t );
            }

            m_Complete = true;
        }
    }

    public class OutofboundsCleaner : YieldProgress
    { 
        bool m_Abort = false;
        float m_Progress = 0f;
        bool m_Complete = false;
        public float GetProgress() => m_Progress;
        public bool IsComplete() => m_Complete;
        public void Abort() => m_Abort = true;
        public void SetData( PackingThreadData data ) => this.data = data;
        public void Start() { }
        public PackingThreadData data;
        
        public IEnumerator WaitFor()
        {
            int removed = 0;
            
            var start_time = Time.realtimeSinceStartup;
            
            var cooldown = start_time;
            
            float len = data.spheres.Count;

            for( var i = 0; i < len; i += 3 )
            {
                if( m_Abort ) break;

                m_Progress = i / len;

                if( i % 666 == 0 )
                {
                    var t = Time.realtimeSinceStartup;
                    
                    if( ( t - cooldown ) * 1000 > PackingConfig.maxTimeLoadInMS  )
                    {
                        cooldown = t;
                        yield return null;
                    }
                }

                var S = data.spheres[ i ];

                if( S.isDead ) continue;

                var R = new Ray( S.position, data.positionSlicer - S.position );

                bool isHit = data.colliderSlicer.Raycast( R , out RaycastHit hit, float.MaxValue );

                if( isHit ) 
                { 
                    S.isDead = true;

                    removed ++ ;
                }
            }

            if( PackingConfig.logThreadTimes )
            {
                var t = ( 1000f * ( Time.realtimeSinceStartup - start_time ) ).ToString( "N2" ) + "ms";
                Debug.Log( $"[PackingCoroutine::{GetType().Name}] complete in " + t );
                Debug.Log( GetType().Name + ".removed = " + removed );
            }

            m_Complete = true;
        }
    }

    
    //public class SurfaceGlue : YieldProgress
    //{ 
    //    bool m_Abort = false;
    //    float m_Progress = 0f;
    //    bool m_Complete = false;
    //    public float GetProgress() => m_Progress;
    //    public bool IsComplete() => m_Complete;
    //    public void Abort() => m_Abort = true;
    //    public void SetData( PackingThreadData data ) => this.data = data;
    //    public void Start() { }
    //    public PackingThreadData data;

    //    public IEnumerator WaitFor()
    //    {
    //        var start_time = Time.realtimeSinceStartup;

    //        var cooldown = start_time;
            
    //        float len = data.spheres.Count;

    //        int debugIndex = data.spheres.Count - 5;

    //        for(var i = 0; i < len; ++i)
    //        {
    //            if( m_Abort ) break;
    //            m_Progress = i / len;

    //            if(i % 50 == 0)
    //            {
    //                var t = Time.realtimeSinceStartup;

    //                if(( t - cooldown ) * 1000 > PackingConfig.maxTimeLoadInMS )
    //                {
    //                    cooldown = t;
    //                    yield return null;
    //                }
    //            }

    //            var sphere = data.spheres[ i ];
                
    //            var point = data.colliderTarget.ClosestPoint( sphere.position );
                
    //            if( i == debugIndex ) data.debugPoints = new List<Vector3>() { sphere.position, point };
                
    //            var delta = point - sphere.position;

    //            var depth = delta.magnitude - sphere.radius;

    //            sphere.position += delta.normalized * depth;

    //            data.spheres[ i ] = sphere;
    //        }
            
    //        var end_t = ( 1000f * ( Time.timeSinceLevelLoad - start_time ) ).ToString( "N1" ) + "ms";

    //        Debug.Log("SurfaceGlue complete in " + end_t );

    //        m_Complete = true;
    //    }
    //}
}
