using System.Collections.Generic;
using System.ComponentModel;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у BaseEntity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// аналог Monitor в EntityFrameworkCore
    /// </summary>
    interface IEntityStateMonitor : INotifyPropertyChanged, IRevertibleChangeTracking
    {
        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        ICollection<BaseEntity> GetAddedItems();
        /// <summary>
        /// Список ВСЕХ сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        ICollection<BaseEntity> GetChangedItems();
        /// <summary>
        /// Список ВСЕХ сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        ICollection<BaseEntity> GetDeletedItems();


        /// <summary>
        /// Добавить граф отслеживаемых объектов (рекурсивный обход дерева по BaseEntity).
        /// Делает фотографию всех сущьностей
        /// </summary>
        /// <param name="rootBaseEntity">Узел графа</param>
        void ApplayGraph(BaseEntity rootBaseEntity);
    }

}
