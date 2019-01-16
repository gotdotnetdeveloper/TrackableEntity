using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackableEntity;

namespace TrackableEntityTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            var user = new User();
            user.Name = "Иванов";
            user.Age = 33;
            user.ListUser = new ObservableCollection<User>();


            user.stringArr = new[] {"1", "2"};
            user.stringList = new List<string>();
            user.stringList.Add("one");

            var es = new EntityStateMonitor();
            es.Aplay<User>(user);
            Assert.IsFalse(es.IsChanges);
            user.Age = 40;
            Assert.IsTrue(es.IsChanges);
            user.Age = 33;
            Assert.IsFalse(es.IsChanges);
        }


        [TestMethod]
        public void TestMethod2()
        {
            var user = new User();
            user.Name = "Иванов";
            user.Age = 33;

            var user2 = new User();
            user2.Name = "Васечкин";
            user2.Age = 7;

            var es = new EntityStateMonitor();
            es.Aplay<User>(user);
            es.Aplay<User>(user2);

            user.Age = 40;
            user2.Age = 8;
            

            //Возращаем назад возраст

            user.Age = 33;
            //Иванова вернули назад, но Васечкин в модифицированном состоянии. Должно быть IsChanges= true
            Assert.IsTrue(es.IsChanges);
            user2.Age = 7;
            //Васечкин вернули назад, Теперь все сущьности в исходном состоянии. Должно быть IsChanges= false
            Assert.IsFalse(es.IsChanges);
        }
    }
    public class User : Entity
    {
        private int _age;
        private string _name;

        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnSetValue(value);
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnSetValue(value);
            }
        }


        public string[] stringArr { get; set; }
        public List<string> stringList { get; set; }

        public ObservableCollection<User> ListUser { get; set; }
    }
}