using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    public enum EntityState
    {
        /// <summary>
        /// </summary>
        None = 0,

        /// <summary>
        /// Не менялась
        /// </summary>
        Unmodified = 1,

        /// <summary>
        /// Изменена
        /// </summary>
        Modified = 2,

        /// <summary>
        /// Новая, Добавлена
        /// </summary>
        New = 3,

        /// <summary>
        /// Помечена как удаленная.
        /// </summary>
        Deleted = 4
    }
}
