using System;
using System.Reflection;

namespace Euclid.Logging
{
    public sealed class Session
    {
        #region Variables
        private readonly string _context,
            _application,
            _sessionId;
        #endregion

        private Session(string context, string application, string sessionId)
        {
            _context = context;
            _application = application;
            _sessionId = sessionId;
        }

        #region Accessors
        public string Context => _context;
        public string Application => _application;
        public string SessionId => _sessionId;
        #endregion

        #region Singleton
        private static volatile Session _instance;
        private static object _lock = new object();
        public static Session Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new Session(Environment.OSVersion.VersionString,
                            Assembly.GetExecutingAssembly().GetName().Name,
                            Guid.NewGuid().ToString());
                    }
                }

                return _instance;
            }
        }
        #endregion
    }
}
