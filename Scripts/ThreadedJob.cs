
// based on Bunny83's progress thread example , link : 
// http://answers.unity.com/answers/357040/view.html

using System.Collections;
using UnityEngine;

namespace SurfacePacking
{
    public class ThreadedJob : YieldProgress
    {
        public float GetProgress() => m_Progress;
        public bool IsComplete() => IsDone;
        
        protected float m_Progress = 0f;
        
        protected PackingThreadData data;

        public void SetData( PackingThreadData data ) => this.data = data;

        public System.Action OnFinished;

        private bool m_IsDone = false;
        private object m_Handle = new object();
        private System.Threading.Thread m_Thread = null;
        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        float start_time = 0f;

        public virtual void Start()
        {
            start_time = Time.timeSinceLevelLoad;

            m_Progress = 0f;

            m_Thread = new System.Threading.Thread(Run);
            m_Thread.Start();
        }
        public virtual void Abort()
        {
            m_Thread.Abort();
        }

        protected virtual void Execute() { }
 
        protected virtual void Finished() { }
 
        public virtual bool Update()
        {
            if (IsDone)
            {
                if( PackingConfig.logThreadTimes )
                {
                    var t = ( 1000f * ( Time.timeSinceLevelLoad - start_time ) ).ToString( "N2" ) + "ms";
                    Debug.Log( $"[ThreadedJob::{GetType().Name}] complete in " + t );
                }

                Finished();
                OnFinished?.Invoke();
                return true;
            }
            return false;
        }
        public IEnumerator WaitFor()
        {
            while( ! Update() )
            {
                yield return null;
            }
        }

        void Run()
        {
            Execute();

            m_Progress = 1f;
            IsDone = true;
        }
    }
}