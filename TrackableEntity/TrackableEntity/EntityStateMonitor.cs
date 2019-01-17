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
    /// Главная функция - отслеживание состояний у Entity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// аналог EntityStateMonitor в EntityFrameworkCore
    /// </summary>
    public class EntityStateMonitor : IEntityStateMonitor
    {
        /// <summary>
        /// Все отслеживаемые сущьности
        /// </summary>
        public readonly Dictionary<Entity, EntityInfo> EntitySet = new Dictionary<Entity, EntityInfo>(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Закешированный справочник типов. (Чтоб рефлексии было поменьше)
        /// </summary>
        public readonly Dictionary<Type, PropertyInfo[]> PropertyInfoDictionary = new Dictionary<Type, PropertyInfo[]>();

        private bool _isChanged;

        public ICollection<Entity> GetAddedItems()
        {
           return EntitySet.Keys.Where(x => x.EntityState == EntityState.New).ToList();
        }

        public ICollection<Entity> GetChangedItems()
        {
            return EntitySet.Keys.Where(x => x.EntityState == EntityState.Modified).ToList();
        }

        public ICollection<Entity> GetDeletedItems()
        {
            return EntitySet.Keys.Where(x => x.EntityState == EntityState.Deleted).ToList();
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
        /// Добавить Для отслеживания.
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="entityCollection"> Коллекция экземпляров.</param>
        public void Aplay<T>(IEnumerable<Entity> entityCollection) where T : class
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
                                           && x.Name != nameof(EntityStateMonitor)
                                           && x.Name != "ChangedProperties"
                                           && x.Name != nameof(EntityState)).ToArray();
            }
        }
        /// <summary>
        /// Добавить сущьность
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="entity">Экземпляр типа</param>
        public void Aplay<T>(Entity  entity) where T : class
        {
            var type = typeof(T);
            EnssureCreateType(type);
            AplayInner(entity, type);
        }

        /// <summary>
        /// Копируем оригинал, инициализируем сущьность.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        private void AplayInner(Entity entity, Type type) 
        {
            var ei = new EntityInfo();
            ei.Entity = entity;
            ei.EntityType = type;

            foreach (var pi in PropertyInfoDictionary[type])
            {
                if (TryGetOriginal(out var originalValue ,pi, entity))
                {
                    var originalValueInfo = new OriginalValueInfo
                    {
                        PropertyInfo = pi,
                        Value = originalValue
                    };

                    ei.OriginalValues.Add(pi.Name, originalValueInfo);
                }
                
            }
            entity.EntityState = EntityState.Unmodified;
            entity.EntityStateMonitor = this;
            EntitySet.Add(entity, ei);
        }

        /// <summary>
        /// По PropertyInfo получить описание сохраненного значения.
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private OriginalValueInfo GetOriginalValueInfoBy(PropertyInfo pi, Entity entity)
        {
            //если этот тип значений или это строка, то копируем значения
            if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
            {
                var p = new OriginalValueInfo
                {
                    PropertyInfo = pi,
                    Value = pi.GetMethod.Invoke(entity, null)
                };
                return p;
            }
            else
            {
                var referense = pi.GetMethod.Invoke(entity, null);
                if (referense is ICloneable cloneable)
                {
                    var p = new OriginalValueInfo
                    {
                        PropertyInfo = pi,
                        Value = cloneable.Clone()
                    };
                    return p;
                }
                else
                {
                    //var bytes = Serializer.Deserialize()
                    // var mc2 = ZeroFormatterSerializer.Deserialize<MyClass>(bytes);
                    //https://stackoverflow.com/questions/4667981/c-sharp-use-system-type-as-generic-parameter
                    //var mc2 = ZeroFormatterSerializer.Deserialize(bytes)
                    // throw new NotImplementedException("TODO: Клонируем значение сериализацией");
                }
            }
            return null;
        }

        /// <summary>
        /// Пробуем получить оригинальное значение
        /// </summary>
        private bool TryGetOriginal(out object original, PropertyInfo pi, Entity entity)
        {
            //если этот тип значений или это строка, то копируем значения
            if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
            {
                original = pi.GetMethod.Invoke(entity, null);
                return true;

            }
            else
            {
                var referense = pi.GetMethod.Invoke(entity, null);
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
        /// <param name="rootEntity">Узел графа</param>
        public void ApplayGraph(Entity rootEntity)
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
            foreach (var entity in EntitySet.Keys.Where(x => x.EntityState != EntityState.Unmodified))
            {
                EntitySet[entity].Entity.EntityState = EntityState.Unmodified;
                //пересохранение Original значения. Только для измененных.

                foreach (var propertyName in entity.ChangedProperties)
                {
                    var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];

                    if (TryGetOriginal(out var original, originalValueInfo.PropertyInfo, entity))
                    {
                        originalValueInfo.Value = original;
                    }
                }
            }

            IsChanged = false;
        }

        /// <summary>
        /// Вернуть все назад. Восстановить из оригинальной копии. entity.Property = original;
        /// </summary>
        public void RejectChanges()
        {
            foreach (var entity in EntitySet.Keys.Where(x=>x.EntityState == EntityState.Modified || x.EntityState == EntityState.Deleted))
            {
                EntitySet[entity].Entity.EntityState = EntityState.Unmodified;
                //пересохранение текущего значения у измененных свойств.

                foreach (var propertyName in entity.ChangedProperties)
                {
                    var originalValueInfo = EntitySet[entity].OriginalValues[propertyName];
                    var monitor = entity.EntityStateMonitor;
                    entity.EntityStateMonitor = null; //уберем ссылку на монитор на момент сета в проперти. что бы не обрабатывалось сравнение.
                    originalValueInfo.PropertyInfo.SetMethod.Invoke(entity, new[] { originalValueInfo.Value });
                    entity.EntityStateMonitor = monitor;
                }
            }
            IsChanged = false;
        }

       
    }
}