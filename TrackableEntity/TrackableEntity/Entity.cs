using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrackableEntity.Annotations;

namespace TrackableEntity
{
   

    /// <summary>
    /// Базовая сущьность.
    /// </summary>
   public  class Entity :  INotifyPropertyChanged
    {
        /// <summary>
        /// Состояние сущьности.
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Контекст отслеживанния.
        /// </summary>
        public EntityStateMonitor EntityStateMonitor { get; set; }

        /// <summary>
        /// Для каждого значимого свойства
        /// </summary>
        public virtual  void OnSetValue(Object value, [CallerMemberName] string propertyName = "")
        {
            
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
