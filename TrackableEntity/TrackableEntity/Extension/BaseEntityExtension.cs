using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrackableEntity.Interface;
using TrackableEntity.Properties;

namespace TrackableEntity.Extension
{
    /// <summary>
    /// Рсширение для клонирования объекта
    /// </summary>
    public static class BaseEntityExtension
    {
        #region Публичные методы
        /// <summary>
        /// Всем свойствам с коллекциям EntityCollection или ValueCollection создать пустые экземпляры.
        /// Для ValueCollection = прокинуть ссылки на BaseEntity.
        /// </summary>
        /// <param name="entity"></param>
        public static void AllCollectionCreateInstanse(this BaseEntity entity)
        {
            var properties = entity.GetType().GetProperties().Where(x =>
                x.CanRead && x.CanWrite &&
                (x.PropertyType.Name == EntityStateMonitor.EntityCollectionPropertyTypeName ||
                 x.PropertyType.Name == EntityStateMonitor.ValueCollectionPropertyTypeName)).ToList();

            var piEntityCollection = properties.Where(x =>
                x.PropertyType.Name == EntityStateMonitor.EntityCollectionPropertyTypeName);
            var piValueCollection = properties.Where(x =>
                x.PropertyType.Name == EntityStateMonitor.ValueCollectionPropertyTypeName);
            foreach (var pi in piValueCollection)
            {
                var constructorInfo = pi.PropertyType.GetConstructor(new[] { typeof(BaseEntity), typeof(string) });
                if (constructorInfo != null)
                {
                    object[] lobject = { entity, pi.Name };
                    var newInstanse = constructorInfo.Invoke(lobject);
                    pi.SetMethod.Invoke(entity, new[] { newInstanse });
                }
            }

            foreach (var pi in piEntityCollection)
            {
                pi.SetMethod.Invoke(entity, new[] { Activator.CreateInstance(pi.PropertyType) });
            }
        }

        /// <summary>
        /// Глубокое клонирование. Обход дерева и построение такой же структуры.
        /// </summary>
        /// <param name="fromClone">Источник клонирования</param>
        /// <returns>Новый экземпляр</returns>
        public static ITrackable DeepClone(this ITrackable fromClone)
        {
            ITrackable toClone = null;
            if (fromClone != null)
            {
                if (fromClone.GetMonitor() is EntityStateMonitor monitor)
                {
                    InnerClone(fromClone, out toClone, monitor.PropertyInfoDictionary);
                }
                else
                {
                    InnerClone(fromClone, out toClone, null);
                }

            }

            return toClone;
        }
        #endregion
        #region Приватные функции
        private static void InnerClone(ITrackable fromClone, out ITrackable toClone,
            Dictionary<Type, PropertyInfo[]> propertyInfoDictionary)
        {
            // Предпологаем, что ITrackable fromClone может быть либо сущьностью, либо коллекцией сущьностей
            if (fromClone is BaseEntity from) // сущьность
            {
                var type = from.GetType();
                PropertyInfo[] piList;

                #region Заполнение piList. Получение информации о свойствах PropertyInfo[]
                if (propertyInfoDictionary != null)
                {
                    if (propertyInfoDictionary.ContainsKey(type))
                        piList = propertyInfoDictionary[type].Where(x => x.CanRead && x.CanWrite).ToArray();
                    else
                    {
                        piList = from.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
                        propertyInfoDictionary.Add(type, piList);
                    }
                }
                else
                {
                    piList = from.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
                }
                #endregion


                toClone = fromClone.Clone() as ITrackable;

                var navigationProrertyList = piList.Where(x => typeof(BaseEntity).IsAssignableFrom(x.PropertyType));
                var entityCollectionPi = piList.Where(x =>
                    x.PropertyType.Name == EntityStateMonitor.EntityCollectionPropertyTypeName);

                foreach (var pi in navigationProrertyList)
                {
                    if (pi.GetValue(from) is BaseEntity newValue)
                    {
                        InnerClone(newValue, out ITrackable toCloneBaseEntity, propertyInfoDictionary);
                        pi.SetValue(toClone, toCloneBaseEntity);
                    }
                }

                foreach (var pi in entityCollectionPi)
                {
                    if (pi.GetValue(from) is ITrackable newEntityCollectionValue)
                    {
                        InnerClone(newEntityCollectionValue, out ITrackable toCloneBaseEntity, propertyInfoDictionary);
                        pi.SetValue(toClone, toCloneBaseEntity);
                    }
                }
            }
            else if (fromClone is IEnumerable<BaseEntity> fromCloneEntityCollection) // коллекция сущьностей
            {
                toClone = Activator.CreateInstance(fromCloneEntityCollection.GetType()) as ITrackable;

                var list = new List<BaseEntity>();
                foreach (var fromEntity in fromCloneEntityCollection)
                {
                    InnerClone(fromEntity, out ITrackable toCloneBaseEntity, propertyInfoDictionary);
                    if (toCloneBaseEntity is BaseEntity be)
                        list.Add(be);
                }

                foreach (var x in list)
                    ((IEntityCollection)toClone)?.AddBaseEntity(x);

            }
            else
            {
                throw new TrakerException("Не корректные данные трекера при клонировании объекта.");
            }
        }
        #endregion

