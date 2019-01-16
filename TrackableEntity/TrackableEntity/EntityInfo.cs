using System;
using System.Collections.Generic;

namespace TrackableEntity
{
    /// <summary>
    /// Метаданные Entity
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// Ссылка на сущьность.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Тип сущьности.
        /// </summary>
        public Type EntityType { get; set; }
        /// <summary>
        /// Состояние сущьностей на начало отслеживания состояния. 
        /// </summary>
        public Dictionary<string,object > OriginalValues { get; set; } = new Dictionary<string, object>();
    }
}