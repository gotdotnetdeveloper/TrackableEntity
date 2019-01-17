using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// Главная функция - отслеживание состояний у Entity. Следить за IsChanges и выдача Add/remove/update коллекций.
    /// аналог EntityStateMonitor в EntityFrameworkCore
    /// </summary>
    interface IEntityStateMonitor : INotifyPropertyChanged
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
        /// Ecть ли изменения. 
        /// реализация - В момент сета или добавления коллекции
        /// </summary>
        /// <returns>true = есть изменения</returns>
        bool IsChanges { get; set; }

        /// <summary>
        /// Добавить граф отслеживаемых объектов (рекурсивный обход дерева по Entity).
        /// Делает фотографию всех сущьностей
        /// </summary>
        /// <param name="rootEntity">Узел графа</param>
        void ApplayGraph(Entity rootEntity);
    }

}
