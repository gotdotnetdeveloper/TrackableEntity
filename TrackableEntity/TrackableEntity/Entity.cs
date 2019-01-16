using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
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
        /// Для каждого значимого свойства, которое имеет отображение в БД
        /// </summary>
        public virtual  void OnSetValue(Object value, [CallerMemberName] string propertyName = "")
        {
            if (EntityStateMonitor != null) // включено отслеживание
            {
                OnSetValueInner(value, propertyName);
            }
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Обработка действий связанных с EntityStateMonitor
        /// </summary>
        /// <param name="value">Значение set</param>
        /// <param name="propertyName"></param>
        private void OnSetValueInner(Object value,  string propertyName )
        {
            var newEqualOriginal = EntityStateMonitor.EntitySet[this].OriginalValues[propertyName].Equals(value);
            if (newEqualOriginal)
            {
                if (EntityStateMonitor.IsChanges)
                {
                    // Нужно проверить, требуется ли выключить флаг IsChanges.
                    // Для этого нам нужно проверить весь набор EntitySet.
                    // Если в EntitySet есть измененные сущьности, то IsChanges=true и выходим из алгоритма.

                    if (EntityStateMonitor.EntitySet.Keys.Count == 1 || EntityStateMonitor.EntitySet.Keys.All(x => x.EntityState == EntityState.Unmodified && x != this))
                    {
                        //Остальные ентити не менялись. (или их нет) то проверяем все остальные поля текущей ентити
                        // у текущей энтити сравнивая с оригиналами ВСЕ свойства.


                        var propertyInfoList = EntityStateMonitor.PropertyInfoDictionary[EntityStateMonitor.EntitySet[this].EntityType];
                        foreach (var pi in propertyInfoList.Where(x => x.Name != propertyName))
                        {
                            var ob = pi.GetMethod.Invoke(this, null);
                            if (!EntityStateMonitor.EntitySet[this].OriginalValues[pi.Name].Equals(ob))
                            {
                                //Нашли другое поле у которого значения отличаются. Дальнеейшее вычисление не важно.
                                //IsChanges = true
                                return;
                            }
                        }
                        // Все значения этой энтити равны сохраненным.
                        EntityState = EntityState.Unmodified;
                        // Раз все значения всех энтити в статусе Unmodified, значит изменений нет.
                        EntityStateMonitor.IsChanges = false;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
