using System;
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

            var es = new EntityStateMonitor();
            es.Aplay<User>(user);
            Assert.IsFalse(es.IsChanges);
            user.Age = 40;
            Assert.IsTrue(es.IsChanges);
            user.Age = 33;
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
    }
}