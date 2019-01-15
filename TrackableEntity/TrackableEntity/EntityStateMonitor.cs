using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у Entity. Следить за IsChanges и выдача Add/remove/update коллекций.
    /// аналог EntityStateMonitor в EntityFrameworkCore
    /// </summary>
    public class EntityStateMonitor : IEntityStateMonitor
    {
        /// <summary>
        /// Все отслеживаемые сущьности
        /// </summary>
        public readonly Dictionary<Entity, EntityInfo> EntitySet = new Dictionary<Entity, EntityInfo>(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Закешированный справочник типов. (Чтоб рефрексии было поменьше)
        /// </summary>
        public readonly Dictionary<Type, PropertyInfo[]> PropertyInfoDictionary = new Dictionary<Type, PropertyInfo[]>();

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
        public bool IsChanges { get; set; }

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
        /// Убедимся, что тип создан в кеше.
        /// </summary>
        /// <param name="type">Тип</param>
        private void EnssureCreateType(Type type)
        {
            if (!PropertyInfoDictionary.ContainsKey(type))
            {
                PropertyInfoDictionary[type] = type.GetProperties(BindingFlags.Public)
                    .Where(x => x.CanWrite && x.CanRead).ToArray();
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
            int i = 0;
            foreach (var pi in PropertyInfoDictionary[type])
            {
                //если этот тип значений или это строка, то копируем значения
                if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
                {
                    EntitySet[entity].OriginalValues[pi.Name] = pi.GetMethod.Invoke(entity, null);
                }
                else //Клонируем значение 
                {

                }
                i++;
            }

            entity.EntityState = EntityState.Unmodified;
            entity.EntityStateMonitor = this;
        }


        /// <summary>
        /// Добавить граф отслеживаемых объектов
        /// </summary>
        /// <param name="rootEntity">Узел графа</param>
        public void ApplayGraph(Entity rootEntity)
        {
            throw new NotImplementedException();
        }
    }
}