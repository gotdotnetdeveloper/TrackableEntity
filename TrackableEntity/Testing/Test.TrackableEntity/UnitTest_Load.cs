using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChangeTracking;
using TrackableEntity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackableEntityTest
{
    /// <summary>
    /// Производительность.
    /// </summary>
    [TestClass]
    public class UnitTest_Load
    {
        /// <summary>
        /// Инит 100 000 значений TrackableEntity.AsTrackable();
        /// и поиск.
        /// </summary>
        [TestMethod]
        public void TestMethod_POCO()
        {
            int count = 0;
            int maxCount = 100000;
            var list = new List<TreeItemPOCO>(maxCount);
            do
            {
                var newItem = new TreeItemPOCO();
                newItem.Id = Guid.NewGuid();
                newItem.ParentId = Guid.NewGuid();
                list.Add(newItem);
                count++;
            } while (count < maxCount);

            var watch = Stopwatch.StartNew();
            var listAsTrackable = list.AsTrackable();
            watch.Stop();
            Debug.Print($"init Milliseconds= {watch.ElapsedMilliseconds}");
            var Id = Guid.NewGuid();
            watch.Restart();
            var tmp = listAsTrackable.Where(x => x.Id == x.ParentId).OrderBy(x => x.Id).ToList();
            //foreach (var treeItemPoco in listAsTrackable)
            //    treeItemPoco.Id = Id;
            watch.Stop();
            Debug.Print($"listAsTrackable Milliseconds= {watch.ElapsedMilliseconds}");
        }
       
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestMethod_BaseEntity()
        {
            int count = 0;
            int maxCount = 100000;
            var list = new List<TreeItemBaseEntity>(maxCount);
            do
            {
                var newItem = new TreeItemBaseEntity();
                newItem.Id = Guid.NewGuid();
                newItem.ParentId = Guid.NewGuid();
                list.Add(newItem);
                count++;
            } while (count < maxCount);

            var watch = Stopwatch.StartNew();
            var es = new EntityStateMonitor();
            es.Apply(list);
            watch.Stop();
            Debug.Print($"init Milliseconds= {watch.ElapsedMilliseconds}");

            var Id = Guid.NewGuid();
            watch.Restart();
            var tmp = list.Where(x => x.Id == x.ParentId).OrderBy(x => x.Id).ToList();
           
            //foreach (var treeItemBaseEntity in list)
            //{
            //    treeItemBaseEntity.Id = Id;
            //}
            watch.Stop();
            Debug.Print($"EntityStateMonitor Milliseconds= {watch.ElapsedMilliseconds}");
        }



        [TestMethod]
        public void TestMethod_BaseEntity_AsTrackable()
        {
            int count = 0;
            int maxCount = 1;
            var list = new List<TreeItemBaseEntity>(maxCount);
            do
            {
                var newItem = new TreeItemBaseEntity();
                newItem.Id = Guid.NewGuid();
                newItem.ParentId = Guid.NewGuid();
                list.Add(newItem);
                count++;
            } while (count < maxCount);

            var watch = Stopwatch.StartNew();

            try
            {
                var tr = list.AsTrackable();

                tr[0].Name = "dsds";

                var isChanged = tr.CastToIChangeTrackableCollection().IsChanged;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

            watch.Stop();
            Debug.Print($"EntityStateMonitor Milliseconds= {watch.ElapsedMilliseconds}");
        }

    }
    public class TreeItemBaseEntity : BaseEntity
    {
        public virtual Guid Id {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        public virtual Guid ParentId
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        public virtual string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }

    public class TreeItemPOCO 
    {
        public virtual Guid Id { get; set; }

        public virtual Guid ParentId
        {
            get; set;
        }

        public virtual string Name { get; set; }
    }


 
}
