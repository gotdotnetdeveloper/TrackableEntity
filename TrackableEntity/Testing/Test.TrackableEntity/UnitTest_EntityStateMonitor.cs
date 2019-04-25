using System;
using System.Collections.Generic;
using System.Linq;
using TrackableEntity;
using TrackableEntity.Extension;
using TrackableEntity.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Test.TrackableEntity
{
    /// <summary>
    /// MonitorUnitTest тест.
    /// </summary>
    [TestClass]
    public class UnitTest_EntityStateMonitor
    {


        /// <summary>
        /// Проверка расширения TraverseGraph. 
        /// </summary>
        [TestMethod]
        public void TestMethod_GetFromCurrent()
        {
            var user1 = new User() { Name = "root" };
            var user2 = new User() { Name = "user2 = EntityCollection" };
            var user_innerLis = new User() { Name = "внутри коллекции" };
            user1.ListUser = new EntityCollection<User>(new[] { user2, user_innerLis });

            user1.TrackableGraph();

            user2.Age += 1;

            var Unmodified = user1.GetFromCurrent().Where(x => x.State == EntityState.Unmodified);
            var Modified = user1.GetFromCurrent().Where(x => x.State == EntityState.Modified); 

            Assert.IsTrue(user1.Monitor != null);
            Assert.IsTrue(user1.ListUser.Monitor == user1.Monitor);
            Assert.IsTrue(user1.Monitor == user2.Monitor);
            Assert.IsTrue(user2.Monitor == user_innerLis.Monitor);

            Assert.IsTrue(Modified.Count() ==1);
            Assert.IsTrue(Unmodified.Count() ==2);

        }




        /// <summary>
        /// TestModel
        /// </summary>
        [TestMethod]
        public void TestMethod_ValueCollection()
        {
            var es = new EntityStateMonitor();
            var model = new TestModel();

            model.MyKeyValuePair = new KeyValuePair<int, string>(1, "Значение");
            model.Collection.Add(model.MyKeyValuePair);

            var model2 = model.Clone() as TestModel;

            Assert.IsTrue(model2.Collection.Count == model.Collection.Count);
            Assert.IsTrue(model2.Collection[0].Value == model.Collection[0].Value);
            Assert.IsTrue(model2.Collection[0].Key == model.Collection[0].Key);

            Assert.IsTrue(!ReferenceEquals(model2, model));
            Assert.IsTrue(es.EntityEquals(model2, model));

            es.Apply(model);

            model.Collection.Add(new KeyValuePair<int, string>(4, "543545"));
            Assert.IsTrue(model.ChangedProperties.Contains("Collection"));
            Assert.IsTrue(model.Monitor.IsChanged);

            model.MyKeyValuePair = new KeyValuePair<int, string>(2, "wwe");
            Assert.IsTrue(model.ChangedProperties.Contains("MyKeyValuePair"));

            model.Monitor.RejectChanges();
            Assert.IsTrue(model.Collection[0].Value == "Значение");

            int[] intArr = new[] { 1, 2 };

            model.ArrayIntCollection.Add(intArr);
            Assert.IsTrue(model.ChangedProperties.Contains("ArrayIntCollection"));
            Assert.IsTrue(model.Monitor.IsChanged);


        }


        [TestMethod]
        public void TestMethod_ITrackableDeepClone()
        {
            var es = new EntityStateMonitor();

            var byteArray = new byte[] { 1, 2, 3 };
            var user1 = new User()
            {
                Name = "root",
                Age = 1,
                ByteArray = byteArray,
                StringArray = new[] { "one", "two" },
            };

            es.Apply(user1);
            user1.Age = 2;

            var adress1 = new Adress();
            adress1.Name = "Ленина 1";
            user1.ListAdress.Add(adress1);

            var user2 = ((ITrackable)user1).DeepClone() as User;


            Assert.IsTrue(user2.Age == 2);
            Assert.IsTrue(user2.StringArray.Contains("one"));
            Assert.IsTrue(!user2.ByteArray.Except(byteArray).Any());
            Assert.IsTrue(es.EntityEquals(user1, user2));
            Assert.IsTrue(!ReferenceEquals(user1, user2));
            Assert.IsTrue(es.EntityEquals(user1.ListAdress[0], user2.ListAdress[0]));
            Assert.IsTrue(!ReferenceEquals(user1.ListAdress[0], user2.ListAdress[0]));

        }


        [TestMethod]
        public void TestMethod_DeepClone()
        {
            var es = new EntityStateMonitor();

            var byteArray = new byte[] { 1, 2, 3 };
            var user1 = new User()
            {
                Name = "root",
                Age = 1,
                ByteArray = byteArray,
                StringArray = new[] { "one", "two" },
            };

            es.Apply(user1);
            user1.Age = 2;

            var adress1 = new Adress();
            adress1.Name = "Ленина 1";
            user1.ListAdress.Add(adress1);

            var user2 = user1.DeepClone() as User;
            es.Apply(user2);
            Assert.IsTrue(ReferenceEquals(user1.Monitor, user2.Monitor));

            Assert.IsTrue(user2.Age == 2);
            Assert.IsTrue(user2.StringArray.Contains("one"));
            Assert.IsTrue(!user2.ByteArray.Except(byteArray).Any());
            Assert.IsTrue(es.EntityEquals(user1, user2));
            Assert.IsTrue(!ReferenceEquals(user1, user2));
            Assert.IsTrue(es.EntityEquals(user1.ListAdress[0], user2.ListAdress[0]));
            Assert.IsTrue(!ReferenceEquals(user1.ListAdress[0], user2.ListAdress[0]));

        }

        /// <summary>
        /// Проверка Clone(). 
        /// </summary>
        [TestMethod]
        public void TestMethod_Clone()
        {
            var es = new EntityStateMonitor();

            var byteArray = new byte[] { 1, 2, 3 };
            var user1 = new User()
            {
                Name = "root",
                Age = 1,
                ByteArray = byteArray,
                StringArray = new[] { "one", "two" },
            };
            var user2 = user1.Clone() as User;

            Assert.IsTrue(user2.Age == 1);
            Assert.IsTrue(user2.StringArray.Contains("one"));
            Assert.IsTrue(!user2.ByteArray.Except(byteArray).Any());
            Assert.IsTrue(es.EntityEquals(user1, user2));

        }

        /// <summary>
        /// Проверка расширения TraverseGraph. 
        /// </summary>
        [TestMethod]
        public void TestMethod_TraverseGraph()
        {
            var user1 = new User() { Name = "root" };
            var user2 = new User() { Name = "user2 = EntityCollection" };
            var user_innerLis = new User() { Name = "внутри коллекции" };
            user1.ListUser = new EntityCollection<User>(new[] { user2, user_innerLis });

            var es = new EntityStateMonitor();



            var handleNode = new Func<ITrackable, bool>(x =>
            {

                if (x is BaseEntity baseEntity) // это сущьность
                {
                    if (es.EntitySet.ContainsKey(baseEntity))
                        return false; //уже мониторится  
                    else
                    {
                        es.Apply(baseEntity);


                        var txt = x.ToString();
                        if (x.GetMonitor() == null)
                            Console.WriteLine($"  entity.Monitor == null{txt}");
                        else
                        {
                            var name = es.EntitySet[baseEntity].OriginalValues["Name"].Value;
                            Console.WriteLine($"{txt} OriginalName={name}");
                        }

                        return true;
                    }

                }
                else if (x != null) // это коллекция
                {
                    x.SetMonitor(es);
                    Console.WriteLine($"x.Monitor = es");
                }

                return true;
            });


            es.TraverseGraph(user1, handleNode);


            Assert.IsTrue(user1.Monitor != null);
            Assert.IsTrue(user1.ListUser.Monitor == user1.Monitor);
            Assert.IsTrue(user1.Monitor == user2.Monitor);
            Assert.IsTrue(user2.Monitor == user_innerLis.Monitor);

            Assert.IsTrue(user1.State == EntityState.Unmodified);
            Assert.IsTrue(user2.State == EntityState.Unmodified);
            Assert.IsTrue(user_innerLis.State == EntityState.Unmodified);
            Assert.IsFalse(user_innerLis.Monitor.IsChanged);

        }

        /// <summary>
        /// Тестирование метода ApplyGraph
        /// </summary>
        [TestMethod]
        public void TestMethod_ApplyGraph()
        {
            var user1 = new User() { Name = "root" };
            var user2 = new User() { Name = "user2 = EntityCollection" };
            var user_innerLis = new User() { Name = "внутри коллекции" };
            user1.ListUser = new EntityCollection<User>(new[] { user2, user_innerLis });

            var es = new EntityStateMonitor();

            es.ApplyGraph(user1);


            Assert.IsTrue(user1.Monitor != null);
            Assert.IsTrue(user1.ListUser.Monitor == user1.Monitor);
            Assert.IsTrue(user1.Monitor == user2.Monitor);
            Assert.IsTrue(user2.Monitor == user_innerLis.Monitor);

            Assert.IsTrue(user1.State == EntityState.Unmodified);
            Assert.IsTrue(user2.State == EntityState.Unmodified);
            Assert.IsTrue(user_innerLis.State == EntityState.Unmodified);
            Assert.IsFalse(user_innerLis.Monitor.IsChanged);

        }

        /// <summary>
        /// Тестирование метода GetOriginalProperty
        /// </summary>
        [TestMethod]
        public void TestMethod_ByteArray_GetOriginal()
        {
            var r2 = new byte[] { 1 };
            var user = new User();

            var es = new EntityStateMonitor();
            es.Apply(user);
            user.ByteArray = r2;
            var isGoodValue = user.GetOriginalProperty<byte[]>("ByteArray", out var original);
            Assert.IsTrue(isGoodValue);
            user.Monitor.AcceptChanges();

            Assert.IsTrue(user.GetOriginalProperty<byte[]>("ByteArray", out var original2));
        }

        [TestMethod]
        public void TestMethod_GetOriginalPropertyNullAble()
        {
            var user = new User { Age = 33 };
            user.Trackable();
            user.AgeNullAble = 1;

            var r1 = user.GetOriginalProperty<int?>("AgeNullAble", out var orig1);
            Assert.IsTrue(r1);
            Assert.IsTrue(orig1 == default(int?));

            try
            {
                var r2 = user.GetOriginalProperty<string>("Age", out var orig2);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidCastException);
            }

        }

        [TestMethod]
        public void TestMethod_Trackable()
        {
            var user = new User { Age = 33 };
            user.Trackable();
            Assert.IsTrue(user.State == EntityState.Unmodified);

            Assert.IsTrue(user.Monitor != null);
            user.Age = 1;
            Assert.IsTrue(user.Monitor.IsChanged);
            var u1 = user.Monitor.GetChangedItems().First();
            var u2 = user.Monitor.GetChangedItems<User>().First();
            Assert.IsTrue(ReferenceEquals(user, u1));
            Assert.IsTrue(ReferenceEquals(u1, u2));
        }

        [TestMethod]
        public void TestMethod_GetOriginalProperty()
        {
            var user = new User { Age = 33 };
            user.Trackable();
            user.Age = 1;

            var r1 = user.GetOriginalProperty<int>("Age", out var orig1);
            Assert.IsTrue(r1);
            Assert.IsTrue(orig1 == 33);

            var r2 = user.GetOriginalProperty<int>("Age_4578", out var orig2);
            Assert.IsFalse(r2);
            Assert.IsTrue(orig2 == default(int));

        }

        [TestMethod]
        public void TestMethod_EntityCollection()
        {
            var user = new User { Age = 33 };
            var es = new EntityStateMonitor();

            var entityCollection = new EntityCollection<User>(es);

            entityCollection.Add(user);
            Assert.IsTrue(entityCollection[0].State == EntityState.New);

            entityCollection[0] = new User { Age = 1 };
            Assert.IsTrue(es.EntitySet[entityCollection[0]] != null);

            entityCollection.Remove(entityCollection[0]);
            Assert.IsTrue(es.EntitySet.Keys.Count == 0);


            Assert.IsFalse(es.IsChanged);
        }

        [TestMethod]
        public void TestMethod_RejectChanges_AcceptChanges()
        {
            var user = new User();
            user.Age = 33;

            var es = new EntityStateMonitor();
            es.Apply(user);

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
            var r1 = new byte[] { 1 };
            var r2 = new byte[] { 1 };

            var r3 = new byte[] { 2 };

            var user = new User();
            user.ByteArray = r1;
            var es = new EntityStateMonitor();
            es.Apply(user);

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
            es.Apply(user);

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
            user.ListUser = new EntityCollection<User>();

            user.StringArray = new[] { "1", "2" };

            var es = new EntityStateMonitor();
            es.Apply(user);
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
            es.Apply(new[] { user, user2 });

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
    /// Тестовая модель
    /// </summary>
    [Serializable]
    public class TestModel : BaseEntity
    {

        public TestModel()
        {
            Collection = new ValueCollection<KeyValuePair<int, string>>(this, nameof(Collection));
            ArrayIntCollection = new ValueCollection<int[]>(this, nameof(ArrayIntCollection));
        }

        public KeyValuePair<int, string> MyKeyValuePair
        {
            get => GetValue<KeyValuePair<int, string>>();
            set => SetValue(value);
        }
        public ValueCollection<KeyValuePair<int, string>> Collection
        {
            get => GetValue<ValueCollection<KeyValuePair<int, string>>>();
            set => SetValue(value);
        }


        public ValueCollection<int[]> ArrayIntCollection
        {
            get => GetValue<ValueCollection<int[]>>();
            set => SetValue(value);
        }

    }



    public class UserSpace
    {
        public User User1 { get; set; }
    }

    /// <summary>
    /// Тестовый класс Пользователь.
    /// </summary>
    [Serializable]
    public class User : BaseEntity
    {
        public int Age
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
        public int? AgeNullAble
        {
            get => GetValue<int?>();
            set => SetValue(value);
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public byte[] ByteArray
        {
            get => GetValue<byte[]>();
            set => SetValue(value);
        }

        public string[] StringArray
        {
            get => GetValue<string[]>();
            set => SetValue(value);
        }

        public User User1 { get; set; }
        public BaseEntity Entity { get; set; }
        public EntityCollection<User> ListUser { get; set; } = new EntityCollection<User>();
        public EntityCollection<Adress> ListAdress { get; set; } = new EntityCollection<Adress>();
    }


    [Serializable]
    public class Adress : BaseEntity
    {


        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public byte[] ByteArray
        {
            get => GetValue<byte[]>();
            set => SetValue(value);
        }

        public string[] StringArray
        {
            get => GetValue<string[]>();
            set => SetValue(value);
        }

        public User User1 { get; set; }
        public EntityCollection<User> ListUser { get; set; } = new EntityCollection<User>();
    }
}