
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace SurfacePacking
{
    public struct Triangle
    {
        public int x;
        public int y;
        public int z;
        public bool partial;
        public float area;
        
        public bool xInRange;
        public bool yInRange;
        public bool zInRange;

        public Plane worldSpacePlane;
        public Vector3 worldSpaceMidPoint;
        public Vector3 worldSpaceNormal;
        
        public void SetValueAt( int axis, int value )
        {
            switch(axis)
            {
                case 0: x = value; xInRange = true; break;
                case 1: y = value; yInRange = true; break;
                case 2: z = value; zInRange = true; break;
                default: throw new System.Exception("bad axis");
            }
        }

        public int GetValueAt( int axis )
        {
            switch(axis)
            {   case 0: return x;case 1: return y;case 2: return z;
                default: throw new System.Exception("bad axis");
            }   
        }

        public Vector3 collisionPointX;
        public Vector3 collisionPointY;
        public Vector3 collisionPointZ;

        public Vector3 GetCollisionPointAt( int axis )
        {   
            switch(axis)
            {   case 0: return collisionPointX;
                case 1: return collisionPointY;
                case 2: return collisionPointZ;
                default: throw new System.Exception("bad axis");
            }   
        }

        public void SetCollisionAt( int axis, Vector3 value )
        {
            switch(axis)
            {
                case 0: collisionPointX = value; xInRange = false; break;
                case 1: collisionPointY = value; yInRange = false; break;
                case 2: collisionPointZ = value; zInRange = false; break;
                default: throw new System.Exception("bad axis");
            }
        }
        
        public Vector3 GetNormal( ref Vector3[] verticies )
        {
            var v0 = verticies[ x ];
            var v1 = verticies[ y ];
            var v2 = verticies[ z ];

            // https://stackoverflow.com/a/1815288
            
            return Vector3.Cross( v1 - v0, v2 - v0 );
        }

        public Vector3 RandomPoint( ref Vector3[] verticies, Random random )
        {
            // https://dev.to/bogdanalexandru/generating-random-points-within-a-polygon-in-unity-nce

            var rand2 = random.NextFloat2( new float2( 1, 1 ) );
            var rand2pow = rand2.x * rand2.x;
            return ( 1 - rand2pow ) * verticies[ this.x ] 
                + ( rand2pow * ( 1 - rand2.y ) ) * verticies[ this.y ] 
                + ( rand2pow * rand2.y ) * verticies[ this.z ];
        }

        public Plane ToPlane( ref Vector3[] verticies )
        {
            // Consider implmenting this one via burst compiler: 
            // https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/closest_point_to_triangle.html

            var v0 = verticies[ x ];
            var v1 = verticies[ y ];
            var v2 = verticies[ z ];

            return new Plane( v0, v1, v2 );
        }
        

        //public Vector3 GetNormalY( List<Vector3> verticies )
        //{
        //    var v0 = verticies[ y ];
        //    var v1 = verticies[ z ];
        //    var v2 = verticies[ x ];
        //    return Vector3.Cross( v1 - v0, v2 - v0 );
        //}
        //public Vector3 GetNormalZ( List<Vector3> verticies )
        //{
        //    var v0 = verticies[ z ];
        //    var v1 = verticies[ x ];
        //    var v2 = verticies[ y ];
        //    return Vector3.Cross( v1 - v0, v2 - v0 );
        //}
    }
}
