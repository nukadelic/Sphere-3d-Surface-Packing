namespace SurfacePacking
{    
    public static class PackingConfig
    {
        /// <summary> 
        /// logout debug data and the time taken per thread 
        /// </summary>
        public static bool logThreadTimes = true;

        /// <summary> 
        /// as the name suggests , a random seed to preserve experimentation consistancy 
        /// </summary>
        public static uint randomSeed = 2305;

        /// <summary> 
        /// When doing sphere collision this ration will determine how much % to seperate the spheares 
        /// away from each other, when set to 1 it will imidiatly result in a perfect seperation between 
        /// each two scanned spheres but will increase the solver difficulty. A good range to play around would be 
        /// somewhere between [ 0.55 ~ 0.85 ] - this value also depends on the solver count
        /// </summary>
        public static float sphereVsSphereCollisionRatio = 0.75f;

        /// <summary>
        /// Multiple scans are needed to pack the spheres with increased accuracy ( better density ) 
        /// </summary>
        public static int solverCount = 3;

        /// <summary>
        /// Max CPU load time per frame ( increase value with cotion ) 
        /// </summary>
        public static int maxTimeLoadInMS = 20;

        /// <summary>
        /// Amount of min shared radius value to mark a sheare as dead 
        /// ( play around 0.25 ~ 0.75 range for better packing density for special shapes ? ) 
        /// </summary>
        public static float sphereCullingValue = 1.25f; 

        /// <summary>
        /// To have a better random distribution of spheres to cover the surface set this number to a bigger value 
        /// this will result in a slight performance impact when set above ~5 times , good mid point is 1.5x 
        /// values less then 1 will result in visialbe partial packing and values below ~.35 will result in 50% of 
        /// the surface area being empty 
        /// </summary>  
        public static float autoDensityMultiplier = 1.5f;

        /// <summary>
        /// Some triangles will be cut in the middle between the two meshes 
        /// if ignore is set to true no spheres will spawn on it 
        /// depending on the mesh resolution this shouldn't be a problem unless you have a low poly object 
        /// in that case set this to false ( may cause out of bounds artifacts ) 
        /// </summary>
        public static bool ignorePartialTriangles = false;
    }
}