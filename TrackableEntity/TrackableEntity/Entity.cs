using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TrackableEntity.Annotations;

namespace TrackableEntity
{
    /// <summary>
    /// Метаданные Entity
    /// </summary>
    public class EntityInfo
    {
        public Entity Entity { get; set; }

        /// <summary>
        /// Состояние сущьностей на начало отслеживания состояния. 
        /// </summary>
        public Dictionary<string,object > OriginalValues { get; set; } = new Dictionary<string, object>();
    }



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
        /// Для каждого значимого свойства, которое имеет отображение в БД
        /// </summary>
        public virtual  void OnSetValue(Object value, [CallerMemberName] string propertyName = "")
        {
            if (EntityStateMonitor != null) // включено отслеживание
            {
                var newEqualOriginal = EntityStateMonitor.EntitySet[this].OriginalValues[propertyName].Equals(value);
                if (newEqualOriginal)
                {
                    if (EntityStateMonitor.IsChanges)
                    {
                        //нужно проверить, требуется ли выключить флаг IsChanges.
                        // Для этого нам нужно проверить весь набор
                        // Если в энтитисете есть измененные сущьности, то выходим из алгоритма.
                        var otherInUnmodifiedState = EntityStateMonitor.EntitySet.Keys.All(x => x.EntityState == EntityState.Unmodified && x != this );

                        if (otherInUnmodifiedState)
                        {
                            //то проверяем все остальные поля 
                            // у текущей энтити сравнивая с оригинал.
                            // Если 
                        }
                        else
                        {
                        //    
                        }
                    }
                }
                else
                {
                    EntityState = EntityState.Modified;
                    //Сигнализируем, что есть изменения
                    EntityStateMonitor.IsChanges = true;
                }
            }
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
