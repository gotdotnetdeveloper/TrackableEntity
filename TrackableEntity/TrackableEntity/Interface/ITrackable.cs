using System;

namespace TrackableEntity.Interface
{
    /// <summary>
    /// У сущьности есть свойство Monitor с интерфейсом IEntityStateMonitor.
    /// А так же интерфейс является меткой, что объект трекается.
    /// </summary>
    public interface ITrackable : ICloneable
    {
        #region Публичные методы
        /// <summary>
        /// У сущьности есть трекер Monitor,
        /// которое умеет  RejectChanges(),  IsChanged , AcceptChanges();
        /// </summary>
        IEntityStateMonitor GetMonitor();

        /// <summary>
        /// Установить монитор.
        /// </summary>
        /// <param name="entityStateMonitor"></param>
        void SetMonitor(EntityStateMonitor entityStateMonitor);
        #endregion
    }
}