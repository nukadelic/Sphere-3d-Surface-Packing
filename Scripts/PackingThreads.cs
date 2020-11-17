using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace SurfacePacking
{
    public class CalculateSurfaceArea : ThreadedJob
    {
        protected override void Execute( )
        {
            data.triangleSurfaceArea = 0f;

            float len = data.triangles.Count;

            for( var i = 0; i < len; ++i )
            {
                m_Progress = i / len;
                
                var triangle = data.triangles[ i ];

                var v0 = data.verticesRaw[ triangle.x ];
                var v1 = data.verticesRaw[ triangle.y ];
                var v2 = data.verticesRaw[ triangle.z ];
                
                // broken
                // http://james-ramsden.com/area-of-a-triangle-in-3d-c-code/
                //var A = v0.x * ( v1.y - v2.y );
                //var B = v1.x * ( v2.y - v0.y );
                //var C = v2.x * ( v0.y - v1.y );
                //float area = Mathf.Abs( A + B + C ) / 2f;
                
                var area = PackingFunctions.CalcTriangleArea( v0, v1, v2 );
                
                data.triangleSurfaceArea += area;
               
                triangle.area = area;

                data.triangles[ i ] = triangle;
            }
        }
    }

    public class SurfaceAreaPortion : ThreadedJob
    { 
        protected override void Execute( )
        {
            data.triangleSurfaceAreaDistribution = new List<float>();

            float len = data.triangles.Count;
            
            for(var i = 0; i < len; ++i)
            {
                m_Progress = i / len;

                var surface = data.triangles[ i ].area;

                var x = surface / data.triangleSurfaceArea;

                data.triangleSurfaceAreaDistribution.Add( x );
            }
        }
    }

    public class TriangulateMidPoints : ThreadedJob
    {
        protected override void Execute( )
        {
            float len = data.triangles.Count;

            for( var i = 0; i < len; ++i )
            {
                m_Progress = i / len;

                var T = data.triangles[ i ];
                
                var v0 = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.x ] );
                var v1 = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.y ] );
                var v2 = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * data.verticesRaw[ T.z ] );
                
                T.worldSpacePlane = new Plane( v0, v1, v2 );
                T.worldSpaceMidPoint = ( v0 + v1 + v2 ) / 3f;
                T.worldSpaceNormal = Vector3.Cross( v1 - v0, v2 - v0 );

                data.triangles[ i ] = T;
            }
        }
    }

    #region PackSpheresDenseBroken

    // del this ? 
    public class PackSpheresDenseBroken : ThreadedJob
    { 
        public int group = 0;
        public float radius = 0.05f;

        protected override void Execute( )
        {
            var count = data.triangleSurfaceArea / ( radius * radius * Mathf.PI ) ;

            int len = data.triangles.Count;

            for(var i = 0; i < count; ++i)
            {
                m_Progress = i / count;

                var tri = data.RandomTrianlge();

                //if( tri.partial )
                //{
                //    // TODO: add flag to make partial triangles be excluded
                //    continue;
                //}

                var vert = tri.RandomPoint( ref data.verticesRaw, data.random );
                var point = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * vert );
                
                point += tri.worldSpaceNormal * radius; // * 1.01f; // extra 1% to void surface touch 

                data.spheres.Add( new SpheresData 
                { 
                    index = i,
                    isDead = false,
                    normal = tri.worldSpaceNormal, 
                    position = point, 
                    radius = radius,
                    group = group
                }  );
            }
        }
    }

    #endregion

    public class PackSpheres : ThreadedJob
    {   
        public float radius = 0.05f;
        public int count = 100;
        public int group = 0;
        
        protected override void Execute( )
        {
            int len = data.triangles.Count;
            
            for( var i = 0; i < count; ++i )
            {
                m_Progress = i / count;
                
                float dart = data.random.NextFloat( 1f );
                int ti = 0;
                float sum = 0f;
                
                for( ; ti < data.triangleSurfaceAreaDistribution.Count; ++ti )
                    if( (sum += data.triangleSurfaceAreaDistribution[ ti ]) >= dart ) break;

                //var ti = data.random.NextInt( len );

                var tri = data.triangles[ ti ];
                
                var vert = tri.RandomPoint( ref data.verticesRaw, data.random );

                var point = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * vert );

                var normal = tri.GetNormal( ref data.verticesRaw ).normalized;

                point += normal * radius * 1.01f; // extra 1% to void surface touch 

                data.spheres.Add( new SpheresData { 
                    index = i, 
                    isDead = false,
                    normal = normal, 
                    position = point, 
                    radius = radius,
                    group = group
                }  );
            }
        }
    }
    
    public class PackSpheresDense : ThreadedJob
    {   
        public float radius = 0.05f;
        public int group = 0;
        
        float count;

        protected override void Finished( )
        {
            if( ! PackingConfig.logThreadTimes ) return;

            Debug.Log("PackSpheresDense.count = " + ((int) count ) );
        }

        protected override void Execute( )
        {
            int len = data.triangles.Count;

            for(var ii = 0; ii < len; ++ii)
            {
                m_Progress = ii / len;

                var tri = data.triangles[ ii ];

                var target_area = tri.area * data.spaceTargetToWorld.lossyScale.magnitude * PackingConfig.autoDensityMultiplier;

                var one_area = radius * radius * Mathf.PI;

                var current_area = 0f;

                if(one_area >= target_area)
                {
                    var sphere = new SpheresData
                    {
                        index = data.spheres.Count,
                        isDead = false,
                        normal = tri.worldSpaceNormal,
                        position = tri.worldSpaceMidPoint,
                        radius = radius,
                        group = group

                    };

                    // extra 1% to void surface touch 
                    sphere.position += sphere.normal * radius * 1.01f;

                    data.spheres.Add( sphere );
                }
                else 
                    while( current_area < target_area )
                    {
                        //var vert = tri.RandomPoint( ref data.verticesRaw, data.random );

                        var vert = data.CalcRandomPoint( ii );

                        var point = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * vert );

                        var normal = tri.GetNormal( ref data.verticesRaw ).normalized;

                        // extra 1% to void surface touch 
                        point += normal * radius * 1.01f;

                        data.spheres.Add( new SpheresData
                        {
                            index = data.spheres.Count,
                            isDead = false,
                            normal = normal,
                            position = point,
                            radius = radius,
                            group = group

                        } );

                        current_area += one_area;
                    }
            }

            //count = data.triangleSurfaceArea / ( radius * radius * Mathf.PI ) * PackingConfig.autoDensityMultiplier;

            //int tri_index = 0;

            //var current = data.triangleSurfaceAreaDistribution[ tri_index ];

            //for(var i = 0; i < count; ++i)
            //{
            //    var tri_area = current * data.triangleSurfaceArea;

            //    m_Progress = i / count;

            //    var tri = data.RandomTrianlge();

            //    var vert = tri.RandomPoint( ref data.verticesRaw, data.random );

            //    var point = data.positionTarget + ( Vector3 ) ( data.spaceTargetToWorld * vert );

            //    var normal = tri.GetNormal( ref data.verticesRaw ).normalized;

            //    point += normal * radius * 1.01f; // extra 1% to void surface touch 

            //    data.spheres.Add( new SpheresData
            //    {
            //        index = i,
            //        isDead = false,
            //        normal = normal,
            //        position = point,
            //        radius = radius,
            //        group = group
            //    } );
            //}
        }
    }
    
    public class CollideSphereVsSphere : ThreadedJob
    { 
        protected override void Execute( )
        {
            int len = data.spheres.Count;

            for( var i = 0; i < len; ++i )
            {
                m_Progress = i / (float) len;

                var S1 = data.spheres[ i ];

                if( S1.isDead ) continue;

                for( var j = i + 1; j < len; ++j )
                {
                    var S2 = data.spheres[ j ];

                    var delta = ( S1.position - S2.position );

                    var deltaMag = delta.magnitude;

                    if( deltaMag > S1.radius + S2.radius ) continue;
                    
                    var half_delta = ( S1.radius + S2.radius - deltaMag ) / 2f;

                    var shift_step = delta.normalized * PackingConfig.sphereVsSphereCollisionRatio * half_delta;

                    if( S1.group >= S2.group )  S1.position += shift_step;
                    if( S2.group >= S1.group )  S2.position -= shift_step;
                    
                    data.spheres[ j ] = S2;
                }

                data.spheres[ i ] = S1;
            }
        }
    }


    public class CollideSphereVsTriangles : ThreadedJob
    { 
        int removed = 0;
        
        protected override void Finished( )
        {
            if( ! PackingConfig.logThreadTimes ) return;

            Debug.Log("CollideSphereVsTriangles.removed = " + removed );
        }

        protected override void Execute( )
        {
            int len = data.spheres.Count;
            
            Dictionary<int,float> distances;

            int[] distanceIndexer;

            for(var i = 0; i < len; ++i)
            {
                m_Progress = i / (float) len;

                var S = data.spheres[ i ];

                if( S.isDead ) continue;
                
                distances = new Dictionary<int, float>();
                
                for( var ti = 0; ti < data.triangles.Count; ++ti )
                {
                    var P = data.triangles[ ti ].worldSpaceMidPoint;
                    distances.Add( ti, ( P - S.position ).magnitude );
                }

                distanceIndexer = distances.OrderBy( x => x.Value ).Select( x => x.Key ).ToArray();
                distances.Clear();
                distances = null;
                var triangle = data.triangles[ distanceIndexer[ 0 ] ];
                distanceIndexer = null;
                
                var point = triangle.worldSpacePlane.ClosestPointOnPlane( S.position );
                
                var delta = point - S.position;

                if( Vector3.Dot( delta, triangle.worldSpacePlane.normal ) > 0f )
                {
                    // special case when sphere is pushed under the surface 

                    S.isDead = true;

                    removed ++ ;
                }
                else
                {
                    var depth = delta.magnitude - S.radius;
                
                    S.position += delta.normalized * depth;
                }
                
                data.spheres[ i ] = S;
            }
        }
    }

    public class SphereCollisionCulling : ThreadedJob
    {
        int count = 0;
        
        protected override void Finished( )
        {
            if( ! PackingConfig.logThreadTimes ) return;

            Debug.Log("SphereCollisionCulling.count = " + count );
        }

        protected override void Execute( )
        {
            int len = data.spheres.Count;
            
            for(var i = 0; i < len; ++i)
            {
                m_Progress = i / ( float ) len;
                
                var S1 = data.spheres[ i ];

                S1.collsion = 0f;

                if( S1.isDead ) continue;

                for(var j = i + 1; j < len; ++j)
                {
                    var S2 = data.spheres[ j ];

                    if( S2.isDead ) continue;

                    var delta = ( S1.position - S2.position );

                    var deltaMag = delta.magnitude;

                    if( deltaMag > S1.radius + S2.radius ) continue;

                    if( S1.group < S2.group ) continue;

                    S1.collsion += ( 1f - deltaMag / ( S1.radius + S2.radius ) );

                    if( S1.collsion > PackingConfig.sphereCullingValue )
                    {
                        count ++ ;

                        S1.isDead = true;
                        
                        break;
                    }
                }

                data.spheres[ i ] = S1;
            }
        }
    }
}
