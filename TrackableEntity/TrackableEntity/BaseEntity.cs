using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Xml.Serialization;
using TrackableEntity.Interface;
using TrackableEntity.Properties;

namespace TrackableEntity
{
    /// <summary>
    /// Базовая сущьность.
    /// <example>
    /// Для отслеживания в Свойствах добавить методы доступа(GetValue, SetValue). Пример:
    /// public  Guid Id
    /// {
    /// get => GetValue&gt;Guid&lt;();
    /// set => SetValue(value);
    /// }
    /// </example>
    /// </summary>
    [Serializable]
    public class BaseEntity : INotifyPropertyChanged, IDisposable, ITrackable
    {
        /// <summary>
        /// Событие изменения свойств.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие Установка монитора
        /// </summary>
        public event OnSetMonitorHandler SetMonitorEvent;

        #region Приватные поля
        private object _lockCurrentProperties = new object();

        /// <summary>
        /// Текущие значение свойств.
        /// </summary>
        [XmlIgnore]
        [CanBeNull]
        private readonly Dictionary<String, Object> _currentProperties = new Dictionary<String, Object>();

        /// <summary>
        /// Признак вызова Dispose().
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        private bool _disposed;
        #endregion
        #region Публичные поля
        /// <summary>
        /// Имена свойств, которые не требуется отслеживать.
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        //public static readonly string[] ExceptPropertyNames = { "Monitor", "ChangedProperties", "CurrentProperties", "State" };
        public static readonly string[] ExceptPropertyNames = { };

        /// <summary>
        /// Текущее состояние сущьности.
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        public EntityState State;

        /// <summary>
        /// Список наименований измененных пропертей.
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        public List<string> ChangedProperties = new List<string>();

        /// <summary>
        /// Мониторинг изменения сущьностей.
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        [CanBeNull]
        public EntityStateMonitor Monitor;
        #endregion
        #region Публичные методы
        /// <summary>
        /// Получить текущее значение.
        /// </summary>
        /// <returns></returns>
        public Object GetCurrentProperties(string value)
        {
            lock (_lockCurrentProperties)
            {
                return _currentProperties[value];
            }
        }

        /// <summary>
        /// Очистить все значения.
        /// </summary>
        /// <returns></returns>
        public void CurrentPropertiesClear()
        {
            lock (_lockCurrentProperties)
            {
                _currentProperties.Clear();
            }
        }

        /// <summary>
        /// Установить текущее поле по имени свойства
        /// </summary>
        public void SetCurrentProperties(string name, object  value)
        {
            lock (_lockCurrentProperties)
            {
                _currentProperties[name] = value;
            }
        }

        /// <summary>
        /// Попытка получить текущее значение.
        /// </summary>
        public bool TryGetValue(string propertyName, out object val)
        {
            bool ok; 
            lock (_lockCurrentProperties)
            {
                ok = _currentProperties.TryGetValue(propertyName,out object value);
                val = value;
                return ok;
            }
        }

        /// <summary>
        /// Получить Оригинальное измененное значение.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Имя.</param>
        /// <param name="originalValue"></param>
        /// <returns>true = нашлось свойство</returns>
        public bool GetOriginalProperty<T>(string name, out T originalValue)
        {
            if (Monitor != null)
            {
                if (Monitor.EntitySet[this].OriginalValues.ContainsKey(name))
                {
                    var val = Monitor.EntitySet[this].OriginalValues[name].Value;

                    if (val == null)
                    {
                        originalValue = default(T);
                        return true;
                    }
                    else
                    {
                        if (val is T variable)
                        {
                            originalValue = variable;
                            return true;
                        }
                        else
                        {
                            throw new InvalidCastException(
                                $"Нет возможности привести свойство {name}={val} к типу {typeof(T)}");
                        }
                    }
                }
            }
            originalValue = default(T);
            return false;
        }

        /// <summary>
        /// Получить ссылку на объект Monitor (мониторинг изменения сущьностей)
        /// </summary>
        /// <returns></returns>
        public IEntityStateMonitor GetMonitor()
        {
            return Monitor;
        }

