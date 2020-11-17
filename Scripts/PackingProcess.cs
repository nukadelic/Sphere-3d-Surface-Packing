namespace SurfacePacking
{
    using System.Collections;
    
    public class PackingProcess
    {
        bool busy = false;
        int stepCount = 0;
        PackingThreadData data;
        YieldProgress active;

        public bool IsPaused = true;

        public bool IsActive => busy;

        public int CurrentStep => stepCount;

        public int TotalStepsCount => 4 + data.sphereSizes.Length * 1 + PackingConfig.solverCount * 6 + 1;

        /// <summary> Get the progress value [0,1] of the current active process  </summary>
        public float CurrentStepProgress => active?.GetProgress() ?? 0f;

        public System.Action OnComplete;

        /// <summary> Make sure to call StopCoroutine before calling this function </summary>
        public void Abort()
        {
            if( active != null ) active.Abort();
        }

        IEnumerator Activate( YieldProgress target )
        {
            active = target;

            target.SetData( data );

            target.Start();

            return target.WaitFor();
        }

        public IEnumerator Execute( PackingThreadData data )
        {
            this.data = data;

            busy = true;

            stepCount = 0;
            
            // 4

            yield return Activate( new CollisionIndexer() );                stepCount ++ ;
            yield return Activate( new CalculateSurfaceArea() );            stepCount ++ ;
            yield return Activate( new SurfaceAreaPortion() );              stepCount ++ ;
            yield return Activate( new TriangulateMidPoints() );            stepCount ++ ;
            
            // 1 * sphereSizes.Lenght 
            
            for( var i = 0; i < data.sphereSizes.Length; ++ i )
            {
                while( IsPaused ) yield return null;

                var pack = new PackSpheresDense();
                pack.group = i;
                pack.radius = data.sphereSizes[ i ];
                yield return Activate( pack );
                stepCount ++ ;
            }
            
            for( var i = 0; i < PackingConfig.solverCount; ++i )
            {
                while( IsPaused ) yield return null;

                // 6 * PackingConfig.solverCount
                
                yield return Activate( new CollideSphereVsSphere() );       stepCount ++ ; // 0
                yield return Activate( new CollideSphereVsSphere() );       stepCount ++ ; // 1
                yield return Activate( new SphereCollisionCulling() );      stepCount ++ ;
                yield return Activate( new CollideSphereVsSphere() );       stepCount ++ ; // 3
                yield return Activate( new CollideSphereVsTriangles() );    stepCount ++ ;
                yield return Activate( new OutofboundsCleaner() );          stepCount ++ ;
                data.Cull();
            }

            busy = false;
            
            OnComplete?.Invoke();
        }
    }
}
