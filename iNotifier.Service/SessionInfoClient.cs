using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace SessionSpotter
{
    public class SessionResult
    {
        public int SeasonID { get; set; }
        public string SeasonName { get; set;}
        public IList<SessionInfo> Sessions { get; set;}
    }

    public class iRacingJsonKeys
    {
        public const string Meta = "m";
        public const string Root = "d.r";
        public const string SessionID = "sessionid";
        public const string NumRegistered = "numregistered";
        public const string SeasonID = "seasonid";
        public const string EventTypeID = "eventtypeid";
        public const string StartTime = "starttime";
    }

    public class SessionInfoClient
    {
        private const string IRACING_DOMAIN = "members.iracing.com";
        private const string IRACING_SESSION_TIMES_URL = "/membersite/member/GetSessionTimes";
        private const string IRACING_SERIES_SESSIONS_PAGE_URL = "/membersite/member/SeriesSessions.do";
        private const string IRACING_SERIES_PARSE_TOKEN_START = "racingpaneldata.series={";
        private const char IRACING_SERIES_PARSE_TOKEN_END = '}';
        private readonly char[] IRACING_TOKEN_SPLIT_CHARS = {':'};
        private const string IRACING_TOKEN_SeasonName = "seasonname";


        private async Task<Tuple<string, CookieContainer>> SendHttpRequestAync(string url, CookieContainer cookieContainer = null)
        {
            var baseAddress = new Uri("http://" + IRACING_DOMAIN);

            if (null == cookieContainer)
            {
                var cookieMonster = new CookieGetter();
                var cookies = cookieMonster.GetCookies(IRACING_DOMAIN);
                cookieContainer = new CookieContainer();
                cookieContainer.Add(baseAddress, cookies);
            }

            using (var clientHandler = new HttpClientHandler { CookieContainer = cookieContainer })
            using (var client = new HttpClient(clientHandler) { BaseAddress = baseAddress })
            {
                Trace.WriteLine("HTTP Request: " + url);
                var result = client.PostAsync(url, null).Result;
                result.EnsureSuccessStatusCode();
                var data = await result.Content.ReadAsStringAsync();

                Config<UserSettings>.Instance.DownloadedBytes += ASCIIEncoding.Unicode.GetByteCount(data);

                return Tuple.Create(data, cookieContainer);
            }
        }

        public void GoToSessionsPage(int seasonId)
        {
            Process.Start(IRACING_SERIES_SESSIONS_PAGE_URL + "?season=" + seasonId);
        }

        public async Task<SessionResult> FetchSessionData(WeekendInfo wi)
        {
            // Keep the name around, as we'll only update it if the season ids have changed
            var res = new SessionResult { SeasonName = wi.SeasonName };

            var jSonResult = await SendHttpRequestAync(IRACING_SESSION_TIMES_URL);
            var jSonData = jSonResult.Item1;
            var cookies = jSonResult.Item2;
            var sessions = ParseJSon(jSonData);
            if (null == sessions) return res;

            // Get the season ID from the first element
            // All elements should have the same season id*
            //  - Unless maybe the season is undergoing a transition??
            //    If that's the case, then we have to filter out the sessions for the season
            //    that is closest in time, and not include the sessions in the future season.
            //    TODO:This needs to be tested.
            res.SeasonID = sessions.FirstOrDefault().HasValue(s => s.SeasonID);
            
            // When getting a fresh list of sessions, ommit the ones that are expired. 
            // It is entirely possible for a session to be determined expired because we have
            // a custom buffer to determine when to mark a session expired.
            // This will also prevent repetitive flagging for trying to update the sessions.
            res.Sessions = sessions.Where(s => !s.IsExpired).ToList();

            // To resolve the seaon name we need to scrape some HTML.
            // This is a big chunk of data, so we're only going to this once when the season id changes
            if (res.SeasonID != wi.SeasonID)
            {
                //Reuse the cookies to avoid looking them up again
                var htmlResult = await SendHttpRequestAync(IRACING_SERIES_SESSIONS_PAGE_URL, cookies);
                var htmlData = htmlResult.Item1;
                var seriesTable = ParseSeriesData(htmlData);
                res.SeasonName = seriesTable[IRACING_TOKEN_SeasonName];
            }

            return res;
        }

        /// <summary>
        /// Sample message:
        ///  {
        ///    "m": {
        ///    
        ///        //These change in updates, so build a lookup dictionary for them
        ///        //i.e. 1 isn't necessarily guaranteed to be the session id
        ///        
        ///        "1": "sessionid",
        ///        "2": "maxtodisplay",
        ///        "3": "raceweek",
        ///        "4": "trackid",
        ///        "5": "reloadtime",
        ///        "6": "numregistered",
        ///        "7": "seasonid",
        ///        "8": "eventtypeid",
        ///        "9": "starttime"
        ///    },
        ///    "d": {
        ///        "5": 227,
        ///        "r": [
        ///            {
        ///                "1": 37642554,
        ///                "2": 5,
        ///                "3": 2,
        ///                "4": 191,
        ///                "6": 8,
        ///                "7": 968,
        ///                "8": 3,
        ///                "9": 1376425800000
        ///            }
        ///        ]
        ///    }
        /// }
        /// </summary>
        /// <param name="jSon"></param>
        /// <returns></returns>
        private IEnumerable<SessionInfo> ParseJSon(string jSon)
        {
            //This will be an 
            JObject jObj = this.TryCatch(() => JObject.Parse(jSon));
            if (null == jObj) return null;

            //Get the definition for the keys from the meta paragraph up top
            //i.e. "1": "maxtodisplay"
            //     "2": "weatherwindspeedunits"
            var keys = jObj.SelectToken(iRacingJsonKeys.Meta);
            if (null == keys) return null;

            var lookup = keys
                .OfType<JProperty>()
                .ToDictionary(p => p.Value.ToString(), p => p.Name.ToLowerInvariant());

            var tokens = jObj.SelectToken(iRacingJsonKeys.Root);
            if (null == tokens) return null;

            return tokens.Select(t =>
            {
                var sri = new SessionInfo();
                sri.NumRegistered = t[lookup[iRacingJsonKeys.NumRegistered]].ToObject<int>();
                sri.SessionID = t[lookup[iRacingJsonKeys.SessionID]].ToObject<long>();
                sri.SeasonID = t[lookup[iRacingJsonKeys.SeasonID]].ToObject<int>();
                sri.EventType = t[lookup[iRacingJsonKeys.EventTypeID]].ToObject<EventType>();
                sri.StartTimeGMT = t[lookup[iRacingJsonKeys.StartTime]].ToObject<double>().ToUnixTime();
                sri.StartTimeLocal = sri.StartTimeGMT.ToLocalTime();
                return sri;
            });
        }

        /// <summary>
        /// This will be somewhere 3/4 of the way down in the html string
        /// racingpaneldata.numcars=2;
        /// racingpaneldata.series={
        ///    raceweek:2,
        ///    isFixedSetup:false,
        ///    seriesname:"inRacingNews Challenge MC",
        ///    seriesname_short:"inRacingNews Challenge MC",
        ///    seasonname:"inRacingNews Challenge - 2013 Season 3",
        ///    seasonname_short:"2013 Season 3",
        ///    seasonyear:2013,
        ///    seriesid:112,
        ///    seasonid:938,
        ///    multiclass:true,
        ///    night:false,
        ///    seriesimg:"http://membersmedia.iracing.com/member_images/series/inracing/logo.jpg",
        ///    changelink:"/membersite/member/Series.do",
        ///    resultslink:"/membersite/member/SeriesResults.do?season=938",
        ///    sessionslink:"/membersite/member/SeriesSessions.do?season=938"
        /// };
        /// </summary>
        /// <param name="htmlData"></param>
        /// <returns></returns>
        private Dictionary<string, string> ParseSeriesData(string htmlData)
        {
            var idx = htmlData.IndexOf(IRACING_SERIES_PARSE_TOKEN_START);
            if (idx < 0) return null;

            // Find the idexes of the braces in "racingpaneldata.series={ ... }" 
            var startIdx = idx + IRACING_SERIES_PARSE_TOKEN_START.Length;
            var endIdx = htmlData.IndexOf(IRACING_SERIES_PARSE_TOKEN_END, startIdx);

            // Get the contents between the braces
            var chunk = htmlData.Substring(startIdx, endIdx - startIdx);

            // Tokenize the contents and build a dictionary from them
            var pairs = chunk.Split(',');           
            var table = new Dictionary<string, string>();
            foreach (var p in pairs)
            {
                var kv = p.Split(IRACING_TOKEN_SPLIT_CHARS, 2);

                //The keys and values need to be cleaned up
                var key = kv[0].Trim();
                var val = kv[1].Trim().Trim('\"');
                table[key] = val; 
            }

            return table;
        }
    }
}