        /// <summary>
        /// Установить монитор отслеживанния.
        /// </summary>
        public virtual void SetMonitor(EntityStateMonitor entityStateMonitor)
        {
            Monitor = entityStateMonitor;
            SetMonitorEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Пересоздает монитор, если его небыло. Атачит сущьность к монитору.
        /// </summary>
        [Obsolete("use Trackable()")]
        public T Trackable<T>() where T : BaseEntity, new()
        {
            return Trackable() as T;
        }

        /// <summary>
        /// Пересоздает монитор, если его небыло. Атачит сущьность к монитору.
        /// </summary>
        public BaseEntity Trackable()
        {
            Monitor?.Dispose();
            SetMonitor(new EntityStateMonitor());
            Monitor?.Apply(this);
            return this;
        }

        /// <summary>
        /// Пересоздает монитор, если его небыло. Старый Dispose(). Присоеденяет текущую коллекцию  к монитору
        /// Начинает отслеживать сущность и любые сущности, которые достижимы при обходе ее навигационных свойств.
        /// Обход рекурсивен, поэтому свойства навигации любых обнаруженных объектов также будут сканироваться.
        /// </summary>
        public BaseEntity TrackableGraph()
        {
            Monitor?.Dispose();
            SetMonitor(new EntityStateMonitor());
            Monitor?.ApplyGraph(this);
            return this;
        }

        /// <summary>
        /// Параметры для передачи в БД.
        /// </summary>
        /// <param name="exceptList">Список исключения.</param>
        /// <returns>Словарь с параметрами и значением.</returns>
        public Dictionary<string, object> GetPropertyValuePairs(string[] exceptList = null)
        {
            var res = new Dictionary<string, object>();
            var propertyInfoList = GetType().GetProperties();
            var validPropertyInfoList = propertyInfoList.Where(x => x.CanRead && !ExceptPropertyNames.Contains(x.Name))
                .AsQueryable();
            if (exceptList != null && exceptList.Any())
            {
                validPropertyInfoList = validPropertyInfoList.Where(x => !exceptList.Contains(x.Name));
            }

            foreach (var propertyInfo in validPropertyInfoList)
            {
                res.Add(propertyInfo.Name, propertyInfo.GetValue(this));
            }

            return res;
        }

        /// <summary>
        /// Вызов события изменения свойств.
        /// </summary>
        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

        /// <summary>
        /// Клонируются только проперти простых типов.  и ObservableCollection Простых типов
        /// </summary>
        /// <returns>Новый экземпляр = простые свойства скопированы. </returns>
        public object Clone()
        {
            return EntityHelper.InnerClone(this, Monitor);
        }

        /// <summary>
        /// Обновление состояния сущьности действий связанных с действиями 
        /// </summary>
        /// <param name="value">Значение set</param>
        /// <param name="propertyName"></param>
        public void UpdateEntityState(Object value, string propertyName)
        {
            if (Monitor != null && Monitor.EntitySet.ContainsKey(this) && Monitor.EntitySet[this].OriginalValues.ContainsKey(propertyName))
            {
                var newEqualOriginal = AreEqualsToOriginal(Monitor.EntitySet[this].OriginalValues[propertyName], value);

                if (newEqualOriginal)
                {
                    if (ChangedProperties.Contains(propertyName))
                        ChangedProperties.Remove(propertyName);

                    if (!ChangedProperties.Any())
                    {
                        if (State == EntityState.Modified)
                            State = EntityState.Unmodified;
                        Monitor.UpdateIsChanged();
                    }
                }
                else
                {
                    if (!ChangedProperties.Contains(propertyName))
                        ChangedProperties.Add(propertyName);

                    if (State == EntityState.Unmodified)
                        State = EntityState.Modified;

                    //Сигнализируем, что есть изменения
                    Monitor.IsChanged = true;
                }
            }

        }

        /// <summary>
        /// Вызов события изменения свойств (Для обратной совместимости с GalaSoft.MvvmLight.ObservableObject) .
        /// </summary>
        [NotifyPropertyChangedInvocator]
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Защищенные и внутренние методы
        /// <summary>
        /// Получить значение свойства.
        /// </summary>
        /// <typeparam name="T">Тип получаемого значения.</typeparam>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>Значение свойства</returns>
        protected T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            if (TryGetValue(propertyName, out var val))
            {
                return (T)val;
            }
            else
            {// еще не делали SetValue для параметра. Добавим дефолтное значение
                SetCurrentProperties(propertyName, default(T));
                return (T) GetCurrentProperties(propertyName);
            }
        }

        /// <summary>
        /// Для каждого значимого свойства, которое имеет отображение в БД
        /// </summary>
        protected virtual void SetValue(Object value, [CallerMemberName] string propertyName = "")
        {
            SetCurrentProperties(propertyName, value);
            if (Monitor != null)
            {
                //var monitor = ((IEntity) this).Monitor;

                if (value is BaseEntity baseEntity)
                {
                    //if(!monitor.EntitySet.ContainsKey(baseEntity))
                    //    monitor.Apply(baseEntity);
                    // TODO: можно автоматом добавить в трекер сущьности
                }
                else if (value is EntityCollection<BaseEntity> entityCollection)
                {
                    // TODO: можно автоматом добавить в трекер сущьности
                    //if (!(monitor.EntitySet.Where())
                    //    ((IEntity)this).Monitor.Apply(baseEntity);
                }
                else
                {
                    UpdateEntityState(value, propertyName);
                }
                Monitor?.OnEntityChanged(this, propertyName);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Приватные функции
        /// <summary>
        /// Оригинальное значение равно
        /// </summary>
        /// <param name="originalInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AreEqualsToOriginal(OriginalValueInfo originalInfo, Object value)
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
            else if (originalInfo.IsBaseEntityProperty || originalInfo.IsEntityCollection)
            {
                return ReferenceEquals(originalInfo.Value, value);
            }
            else //referense type, Обработка массивов из значимых типов
            {
                //тут точно знаем, что оба не null 
                if (originalInfo.Value is IEnumerable originalEnumerable && value is IEnumerable valueEnumerable)
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
                                $"При вызове: TrackableEntity.AreEqualsToOriginal(..); Свойство={originalInfo.PropertyInfo.Name} не является коллекцией значимых типов (IEnumerable<ValueType>) ");
                        }

                        if (!isItemEquals)
                            return false; //нашли значение не равное

                    }
                }

            }
            return true;
        }
        #endregion
        #region Очистка
        /// <summary>
        /// Уничтоженио объекта по паттерну microsoft.
        /// </summary>
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
                    CurrentPropertiesClear();
                    SetMonitor(null);
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
        #endregion
    }


    /// <summary>
    /// Установка монитора
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public delegate void OnSetMonitorHandler(object sender, EventArgs e);
}

