using System.Collections.Generic;
using System.ComponentModel;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у Entity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// аналог EntityStateMonitor в EntityFrameworkCore
    /// </summary>
    interface IEntityStateMonitor : INotifyPropertyChanged, IRevertibleChangeTracking
    {
        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        ICollection<Entity> GetAddedItems();
        /// <summary>
        /// Список ВСЕХ сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        ICollection<Entity> GetChangedItems();
        /// <summary>
        /// Список ВСЕХ сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        ICollection<Entity> GetDeletedItems();


        /// <summary>
        /// Добавить граф отслеживаемых объектов (рекурсивный обход дерева по Entity).
        /// Делает фотографию всех сущьностей
        /// </summary>
        /// <param name="rootEntity">Узел графа</param>
        void ApplayGraph(Entity rootEntity);
    }

}
