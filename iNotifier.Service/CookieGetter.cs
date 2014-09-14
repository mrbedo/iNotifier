using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SessionSpotter
{
    public class CookieGetter
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookie(string lpszUrl, string lpszCookieName, StringBuilder lpszCookieData, ref int lpdwSize);

        // Declare the Ex versions of Set and Get: 
        //(lpReserved is always null, it is reserved for future use) 
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            Int32 dwFlags,
            IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;
        private const Int32 INTERNET_COOKIE_THIRD_PARTY = 0x10; 

        private readonly string[] IE_COOKIE_SEPARATOR = new string[] { "\n*\n" };

        public CookieCollection GetCookies(string domain)
        {
            var cookieJar = new CookieCollection();
            var cookies = new Dictionary<string, string>();
            GetCookieChrome(domain, ref cookieJar);
            //if (!)
            //{
            //GetCookieFromIE(domain, ref cookieJar);
            //}            

            return cookieJar;
        }

        private IList<string> GetIECookiePaths()
        {
            var paths = new List<string>();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            if (Directory.Exists(path))
            {
                paths.Add(path);
            }

            path = Path.Combine(path, "low");
            if (Directory.Exists(path))
            {
                paths.Add(path);
            }

            //path = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            //if (Directory.Exists(path))
            //{
            //    paths.

            return paths;
        }

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri uri_)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            var uri = new Uri("http://members.iracing.com/");
            Int32 cookieType = 0x000;
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, cookieType, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, cookieType, IntPtr.Zero))
                    return null;
            }

            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        /// <summary>
        /// Get the Value from the Internet Explorer cookie file.
        /// 
        /// IE Cookie file format (separated by \n)
        /// --------------------------------
        /// Cookie name
        /// Cookie value
        /// Host/path for the web server setting the cookie
        /// Flags
        /// Exirpation time (low)
        /// Expiration time (high)
        /// Creation time (low)
        /// Creation time (high)
        /// Record delimiter (*)
        /// </summary>
        private bool GetCookieFromIE(string domainPattern, ref CookieCollection cookies)
        {
            try
            {
                var cookiess = GetUriCookieContainer(new Uri("http://iracing.com"));
                //NAME  user 
                //DOMAIN  members.iracing.com 
                //PATH  / 
                //EXPIRES  8/18/2013 12:08:15 PM 


                //NAME  irchat_members 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  ROUTE 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  irchatwin 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  irsso_members 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  ROUTESSL 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  AWSELB 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 


                //NAME  JSESSIONID 
                //DOMAIN  iracing.com 
                //PATH  / 
                //EXPIRES  At the end of the Session 



                //string url = @"http://iracing.com";
                //string cookieName = @"ROUTE"; 
                //string cookieBuilder = String.Empty; 
                //int datasize;



                //var sCookieData = new StringBuilder(255);
                //System.UInt32 iCookieData = new UInt16();



                //InternetGetCookieEx(url, cookieName, cookieBuilder, ref iCookieData, INTERNET_COOKIE_THIRD_PARTY, null);



                //var cookie = cookieBuilder;


                //// find out how big a buffer is needed
                //int size = 0;
                //InternetGetCookieEx("http://iracing.com", null, null, ref size);

                //// create buffer of correct size
                //StringBuilder lpszCookieData = new StringBuilder(size);
                //InternetGetCookieEx("http://iracing.com", null, lpszCookieData, ref size);

                //// get cookie
                //string cookie = lpszCookieData.ToString();

                //var cookiePaths = GetIECookiePaths();
                //if (0 == cookiePaths.Count) return false;

                ////TODO: Make code asynchronous and parallel
                //foreach (var cookiePath in cookiePaths)
                //{
                //    var textFiles = Directory.GetFiles(cookiePath, "*.txt");
                //    foreach (string filePath in textFiles)
                //    {
                //        var sr = File.OpenText(filePath);
                //        var strCookie = sr.ReadToEnd();
                //        sr.Close();

                //        var innterStrCookies = strCookie.Split(IE_COOKIE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                //        foreach (var innerCookie in innterStrCookies)
                //        {
                //            Trace.WriteLine(innerCookie);
                //            var fields = strCookie.Split('\n');
                //            if (fields[2].Contains(domainPattern))
                //            {
                //                cookies.Add(new Cookie(fields[0], fields[1]));
                //            }
                //        }
                //    }
                //}

                return true;
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }

            return false;
        }

        /// <summary>
        /// Get the path to the Chrome cookie file.
        /// </summary>
        /// <returns></returns>
        private string GetChromeCookiePath()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default");
            return Directory.Exists(path) ? path : null;
        }

        /// <summary>
        /// Get the Value from the Chrome cookie file.
        /// SQLLite file.
        /// </summary>
        private bool GetCookieChrome(string domain, ref CookieCollection cookies)
        {
            // Check to see if Chrome Installed
            var path = GetChromeCookiePath();
            if (null == path) return false; // Nope, perhaps another browser

            try
            {
                var cookieFile = Path.Combine(path, "cookies");
                var dbConnectionString = "Data Source=" + cookieFile + ";pooling=false";

                using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = String.Format("SELECT name, value FROM cookies WHERE host_key LIKE '{0}';", domain);
                            //AND name LIKE '%" + field + "%'
                            conn.Open();

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    cookies.Add(new Cookie(reader.GetString(0), reader.GetString(1)));
                                }

                            }
                        }
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }

            return false;
        }


        /// <summary>
        ///  brief Get the path to the Fire Fox cookie file.
        ///  return string The path if successful, otherwise an empty string
        /// </summary>
        private static string GetFireFoxCookiePath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Mozilla\Firefox\Profiles\");

            try
            {
                var di = new DirectoryInfo(path);
                var dir = di.GetDirectories("*.default");
                if (1 > dir.Length) return null;

                path = Path.Combine(dir[0].Name, "cookies.sqlite");
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }

            return Directory.Exists(path) ? path : null;
        }

        ///
        ///  @brief Get the Value from the Fire Fox cookie file.
        /// 
        ///  @param strHost The host or website name.
        ///  @param strField The cookie field name.
        ///  @param Value a string to recieve the Field Value if any found.
        /// 
        ///  @return bool true if successful
        /// 
        private static bool GetCookieFireFox(string strHost, string strField, ref string Value)
        {
            Value = string.Empty;
            bool fRtn = false;
            string strPath, strTemp, strDb;
            strTemp = string.Empty;

            // Check to see if FireFox Installed
            strPath = GetFireFoxCookiePath();
            if (null == strPath) // Nope, perhaps another browser
                return false;

            try
            {
                // First copy the cookie jar so that we can read the cookies from unlocked copy while
                // FireFox is running
                strTemp = strPath + ".temp";
                strDb = "Data Source=" + strTemp + ";pooling=false";

                File.Copy(strPath, strTemp, true);

                // Now open the temporary cookie jar and extract Value from the cookie if
                // we find it.
                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT value FROM moz_cookies WHERE host LIKE '%" +
                            strHost + "%' AND name LIKE '%" + strField + "%';";

                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Value = reader.GetString(0);
                                if (!Value.Equals(string.Empty))
                                {
                                    fRtn = true;
                                    break;
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
                Value = string.Empty;
                fRtn = false;
            }

            // All done clean up
            if (string.Empty != strTemp)
            {
                File.Delete(strTemp);
            }
            return fRtn;
        }
    }
}
