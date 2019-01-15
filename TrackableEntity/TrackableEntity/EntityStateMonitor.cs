using System;
using System.Collections.Generic;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у Entity. Следить за IsChanges и выдача Add/remove/update коллекций.
    /// аналог EntityStateMonitor в EntityFrameworkCore
    /// </summary>
    public class EntityStateMonitor : IEntityStateMonitor
    {
        //1) Original(object referense, int TrakerHashcode() - valueСостояние для трекера)
        public readonly Dictionary<Entity, object> OriginalDictionary = new Dictionary<Entity, object>(ReferenceEqualityComparer.Instance);

        //2) Current(object referense, valueСостояние для трекера, EntityState(Add/remove/IsChanges/NonChanges) )
        /// <summary>
        /// Добавленные сущьности 
        /// </summary>
        public readonly Dictionary<Entity, object> NewDictionary = new Dictionary<Entity, object>(ReferenceEqualityComparer.Instance);

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
        /// Есть ли изменения
        /// </summary>
        public bool IsChanges { get; set; }

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