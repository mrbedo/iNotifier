using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SessionSpotter
{
    public class UserSettings : Settings, INotifyPropertyChanged
    {
        private bool _showRace;
        private bool _showQual;
        private bool _showTT;
        private long _finalReminderSeconds;
        private long _rLowAlertSeconds;
        private long _rHighAlertSeconds;
        private long _qLowAlertSeconds;
        private long _qHighAlertSeconds;
        private long _tLowAlertSeconds;
        private long _tHighAlertSeconds;
        private bool _isSpeechAlertingEnabled;
        private bool _isTextAlertingEnabled;
        private bool _isWindowTopMost;
        private bool _isDebugPaneVisible;

        public Rect? WindowRect { get; set; }

        private long _downloadedBytes;

        [XmlIgnore]
        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set
            {
                _downloadedBytes = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowRace
        {
            get { return _showRace; }
            set
            {
                _showRace = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowQual
        {
            get { return _showQual; }
            set
            {
                _showQual = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowTT
        {
            get { return _showTT; }
            set
            {
                _showTT = value;
                RaisePropertyChanged();
            }
        }

        public long FinalReminderSeconds
        {
            get { return _finalReminderSeconds; }
            set
            {
                _finalReminderSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long RaceLowAlertSeconds
        {
            get { return _rLowAlertSeconds; }
            set
            {
                _rLowAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long RaceHighAlertSeconds
        {
            get { return _rHighAlertSeconds; }
            set
            {
                _rHighAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long QualLowAlertSeconds
        {
            get { return _qLowAlertSeconds; }
            set
            {
                _qLowAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long QualHighAlertSeconds
        {
            get { return _qHighAlertSeconds; }
            set
            {
                _qHighAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long TTLowAlertSeconds
        {
            get { return _tLowAlertSeconds; }
            set
            {
                _tLowAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public long TTHighAlertSeconds
        {
            get { return _tHighAlertSeconds; }
            set
            {
                _tHighAlertSeconds = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSpeechAlertingEnabled
        {
            get { return _isSpeechAlertingEnabled; }
            set
            {
                _isSpeechAlertingEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IsTextAlertingEnabled
        {
            get { return _isTextAlertingEnabled; }
            set
            {
                _isTextAlertingEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IsWindowTopmost
        {
            get { return _isWindowTopMost; }
            set
            {
                _isWindowTopMost = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDebugPaneVisible
        {
            get { return _isDebugPaneVisible; }
            set
            {
                _isDebugPaneVisible = value;
                RaisePropertyChanged();
            }
        }

        public UserSettings()
        {
            Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "iNotifier", "userSettings.xml");
            SetDefaults();
        }

        public void SetDefaults()
        {
            _suppressNotification = true;

            ShowTT = false;
            ShowQual = true;
            ShowRace = true;

            FinalReminderSeconds = 10;
            RaceLowAlertSeconds = 300;
            RaceHighAlertSeconds = 60;

            QualLowAlertSeconds = 60;
            QualHighAlertSeconds = 30;

            TTLowAlertSeconds = 30;
            TTHighAlertSeconds = 0;

            IsWindowTopmost = true;
            IsDebugPaneVisible = false;

            IsTextAlertingEnabled = true;
            IsSpeechAlertingEnabled = true;

            _suppressNotification = false;

            RaisePropertyChangedAll();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedAll()
        {
            if (_suppressNotification) return;
            PropertyChanged.RaisePropertyChanged(this, String.Empty);
        }

        private bool _suppressNotification;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_suppressNotification) return;
            PropertyChanged.RaisePropertyChanged(this, propertyName);
        }
    }
}
