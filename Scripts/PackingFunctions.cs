
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SurfacePacking
{
    public static class PackingFunctions
    {
        public static int LinearTriangleIndex( this PackingThreadData data, float value )
        {
            float sum = 0f;

            var len = data.triangleSurfaceAreaDistribution.Count;
            
            for( var i = 0; i < len; ++ i )
                if( (sum += data.triangleSurfaceAreaDistribution[ i ]) >= value ) 
                    return i;

            return len - 1;
        }

        public static Triangle RandomTrianlge( this PackingThreadData data )
        {
            return data.triangles[ data.LinearTriangleIndex( data.random.NextFloat( 1f ) ) ];

            //int ti = 0;
            //float sum = 0f;
            //float dart = data.random.NextFloat( 1f );
            
            /// TRY: replace random2 with this (?) 
            //float tri_random = 0f; 
            
            //for( ; ti < data.triangleSurfaceAreaDistribution.Count; ++ ti )
            //{ 
            //    if( (sum += data.triangleSurfaceAreaDistribution[ ti ]) >= dart ) 
            //    {
            //        tri_random = ( sum - dart ) / data.triangleSurfaceAreaDistribution[ ti ];
            //        break;
            //    }
            //}
            
            //return data.triangles[ ti ];
        }
        
        public static Vector3 CalcRandomPoint( this PackingThreadData data, int triangle_index )
        {
            // https://dev.to/bogdanalexandru/generating-random-points-within-a-polygon-in-unity-nce

            var tri = data.triangles[ triangle_index ];

            var rand2 = data.random.NextFloat2( new float2( 1, 1 ) );
            var rand2pow = rand2.x * rand2.x;
            return ( 1 - rand2pow ) * data.verticesRaw[ tri.x ] 
                + ( rand2pow * ( 1 - rand2.y ) ) * data.verticesRaw[ tri.y ] 
                + ( rand2pow * rand2.y ) * data.verticesRaw[ tri.z ];
        }

        public static float CalcTriangleArea( Vector3 A, Vector3 B, Vector3 C )
        {
            // http://james-ramsden.com/area-of-a-triangle-in-3d-c-code/

            var a = Vector3.Distance( A, B );
            var b = Vector3.Distance( B, C );
            var c = Vector3.Distance( C, A );
            var s = (a + b + c) / 2f;
            return Mathf.Sqrt( s * (s-a) * (s-b) * (s-c) ); 
        }

        public static void Cull( this PackingThreadData data )
        {
            var culled = new List<SpheresData>();

            foreach( var S in data.spheres )

                if( ! S.isDead ) culled.Add( S );

            data.spheres = culled;
        }
    }
}
