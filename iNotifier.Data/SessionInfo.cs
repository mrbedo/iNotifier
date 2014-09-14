using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionSpotter
{
    /// <summary>
    //WeekendInfo:
    //TrackName: montreal
    //TrackID: 218
    //TrackLength: 4.31 km
    //TrackDisplayName: Circuit Gilles Villeneuve
    //TrackDisplayShortName: Montreal
    //TrackCity: Montreal
    //TrackCountry: Canada
    //TrackAltitude: 8.39 m
    //TrackLatitude: 45.500092 m
    //TrackLongitude: -73.522985 m
    //TrackNumTurns: 13
    //TrackPitSpeedLimit: 64.37 kph
    //TrackType: road course
    //TrackWeatherType: Constant
    //TrackSkies: Partly Cloudy
    //TrackSurfaceTemp: 39.81 C
    //TrackAirTemp: 25.56 C
    //TrackAirPressure: 29.89 Hg
    //TrackWindVel: 0.89 m/s
    //TrackWindDir: 0.00 rad
    //TrackRelativeHumidity: 55 %
    //TrackFogLevel: 0 %
    //SeriesID: 114
    //SeasonID: 963
    //SessionID: 37645973
    //SubSessionID: 8880346
    //LeagueID: 0
    //Official: 1
    //RaceWeek: 2
    //EventType: Practice
    //Category: Road
    //SimMode: full
    //WeekendOptions:
    // NumStarters: 44
    // StartingGrid: 2x2 inline pole on left
    // QualifyScoring: best lap
    // CourseCautions: off
    // StandingStart: 1
    // Restarts: single file
    // WeatherType: Constant
    // Skies: Partly Cloudy
    // WindDirection: N
    // WindSpeed: 3.22 km/h
    // WeatherTemp: 25.56 C
    // RelativeHumidity: 55 %
    // FogLevel: 0 %
    // Unofficial: 0
    // CommercialMode: consumer
    // NightMode: 0
    // IsFixedSetup: 0
    // StrictLapsChecking: default
    // HasOpenRegistration: 1
    // HardcoreLevel: 0
    /// </summary>
    public class WeekendInfo : NotificationObject
    {
        private int _seriesID;
        private int _seasonID;
        private string _seasonName;

        public int SeriesID
        {
            get { return _seriesID; }
            set
            {
                _seriesID = value;
                RaisePropertyChanged();
            }
        }

        public int SeasonID
        {
            get { return _seasonID; }
            set
            {
                _seasonID = value;
                RaisePropertyChanged();
            }
        }

        public string SeasonName
        {
            get { return _seasonName; }
            set
            {
                _seasonName = value;
                RaisePropertyChanged();
            }
        }
    }

    public enum EventType
    {        
        All = 0,
        Qualifying = 3,
        TimeTrial = 4,
        Race = 5,
    };

    public class SessionInfoIDComparer : IEqualityComparer<SessionInfo>
    {
        public bool Equals(SessionInfo x, SessionInfo y)
        {
            return x.SessionID == y.SessionID;
        }

        public int GetHashCode(SessionInfo obj)
        {
            return obj.SessionID.GetHashCode();
        }
    }

    public class SessionInfoStartTimeComparer : IEqualityComparer<SessionInfo>
    {
        public bool Equals(SessionInfo x, SessionInfo y)
        {
            return x.StartTimeGMT.Equals(y.StartTimeGMT);
        }

        public int GetHashCode(SessionInfo obj)
        {
            return obj.StartTimeGMT.GetHashCode();
        }
    }

    public class SessionInfo : NotificationObject
    {
        private static long _objectID = 0;
        public static readonly SessionInfoIDComparer SessionIDComparer = new SessionInfoIDComparer();
        public static readonly SessionInfoStartTimeComparer StartTimeUTCComparer = new SessionInfoStartTimeComparer();
        public SessionInfo()
        {
            ObjectID = _objectID++;
            JoinBuffer = TimeSpan.FromSeconds(15);
        }

        public long ObjectID { get; private set; }

        public bool IsVisible
        {
            get
            {
                return (EventType == EventType.Race && Config<UserSettings>.Instance.ShowRace)
                    || (EventType == EventType.Qualifying && Config<UserSettings>.Instance.ShowQual)
                    || (EventType == EventType.TimeTrial && Config<UserSettings>.Instance.ShowTT);
            }
        }

        public long SessionID { get; set; }
        public int SeasonID { get; set; }
        public EventType EventType { get; set; }
        public DateTime StartTimeGMT { get; set; }
        public DateTime StartTimeLocal { get; set; }
        public int NumRegistered { get; set; }
        public TimeSpan JoinBuffer { get; set; }

        /// <summary>
        /// Amount of time remaining until the user should join.
        /// Not the same as the difference between now and the scheduled session time
        /// because iRacing will take the link off the roster a few seconds before its due.
        /// </summary>
        public TimeSpan RemainingTimeToJoin
        {
            get { return StartTimeLocal - DateTime.Now - JoinBuffer; }
        }

        public double RemainingSeconds
        {
            get { return RemainingTimeToJoin.TotalSeconds; }
        }

        public bool IsExpired
        {
            get { return RemainingTimeToJoin.TotalSeconds < 0; }
        }

        public bool IsFinalAlert
        {
            get { return RemainingTimeToJoin <= TimeSpan.FromSeconds(Config<UserSettings>.Instance.FinalReminderSeconds); }
        }

        public bool IsHighAlert
        {
            get { return RemainingTimeToJoin <= GetHighAlarmSetting(EventType, true); }
        }

        public bool IsLowAlert
        {
            get { return RemainingTimeToJoin <= GetHighAlarmSetting(EventType, false); }
        }

        private TimeSpan GetHighAlarmSetting(EventType eventType, bool high)
        {
            var s = Config<UserSettings>.Instance;
            switch (eventType)
            {
                case EventType.Race: return TimeSpan.FromSeconds(high ? s.RaceHighAlertSeconds : s.RaceLowAlertSeconds);
                case EventType.Qualifying: return TimeSpan.FromSeconds(high ? s.QualHighAlertSeconds : s.QualLowAlertSeconds);
                case EventType.TimeTrial: return TimeSpan.FromSeconds(high ? s.TTHighAlertSeconds : s.TTLowAlertSeconds);
                default: return TimeSpan.FromSeconds(0);
            }
        }

        public void Refresh()
        {
            RaisePropertyChangedAll();
        }

        public override string ToString()
        {
            return String.Format("{0}\t::\t{1} - {2} GMT  [{3}] ({4})",
                RemainingTimeToJoin.ToString("c"),
                EventType,
                StartTimeGMT.ToString("HH:mm"),
                StartTimeLocal.ToString("hh:mm"),
                NumRegistered);
        }

        public bool NotifiedOnLowAlert { get; set; }
        public bool NotifiedOnHighAlert { get; set; }
        public bool NotifiedOnFinalAlert { get; set; }

        public string RemainingTimeSpeech()
        {
            var sb = new StringBuilder();
            var h = RemainingTimeToJoin.Hours;
            var m = RemainingTimeToJoin.Minutes;
            var s = RemainingTimeToJoin.Seconds;

            sb.Append(EventTypeToText(EventType)).Append(" in ");

            if (0 < h) sb.Append(h).Append(1 == h ? " hour" : " hours");
            if (0 < m) sb.Append(m).Append(1 == m ? " minute" : " minutes");
            if (0 < s)
            {
                if (0 < m) sb.Append(" and");
                sb.Append(s).Append(1 == s ? " second" : " seconds");
            }

            if (0 < NumRegistered)
            {
                sb.Append(" with ")
                    .Append(NumRegistered)
                    .Append(1 == NumRegistered ? " driver" : " drivers")
                    .Append("registered");
            }

            return sb.Append(".").ToString();
        }

        public string RemainingTimeFormatted
        {
            get
            {
                var sb = new StringBuilder();
                var h = RemainingTimeToJoin.Hours;
                var m = RemainingTimeToJoin.Minutes;
                var s = RemainingTimeToJoin.Seconds;

                if (0 < h) sb.Append(h).Append(":");
                return sb.Append(RemainingTimeToJoin.ToString("mm\\:ss")).ToString();
            }
        }

        public string RemainingTimeText()
        {
            var sb = new StringBuilder("iNotifier - ");
            sb.Append(EventTypeToText(EventType))
                .Append(" in ")
                .Append(RemainingTimeFormatted);

            if (0 < NumRegistered)
            {
                sb.Append(" - ")
                    .Append(NumRegistered)
                    .Append(1 == NumRegistered ? " driver" : " drivers")
                    .Append("registered");
            }

            return sb.ToString();
        }

        private string EventTypeToText(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Race: return "Race";
                case EventType.Qualifying: return "Qualifying";
                case EventType.TimeTrial: return "Time trial";
            }

            return null;
        }
    }
}
