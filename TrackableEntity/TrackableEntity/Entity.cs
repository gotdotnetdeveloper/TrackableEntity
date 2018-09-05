using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// Сущьность отслеживается
    /// </summary>
    class Entity : IRevertibleChangeTracking, System.ComponentModel.IChangeTracking, INotifyPropertyChanged, INotifyPropertyChanging
    {

        /// <summary>Реализация сеттера для примитивного свойства</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="oldValue">Поле для хранения значения (по ссылке)</param>
        /// <param name="newValue">Новое значение свойства</param>
        protected void SetValue<T>(string propertyName, ref T oldValue, T newValue)
        {
            if (object.Equals((object)oldValue, (object)newValue))
                return;
            this.RaiseDataMemberChanging(propertyName);

            oldValue = newValue;
            this.RaiseDataMemberChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            PropertyChangingEventHandler propertyChanging = this.PropertyChanging;
            if (propertyChanging == null)
                return;
            propertyChanging((object)this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            //if (this.CurrentSet != null)
            //{
            //    if (this.MetaType.PrimaryKeyProperties.ContainsKey(e.PropertyName))
            //    {
            //        this._identity = (EntityKey)null;
            //        this.CurrentSet.UpdateRelatedAssociations(this, e.PropertyName);
            //    }
            //    else if (this.MetaType.ForeignKeyProperties.ContainsKey(e.PropertyName) || e.PropertyName == "EntityState")
            //        this.CurrentSet.UpdateRelatedAssociations(this, e.PropertyName);
            //}
            //if (this.IsEditing && !this.RaisePropertyChanges)
            //    return;
            //if (this._proxies != null)
            //{
            //    for (int index = 0; index < this._proxies.Count; ++index)
            //    {
            //        WeakReference proxy = this._proxies[index];
            //        if (proxy != null)
            //        {
            //            Entity target = proxy.Target as Entity;
            //            DisposableObject entitySet;
            //            if (target == null || (entitySet = target.EntitySet as DisposableObject) != null && entitySet.IsDisposed)
            //                this._proxies[index] = (WeakReference)null;
            //            else
            //                target.RaiseDataMemberChanged(e.PropertyName);
            //        }
            //    }
            //}
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.PropertyChanged((object)this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected internal void RaisePropertyChanging(string propertyName)
        {
            this.OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected internal void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaiseDataMemberChanging(string propertyName)
        {
            // _oldPropertyCollection[propertyName]
            this.RaisePropertyChanging(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaiseDataMemberChanged(string propertyName)
        {

            //this.ChangeTracker.DataMemberChanged(propertyName);
            this.RaisePropertyChanged(propertyName);
        }


        /// <summary>
        /// Флаг отслеживания изменений
        /// </summary>
        public bool IsTrackable { get; set; }

        /// <summary>
        /// Начать отслеживать изменения
        /// </summary>
        public void BeginTrack()
        {
            //   _oldPropertyCollection.Clear();
            IsTrackable = true;
        }
        //private Dictionary<string,object> _oldPropertyCollection = new Dictionary<string, object>();

        /// <summary>
        /// Принять изменения
        /// </summary>
        public void AcceptChanges()
        {

        }

        public bool IsChanged { get; }

        /// <summary>
        /// Отменить изменения
        /// </summary>
        public void RejectChanges()
        {

        }
    }
}
