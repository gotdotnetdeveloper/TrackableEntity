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
    /// Базовая сущьность.
    /// </summary>
    public  class Entity :  INotifyPropertyChanged
    {
        /// <summary>
        /// Текущее состояние сущьности.
        /// </summary>
        public EntityState EntityState
        {
            get => _entityState;
            set
            {
                _entityState = value;
                OnPropertyChanged();
            } 
        }
        /// <summary>
        /// Список измененных пропертей
        /// </summary>
        public List<string> ChangedProperties { get; set; } = new List<string>();

        /// <summary>
        /// Контекст отслеживанния.
        /// </summary>
        public EntityStateMonitor EntityStateMonitor { get; set; }

        
        private EntityState _entityState;

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
        /// IStructuralEquatable
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        bool ArrayEquals(object current,  object other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            Array a1 = current as Array;
            Array a2 = other as Array;

            if (a1 == null && a2 == null)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a2.Length != a1.Length)
                return false;

            for (int index = 0; index < a2.Length; ++index)
            {
                object x = a1.GetValue(index);
                object y = a2.GetValue(index);
                if (!x.Equals( y))
                    return false;
            }
            return true;
        }


        private bool AreEqualsToOriginal(OriginalValueInfo originalInfo,  Object value)
        {
            if (originalInfo.Value == null && value == null)
                return true;

            if (originalInfo.Value == null || value == null)
                return false;

            if (originalInfo.PropertyInfo.PropertyType.IsValueType
                || originalInfo.PropertyInfo.PropertyType == typeof(string))
            {
                
                return originalInfo.Value.Equals(value);
            }
            else //referense type
            {
              

                //тут точно знаем, что оба не null 
                if (originalInfo.Value is IEnumerable originalEnumerable && value  is IEnumerable valueEnumerable)
                {
                    var e1 = originalEnumerable.GetEnumerator();
                    var e2 = valueEnumerable.GetEnumerator();
                    e1.Reset();
                    e2.Reset();

                    while (true)
                    {
                        var nextExist1 = e1.MoveNext();
                        var nextExist2 = e2.MoveNext();

                        if (nextExist1 != nextExist2)
                            return false; //количество элементов разное

                        if (!nextExist1)
                            return true; //закончились элементы для проверки. И все элементы равны

                        bool isItemEquals;

                        if ((e1.Current is ValueType || e1.Current is string) && (e2.Current is ValueType || e2.Current is string))
                        {
                            isItemEquals = e1.Current.Equals(e2.Current);
                        }
                        else
                        {
                            throw new Exception(
                                $"PropertyName={originalInfo.PropertyInfo.Name}; TrackableEntity.AreEqualsToOriginal(..) Can get equals only IEnumerable<ValueType>! ");
                        }

                        if (!isItemEquals)
                            return false; //нашли значение не равное

                    }
                }
                
            }
            return true;
        }


        /// <summary>
        /// Обработка действий связанных с EntityStateMonitor
        /// </summary>
        /// <param name="value">Значение set</param>
        /// <param name="propertyName"></param>
        private void OnSetValueInner(Object value,  string propertyName )
        {
            var newEqualOriginal = AreEqualsToOriginal(EntityStateMonitor.EntitySet[this].OriginalValues[propertyName] , value);

            if (newEqualOriginal)
            {
                if (ChangedProperties.Contains(propertyName))
                    ChangedProperties.Remove(propertyName);

                if (!ChangedProperties.Any())
                {
                    EntityState = EntityState.Unmodified;
                    EntityStateMonitor.IsChanged = EntityStateMonitor.EntitySet.Keys.Any(x => x.EntityState != EntityState.Unmodified);
                }
            }
            else
            {
                if (!ChangedProperties.Contains(propertyName))
                ChangedProperties.Add(propertyName);

                EntityState = EntityState.Modified;
                //Сигнализируем, что есть изменения
                EntityStateMonitor.IsChanged = true;
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