        /// <summary>
        /// Получить из делева плоский список ВСЕХ дочерние сущьностей, относительно текущего узла 
        /// </summary>
        /// <param name="rootEntity">Элемент, от которого поиск</param>
        /// <param name="predicat">Фильтр полученных элементов </param>
        public static IEnumerable<BaseEntity> GetFromCurrent([NotNull] this ITrackable rootEntity , Func<BaseEntity, bool> predicat = null)
        {
            var flatList = new Dictionary<BaseEntity, BaseEntity>(ReferenceEqualityComparer.Instance);

        var monitor = rootEntity.GetMonitor() as EntityStateMonitor;
            if (monitor == null)
                return new List<BaseEntity>();


            monitor.TraverseGraph(rootEntity, x =>
            {
                if (x is BaseEntity baseEntity) // это сущьность
                {
                    if (!flatList.ContainsKey(baseEntity))
                    {
                        if (predicat != null)
                        {
                            if (predicat.Invoke(baseEntity))
                            {
                                flatList.Add(baseEntity, baseEntity);
                            }
                        }
                        else
                        {
                            flatList.Add(baseEntity, baseEntity);
                        }
                    }
                }
                return true; //обходим дерево дальше
            });

            return flatList.Keys;
        }

      

        /// <summary>
        /// Список сущьностей измененных, относительно текущего узла.
        /// </summary>
        public static IEnumerable<BaseEntity> GetChangedFromCurrent([NotNull] this ITrackable rootEntity)
        {
            return rootEntity.GetFromCurrent().Where(x => x.State == EntityState.Modified);
        }

        /// <summary>
        /// Список сущьностей новых, относительно текущего узла.
        /// </summary>
        public static IEnumerable<BaseEntity> GetAddedFromCurrent([NotNull] this ITrackable rootEntity)
        {
            return rootEntity.GetFromCurrent().Where(x=>x.State == EntityState.New);
        }

        /// <summary>
        /// Список сущьностей удаленных, относительно текущего узла.
        /// </summary>
        public static IEnumerable<BaseEntity> GetDeletedFromCurrent([NotNull] this ITrackable rootEntity)
        {
            return rootEntity.GetFromCurrent().Where(x => x.State == EntityState.Deleted);
        }



        /// <summary>
        /// Очистить все дочернии коллекции, начиная от текущего. (При очищении, помечаются в трекере как удаленные)
        /// </summary>
        /// <param name="rootEntity">Элемент, от которого поиск</param>
        public static void ClearAllCollectionFromCurrent([NotNull] this ITrackable rootEntity)
        {
            var flatList = new Dictionary<IList, IList>(ReferenceEqualityComparer.Instance);

            var monitor = rootEntity.GetMonitor() as EntityStateMonitor;
            if (monitor == null)
                return ;

            monitor.TraverseGraph(rootEntity, x =>
            {

                if( x is IList list && !flatList.ContainsKey(list))
                    flatList.Add(list, list);

                return true; //обходим дерево дальше
            });

            foreach (var list in flatList.Keys)
            {
                list.Clear();
            }
        }


    }
}
