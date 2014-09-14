using SessionSpotter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace System
{
    public static class Extensions
    {
        public static void Raise<T>(this Action<T> del, T arg)
        {
            var d = del;
            if (null != d) d(arg);
        }

        public static R Raise<T, R>(this Func<T, R> del, T arg, R defaultValue = default(R))
        {
            var d = del;
            return (null != d) ? d(arg) : defaultValue;
        }

        public static bool HasValue<T>(this T obj, Func<T, bool> whenTrue = null, Func<T, bool> whenFalse = null) where T : class
        {
            return null != obj ? whenTrue.Raise(obj, true) : whenFalse.Raise(obj, false);
        }

        public static R HasValue<T, R>(this T obj, Func<T, R> whenTrue = null, Func<T, R> whenFalse = null) where T : class
        {
            return null != obj ? whenTrue.Raise(obj) : whenFalse.Raise(obj);
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="obj">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static bool CheckParent(this DependencyObject obj, Func<DependencyObject, bool> match)
        {
            var parent = (obj is Visual) 
                ? VisualTreeHelper.GetParent(obj) 
                : LogicalTreeHelper.GetParent(obj);

            return (null != parent) && (match(parent) || CheckParent(parent, match));
        }

        public static void RaisePropertyChanged(this PropertyChangedEventHandler propertyChangedEvent, object sender, [CallerMemberName] string propertyName = null)
        {
            var h = propertyChangedEvent;
            if (null != h)
            {
                h(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static R TryCatch<R>(this object obj, Func<R> func, R defaultValue = default(R))
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Log.Instance.Error(String.Format("{0} object of type {1} trew an exception: {2}", obj, obj.HasValue(o => o.GetType()), e));
                return defaultValue;
            }
        }

        public static DateTime ToUnixTime(this double seconds)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(seconds);
        }
    }
}
