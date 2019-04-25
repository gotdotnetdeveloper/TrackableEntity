using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TrackableEntity.Interface;

namespace TrackableEntity
{
    /// <summary>
    /// Коллекция сущьностей.
    /// public interface  
    /// </summary>
    [Serializable]
    public class EntityCollection<TEntity> : ObservableCollection<TEntity>, ITrackable , IEntityCollection
        where TEntity : BaseEntity, new()
    {
        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public EntityCollection()
        {
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public EntityCollection(EntityStateMonitor monitor)
        {
            Monitor = monitor;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public EntityCollection(IEnumerable<TEntity> items, EntityStateMonitor monitor = null) : base(items)
        {
            Monitor = monitor;
        }

        #region Публичные поля
        /// <summary>
        /// Ссылка на мониторинг
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        public EntityStateMonitor Monitor;
        #endregion
        #region Публичные методы
        /// <summary>
        /// Добавить список в коллекцию.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TEntity> items)
        {
            foreach (var entity in items)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// У сущьности есть трекер Monitor,
        /// которое умеет  RejectChanges(),  IsChanged , AcceptChanges();
        /// </summary>
        public IEntityStateMonitor GetMonitor()
        {
            return Monitor;
        }

        /// <summary>
        /// У сущьности есть трекер Monitor,
        /// которое умеет  RejectChanges(),  IsChanged , AcceptChanges();
        /// </summary>
        public void SetMonitor(EntityStateMonitor entityStateMonitor)
        {
            Monitor = entityStateMonitor;
        }

        /// <summary>
        /// Пересоздает монитор, если его небыло. Старый Dispose(). Присоеденяет текущую коллекцию  к монитору
        /// </summary>
        public EntityCollection<TEntity> Trackable()
        {
            Monitor?.Dispose();
            SetMonitor(new EntityStateMonitor());
            Monitor?.Apply(Items);
            return this;
        }

        /// <summary>
        /// Пересоздает монитор, если его небыло. Старый Dispose(). Присоеденяет текущую коллекцию  к монитору
        /// Начинает отслеживать сущность и любые сущности, которые достижимы при обходе ее навигационных свойств.
        /// Обход рекурсивен, поэтому свойства навигации любых обнаруженных объектов также будут сканироваться.
        /// Если состояние не задано, объект остается без отслеживания.
        /// </summary>
        public EntityCollection<TEntity> TrackableGraph()
        {
            
            Monitor?.Dispose();
            SetMonitor( new EntityStateMonitor());
            Monitor?.ApplyGraph(this); 
            return this;
        }

        /// <summary>
        /// Клонируются бинарной сериализацией. 
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            return BinaryFormatterClone(this);
        }

        /// <summary>
        /// Поверхностное клонирование объекта рефлекшеном.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (Items.Any())
            {
                var returnList = new EntityCollection<TEntity>();
                foreach (var entity in Items)
                {
                    returnList.Add(entity.Clone() as TEntity);
                }

                return returnList.Reverse();
            }

            return new EntityCollection<TEntity>();
        }

        /// <summary>
        /// Добавление сущьности в коллекцию.
        /// </summary>
        /// <param name="baseEntity">Сущьность.</param>
        public void AddBaseEntity(BaseEntity baseEntity)
        {
            if(baseEntity is TEntity entity)
                Items.Add(entity);
        }
        #endregion
        #region Защищенные и внутренние методы
        /// <summary>
        /// Удалить все сущьности из коллекции.
        /// </summary>
        protected override void ClearItems()
        {
            if (Monitor != null)
            {
                foreach (var entity in Items)
                {
                    if (Monitor.EntitySet.ContainsKey(entity))
                    {
                        var state = Monitor.EntitySet[entity].Entity.State;

                        if (state == EntityState.New)
                        {
                            Monitor.EntitySet.Remove(entity);
                            entity.Dispose();
                        }
                        else if (state == EntityState.Modified || state == EntityState.Unmodified)
                        {
                            Monitor.EntitySet[entity].Entity.State = EntityState.Deleted;
                        }
                    }
                }
                Monitor.UpdateIsChanged();
            }

            IList<TEntity> tempList = null;
            if (Monitor != null)
                tempList = Items.ToList();

            base.ClearItems();

            if (Monitor != null && tempList!= null)
            {
                foreach (var entity in tempList)
                    Monitor?.OnEntityChanged(entity);
            }
            
        }

        /// <summary>
        /// Вставить новую сущьность.
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <param name="item">Сущьность.</param>
        protected override void InsertItem(int index, TEntity item)
        {
            if (Monitor != null)
            {
                if (!Monitor.EntitySet.ContainsKey(item))
                {
                    Monitor.Apply(item);
                    item.State = EntityState.New;

                    Monitor.IsChanged = true;
                }
            }

            base.InsertItem(index, item);
            Monitor?.OnEntityChanged(item);
        }

        /// <summary>
        /// Удалить элемент.
        /// </summary>
        /// <param name="index">Индекс элемента.</param>
        protected override void RemoveItem(int index)
        {
            var entity = this[index];
            if (Monitor != null)
            {
                if (Monitor.EntitySet.ContainsKey(entity))
                {
                    var state = Monitor.EntitySet[entity].Entity.State;

                    if (state == EntityState.New)
                    {
                        Monitor.EntitySet.Remove(entity);
                        entity.Dispose();
                    }

                    else if (state == EntityState.Modified || state == EntityState.Unmodified)
                    {
                        Monitor.EntitySet[entity].Entity.State = EntityState.Deleted;
                    }

                    Monitor.UpdateIsChanged();
                }
            }
            base.RemoveItem(index);
            Monitor?.OnEntityChanged(entity);
        }

        /// <summary>
        /// Заместить по индексу новым объектом.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="item">Элемент для вставки/реплайса</param>
        protected override void SetItem(int index, TEntity item)
        {
            if (Monitor != null)
            {
                if (Items[index] is TEntity entity && !ReferenceEquals(item, entity))
                {
                    if (Monitor.EntitySet.ContainsKey(entity))
                    {
                        Monitor.EntitySet.Remove(entity);
                        entity.Dispose();
                    }
                }
                if (!Monitor.EntitySet.ContainsKey(item))
                {
                    Monitor.Apply(item);
                    item.State = EntityState.New;
                }
                Monitor.UpdateIsChanged();
            }
            base.SetItem(index, item);
            Monitor?.OnEntityChanged(item );
        }
        #endregion
        #region Приватные функции
        /// <summary>
        /// Клонирование списка объектов.
        /// </summary>
        private T BinaryFormatterClone<T>(T oldList)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, oldList);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
        #endregion
    }
}