using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TrackableEntity.Annotations;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у BaseEntity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// аналог Monitor в EntityFrameworkCore
    /// </summary>
    public class EntityStateMonitor : IEntityStateMonitor, IDisposable
    {
        /// <summary>
        /// Признак вызова Dispose().
        /// </summary>
        protected bool _disposed;
        /// <summary>
        /// Есть ли изменения.
        /// </summary>
        private bool _isChanged;
        /// <summary>
        /// Имена свойств, которые не требуется отслеживать.
        /// </summary>
        private static readonly string[] ExceptPropertyNames = new[] { "EntityStateMonitor", "ChangedProperties", "CurrentProperties", "EntityState" };

        /// <summary>
        /// Хранилище отслеживаемых сущьностей.
        /// </summary>
        public readonly Dictionary<BaseEntity, EntityInfo> EntitySet = new Dictionary<BaseEntity, EntityInfo>(ReferenceEqualityComparer.Instance);
        
        /// <summary>
        /// Закешированный справочник типов. (Чтоб рефлексии вызывалось меньше)
        /// </summary>
        public readonly Dictionary<Type, PropertyInfo[]> PropertyInfoDictionary = new Dictionary<Type, PropertyInfo[]>();


        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        public ICollection<BaseEntity> GetAddedItems()
        {
           return EntitySet.Keys.Where(x => x.State == EntityState.New).ToList();
        }
        /// <summary>
        /// Список ВСЕХ сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        public ICollection<BaseEntity> GetChangedItems()
        {
            return EntitySet.Keys.Where(x => x.State == EntityState.Modified).ToList();
        }
        /// <summary>
        /// Список ВСЕХ сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        public ICollection<BaseEntity> GetDeletedItems()
        {
            return EntitySet.Keys.Where(x => x.State == EntityState.Deleted).ToList();
        }

        /// <summary>
        /// Есть ли изменения
        /// </summary>
        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                if (_isChanged != value)
                {
                    _isChanged = value;
                    OnPropertyChanged();
                }
            } 
        }

        /// <summary>
        /// Добавить сущьности в контейнер для отслеживания. Откопировать свойства. Закешировать тип свойств.
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="entityCollection"> Коллекция экземпляров.</param>
        public void Aplay<T>(IEnumerable<BaseEntity> entityCollection) where T : class
        {
            var type = typeof(T);
            EnssureCreateType(type);
            foreach (var entity in entityCollection)
            {
                AplayInner(entity, type);
            }
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
                                           && !ExceptPropertyNames.Contains(x.Name)).ToArray();
            }
        }
        /// <summary>
        /// Добавить сущьность в контейнер для отслеживания. Откопировать свойства. Закешировать тип свойств.
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="baseEntity">Экземпляр типа</param>
        public void Aplay<T>(BaseEntity  baseEntity) where T : class
        {
            var type = typeof(T);
            EnssureCreateType(type);
            AplayInner(baseEntity, type);
        }

        /// <summary>
        /// Копируем оригинал, инициализируем сущьность.
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <param name="type"></param>
        private void AplayInner(BaseEntity baseEntity, Type type) 
        {
            var ei = new EntityInfo();
            ei.BaseEntity = baseEntity;
            ei.EntityType = type;

            foreach (var pi in PropertyInfoDictionary[type])
            {
                if (TryMakeOriginal(out var originalValue ,pi, baseEntity))
                {
                    var originalValueInfo = new OriginalValueInfo
                    {
                        PropertyInfo = pi,
                        Value = originalValue
                    };

                    ei.OriginalValues.Add(pi.Name, originalValueInfo);
                }
                
            }
            baseEntity.State = EntityState.Unmodified;
            ((IEntity) baseEntity).Monitor = this;
            EntitySet.Add(baseEntity, ei);
        }

        /// <summary>
        /// Пробуем получить оригинальное значение
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
                var referense = pi.GetMethod.Invoke(baseEntity, null);
                if (referense is ICloneable cloneable)
                {
                    original = cloneable.Clone();
                    return true;
                }
                else
                {
                    original = null;
                    return false;
                    //var bytes = Serializer.Deserialize()
                    // var mc2 = ZeroFormatterSerializer.Deserialize<MyClass>(bytes);
                    //https://stackoverflow.com/questions/4667981/c-sharp-use-system-type-as-generic-parameter
                    //var mc2 = ZeroFormatterSerializer.Deserialize(bytes)
                    // throw new NotImplementedException("TODO: Клонируем значение сериализацией");
                }
            }
        }



        /// <summary>
        /// Добавить граф отслеживаемых объектов
        /// </summary>
        /// <param name="rootBaseEntity">Узел графа</param>
        public void ApplayGraph(BaseEntity rootBaseEntity)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Текущее состояние теперь актуальное. Сохранить заново original.
        /// </summary>
        public void AcceptChanges()
        {
            foreach (var entity in EntitySet.Keys.Where(x => x.State != EntityState.Unmodified))
            {
                EntitySet[entity].BaseEntity.State = EntityState.Unmodified;
                //пересохранение Original значения. Только для измененных.

                foreach (var propertyName in entity.ChangedProperties)
                {
                    var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];

                    if (TryMakeOriginal(out var original, originalValueInfo.PropertyInfo, entity))
                    {
                        originalValueInfo.Value = original;
                    }
                }
            }

            IsChanged = false;
        }

        /// <summary>
        /// Вернуть все назад. Восстановить из оригинальной копии. baseEntity.Property = original;
        /// </summary>
        public void RejectChanges()
        {
            foreach (var entity in EntitySet.Keys.Where(x=>x.State == EntityState.Modified || x.State == EntityState.Deleted))
            {
                EntitySet[entity].BaseEntity.State = EntityState.Unmodified;
                //пересохранение текущего значения у измененных свойств.

                foreach (var propertyName in entity.ChangedProperties)
                {
                    var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];
                    var monitor = ((IEntity) entity).Monitor;
                    ((IEntity) entity).Monitor = null; //уберем ссылку на монитор на момент сета в проперти. что бы не обрабатывалось сравнение.
                    originalValueInfo.PropertyInfo.SetMethod.Invoke(entity, new[] { originalValueInfo.Value });
                    ((IEntity) entity).Monitor = monitor;
                }
            }
            IsChanged = false;
        }


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
                    EntitySet.Clear();
                    PropertyInfoDictionary.Clear();
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

    }
}