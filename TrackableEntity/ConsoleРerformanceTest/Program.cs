using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackableEntity;

namespace ConsoleРerformanceTest
{
    /// <summary>
    /// Для анализа производительности.
    /// </summary>
    class Program
    {

        static void Main(string[] args)
        {
            int count = 0;
            int maxCount = 100000;
            var list = new List<object>(maxCount);
            do
            {
                list.Add(Guid.NewGuid());
                count++;
            } while (count < maxCount);

            var id = Guid.NewGuid();
            var watch = Stopwatch.StartNew();
            foreach (var o in list.OfType<Guid>())
            {
                var x = o == id;
            }
            watch.Stop();
            Console.WriteLine($"o.Equals(id); Milliseconds= {watch.ElapsedMilliseconds}");


             watch.Restart();
            foreach (var o in list)
            {
                var x = o.Equals(id);
            }
            
            watch.Stop();
            Console.WriteLine($"OfType<Guid>() Milliseconds= {watch.ElapsedMilliseconds}");
            Console.ReadLine();
        }


        static void Main123123(string[] args)
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
            es.Aplay<TreeItemBaseEntity>(list);
            watch.Stop();
            Console.WriteLine($"init Milliseconds= {watch.ElapsedMilliseconds}");

            //var Id = Guid.NewGuid();
            watch.Restart();
            var tmp = list.Where(x => x.Id == x.ParentId).OrderBy(x => x.Id).ToList();

            //foreach (var treeItemBaseEntity in list)
            //{
            //    treeItemBaseEntity.Id = Id;
            //}
            watch.Stop();
            Console.WriteLine($"EntityStateMonitor Milliseconds= {watch.ElapsedMilliseconds}");
            Console.ReadLine();
        }
        public class TreeItemBaseEntity : BaseEntity
        {
            public virtual Guid Id
            {
                get => GetValue<Guid>();
                set => SetValue(value);
            }

            public virtual Guid ParentId
            {
                get => GetValue<Guid>();
                set => SetValue(value);
            }

            public virtual string Name { get; set; }
        }
    }
}
