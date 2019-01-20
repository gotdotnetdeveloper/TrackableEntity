using System;
using System.Collections.Generic;

namespace TrackableEntity
{
    /// <summary>
    /// Внутренний интерфейс Entity. Скрывает некоторые свойства для IntelliSense.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Мониторинг состояния сущьностей.
        /// </summary>
        EntityStateMonitor Monitor { get; set; }
        /// <summary>
        /// Хранилище для текущих значений свойств которые отслеживаются.
        /// </summary>
        Dictionary<String, Object> CurrentProperties { get; set; }
    }
}