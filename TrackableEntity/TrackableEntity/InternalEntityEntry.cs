using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// Описание сущьности
    /// </summary>
    public  partial class InternalEntityEntry
    {
        /// <summary>
        /// 
        /// </summary>
        private OriginalValues _originalValues;

        /// <summary>
        /// Cсылка на оригинальную сущьность.
        /// </summary>
        public  object Entity { get; set; }
    }
}
