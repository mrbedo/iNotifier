using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SessionSpotter
{
    public class SessionItemTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _defaultTemplate = new DataTemplate();
        public DataTemplate RaceItemTemplate { get; set; }
        public DataTemplate QualItemTemplate { get; set; }
        public DataTemplate TTItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var si = item as SessionInfo;
            if (null == si) return _defaultTemplate;

            switch (si.EventType)
            {
                case EventType.Race: return RaceItemTemplate;
                case EventType.Qualifying: return QualItemTemplate;
                case EventType.TimeTrial: return TTItemTemplate;
                default: return _defaultTemplate;
            }
        }
    }

    public class DictionaryConverter : IValueConverter
    {
        public Dictionary<object, object> Values { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (null != value && Values.ContainsKey(value)) ? Values[value] : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class EventTypeAcronymConverter : DictionaryConverter
    {
        public EventTypeAcronymConverter()
        {
            Values = new Dictionary<object, object>
            { 
                { EventType.Race, "R" }, 
                { EventType.Qualifying, "Q" }, 
                { EventType.TimeTrial, "T" } 
            };
        }
    }

    public class RegisteredUsersConverter : IValueConverter
    {
        public Dictionary<object, object> Values { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (0 == (int)value) ? null : String.Format("({0})", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public Dictionary<object, object> Values { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var seconds = value as long?;
            return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : (TimeSpan?)null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
