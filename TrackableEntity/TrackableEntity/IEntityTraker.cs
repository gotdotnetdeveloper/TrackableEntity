using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// аналог StateManager в EntityFrameworkCore
    /// </summary>
    interface IEntityTraker 
    {
        /// <summary>
        /// Список сущьностей добавленных.
        /// </summary>
        /// <returns></returns>
        ICollection<object> GetAddedItems();
        /// <summary>
        /// Список сущьностей измененных.
        /// </summary>
        /// <returns></returns>
        ICollection<object> GetChangedItems();
        /// <summary>
        /// Список сущьностей удаленных.
        /// </summary>
        /// <returns></returns>
        ICollection<object> GetDeletedItems();

        /// <summary>
        /// Проверить, есть ли изменения
        /// </summary>
        /// <returns>true = есть изменения</returns>
        bool HasChanges();

        /// <summary>
        /// Добавить граф отслеживаемых объектов
        /// </summary>
        /// <param name="rootEntity">Узел графа</param>
        void ApplayGraph(object rootEntity);
    }


    //  internal IDictionary<string, object> OriginalValues
}
