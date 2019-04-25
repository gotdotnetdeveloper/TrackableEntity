namespace TrackableEntity
{
    /// <summary>
    /// Имеется метод добавления сущьности в коллекцию.
    /// </summary>
    public interface IEntityCollection
   {
       #region Публичные методы
       /// <summary>
        /// Добавление сущьности в коллекцию.
        /// </summary>
        /// <param name="baseEntity">Сущьность.</param>
        void AddBaseEntity(BaseEntity baseEntity);
       #endregion
   }
}
