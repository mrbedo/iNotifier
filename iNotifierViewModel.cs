using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

using iRacingSdkWrapper;
using iRSDKSharp;
using System.Windows.Threading;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Practices.Prism.Commands;
using System.Reflection;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using System.Threading;

namespace SessionSpotter
{
    public class iNotifierViewModel : NotificationObject, IDisposable
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private SessionInfoClient _sessionClient = new SessionInfoClient();
        private DispatcherTimer _timer = new DispatcherTimer();
        private SdkWrapper _iRacing;
        private SpeechSynthesizer _synth = new SpeechSynthesizer();
        private BackgroundWorker _bgTextWorker = new BackgroundWorker();

        public TimeSpan SessionNormalFetchTimeInterval { get; set; }
        public TimeSpan SessionHighFetchTimeInterval { get; set; }

        public SessionsCollection Sessions { get; set; }

        public UserSettings Settings { get { return Config<UserSettings>.Instance; } }

        public ICommand RestoreDefaultsCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NavigateToSeasonSchedule { get; set; }
        
        public string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        private WeekendInfo _weekendInfo;
        public WeekendInfo WeekendInfo
        {
            get { return _weekendInfo; }
            set
            {
                _weekendInfo = value;
                RaisePropertyChanged();
            }
        }

        private string _output;
        public string Output
        {
            get { return _output; }
            set
            {
                _output = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? _currentTime;
        public DateTime? CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? _lastUpdatedTime;
        public DateTime? LastUpdatedTime
        {
            get { return _lastUpdatedTime; }
            set
            {
                _lastUpdatedTime = value;
                RaisePropertyChanged();
            }
        }

        public bool _isSettingsVisible;
        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set
            {
                _isSettingsVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        private bool _busyUpdatingSessions;
        public bool BusyUpdatingSessions
        {
            get { return _busyUpdatingSessions; }
            set
            {
                _busyUpdatingSessions = value;
                RaisePropertyChanged();
            }
        }

        public iNotifierViewModel()
        {
            Sessions = new SessionsCollection();
            WeekendInfo = new SessionSpotter.WeekendInfo();
            RestoreDefaultsCommand = new DelegateCommand(() =>
            {
                Config<UserSettings>.Instance.SetDefaults();
                RaisePropertyChanged(); //This needs to be called for some reason
            });
            RefreshCommand = new DelegateCommand(UpdateSessions);
            NavigateToSeasonSchedule = new DelegateCommand(() =>
            {
                Process.Start("http://members.iracing.com/membersite/member/SelectSeries.do?&season=" + WeekendInfo.SeasonID);
            });
        }

        public void Init()
        {
            UpdateSessions();

            _synth.SetOutputToDefaultAudioDevice();

            _timer.Tick += OnTimerElapsed;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();

            _bgTextWorker.DoWork += TextWorkerDoWork;

            _iRacing = new SdkWrapper();
            _iRacing.Connected += (o, e) => IsConnected = true;
            _iRacing.SessionInfoUpdated += OnSessionInfoUpdated;
            _iRacing.Disconnected += (o, e) => IsConnected = false;
            _iRacing.Start();

            Log.Instance.Trace("iNotifier started.");
        }

        private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            //WeekendInfo = ParseWeekendInfo(e.SessionInfo);
            UpdateSessions();
        }

        //private WeekendInfo ParseWeekendInfo(string iRacingYaml)
        //{
        //    return new WeekendInfo
        //    {
        //        SeriesID = int.Parse(YamlParser.Parse(iRacingYaml, "WeekendInfo:SeriesID:")),
        //        SeasonID = int.Parse(YamlParser.Parse(iRacingYaml, "WeekendInfo:SeasonID:"))
        //    };
        //}

        void OnTimerElapsed(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now;

            if (!Sessions.Valid) return;

            var someVisibleSessionsAreExpired = false;
            foreach (var s in Sessions.All)
            {
                s.Refresh();
                if (s.IsVisible && s.IsExpired)
                {
                    someVisibleSessionsAreExpired = true;
                }
            }
            
            Alert(Sessions);

            if (someVisibleSessionsAreExpired)
            {
                UpdateSessions();
            }
        }

        private void Alert(SessionsCollection sessions)
        {
            if (Config<UserSettings>.Instance.ShowRace)
            {
                foreach (var r in sessions.RaceSessions.Where(s => s.IsLowAlert))
                {
                    if (Notify(r)) return;
                }
            }

            if (Config<UserSettings>.Instance.ShowQual)
            {
                foreach (var q in sessions.QualSessions.Where(s => s.IsLowAlert))
                {
                    if (Notify(q)) return;
                }
            }

            if (Config<UserSettings>.Instance.ShowTT)
            {
                foreach (var t in sessions.TTSessions.Where(s => s.IsLowAlert))
                {
                    if (Notify(t)) return;
                }
            }
        }

        private bool Notify(SessionInfo session)
        {
            bool notify = false;

            if (session.IsLowAlert && !session.NotifiedOnLowAlert)
            {
                session.NotifiedOnLowAlert = true;
                notify = true;
            }
            else
            if (session.IsHighAlert && !session.NotifiedOnHighAlert)
            {
                session.NotifiedOnHighAlert = true;
                notify = true;
            }
            else
            if (session.IsFinalAlert && !session.NotifiedOnFinalAlert)
            {
                session.NotifiedOnFinalAlert = true;
                notify = true;
            }

            if (notify)
            {
                var s = Config<UserSettings>.Instance;
                if (s.IsSpeechAlertingEnabled)
                {
                    Speak(session.RemainingTimeSpeech());
                }

                if (s.IsTextAlertingEnabled && IsConnected && IsIRacingWindowForeground())
                {
                    Text(session.RemainingTimeText());
                }
            }

            return notify;
        }

        private bool IsIRacingWindowForeground()
        {
            var activeWindowTitle = GetActiveWindowTitle();
            return activeWindowTitle == "iRacing.com Simulator";
        }

        private void Speak(string text)
        {
            _synth.SpeakAsync(text);
        }

        private ConcurrentQueue<string> _textQueue = new ConcurrentQueue<string>();

        private void Text(string text)
        {
            _textQueue.Enqueue(text);
            if (!_bgTextWorker.IsBusy)
            {
                _bgTextWorker.RunWorkerAsync();
            }
        }

        void TextWorkerDoWork(object sender, DoWorkEventArgs e)
        {

            string text;
            while (_textQueue.TryDequeue(out text))
            {
                try
                {
                    //SendKeys.SendWait("{ESC}");
                    //SendKeys.SendWait("{ESC}");
                    _iRacing.BroadcastMsg(BroadcastMessageTypes.ChatComand, (int)ChatCommandMode.Cancel);
                    _iRacing.BroadcastMsg(BroadcastMessageTypes.ChatComand, (int)ChatCommandMode.BeginChat);
                    Thread.Sleep(100);
                    SendKeys.SendWait("#");
                    Thread.Sleep(100);
                    foreach (var c in text)
                    {
                        SendKeys.SendWait(c.ToString());
                    }
                    Thread.Sleep(500);
                    SendKeys.SendWait("{ENTER}");
                }
                catch (Exception ex)
                {
                    Log.Instance.Error(ex);
                }
            }
        }

        
        private void UpdateSessions()
        {
            if (BusyUpdatingSessions)
            {
                return;
            }

            BusyUpdatingSessions = true;
            _sessionClient
                .FetchSessionData(WeekendInfo)
                .ContinueWith(t =>
                {
                    if (null == t.Result) return;

                    Sessions.All = GetMergedSessions(Sessions.All, t.Result.Sessions);
                    WeekendInfo.SeasonName = t.Result.SeasonName;
                    WeekendInfo.SeasonID = t.Result.SeasonID;
                    BusyUpdatingSessions = false;
                    LastUpdatedTime = DateTime.Now;
                });
        }

        private IEnumerable<SessionInfo> GetMergedSessions(IEnumerable<SessionInfo> original, IEnumerable<SessionInfo> update)
        {
            if (null == original) return update;

            var preserved = original.Where(s => !s.IsExpired);
            var newItems = update.Except(preserved, SessionInfo.StartTimeUTCComparer);

            return preserved.Concat(newItems).ToList();
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            return GetWindowText(handle, Buff, nChars) > 0 
                ? Buff.ToString() 
                : null;
        }

        public void Dispose()
        {
            _iRacing.Stop();
            _timer.Stop();
            _synth.Dispose();
        }
    }
}
