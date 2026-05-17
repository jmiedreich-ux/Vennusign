using RepoDb.Interfaces;

namespace DataManager.DataAccess.TraceLogging
{
    public static class TraceFactory
    {
        private static object m_syncLock = new object();
        private static ITrace m_trace = null;

        public static ITrace CreateTracer()
        {
            if (m_trace == null)
            {
                lock (m_syncLock)
                {
                    if (m_trace == null)
                    {
                        //m_trace = new DataAcessTrace();
                    }
                }
            }
            return m_trace;
        }
    }

}
