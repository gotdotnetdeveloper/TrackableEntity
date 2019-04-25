using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TrackableEntity.Interface;
using TrackableEntity.Properties;

namespace TrackableEntity
{
    /// <summary>
    /// Мониторинг изменения сущьностей.
    /// Главная функция - отслеживание состояний у BaseEntity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// аналог Monitor в EntityFrameworkCore
    /// </summary>
    [Serializable]
    public class EntityStateMonitor : IEntityStateMonitor, IDisposable
    {
        /// <summary>
        /// Строка, по которой идентифицируется свойство EntityCollection BaseEntity
        /// </summary>
        public const string EntityCollectionPropertyTypeName = "EntityCollection`1";
        /// <summary>
        /// Строка, по которой идентифицируется свойство ValueCollection BaseEntity
        /// </summary>
        public const string ValueCollectionPropertyTypeName = "ValueCollection`1";

        /// <summary>
        /// Событие изменение свойств.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие изменения Entity. Постфактум.
        /// </summary>
        public event EntityChangedEventHandler EntityChangedEvent;

        #region Приватные поля
        /// <summary>
        /// Есть ли изменения.
        /// </summary>
        private bool _isChanged;
        #endregion
        #region Публичные поля
        /// <summary>
        /// Хранилище отслеживаемых сущьностей.
        /// </summary>
        public readonly Dictionary<BaseEntity, EntityInfo> EntitySet = new Dictionary<BaseEntity, EntityInfo>(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Закешированный справочник типов. (Чтоб рефлексии вызывалось меньше)
        /// </summary>
        public readonly Dictionary<Type, PropertyInfo[]> PropertyInfoDictionary = new Dictionary<Type, PropertyInfo[]>();
        #endregion
        #region Защищенные поля
        /// <summary>
        /// Признак вызова Dispose().
        /// </summary>
        protected bool _disposed;
        #endregion
        #region Публичные свойства
        /// <summary>
        /// Есть ли изменения
        /// </summary>
        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                OnPropertyChanged();
            }
        }

        #endregion
        #region Публичные методы
        /// <summary>
        /// Евляются ли 2 сущьности равны по значениям.
        /// </summary>
        /// <param name="obj1">Entity 1</param>
        /// <param name="obj2">Entity 2</param>
        /// <returns></returns>
        public bool EntityEquals(BaseEntity obj1, BaseEntity obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null || obj2 == null)
                return false;

            //тут точно оба не null.
            var type1 = obj1.GetType();
            var type2 = obj2.GetType();
            EnssureCreateType(type1);
            EnssureCreateType(type2);


