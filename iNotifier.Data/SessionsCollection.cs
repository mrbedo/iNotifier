using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionSpotter
{
    public class SessionsTriple
    {
        public IEnumerable<SessionInfo> All { get { return RaceSessions.Concat(QualSessions).Concat(QualSessions); } }
        public IEnumerable<SessionInfo> TTSessions { get; set; }
        public IEnumerable<SessionInfo> RaceSessions { get; set; }
        public IEnumerable<SessionInfo> QualSessions { get; set; }
    }

    public class SessionsCollection : NotificationObject
    {
        public bool Valid { get { return null != All; } }
        public IEnumerable<SessionInfo> TTSessions { get; private set; }
        public IEnumerable<SessionInfo> RaceSessions { get; private set; }
        public IEnumerable<SessionInfo> QualSessions { get; private set; }

        private IEnumerable<SessionInfo> _all;
        public IEnumerable<SessionInfo> All
        {
            get { return _all; }
            set
            {
                _all = value;

                if (null != _all)
                {
                    //Ordering ensures that latest goes on top when rendered
                    TTSessions = _all.Where(s => EventType.TimeTrial == s.EventType).OrderByDescending(s => s.StartTimeGMT);
                    RaceSessions = _all.Where(s => EventType.Race == s.EventType).OrderByDescending(s => s.StartTimeGMT);
                    QualSessions = _all.Where(s => EventType.Qualifying == s.EventType).OrderByDescending(s => s.StartTimeGMT);
                }
                else
                {
                    TTSessions = null;
                    RaceSessions = null;
                    QualSessions = null;
                }

                RaisePropertyChangedAll();
            }
        }
    }
}
