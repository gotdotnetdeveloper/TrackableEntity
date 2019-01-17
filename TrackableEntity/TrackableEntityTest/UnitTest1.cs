using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackableEntity;

namespace TrackableEntityTest
{
    /// <summary>
    /// MonitorUnitTest тест.
    /// </summary>
    [TestClass]
    public class EntityStateMonitorUnitTest
    {
        [TestMethod]
        public void TestMethod_RejectChanges_AcceptChanges()
        {
            var user = new User();
            user.Age = 33;

            var es = new EntityStateMonitor();
            es.Aplay<User>(user);

            Assert.IsFalse(es.IsChanged);
            user.Age = 40;
            Assert.IsTrue(es.IsChanged);

            es.RejectChanges();
            Assert.IsTrue(user.Age == 33);
            Assert.IsFalse(es.IsChanged);

            user.Age = 40;
            Assert.IsTrue(es.IsChanged);
            es.AcceptChanges();
            Assert.IsTrue(user.Age == 40);
            Assert.IsFalse(es.IsChanged);
        }

        /// <summary>
        /// Простой метод редактирования массива (ссылочного типа)
        ///  и возврата к первоночальному значению.
        /// </summary>
        [TestMethod]
        public void TestMethod_Array_EditChange_IsChanges()
        {
            var r1 = new byte[] {1};
            var r2 = new byte[] { 1 };

            var r3 = new byte[] { 2 };

            var user = new User();
            user.ByteArray = r1;
            var es = new EntityStateMonitor();
            es.Aplay<User>(user);

            Assert.IsFalse(es.IsChanged);

            user.ByteArray = r2;
            Assert.IsFalse(es.IsChanged);

            user.ByteArray = r3;
            Assert.IsTrue(es.IsChanged);

            user.ByteArray = r2;
            Assert.IsFalse(es.IsChanged);
        }

        [TestMethod]
        public void TestMethod_StringArray_EditChange_IsChanges()
        {
            var r1 = new string[] { "1" };
            var r2 = new string[] { "1" };

            var r3 = new string[] { "2" };

            var user = new User();
            user.StringArray = r1;
            var es = new EntityStateMonitor();
            es.Aplay<User>(user);

            Assert.IsFalse(es.IsChanged);

            user.StringArray = r2;
            Assert.IsFalse(es.IsChanged);

            user.StringArray = r3;
            Assert.IsTrue(es.IsChanged);

            user.StringArray = r2;
            Assert.IsFalse(es.IsChanged);
        }


        /// <summary>
        /// Простой метод редактирования инта и возврата к первоночальному значению.
        /// </summary>
        [TestMethod]
        public void TestMethod_RandoomEdit_IsChanges()
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
            Assert.IsFalse(es.IsChanged);
            user.Age = 40;
            Assert.IsTrue(es.IsChanged);
            user.Age = 33;
            Assert.IsFalse(es.IsChanged);
        }

        /// <summary>
        /// Простой метод редактирования инта у двух сущьностей и возврата к первоночальному значению.
        /// </summary>
        [TestMethod]
        public void TestMethod_Edit_2Entity()
        {
            var user = new User();
            user.Name = "Иванов";
            user.Age = 33;

            var user2 = new User();
            user2.Name = "Васечкин";
            user2.Age = 7;

            var es = new EntityStateMonitor();
            es.Aplay<User>( new []{ user,user2 } );

            user.Age = 40;
            user2.Age = 8;
            

            //Возращаем назад возраст

            user.Age = 33;
            //Иванова вернули назад, но Васечкин в модифицированном состоянии. Должно быть IsChanged= true
            Assert.IsTrue(es.IsChanged);
            user2.Age = 7;
            //Васечкин вернули назад, Теперь все сущьности в исходном состоянии. Должно быть IsChanged= false
            Assert.IsFalse(es.IsChanged);
        }
    }

    /// <summary>
    /// Тестовый класс Пользователь.
    /// </summary>
    public class User : Entity
    {
        private int _age;
        private string _name;
        private byte[] _byteArray;
        private string[] _stringArray;
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

        public byte[] ByteArray
        {
            get => _byteArray;
            set
            {
                _byteArray = value;
                OnSetValue(value);
            }
        }

   public string[] StringArray
        {
            get => _stringArray;
            set
            {
                _stringArray = value;
                OnSetValue(value);
            }
        }

        public string[] stringArr { get; set; }
        public List<string> stringList { get; set; }

        public ObservableCollection<User> ListUser { get; set; }
    }
}