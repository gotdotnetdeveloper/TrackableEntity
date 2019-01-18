using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TrackableEntity
{
    /// <summary>
    /// Коллекция сущьностей.
    /// </summary>
    public class EntityCollection<TEntity> : ObservableCollection<BaseEntity> where TEntity : BaseEntity, new()
    {
        public EntityCollection()
        {
        }

        public EntityCollection(EntityStateMonitor monitor)
        {
            EntityStateMonitor = monitor;
        }

        public EntityCollection(EntityStateMonitor monitor, IEnumerable<TEntity> items) : base(items)
        {
            EntityStateMonitor = monitor;
        }

        /// <summary>
        /// Ссылка на мониторинг
        /// </summary>
        public EntityStateMonitor EntityStateMonitor { get; set; }

        /// <summary>
        /// Удалить все сущьности из коллекции.
        /// </summary>
        protected override void ClearItems()
        {
            if (EntityStateMonitor != null)
            {
                foreach (var entity in Items)
                {
                    if (EntityStateMonitor.EntitySet.ContainsKey(entity))
                    {
                        if (EntityStateMonitor.EntitySet[entity].BaseEntity.State == EntityState.New)
                        {
                            EntityStateMonitor.EntitySet.Remove(entity);
                            EntityStateMonitor.IsChanged = EntityStateMonitor.EntitySet.Keys.Any(x => x.State != EntityState.Unmodified);
                        }
                            
                        else
                        {
                            EntityStateMonitor.EntitySet[entity].BaseEntity.State = EntityState.Deleted;
                            EntityStateMonitor.IsChanged = true;
                        }
                    }
                }
            }

            base.ClearItems();
        }

        /// <summary>
        /// Вставить новую сущьность.
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <param name="item">Сущьность.</param>
        protected override void InsertItem(int index, BaseEntity item)
        {
            if (EntityStateMonitor != null)
            {
                if (!EntityStateMonitor.EntitySet.ContainsKey(item))
                {
                    EntityStateMonitor.Aplay<TEntity>(item);
                    item.State = EntityState.New;

                    EntityStateMonitor.IsChanged = true;
                }
            }

            base.InsertItem(index, item);
        }


        /// <summary>
        /// Удалить элемент.
        /// </summary>
        /// <param name="index">Индекс элемента.</param>
        protected override void RemoveItem(int index)
        {
            if (EntityStateMonitor != null)
            {
                var entity = this[index];

                if (EntityStateMonitor.EntitySet.ContainsKey(entity))
                {
                    if (EntityStateMonitor.EntitySet[entity].BaseEntity.State == EntityState.New)
                        EntityStateMonitor.EntitySet.Remove(entity);
                    else
                    {
                        EntityStateMonitor.EntitySet[entity].BaseEntity.State = EntityState.Deleted;
                    }

                    EntityStateMonitor.IsChanged = EntityStateMonitor.EntitySet.Keys.Any(x => x.State != EntityState.Unmodified);
                }
            }

            base.RemoveItem(index);
        }

        /// <summary>
        /// Заместить по индексу новым объектом.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="item">Элемент для вставки/реплайса</param>
        protected override void SetItem(int index, BaseEntity item)
        {
            if (EntityStateMonitor != null)
            {
                if (Items[index] is TEntity entity && !ReferenceEquals(item, entity))
                {
                    if (EntityStateMonitor.EntitySet.ContainsKey(entity))
                    {
                        EntityStateMonitor.EntitySet.Remove(entity);
                        // TODO Диспоуз entity 
                    }
                }

                if (!EntityStateMonitor.EntitySet.ContainsKey(item))
                {
                    EntityStateMonitor.Aplay<TEntity>(item);
                    item.State = EntityState.New;
                }
                    

                EntityStateMonitor.IsChanged = EntityStateMonitor.EntitySet.Keys.Any(x => x.State != EntityState.Unmodified);
            }

            base.SetItem(index, item);
        }

    }
}