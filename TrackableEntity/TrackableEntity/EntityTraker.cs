using System;
using System.Collections.Generic;

namespace TrackableEntity
{
    /// <summary>
    /// Аналог StateManager в EntityFrameworkCore
    /// </summary>
    public class EntityTraker : IEntityTraker
    {
        private readonly Dictionary<object, InternalEntityEntry> _entityReferenceMap
            = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);

        public ICollection<object> GetAddedItems()
        {
            throw new NotImplementedException();
        }

        public ICollection<object> GetChangedItems()
        {
            throw new NotImplementedException();
        }

        public ICollection<object> GetDeletedItems()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Проверить, есть ли изменения
        /// </summary>
        /// <returns>true = есть изменения</returns>
        public bool HasChanges()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Добавить граф отслеживаемых объектов
        /// </summary>
        /// <param name="rootEntity">Узел графа</param>
        public void ApplayGraph(object rootEntity)
        {
            throw new NotImplementedException();
        }
    }
}