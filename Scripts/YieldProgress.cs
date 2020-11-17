
namespace SurfacePacking
{ 
    using System.Collections;
    public interface YieldProgress
    {
        float GetProgress();
        bool IsComplete();
        void Abort();
        void SetData( PackingThreadData data );
        void Start();
        IEnumerator WaitFor();
    }
}