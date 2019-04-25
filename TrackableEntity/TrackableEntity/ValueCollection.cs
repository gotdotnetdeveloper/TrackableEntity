using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TrackableEntity.Properties;

namespace TrackableEntity
{
    /// <summary>
    /// Отслеживаемая трекером коллекция типов значений. Отслеживается (Add,del,remove).
    /// </summary>
    [Serializable]
    public class ValueCollection<TValue> : ObservableCollection<TValue> , ICloneable
    {
        /// <summary>
        /// Создание отслеживаемой коллекции
        /// </summary>
        /// <param name="entity">Владелец этой коллекции</param>
        /// <param name="propertyName">Наименование свойства у владельца</param>
        public ValueCollection([NotNull] BaseEntity entity, string propertyName)
        {
            InitValueCollection(entity, propertyName);
        }

        /// <summary>
        /// Инициализация коллекции для дальнейщего отслеживания
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        public void InitValueCollection([NotNull] BaseEntity entity, string propertyName)
        {
            _parentEntity = entity;
            _parentEntityPropertyName = propertyName;
        }

        /// <summary>
        /// Конструктор без отслеживания изменений.
        /// </summary>
        public ValueCollection()
        {}

        #region Публичные поля

        /// <summary>
        /// Ссылка на сущьность - владелеца  ValueCollection
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        [CanBeNull]
        private BaseEntity _parentEntity;


        /// <summary>
        /// Наименование свойства у ParentEntity
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        [CanBeNull]
        private string _parentEntityPropertyName;

        #endregion
        #region Публичные методы
        /// <summary>
        /// Добавить список в коллекцию.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TValue> items)
        {
            foreach (var entity in items)
            {
                Add(entity);
            }

            _parentEntity?.UpdateEntityState(this, _parentEntityPropertyName);
        }

        /// <summary>
        /// Клонируются бинарной сериализацией. 
        /// </summary>
        /// <returns></returns>
        public object BinaryFormatterClone()
        {
            return InnerBinaryFormatterClone(this);
        }

        /// <summary>
        /// Клонирование объекта. Предпологаем, что элементы - простые типы.
        /// </summary>
        /// <returns>Новый экземпляр.</returns>
        public object Clone()
        {
            var returnList = new ValueCollection<TValue>(_parentEntity, _parentEntityPropertyName);
            if (Items.Any())
            {
                var reverseList = new List<TValue>(Items.Count);
                foreach (var entity in Items)
                {
                    var x = EntityHelper.InnerClone(entity, _parentEntity?.Monitor);
                    reverseList.Add((TValue)x);
                }
                returnList.AddRange(reverseList);
            }
            return returnList;
        }

        #endregion
        #region Защищенные и внутренние методы
        /// <summary>
        /// Удалить все сущьности из коллекции.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            _parentEntity?.UpdateEntityState(this, _parentEntityPropertyName);
            _parentEntity?.Monitor?.OnEntityChanged(_parentEntity, _parentEntityPropertyName);
        }

        /// <summary>
        /// Вставить новую сущьность.
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <param name="item">Сущьность.</param>
        protected override void InsertItem(int index, TValue item)
        {
            base.InsertItem(index, item);
            _parentEntity?.UpdateEntityState(this, _parentEntityPropertyName);
            _parentEntity?.Monitor?.OnEntityChanged(_parentEntity, _parentEntityPropertyName);
        }

        /// <summary>
        /// Удалить элемент.
        /// </summary>
        /// <param name="index">Индекс элемента.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _parentEntity?.UpdateEntityState(this, _parentEntityPropertyName);
            _parentEntity?.Monitor?.OnEntityChanged(_parentEntity, _parentEntityPropertyName);
        }

        /// <summary>
        /// Заместить по индексу новым объектом.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="item">Элемент для вставки/реплайса</param>
        protected override void SetItem(int index, TValue item)
        {
            base.SetItem(index, item);
            _parentEntity?.UpdateEntityState(this, _parentEntityPropertyName);
            _parentEntity?.Monitor?.OnEntityChanged(_parentEntity, _parentEntityPropertyName);
        }
        #endregion
        #region Приватные функции
        /// <summary>
        /// Клонирование списка объектов.
        /// </summary>
        private T InnerBinaryFormatterClone<T>(T oldList)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, oldList);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
        #endregion
    }
}