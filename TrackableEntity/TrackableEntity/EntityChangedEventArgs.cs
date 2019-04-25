using System.ComponentModel;
using System.Security.Permissions;

namespace TrackableEntity
{
    /// <summary>
    /// Делегат изменения IsChanged.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public delegate void EntityChangedEventHandler(object sender, EntityChangedEventArgs e);

    /// <summary>
    /// Аргумент для IsChanged события.
    /// </summary>
    public class EntityChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public EntityChangedEventArgs(BaseEntity entity, string propertyName):base(propertyName)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Сущьность, которая инициировала изменение.
        /// </summary>
        public virtual BaseEntity Entity { get; }
    }
}