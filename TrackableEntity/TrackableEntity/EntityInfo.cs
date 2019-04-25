using System;
using System.Collections.Generic;

namespace TrackableEntity
{
    /// <summary>
    /// Метаданные BaseEntity
    /// </summary>
    [Serializable]
    public class EntityInfo
    {
        #region Публичные свойства
        /// <summary>
        /// Ссылка на сущьность.
        /// </summary>
        public BaseEntity Entity { get; set; }

        /// <summary>
        /// Тип сущьности.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// Состояние сущьностей на начало отслеживания состояния. 
        /// </summary>
        public Dictionary<string, OriginalValueInfo> OriginalValues { get; set; } = new Dictionary<string, OriginalValueInfo>();
        #endregion
    }
}