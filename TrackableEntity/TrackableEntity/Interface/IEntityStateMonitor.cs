using System.Collections.Generic;
using System.ComponentModel;
using TrackableEntity.Properties;

namespace TrackableEntity.Interface
{
    /// <summary>
    /// Мониторинг изменения сущьностей.
    /// Главная функция - отслеживание состояний у BaseEntity. Следить за IsChanged и выдача Add/remove/update коллекций.
    /// </summary>
    public interface IEntityStateMonitor : INotifyPropertyChanged, IRevertibleChangeTracking
    {
        #region Публичные методы
        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        IEnumerable<BaseEntity> GetAddedItems();

        /// <summary>
        /// Список ВСЕХ  сущьностей добавленных. По заданному типу Т.
        /// </summary>
        IEnumerable<T> GetAddedItems<T>() where T : BaseEntity, new();

        /// <summary>
        /// Список ВСЕХ сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        IEnumerable<BaseEntity> GetChangedItems();

        /// <summary>
        /// Список ВСЕХ сущьностей измененных. По заданному типу Т.
        /// </summary>
        IEnumerable<T> GetChangedItems<T>() where T: BaseEntity,new ();

        /// <summary>
        /// Список ВСЕХ сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        IEnumerable<BaseEntity> GetDeletedItems();

        /// <summary>
        /// Список ВСЕХ сущьностей удаленных. По заданному типу Т.
        /// </summary>
        IEnumerable<T> GetDeletedItems<T>() where T : BaseEntity, new();

        /// <summary>
        /// Очистить монитор от информации о сущьностях
        /// </summary>
        void Clear();

        /// <summary>
        /// Сравнить две энтити
        /// </summary>
        /// <param name="obj1">Сущьность 1</param>
        /// <param name="obj2">Сущьность 2</param>
        /// <returns>true = сущьности навны.</returns>
        bool EntityEquals(BaseEntity obj1, BaseEntity obj2);

        /// <summary>
        /// Применить все дерево объектов к монитору для отслеживание, начиная с головного объекта.
        /// </summary>
        /// <param name="imonitor">Головной объект.</param>
        void ApplyGraph([NotNull] ITrackable imonitor);
        #endregion
    }

}
