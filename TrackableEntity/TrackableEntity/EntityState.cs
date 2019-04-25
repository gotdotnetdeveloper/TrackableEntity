namespace TrackableEntity
{
    /// <summary>
    /// Состояние сущьности в данный момент.
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// Не трекаемая, отсоедененная сущьность.
        /// </summary>
        Detached = 0,

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
