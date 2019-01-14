using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrackableEntity
{
    /// <summary>
    /// Сравнение уникальности по ссылкам в памяти (Reference).
    /// </summary>
    public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        private ReferenceEqualityComparer()
        {
        }

        /// <summary>
        /// статический Экземпляр.
        /// </summary>
        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();
        /// <summary>
        /// Сравнение по ссылкам.
        /// </summary>
        /// <param name="x">Первое значениие.</param>
        /// <param name="y">Второе значение.</param>
        /// <returns>Равны ли объекты.</returns>
        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

        /// <summary>
        /// Хеш-код
        /// </summary>
        /// <param name="obj">Объект у которого получаем Хеш-код</param>
        /// <returns>уникальный для экземпляра Хеш-код</returns>
        int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
