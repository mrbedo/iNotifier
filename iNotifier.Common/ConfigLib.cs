using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;


namespace SessionSpotter
{
    [Serializable]
    public abstract class Settings
    {
        [XmlIgnore]
        public string Location { get; set; }
    }

    public static class Config<T>
        where T : Settings, new()
    {
        private static T _instance;
        private static object _locker = new object();

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_locker)
                    {
                        if (null == _instance)
                        {
                            _instance = new T();
                        }
                    }
                }

                return _instance;
            }
        }

        public static bool Load(string location = null)
        {
            try
            {
                if (null == location)
                {
                    location = Instance.Location;
                }

                if (!File.Exists(location))
                    throw new ArgumentException(location);

                var sx = new XmlSerializer(typeof(T));

                using (var reader = new StreamReader(location))
                {
                    _instance = sx.Deserialize(reader) as T;
                }

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }

            return false;
        }

        public static bool Save(string location = null)
        {
            try
            {
                if (null == location)
                {
                    location = Instance.Location;
                }

                if (String.IsNullOrEmpty(location))
                    throw new ArgumentException(location);

                var path = Path.GetDirectoryName(Path.GetFullPath(location));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var sx = new XmlSerializer(typeof(T));

                using (var writer = new StreamWriter(location))
                {
                    sx.Serialize(writer, Instance);
                }

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
