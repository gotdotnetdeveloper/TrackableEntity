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
    public  class BaseEntity :  INotifyPropertyChanged, IEntity, IDisposable
    {
        /// <summary>
        /// Текущее состояние сущьности.
        /// </summary>
        public EntityState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            } 
        }
        /// <summary>
        /// Список измененных пропертей
        /// </summary>
        public List<string> ChangedProperties { get; set; } = new List<string>();

        /// <summary>
        /// Монитор отслеживанния.
        /// </summary>
        EntityStateMonitor IEntity.Monitor { get; set; }

        /// <summary>
        /// Текущие значение свойств.
        /// </summary>
        Dictionary<String, Object> IEntity.CurrentProperties { get; set; } = new Dictionary<String, Object>();


        private EntityState _state;
        /// <summary>
        /// Признак вызова Dispose().
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// Получить значение свойства.
        /// </summary>
        /// <typeparam name="T">Тип получаемого значения.</typeparam>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>Значение свойства</returns>
        protected T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            if (((IEntity) this).CurrentProperties.TryGetValue(propertyName, out var val))
            {
                return (T)val;
            }
            else
            {// еще не делали SetValue для параметра. Добавим дефолтное значение
                ((IEntity) this).CurrentProperties[propertyName] = default(T);
                return (T) ((IEntity) this).CurrentProperties[propertyName] ;
            }
        }

        /// <summary>
        /// Для каждого значимого свойства, которое имеет отображение в БД
        /// </summary>
        protected virtual  void SetValue(Object value, [CallerMemberName] string propertyName = "")
        {
            ((IEntity) this).CurrentProperties[propertyName] = value;

            if (((IEntity) this).Monitor != null) // включено отслеживание
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
        /// Обработка действий связанных с Monitor
        /// </summary>
        /// <param name="value">Значение set</param>
        /// <param name="propertyName"></param>
        private void OnSetValueInner(Object value,  string propertyName )
        {
            var newEqualOriginal = AreEqualsToOriginal(((IEntity) this).Monitor.EntitySet[this].OriginalValues[propertyName] , value);

            if (newEqualOriginal)
            {
                if (ChangedProperties.Contains(propertyName))
                    ChangedProperties.Remove(propertyName);

                if (!ChangedProperties.Any())
                {
                    State = EntityState.Unmodified;
                    ((IEntity) this).Monitor.IsChanged = ((IEntity) this).Monitor.EntitySet.Keys.Any(x => x.State != EntityState.Unmodified);
                }
            }
            else
            {
                if (!ChangedProperties.Contains(propertyName))
                ChangedProperties.Add(propertyName);

                State = EntityState.Modified;
                //Сигнализируем, что есть изменения
                ((IEntity) this).Monitor.IsChanged = true;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            // Так как вызов очистки произошел через IDisposable, 
            // убирает объект из очереди финализации GC
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Защищенный Dispose с возможностью перегрузки.
        /// </summary>
        /// <param name="disposing"> Признак вызова Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
               
                try
                {
                    // Освобождение управляемых ресурсов тут.
                    ((IEntity) this).CurrentProperties.Clear();
                    ((IEntity) this).Monitor = null;
                }
                catch
                {
                    // ignored
                }
            }
            // Освобождение НЕуправляемых ресурсов тут.
            //

            _disposed = true;
        }
    }
}