            var pInfo1 = PropertyInfoDictionary[type1];
            var pInfo2 = PropertyInfoDictionary[type2];
            if (pInfo1 != pInfo2)
                return false;
            /*
            if (pInfo1.Length != pInfo2.Length)
                return false;
            // сравним типы и названия переменных
            for (int i = 0; i < pInfo1.Length; i++)
            {
                if (pInfo1[i].Name != pInfo2[i].Name)
                    return false;
                if (pInfo1[i].PropertyType != pInfo2[i].PropertyType)
                    return false;
            }
            */
            // сравним значения
            for (int i = 0; i < pInfo1.Length; i++)
            {
                if (pInfo1[i].PropertyType.IsValueType || pInfo1[i].PropertyType == typeof(string))
                {
                    var protertyValue1 = pInfo1[i].GetMethod.Invoke(obj1, null);
                    var protertyValue2 = pInfo2[i].GetMethod.Invoke(obj2, null);

                    if (protertyValue1 == null && protertyValue2 == null)
                        continue;

                    if (protertyValue1 == null || protertyValue2 == null)
                        return false;

                    if (!protertyValue1.Equals(protertyValue2))
                        return false;
                }
                else if (pInfo1[i].PropertyType.IsArray)
                {
                    var array1 = pInfo1[i].GetMethod.Invoke(obj1, null);
                    var array2 = pInfo2[i].GetMethod.Invoke(obj2, null);

                    if (array1 == null && array2 == null)
                        continue;

                    if (array1 == null || array2 == null)
                        return false;

                    return ArrayEquals((Array)array1, (Array)array2);
                }

            }
            return true;
        }

        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseEntity> GetAddedItems()
        {
            return EntitySet.Keys.Where(x => x.State == EntityState.New);
        }

        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных. По заданному типу Т.
        /// </summary>
        public IEnumerable<T> GetAddedItems<T>() where T : BaseEntity, new()
        {
            return EntitySet.Keys.OfType<T>().Where(x => x.State == EntityState.New);
        }

        /// <summary>
        /// Список ВСЕХ сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseEntity> GetChangedItems()
        {
            return EntitySet.Keys.Where(x => x.State == EntityState.Modified);
        }

        /// <summary>
        /// Список ВСЕХ сущьностей измененных. По заданному типу Т.
        /// </summary>
        public IEnumerable<T> GetChangedItems<T>() where T : BaseEntity, new()
        {
            return EntitySet.Keys.OfType<T>().Where(x => x.State == EntityState.Modified);
        }

        /// <summary>
        /// Список ВСЕХ сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseEntity> GetDeletedItems()
        {
            return EntitySet.Keys.Where(x => x.State == EntityState.Deleted);
        }

        /// <summary>
        /// Список ВСЕХ сущьностей удаленных. По заданному типу Т.
        /// </summary>
        public IEnumerable<T> GetDeletedItems<T>() where T : BaseEntity, new()
        {
            return EntitySet.Keys.OfType<T>().Where(x => x.State == EntityState.Deleted);
        }

        /// <summary>
        /// Очистить монитор от информации о сущьностях
        /// </summary>
        public void Clear()
        {
            EntitySet?.Clear();
            PropertyInfoDictionary?.Clear();
        }

        /// <summary>
        /// Обновить IsChanged
        /// </summary>
        public void UpdateIsChanged()
        {
            IsChanged = EntitySet.Keys.Any(x =>
                x.State == EntityState.Modified || x.State == EntityState.Deleted || x.State == EntityState.New);
        }

        /// <summary>
        /// Изменились ли сущьности определенного типа.
        /// </summary>
        /// <typeparam name="T">Тип проверяемых значений</typeparam>
        /// <returns>true = были изменения в сущьностях T</returns>
        public bool IsChangedOfType<T>() where T : BaseEntity, new()
        {
            return EntitySet.Keys.OfType<T>().Any(x => x.State != EntityState.Unmodified);
        }

        /// <summary>
        /// Добавить сущьности в контейнер для отслеживания. Откопировать свойства. Закешировать тип свойств.
        /// </summary>
        /// <param name="entityCollection"> Коллекция экземпляров.</param>
        public void Apply(IEnumerable<BaseEntity> entityCollection)
        {
            if (entityCollection is ITrackable ec)
            {
                ec.SetMonitor(this);
            }

            foreach (var entity in entityCollection)
            {
                var type = entity.GetType();
                EnssureCreateType(type);
                ApplyInner(entity, type);
            }
        }

        /// <summary>
        /// Добавить сущьность в контейнер для отслеживания. Откопировать свойства. Закешировать тип свойств.
        /// </summary>
        /// <param name="baseEntity">Экземпляр типа</param>
        public void Apply(BaseEntity baseEntity)
        {
            var type = baseEntity.GetType();
            EnssureCreateType(type);
            ApplyInner(baseEntity, type);
        }

        /// <summary>
        /// Начинает отслеживать сущность и любые сущности, которые достижимы при обходе ее навигационных свойств.
        /// Обход рекурсивен, поэтому свойства навигации любых обнаруженных объектов также будут сканироваться.
        /// </summary>
        public virtual void ApplyGraph([NotNull] ITrackable rootEntity)
        {
            TraverseGraph(rootEntity, x =>
            {
                if (x is BaseEntity baseEntity)// это сущьность
                {
                    if (EntitySet.ContainsKey(baseEntity))
                        return false; //уже мониторится  
                    Apply(baseEntity);
                    return true;

                }
                x?.SetMonitor(this);
                return true;
            });
        }

        /// <summary>
        /// Обход графа. Анализирует EntityCollection - коллекции, BaseEntity - ссылка
        /// </summary>
        /// <param name="node">Сущьность с которой начинаем обход графа.</param>
        /// <param name="handleNode">
        /// Делегат для настройки информации отслеживания изменений для каждого объекта. Второй параметр к
        /// callback - произвольный объект состояния, переданный выше. 
        /// Итерация графика не будет продолжаться вниз по графику если обратный вызов возвращает <c> false </c>.
        /// </param>
        public virtual void TraverseGraph(ITrackable node, Func<ITrackable, bool> handleNode)
        {
            if (!handleNode(node))
            {
                return;
            }

            if (node is IEnumerable<BaseEntity> enumerableEntity)
            {
                foreach (var baseEntity in enumerableEntity)
                {
                    TraverseGraph(baseEntity, handleNode);
                }
            }
            else if (node != null)
            {
                var type = node.GetType();
                EnssureCreateType(type);

                var navigationProrertyList = PropertyInfoDictionary[type].Where(x => typeof(BaseEntity).IsAssignableFrom(x.PropertyType)).ToList();
                foreach (var navigationPi in navigationProrertyList)
                {
                    TraverseGraph(navigationPi.GetMethod.Invoke(node, null) as BaseEntity, handleNode);
                }

                var entityCollectionPi = PropertyInfoDictionary[type].Where(x => x.PropertyType.Name == EntityCollectionPropertyTypeName);
                foreach (var pi in entityCollectionPi)
                {
                    TraverseGraph(pi.GetMethod.Invoke(node, null) as ITrackable, handleNode);
                }
            }
        }

        /// <summary>
        /// Обход графа c контекстом. Анализирует EntityCollection - коллекции, BaseEntity - ссылка
        /// </summary>
        /// <param name="node">Сущьность с которой начинаем обход графа.</param>
        /// <param name="context">Некий контекст, который может использоватся при обходе графа</param>
        /// <param name="handleNode">
        /// Делегат для настройки информации отслеживания изменений для каждого объекта. Второй параметр к
        /// callback - произвольный объект состояния, переданный выше. 
        /// Итерация графика не будет продолжаться вниз по графику если обратный вызов возвращает <c> false </c>.
        /// </param>
        public virtual void TraverseGraphWitnContext(ITrackable node, object context, Func<ITrackable, object, bool> handleNode)
        {
            if (!handleNode(node, context))
            {
                return;
            }

            if (node is IEnumerable<BaseEntity> enumerableEntity)
            {
                foreach (var baseEntity in enumerableEntity)
                {
                    TraverseGraphWitnContext(baseEntity, context, handleNode);
                }
            }
            else if (node != null)
            {
                var type = node.GetType();
                EnssureCreateType(type);

                var navigationProrertyList = PropertyInfoDictionary[type].Where(x => typeof(BaseEntity).IsAssignableFrom(x.PropertyType)).ToList();
                foreach (var navigationPi in navigationProrertyList)
                {
                    TraverseGraphWitnContext(navigationPi.GetMethod.Invoke(node, null) as BaseEntity, context, handleNode);
                }

                var entityCollectionPi = PropertyInfoDictionary[type].Where(x => x.PropertyType.Name == EntityCollectionPropertyTypeName);
                foreach (var pi in entityCollectionPi)
                {
                    TraverseGraphWitnContext(pi.GetMethod.Invoke(node, null) as ITrackable, context, handleNode);
                }
            }
        }

        /// <summary>
        /// Текущее состояние теперь актуальное. Сохранить заново original.
        /// </summary>
        public void AcceptChanges()
        {
            foreach (var entity in EntitySet.Keys.Where(x => x.State != EntityState.Unmodified).ToList())
            {
                switch (entity.State)
                {
                    case EntityState.Deleted:
                        EntitySet.Remove(entity);
                        break;
                    case EntityState.New:
                    case EntityState.Modified:
                        EntitySet[entity].Entity.State = EntityState.Unmodified;
                        //пересохранение Original значения. Только для измененных.
                        //TODO:
                        //Временная заглушка, нужно отловить ошибку доступа к ChangedProperties, 
                        //т.к. что то изменяет состав коллекции во время цикла ниже.
                        var list = entity.ChangedProperties.ToList();
                        foreach (var propertyName in list)
                        //foreach (var propertyName in entity.ChangedProperties)
                        {
                            var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];

                            if (TryMakeOriginal(out var original, originalValueInfo.PropertyInfo, entity))
                            {
                                originalValueInfo.Value = original;
                            }
                        }
                        break;
                }
            }
            IsChanged = false;
        }

        /// <summary>
        /// Вернуть все назад. Восстановить из оригинальной копии. baseEntity.Property = original;
        /// </summary>
        public void RejectChanges()
        {
            var editAndDelList = EntitySet.Keys.Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted).ToList();
            var addedList = EntitySet.Keys.Where(x => x.State == EntityState.New).ToList();

            foreach (var entity in addedList)
            {
                EntitySet.Remove(entity);
                entity.Dispose();
            }

            foreach (var entity in editAndDelList)
            {
                //пересохранение текущего значения у измененных свойств.
                var сhangedProperties = entity.ChangedProperties.ToList();
                foreach (var propertyName in сhangedProperties)
                {
                    var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];
                    object newCurrent = null;

                    newCurrent = EntityHelper.InnerClone(originalValueInfo.Value, this);

                    entity.SetCurrentProperties(propertyName , newCurrent);
                    entity.RaisePropertyChanged(propertyName);
                }
                EntitySet[entity].Entity.State = EntityState.Unmodified;
                entity.ChangedProperties.Clear();
            }

            IsChanged = false;
        }

        #endregion
        #region Защищенные и внутренние методы
        /// <summary>
        /// Событие изменение свойств.
        /// </summary>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Событие изменения Entity. Постфактум.
        /// </summary>
        public virtual void OnEntityChanged(BaseEntity entity, [CallerMemberName] string propertyName = null)
        {
            EntityChangedEvent?.Invoke(this, new EntityChangedEventArgs(entity ,propertyName));
        }

        #endregion
        #region Приватные функции
        /// <summary>
        /// Сравнить любые массивы
        /// </summary>
        private bool ArrayEquals(Array array, Array x2)
        {
            if (array == null)
                return false;
            if (x2 == array)
                return true;

            if (array.Length != x2.Length)
                return false;
            for (int index = 0; index < array.Length; ++index)
            {
                object x = x2.GetValue(index);
                object y = array.GetValue(index);
                if (!x.Equals(y))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Убедимся, что тип создан Reflection в кеше для ускорения.
        /// </summary>
        /// <param name="type">Тип</param>
        private void EnssureCreateType(Type type)
        {
            if (!PropertyInfoDictionary.ContainsKey(type))
            {
                PropertyInfoDictionary[type] = type.GetProperties()
                    .Where(x => x.CanWrite && x.CanRead
                                           && !BaseEntity.ExceptPropertyNames.Contains(x.Name)).ToArray();
            }
        }

        /// <summary>
        /// Копируем оригинал, инициализируем сущьность.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <exception cref="TrakerException"></exception>
        private void ApplyInner(BaseEntity entity, Type type)
        {
            if (EntitySet.ContainsKey(entity))
                throw new TrakerException($"Сущьность {entity}{type} уже добавлена для отслеживания");

            var ei = new EntityInfo {Entity = entity, EntityType = type};

            foreach (var pi in PropertyInfoDictionary[type])
            {
                if (pi.PropertyType.Name == EntityCollectionPropertyTypeName)
                {
                    var originalValueInfo = new OriginalValueInfo
                    {
                        IsEntityCollection = true,
                        PropertyInfo = pi,
                        Value = pi.GetMethod.Invoke(entity, null)
                    };
                    ei.OriginalValues.Add(pi.Name, originalValueInfo);
                }
                else if (typeof(BaseEntity).IsAssignableFrom(pi.PropertyType))
                {
                    var originalValueInfo = new OriginalValueInfo
                    {
                        IsBaseEntityProperty = true,
                        PropertyInfo = pi,
                        Value = pi.GetMethod.Invoke(entity, null)
                    };
                    ei.OriginalValues.Add(pi.Name, originalValueInfo);
                }
                else if (TryMakeOriginal(out var originalValue, pi, entity))
                {
                    var originalValueInfo = new OriginalValueInfo
                    {
                        PropertyInfo = pi,
                        Value = originalValue
                    };

                    ei.OriginalValues.Add(pi.Name, originalValueInfo);
                }

            }
            entity.State = EntityState.Unmodified;
            entity.SetMonitor(this);
            EntitySet.Add(entity, ei);
        }

        /// <summary>
        /// Пробуем создать экземпляр значение для бекапа (Original)
        /// </summary>
        private bool TryMakeOriginal(out object original, PropertyInfo pi, BaseEntity baseEntity)
        {
            //если этот тип значений или это строка, то копируем значения
            if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
            {
                original = pi.GetMethod.Invoke(baseEntity, null);
                return true;
            }
            else
            {
                if (pi.PropertyType.IsArray) // массив  - точно реализует ICloneable 
                {
                    var referense = pi.GetMethod.Invoke(baseEntity, null);
                    if (referense == null)
                    {
                        // массив не инициализирован, но свойство валидно.
                        original = null;
                        return true;
                    }
                    else if (referense is ICloneable cloneable)
                    {
                        original = cloneable.Clone();
                        return true;
                    }
                }
                else if (typeof(IList).IsAssignableFrom(pi.PropertyType) && !typeof(ITrackable).IsAssignableFrom(pi.PropertyType))
                {
                    var referense = pi.GetMethod.Invoke(baseEntity, null);
                    if (referense == null)
                    {
                        // IList не инициализирован, но свойство валидно.
                        original = null;
                        return true;
                    }

                    if (pi.PropertyType.Name == ValueCollectionPropertyTypeName )
                    {
                        if (referense is ICloneable cloneable)
                        {
                            original = cloneable.Clone();
                            return true;
                        }
                            throw new NotSupportedException($"тип {pi.PropertyType} свойства{pi} не реализует ICloneable ");

                    }
                    else
                    {
                         if (referense is IList iList)
                        {
                            IList newList = Activator.CreateInstance(pi.PropertyType) as IList;
                            
                            if (newList != null)
                            {
                                var tempList = new List<object>(iList.Count);
                                foreach (var item in iList)
                                {
                                    tempList.Add(EntityHelper.InnerClone(item, this));
                                }

                                foreach (var temp in tempList)
                                    newList.Add(temp);

                                original = newList;
                                return true;
                            }
                        }
                    }

                    
                }
            }
            original = null;
            return false;
        }
        #endregion
        #region Очистка
        /// <summary>
        /// Очистка ресурсов.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Так как вызов очистки произошел через IDisposable, 
            // убирает объект из очереди финализации GC
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Защищенный Dispose с возможностью перегрузки.
        /// </summary>
        /// <param name="disposing"> Признак вызова Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {

                try
                {
                    // Освобождение управляемых ресурсов тут.
                    Clear();
                }
                catch
                {
                    // ignored
                }
            }
            // Освобождение НЕуправляемых ресурсов тут.
            //

            _disposed = true;
        }
        #endregion
    }
}