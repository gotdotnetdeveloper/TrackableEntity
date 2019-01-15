using System.Collections.Generic;
using System.Collections.ObjectModel;
using TrackableEntity.Annotations;

namespace TrackableEntity
{
    /// <summary>
    /// Коллекция сущьностей.
    /// </summary>
    public class EntityCollection : ObservableCollection<Entity> 
    {

        public EntityCollection()
        {
            
        }
        public EntityCollection(Entity parent = null)
        {
            _parent = parent;
        }
        public EntityCollection( IEnumerable<Entity> EntityCollection, [CanBeNull] Entity parent = null):base(EntityCollection)
        {
            _parent = parent;
        }



        /// <summary>
        /// Родительская Entity
        /// </summary>
        private readonly Entity _parent;

        private EntityStateMonitor _entityStateMonitor;

        /// <summary>
        /// Ленивая инициализация из parent
        /// </summary>
        public EntityStateMonitor EntityStateMonitor
        {
            get
            {
                if (_entityStateMonitor == null)
                {
                    if (_parent != null)
                    {
                        _entityStateMonitor = _parent.EntityStateMonitor;
                    }

                    if (_entityStateMonitor == null)
                    {
                        _entityStateMonitor = new EntityStateMonitor();
                    }
                }

                return _entityStateMonitor;
            } 
           
        }

        protected override void ClearItems()
        {
            //foreach (var entity in Items)
            //{
            //    _entityStateMonitor.OriginalDictionary[entity]
            //}
            
            base.ClearItems();
        }

        protected override void InsertItem(int index, Entity item)
        {
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Entity item)
        {
            base.SetItem(index, item);
        }


     

       
    }
}