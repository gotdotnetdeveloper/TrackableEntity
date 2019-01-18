using System;
using System.Collections.Generic;
using System.Reflection;

namespace TrackableEntity
{
    /// <summary>
    /// Метаданные BaseEntity
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// Ссылка на сущьность.
        /// </summary>
        public BaseEntity BaseEntity { get; set; }

        /// <summary>
        /// Тип сущьности.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// Состояние сущьностей на начало отслеживания состояния. 
        /// </summary>
        public Dictionary<string, OriginalValueInfo> OriginalValues { get; set; } = new Dictionary<string, OriginalValueInfo>();
    }

    /// <summary>
    /// Информация о сохраненном значениии
    /// </summary>
    public class OriginalValueInfo
    {
      public  PropertyInfo PropertyInfo { get; set; }
        public object Value { get; set; }
    }
}