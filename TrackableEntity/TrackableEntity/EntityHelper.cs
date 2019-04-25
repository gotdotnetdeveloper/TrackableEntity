using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrackableEntity.Interface;
using TrackableEntity.Properties;

namespace TrackableEntity
{
    /// <summary>
    /// Вспомогательные методы.
    /// </summary>
    public static class EntityHelper
    {
        #region Публичные методы
        /// <summary>
        /// Клонируем простые типы и коллекции с простыми типами, Рекурсивно идем по IList.
        /// Клонируются только Array, ValueType, string.
        /// </summary>
        public static object InnerClone(object o, [CanBeNull] EntityStateMonitor monitor)
        {
            if (o == null)
                return null;

            PropertyInfo[] piList = null;
            Type type = o.GetType();
            object newObject = null;
            if (type.IsArray)
            {
                if (o is ICloneable cloneable)
                {
                    return cloneable.Clone();
                }
            }
            else if (type.IsValueType || type == typeof(string))
            {
                return o;
            }
            else if ((typeof(IList).IsAssignableFrom(type) && !typeof(ITrackable).IsAssignableFrom(type)))
            {
                if (o is IList iList)
                {
                    var newListType = iList.GetType();

                    IList newList = null;
                    if (iList is ICloneable cloneable)
                    {
                        return cloneable.Clone();
                    }
                    else
                    {
                        newList = Activator.CreateInstance(newListType) as IList;
                    }


                    if (newList != null)
                    {
                        var tempList = new List<object>(iList.Count);
                        foreach (var item in iList)
                        {
                            tempList.Add(InnerClone(item, monitor));
                        }

                        foreach (var temp in tempList)
                            newList.Add(temp);

                        return newList;
                    }
                }

            }


            if (monitor == null && o is ITrackable trackable && trackable.GetMonitor() is EntityStateMonitor m)
                monitor = m;

            if (monitor != null)
            {
                if (monitor.PropertyInfoDictionary.ContainsKey(type))
                    piList = monitor.PropertyInfoDictionary[type];
                else
                {
                    piList = type.GetProperties();
                    monitor.PropertyInfoDictionary.Add(type, piList);
                }
            }
            else
            {
                piList = type.GetProperties();
            }

            var correctProperty = piList.Where(x =>
                x.CanRead && x.CanWrite &&
                (x.PropertyType.IsArray || x.PropertyType.IsValueType || x.PropertyType == typeof(string)
                 || (typeof(IList).IsAssignableFrom(x.PropertyType) &&
                     !typeof(ITrackable).IsAssignableFrom(x.PropertyType))));

            newObject = Activator.CreateInstance(type);



            foreach (var pi in correctProperty)
            {
                var currentProperty = pi.GetMethod.Invoke(o, null);

                pi.SetMethod.Invoke(newObject, new[] {InnerClone(currentProperty, monitor)});

            }

            return newObject;
        }
        #endregion
    }
}
