using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// Хранение значений одного свойства энтити
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// </summary>
        object this[int index] { get; set; }

        /// <summary>
        /// </summary>
        T GetValue<T>(int index);
    }

    /// <summary>
    /// Хранение типизированного<T0> значений одного свойства энтити
    /// </summary>
    public sealed class Snapshot<T0, T1>
    : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1>>();

        /// <summary>
        /// </summary>
        public Snapshot(T0 value0,T1 value1)
        {
            _value0 = value0;
            _value1 = value1;
        }

        private T0 _value0;
        private T1 _value1;

        /// <summary>
        /// </summary>
        public T GetValue<T>(int index)
            => ((Func<Snapshot<T0, T1>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }


    /// <summary>
    /// Хранение типизированного<T0> значений одного свойства энтити
    /// </summary>
    public sealed class Snapshot<T0>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Snapshot(T0 value0)
        {
            _value0 = value0;
        }

        private T0 _value0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public T GetValue<T>(int index)
            => ((Func<Snapshot<T0>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }


    public sealed class Snapshot : ISnapshot
    {

        private Snapshot()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ISnapshot Empty = new Snapshot();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object this[int index]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new IndexOutOfRangeException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public T GetValue<T>(int index)
        {
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Массив методов, Создает объект MemberExpression, представляющий доступ к полю
        /// </summary>
        public static Delegate[] CreateReaders<TSnapshot>()
        {
            var genericArguments = typeof(TSnapshot).GetTypeInfo().GenericTypeArguments;
            var delegates = new Delegate[genericArguments.Length];

            for (var i = 0; i < genericArguments.Length; ++i)
            {
                var snapshotParameter = Expression.Parameter(typeof(TSnapshot), "snapshot");

                delegates[i] = Expression.Lambda(
                        typeof(Func<,>).MakeGenericType(typeof(TSnapshot), genericArguments[i]),
                        Expression.Field(snapshotParameter, "_value" + i), snapshotParameter)
                    .Compile();
            }

            return delegates;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Type CreateSnapshotType( Type[] types)
        {
            switch (types.Length)
            {
                case 1:
                    return typeof(Snapshot<>).MakeGenericType(types);
                case 2:
                    return typeof(Snapshot<,>).MakeGenericType(types);
               
            }

            throw new IndexOutOfRangeException();
        }




    }
}
