using System;
using System.Reflection;

namespace TrackableEntity
{
    /// <summary>
    /// Информация о сохраненном значениии
    /// </summary>
    [Serializable]
    public class OriginalValueInfo
    {
        #region Публичные свойства
        /// <summary>
        /// Это проперти наследник от BaseEntity
        /// </summary>
        public bool IsBaseEntityProperty { get; set; }

        /// <summary>
        /// Реализация EntityCollection
        /// </summary>
        public bool IsEntityCollection { get; set; }

        /// <summary>
        /// Информация о проперти
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Оригинальное значение до начала отслеживания в трекинге
        /// </summary>
        public object Value { get; set; }
        #endregion
    }
}